using StudentReportInitial.Models;
using StudentReportInitial.Data;
using System.Data.SqlClient;
using System.Data;
using System.Windows.Forms;
using System.Drawing;

namespace StudentReportInitial.Forms
{
    public partial class ProfessorAttendancePanel : UserControl
    {
        private User currentProfessor;
        private DataGridView dgvStudents;
        private ComboBox cmbSubject;
        private DateTimePicker dtpDate;
        private Button btnSaveAttendance;
        private Button btnRefresh;
        private Panel pnlAttendanceForm;
        private List<Student> currentStudents = new();

        public ProfessorAttendancePanel(User professor)
        {
            currentProfessor = professor;
            InitializeComponent();
            ApplyModernStyling();
            LoadSubjects();
            LoadStudents();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // Main container
            this.Size = new Size(1000, 600);
            this.BackColor = Color.FromArgb(248, 250, 252);

            // Stats panel
            var pnlStats = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.White,
                Padding = new Padding(20)
            };

            var lblStats = new Label
            {
                AutoSize = true,
                Location = new Point(20, 20),
                Text = "Loading attendance statistics..."
            };
            pnlStats.Controls.Add(lblStats);

            // Header panel
            var pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 100,
                BackColor = Color.White,
                Padding = new Padding(20)
            };

            var lblTitle = new Label
            {
                Text = "Record Attendance",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(51, 65, 85),
                AutoSize = true,
                Location = new Point(20, 20)
            };

            // Subject selection
            var lblSubject = new Label
            {
                Text = "Subject:",
                Location = new Point(20, 50),
                AutoSize = true
            };

            cmbSubject = new ComboBox
            {
                Location = new Point(80, 48),
                Size = new Size(200, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbSubject.SelectedIndexChanged += CmbSubject_SelectedIndexChanged;

            // Date selection
            var lblDate = new Label
            {
                Text = "Date:",
                Location = new Point(300, 50),
                AutoSize = true
            };

            dtpDate = new DateTimePicker
            {
                Location = new Point(340, 48),
                Size = new Size(150, 25),
                Value = DateTime.Today
            };
            dtpDate.ValueChanged += DtpDate_ValueChanged;

            btnRefresh = new Button
            {
                Text = "Refresh",
                Location = new Point(510, 47),
                Size = new Size(80, 27),
                BackColor = Color.FromArgb(59, 130, 246),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                
                Cursor = Cursors.Hand
            };
            btnRefresh.Click += BtnRefresh_Click;

            pnlHeader.Controls.AddRange(new Control[] { lblTitle, lblSubject, cmbSubject, lblDate, dtpDate, btnRefresh });

            // Students grid
            dgvStudents = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = true,
                ReadOnly = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowHeadersVisible = false
            };

            // Add attendance status column
            var statusColumn = new DataGridViewComboBoxColumn
            {
                Name = "AttendanceStatus",
                HeaderText = "Status",
                DataPropertyName = "AttendanceStatus",
                Width = 120
            };
            statusColumn.Items.AddRange(new[] { "Present", "Absent", "Late", "Excused" });
            dgvStudents.Columns.Add(statusColumn);

            // Add notes column
            var notesColumn = new DataGridViewTextBoxColumn
            {
                Name = "Notes",
                HeaderText = "Notes",
                DataPropertyName = "Notes",
                Width = 200
            };
            dgvStudents.Columns.Add(notesColumn);

            // Action panel
            var pnlActions = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 60,
                BackColor = Color.White,
                Padding = new Padding(20)
            };

            btnSaveAttendance = new Button
            {
                Text = "Save Attendance",
                Size = new Size(150, 35),
                Location = new Point(20, 12),
                BackColor = Color.FromArgb(34, 197, 94),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            btnSaveAttendance.Click += BtnSaveAttendance_Click;

            pnlActions.Controls.Add(btnSaveAttendance);

            this.Controls.Add(dgvStudents);
            this.Controls.Add(pnlActions);
            this.Controls.Add(pnlHeader);

            this.ResumeLayout(false);
        }

        private void ApplyModernStyling()
        {
            var font = new Font("Segoe UI", 9);
            var headerFont = new Font("Segoe UI", 10, FontStyle.Bold);

            foreach (Control control in this.Controls)
            {
                if (control is Button button)
                {
                    button.Font = font;
                }
                else if (control is ComboBox comboBox)
                {
                    comboBox.Font = font;
                }
                else if (control is DateTimePicker dateTimePicker)
                {
                    dateTimePicker.Font = font;
                }
            }

            dgvStudents.Font = font;
            dgvStudents.ColumnHeadersDefaultCellStyle.Font = headerFont;
            dgvStudents.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
            dgvStudents.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(51, 65, 85);
            dgvStudents.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
        }

        private async void LoadSubjects()
        {
            try
            {
                using var connection = DatabaseHelper.GetConnection();
                await connection.OpenAsync();

                var query = @"
                    SELECT DISTINCT s.Name, s.GradeLevel, s.Section
                    FROM Subjects s
                    WHERE s.ProfessorId = @professorId AND s.IsActive = 1
                    ORDER BY s.Name";

                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@professorId", currentProfessor.Id);

                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var subjectName = reader.GetString("Name");
                    var gradeLevel = reader.GetString("GradeLevel");
                    var section = reader.GetString("Section");
                    cmbSubject.Items.Add($"{subjectName} - {gradeLevel} - {section}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading subjects: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void LoadStudents()
        {
            if (cmbSubject.SelectedIndex == -1) return;

            try
            {
                var selectedSubject = cmbSubject.SelectedItem.ToString();
                var parts = selectedSubject.Split(" - ");
                var subjectName = parts[0];
                var gradeLevel = parts[1];
                var section = parts[2];

                using var connection = DatabaseHelper.GetConnection();
                await connection.OpenAsync();

                var query = @"
                    SELECT s.Id, s.StudentId, s.FirstName, s.LastName, s.GradeLevel, s.Section
                    FROM Students s
                    WHERE s.GradeLevel = @gradeLevel AND s.Section = @section AND s.IsActive = 1
                    ORDER BY s.LastName, s.FirstName";

                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@gradeLevel", gradeLevel);
                command.Parameters.AddWithValue("@section", section);

                using var adapter = new SqlDataAdapter(command);
                var dataTable = new DataTable();
                adapter.Fill(dataTable);

                // Add attendance columns
                dataTable.Columns.Add("AttendanceStatus", typeof(string));
                dataTable.Columns.Add("Notes", typeof(string));

                // Set default values
                foreach (DataRow row in dataTable.Rows)
                {
                    row["AttendanceStatus"] = "Present";
                    row["Notes"] = "";
                }

                dgvStudents.DataSource = dataTable;

                // Format columns
                if (dgvStudents.Columns.Count > 0)
                {
                    dgvStudents.Columns["Id"].Visible = false;
                    dgvStudents.Columns["StudentId"].HeaderText = "Student ID";
                    dgvStudents.Columns["FirstName"].HeaderText = "First Name";
                    dgvStudents.Columns["LastName"].HeaderText = "Last Name";
                    dgvStudents.Columns["GradeLevel"].HeaderText = "Grade";
                    dgvStudents.Columns["Section"].HeaderText = "Section";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading students: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CmbSubject_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadStudents();
        }

        private async void DtpDate_ValueChanged(object sender, EventArgs e)
        {
            if (cmbSubject.SelectedIndex == -1) return;

            try
            {
                var selectedSubject = cmbSubject.SelectedItem?.ToString();
                if (string.IsNullOrEmpty(selectedSubject))
                    return;
                    
                var parts = selectedSubject.Split(" - ");
                if (parts.Length < 1)
                    return;
                    
                var subjectName = parts[0];

                using var connection = DatabaseHelper.GetConnection();
                await connection.OpenAsync();

                var query = @"
                    SELECT a.StudentId, a.Status, a.Notes
                    FROM Attendance a
                    WHERE a.ProfessorId = @professorId 
                        AND a.Subject = @subject 
                        AND a.Date = @date";

                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@professorId", currentProfessor.Id);
                command.Parameters.AddWithValue("@subject", subjectName);
                command.Parameters.AddWithValue("@date", dtpDate.Value.Date);

                using var reader = await command.ExecuteReaderAsync();
                var attendanceData = new Dictionary<int, (AttendanceStatus status, string notes)>();

                while (await reader.ReadAsync())
                {
                    var studentId = reader.GetInt32(0);
                    var status = (AttendanceStatus)reader.GetInt32(1);
                    var notes = reader.IsDBNull(2) ? "" : reader.GetString(2);
                    attendanceData[studentId] = (status, notes);
                }

                // Update the grid with existing attendance data
                foreach (DataGridViewRow row in dgvStudents.Rows)
                {
                    if (row.Cells["Id"].Value != null)
                    {
                        var studentId = Convert.ToInt32(row.Cells["Id"].Value);
                        if (attendanceData.TryGetValue(studentId, out var record))
                        {
                            row.Cells["AttendanceStatus"].Value = record.status.ToString();
                            row.Cells["Notes"].Value = record.notes;
                        }
                        else
                        {
                            row.Cells["AttendanceStatus"].Value = "Present";
                            row.Cells["Notes"].Value = "";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading existing attendance: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            LoadStudents();
        }

        private async void BtnSaveAttendance_Click(object sender, EventArgs e)
        {
            if (cmbSubject.SelectedIndex == -1)
            {
                MessageBox.Show("Please select a subject first.", "Warning", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var selectedSubject = cmbSubject.SelectedItem?.ToString();
                if (string.IsNullOrEmpty(selectedSubject))
                {
                    MessageBox.Show("Please select a valid subject.", "Warning",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                    
                var parts = selectedSubject.Split(" - ");
                if (parts.Length < 1)
                {
                    MessageBox.Show("Invalid subject format.", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                    
                var subjectName = parts[0];

                var attendanceRecords = new List<Attendance>();

                foreach (DataGridViewRow row in dgvStudents.Rows)
                {
                    if (row.Cells["Id"].Value != null)
                    {
                        var attendance = new Attendance
                        {
                            StudentId = Convert.ToInt32(row.Cells["Id"].Value),
                            ProfessorId = currentProfessor.Id,
                            Subject = subjectName,
                            Date = dtpDate.Value.Date,
                            Status = GetAttendanceStatus(row.Cells["AttendanceStatus"].Value?.ToString()),
                            Notes = row.Cells["Notes"].Value?.ToString(),
                            RecordedDate = DateTime.Now
                        };
                        attendanceRecords.Add(attendance);
                    }
                }

                await SaveAttendanceRecordsAsync(attendanceRecords);
                MessageBox.Show("Attendance saved successfully!", "Success", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving attendance: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private AttendanceStatus GetAttendanceStatus(string? status)
        {
            return (status ?? "Present") switch
            {
                "Present" => AttendanceStatus.Present,
                "Absent" => AttendanceStatus.Absent,
                "Late" => AttendanceStatus.Late,
                "Excused" => AttendanceStatus.Excused,
                _ => AttendanceStatus.Present
            };
        }

        private async Task SaveAttendanceRecordsAsync(List<Attendance> attendanceRecords)
        {
            using var connection = DatabaseHelper.GetConnection();
            await connection.OpenAsync();

            // Delete existing attendance for the same date and subject
            var deleteQuery = @"
                DELETE FROM Attendance 
                WHERE ProfessorId = @professorId AND Subject = @subject AND Date = @date";

            using var deleteCommand = new SqlCommand(deleteQuery, connection);
            deleteCommand.Parameters.AddWithValue("@professorId", currentProfessor.Id);
            deleteCommand.Parameters.AddWithValue("@subject", attendanceRecords.First().Subject);
            deleteCommand.Parameters.AddWithValue("@date", dtpDate.Value.Date);
            await deleteCommand.ExecuteNonQueryAsync();

            // Insert new attendance records
            var insertQuery = @"
                INSERT INTO Attendance (StudentId, ProfessorId, Subject, Date, Status, Notes, RecordedDate)
                VALUES (@studentId, @professorId, @subject, @date, @status, @notes, @recordedDate)";

            foreach (var attendance in attendanceRecords)
            {
                using var insertCommand = new SqlCommand(insertQuery, connection);
                insertCommand.Parameters.AddWithValue("@studentId", attendance.StudentId);
                insertCommand.Parameters.AddWithValue("@professorId", attendance.ProfessorId);
                insertCommand.Parameters.AddWithValue("@subject", attendance.Subject);
                insertCommand.Parameters.AddWithValue("@date", attendance.Date);
                insertCommand.Parameters.AddWithValue("@status", (int)attendance.Status);
                insertCommand.Parameters.AddWithValue("@notes", attendance.Notes ?? "");
                insertCommand.Parameters.AddWithValue("@recordedDate", attendance.RecordedDate);

                await insertCommand.ExecuteNonQueryAsync();
            }
        }
    }
}

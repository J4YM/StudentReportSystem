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
        private Label lblStats = null!;

        public ProfessorAttendancePanel(User professor)
        {
            currentProfessor = professor;
            InitializeComponent();
            ApplyModernStyling();
            LoadSubjects();
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

            lblStats = new Label
            {
                AutoSize = true,
                Location = new Point(20, 20),
                Text = "Please select a subject to view attendance statistics.",
                Font = new Font("Segoe UI", 9)
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
                MultiSelect = false,
                ReadOnly = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowHeadersVisible = false,
                AutoGenerateColumns = false,
                EditMode = DataGridViewEditMode.EditOnEnter
            };

            // Add columns manually
            dgvStudents.Columns.Add("Id", "ID");
            dgvStudents.Columns["Id"].Visible = false;
            dgvStudents.Columns["Id"].DataPropertyName = "Id";

            dgvStudents.Columns.Add("StudentId", "Student ID");
            dgvStudents.Columns["StudentId"].DataPropertyName = "StudentId";
            dgvStudents.Columns["StudentId"].Width = 100;

            dgvStudents.Columns.Add("FirstName", "First Name");
            dgvStudents.Columns["FirstName"].DataPropertyName = "FirstName";
            dgvStudents.Columns["FirstName"].Width = 150;

            dgvStudents.Columns.Add("LastName", "Last Name");
            dgvStudents.Columns["LastName"].DataPropertyName = "LastName";
            dgvStudents.Columns["LastName"].Width = 150;

            dgvStudents.Columns.Add("GradeLevel", "Grade");
            dgvStudents.Columns["GradeLevel"].DataPropertyName = "GradeLevel";
            dgvStudents.Columns["GradeLevel"].Width = 80;

            dgvStudents.Columns.Add("Section", "Section");
            dgvStudents.Columns["Section"].DataPropertyName = "Section";
            dgvStudents.Columns["Section"].Width = 80;

            // Use TextBox column for status (we'll add a separate ComboBox control)
            dgvStudents.Columns.Add("AttendanceStatus", "Status");
            dgvStudents.Columns["AttendanceStatus"].Width = 120;
            dgvStudents.Columns["AttendanceStatus"].ReadOnly = true;

            dgvStudents.Columns.Add("Notes", "Notes");
            dgvStudents.Columns["Notes"].Width = 200;

            // Event handlers for manual ComboBox handling
            dgvStudents.CellClick += DgvStudents_CellClick;
            dgvStudents.CurrentCellDirtyStateChanged += DgvStudents_CurrentCellDirtyStateChanged;

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

            // Add controls in correct order
            this.Controls.Add(dgvStudents);
            this.Controls.Add(pnlActions);
            this.Controls.Add(pnlHeader);
            this.Controls.Add(pnlStats);

            this.ResumeLayout(false);
        }

        private ComboBox? statusComboBox;

        private void DgvStudents_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;

            if (dgvStudents.Columns[e.ColumnIndex].Name == "AttendanceStatus")
            {
                // Remove existing ComboBox if any
                if (statusComboBox != null)
                {
                    dgvStudents.Controls.Remove(statusComboBox);
                    statusComboBox.Dispose();
                    statusComboBox = null;
                }

                // Create a new ComboBox
                var comboBox = new ComboBox
                {
                    DropDownStyle = ComboBoxStyle.DropDownList,
                    FlatStyle = FlatStyle.Flat
                };
                comboBox.Items.AddRange(new[] { "Present", "Absent", "Late", "Excused" });

                // Set current value
                var currentValue = dgvStudents[e.ColumnIndex, e.RowIndex].Value?.ToString() ?? "Present";
                comboBox.SelectedItem = currentValue;

                // Position the ComboBox over the cell
                var rect = dgvStudents.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, true);
                comboBox.Location = rect.Location;
                comboBox.Size = rect.Size;

                // Store reference
                statusComboBox = comboBox;

                // Capture the current row and column for the event handlers
                int currentRow = e.RowIndex;
                int currentCol = e.ColumnIndex;

                // Helper method to save the selected value
                Action<ComboBox> saveValue = (cb) =>
                {
                    if (cb != null && cb.SelectedItem != null && !cb.Disposing && 
                        currentRow >= 0 && currentRow < dgvStudents.Rows.Count && 
                        currentCol >= 0 && currentCol < dgvStudents.Columns.Count)
                    {
                        try
                        {
                            var selectedValue = cb.SelectedItem.ToString();
                            var cell = dgvStudents[currentCol, currentRow];
                            cell.Value = selectedValue;
                            // Force the DataGridView to commit the edit
                            dgvStudents.NotifyCurrentCellDirty(true);
                            dgvStudents.EndEdit();
                            // Refresh the cell to ensure the value is displayed
                            dgvStudents.InvalidateCell(currentCol, currentRow);
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error saving attendance status: {ex.Message}");
                        }
                    }
                };

                // Handle selection change - update immediately
                comboBox.SelectedIndexChanged += (s, args) =>
                {
                    var cb = s as ComboBox;
                    if (cb != null)
                    {
                        saveValue(cb);
                    }
                };

                // Handle when ComboBox closes - ensure value is saved
                comboBox.DropDownClosed += (s, args) =>
                {
                    var cb = s as ComboBox;
                    if (cb != null && !cb.Disposing)
                    {
                        saveValue(cb);
                        
                        // Use BeginInvoke to remove ComboBox after UI updates
                        dgvStudents.BeginInvoke(new Action(() =>
                        {
                            if (dgvStudents.Controls.Contains(cb))
                            {
                                dgvStudents.Controls.Remove(cb);
                            }
                            if (!cb.IsDisposed)
                            {
                                cb.Dispose();
                            }
                            if (statusComboBox == cb)
                            {
                                statusComboBox = null;
                            }
                        }));
                    }
                };

                // Handle when ComboBox loses focus - ensure value is saved
                comboBox.Leave += (s, args) =>
                {
                    var cb = s as ComboBox;
                    if (cb != null && !cb.Disposing && dgvStudents.Controls.Contains(cb))
                    {
                        saveValue(cb);
                        
                        // Use BeginInvoke to remove ComboBox after UI updates
                        dgvStudents.BeginInvoke(new Action(() =>
                        {
                            if (dgvStudents.Controls.Contains(cb))
                            {
                                dgvStudents.Controls.Remove(cb);
                            }
                            if (!cb.IsDisposed)
                            {
                                cb.Dispose();
                            }
                            if (statusComboBox == cb)
                            {
                                statusComboBox = null;
                            }
                        }));
                    }
                };

                // Add to DataGridView and show
                dgvStudents.Controls.Add(comboBox);
                comboBox.Focus();
                comboBox.DroppedDown = true;
            }
        }

        private void DgvStudents_CurrentCellDirtyStateChanged(object? sender, EventArgs e)
        {
            if (dgvStudents.IsCurrentCellDirty)
            {
                dgvStudents.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        }

        private void ApplyModernStyling()
        {
            var font = new Font("Segoe UI", 9);
            var headerFont = new Font("Segoe UI", 10, FontStyle.Bold);

            foreach (Control control in this.Controls)
            {
                if (control is Panel panel)
                {
                    foreach (Control childControl in panel.Controls)
                    {
                        if (childControl is Button button)
                        {
                            button.Font = font;
                        }
                        else if (childControl is ComboBox comboBox)
                        {
                            comboBox.Font = font;
                        }
                        else if (childControl is DateTimePicker dateTimePicker)
                        {
                            dateTimePicker.Font = font;
                        }
                    }
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
                    WHERE s.ProfessorId = @professorId AND s.IsActive = 1";

                // Add branch filter based on professor's branch
                var professorBranchId = await BranchHelper.GetUserBranchIdAsync(currentProfessor.Id);
                if (professorBranchId > 0)
                {
                    query += " AND s.BranchId = @branchId";
                }

                query += " ORDER BY s.Name";

                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@professorId", currentProfessor.Id);
                
                if (professorBranchId > 0)
                {
                    command.Parameters.AddWithValue("@branchId", professorBranchId);
                }

                using var reader = await command.ExecuteReaderAsync();
                cmbSubject.Items.Clear();
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
                var selectedSubject = cmbSubject.SelectedItem?.ToString();
                if (string.IsNullOrEmpty(selectedSubject)) return;

                var parts = selectedSubject.Split(" - ");
                if (parts.Length < 3) return;

                var subjectName = parts[0];
                var gradeLevel = parts[1];
                var section = parts[2];

                using var connection = DatabaseHelper.GetConnection();
                await connection.OpenAsync();

                var query = @"
                    SELECT s.Id, s.StudentId, s.FirstName, s.LastName, s.GradeLevel, s.Section
                    FROM Students s
                    WHERE s.GradeLevel = @gradeLevel AND s.Section = @section AND s.IsActive = 1";

                // Add branch filter based on professor's branch
                var professorBranchId = await BranchHelper.GetUserBranchIdAsync(currentProfessor.Id);
                if (professorBranchId > 0)
                {
                    query += " AND s.BranchId = @branchId";
                }

                query += " ORDER BY s.LastName, s.FirstName";

                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@gradeLevel", gradeLevel);
                command.Parameters.AddWithValue("@section", section);
                
                if (professorBranchId > 0)
                {
                    command.Parameters.AddWithValue("@branchId", professorBranchId);
                }

                using var adapter = new SqlDataAdapter(command);
                var dataTable = new DataTable();
                adapter.Fill(dataTable);

                dgvStudents.DataSource = dataTable;

                // Set default values for attendance columns
                foreach (DataGridViewRow row in dgvStudents.Rows)
                {
                    row.Cells["AttendanceStatus"].Value = "Present";
                    row.Cells["Notes"].Value = "";
                }

                // Load existing attendance for the selected date
                await LoadExistingAttendanceAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading students: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task LoadExistingAttendanceAsync()
        {
            if (cmbSubject.SelectedIndex == -1) return;

            try
            {
                var selectedSubject = cmbSubject.SelectedItem?.ToString();
                if (string.IsNullOrEmpty(selectedSubject)) return;

                var parts = selectedSubject.Split(" - ");
                if (parts.Length < 1) return;

                var subjectName = parts[0];

                using var connection = DatabaseHelper.GetConnection();
                await connection.OpenAsync();

                var query = @"
                    SELECT a.StudentId, a.Status, a.Notes
                    FROM Attendance a
                    INNER JOIN Students s ON a.StudentId = s.Id
                    WHERE a.ProfessorId = @professorId 
                        AND a.Subject = @subject 
                        AND CAST(a.Date AS DATE) = CAST(@date AS DATE)";

                // Add branch filter based on professor's branch
                var professorBranchId = await BranchHelper.GetUserBranchIdAsync(currentProfessor.Id);
                if (professorBranchId > 0)
                {
                    query += " AND a.BranchId = @branchId";
                }

                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@professorId", currentProfessor.Id);
                command.Parameters.AddWithValue("@subject", subjectName);
                command.Parameters.AddWithValue("@date", dtpDate.Value.Date);
                
                if (professorBranchId > 0)
                {
                    command.Parameters.AddWithValue("@branchId", professorBranchId);
                }

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

        private async void CmbSubject_SelectedIndexChanged(object? sender, EventArgs e)
        {
            LoadStudents();
            await UpdateAttendanceStatsAsync();
        }

        private async void DtpDate_ValueChanged(object? sender, EventArgs e)
        {
            await LoadExistingAttendanceAsync();
            await UpdateAttendanceStatsAsync();
        }

        private void BtnRefresh_Click(object? sender, EventArgs e)
        {
            LoadStudents();
        }

        private async void BtnSaveAttendance_Click(object? sender, EventArgs e)
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
                await UpdateAttendanceStatsAsync();
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

            var deleteQuery = @"
                DELETE FROM Attendance 
                WHERE ProfessorId = @professorId AND Subject = @subject AND CAST(Date AS DATE) = CAST(@date AS DATE)";

            // Add branch filter based on professor's branch
            var professorBranchId = await BranchHelper.GetUserBranchIdAsync(currentProfessor.Id);
            if (professorBranchId > 0)
            {
                deleteQuery += " AND BranchId = @branchId";
            }

            using var deleteCommand = new SqlCommand(deleteQuery, connection);
            deleteCommand.Parameters.AddWithValue("@professorId", currentProfessor.Id);
            deleteCommand.Parameters.AddWithValue("@subject", attendanceRecords.First().Subject);
            deleteCommand.Parameters.AddWithValue("@date", dtpDate.Value.Date);
            
            if (professorBranchId > 0)
            {
                deleteCommand.Parameters.AddWithValue("@branchId", professorBranchId);
            }
            
            await deleteCommand.ExecuteNonQueryAsync();

            var insertQuery = @"
                INSERT INTO Attendance (StudentId, ProfessorId, Subject, Date, Status, Notes, RecordedDate, BranchId)
                VALUES (@studentId, @professorId, @subject, @date, @status, @notes, @recordedDate, @branchId)";

            string professorName = $"{currentProfessor.FirstName} {currentProfessor.LastName}";
            var subjectName = attendanceRecords.First().Subject;

            foreach (var attendance in attendanceRecords)
            {
                // Get student's branch ID for the attendance record
                var studentBranchQuery = "SELECT BranchId FROM Students WHERE Id = @studentId";
                int branchId = 0;
                using var studentBranchCommand = new SqlCommand(studentBranchQuery, connection);
                studentBranchCommand.Parameters.AddWithValue("@studentId", attendance.StudentId);
                var branchResult = await studentBranchCommand.ExecuteScalarAsync();
                if (branchResult != null && branchResult != DBNull.Value)
                {
                    branchId = Convert.ToInt32(branchResult);
                }

                using var insertCommand = new SqlCommand(insertQuery, connection);
                insertCommand.Parameters.AddWithValue("@studentId", attendance.StudentId);
                insertCommand.Parameters.AddWithValue("@professorId", attendance.ProfessorId);
                insertCommand.Parameters.AddWithValue("@subject", attendance.Subject);
                insertCommand.Parameters.AddWithValue("@date", attendance.Date);
                insertCommand.Parameters.AddWithValue("@status", (int)attendance.Status);
                insertCommand.Parameters.AddWithValue("@notes", attendance.Notes ?? "");
                insertCommand.Parameters.AddWithValue("@recordedDate", attendance.RecordedDate);
                insertCommand.Parameters.AddWithValue("@branchId", branchId);

                await insertCommand.ExecuteNonQueryAsync();

                try
                {
                    var studentQuery = @"
                        SELECT s.FirstName, s.LastName, s.Phone, u.Phone as GuardianPhone
                        FROM Students s
                        LEFT JOIN Users u ON s.GuardianId = u.Id
                        WHERE s.Id = @studentId";

                    using var studentCommand = new SqlCommand(studentQuery, connection);
                    studentCommand.Parameters.AddWithValue("@studentId", attendance.StudentId);

                    using var studentReader = await studentCommand.ExecuteReaderAsync();
                    if (await studentReader.ReadAsync())
                    {
                        string studentName = $"{studentReader.GetString("FirstName")} {studentReader.GetString("LastName")}";
                        string phoneNumber = studentReader.IsDBNull("GuardianPhone")
                            ? (studentReader.IsDBNull("Phone") ? "" : studentReader.GetString("Phone"))
                            : studentReader.GetString("GuardianPhone");

                        if (string.IsNullOrEmpty(phoneNumber) && !studentReader.IsDBNull("Phone"))
                        {
                            phoneNumber = studentReader.GetString("Phone");
                        }

                        if (!string.IsNullOrEmpty(phoneNumber) && PhoneValidator.IsValidPhilippinesMobile(phoneNumber))
                        {
                            int attendanceCount = await GetStudentAttendanceCountAsync(connection, attendance.StudentId);

                            await SmsService.SendAttendanceNotificationAsync(
                                phoneNumber,
                                studentName,
                                subjectName,
                                attendance.Status.ToString(),
                                attendance.Date,
                                professorName,
                                attendanceCount
                            );
                        }
                    }
                    studentReader.Close();
                }
                catch (Exception smsEx)
                {
                    System.Diagnostics.Debug.WriteLine($"SMS notification failed: {smsEx.Message}");
                }
            }
        }

        private async Task<int> GetStudentAttendanceCountAsync(SqlConnection connection, int studentId)
        {
            try
            {
                var countQuery = @"
                    SELECT COUNT(*) 
                    FROM Attendance 
                    WHERE StudentId = @studentId";

                using var countCommand = new SqlCommand(countQuery, connection);
                countCommand.Parameters.AddWithValue("@studentId", studentId);

                var result = await countCommand.ExecuteScalarAsync();
                return result != null ? Convert.ToInt32(result) : 0;
            }
            catch
            {
                return 0;
            }
        }

        private async Task UpdateAttendanceStatsAsync()
        {
            if (cmbSubject.SelectedIndex == -1)
            {
                lblStats.Text = "Please select a subject to view attendance statistics.";
                return;
            }

            try
            {
                var selectedSubject = cmbSubject.SelectedItem?.ToString();
                if (string.IsNullOrEmpty(selectedSubject))
                {
                    lblStats.Text = "Please select a subject to view attendance statistics.";
                    return;
                }

                var parts = selectedSubject.Split(" - ");
                if (parts.Length < 1)
                {
                    lblStats.Text = "Invalid subject format.";
                    return;
                }

                var subjectName = parts[0];

                using var connection = DatabaseHelper.GetConnection();
                await connection.OpenAsync();

                var statsQuery = @"
                    SELECT 
                        SUM(CASE WHEN Status = 1 THEN 1 ELSE 0 END) as Present,
                        SUM(CASE WHEN Status = 2 THEN 1 ELSE 0 END) as Absent,
                        SUM(CASE WHEN Status = 3 THEN 1 ELSE 0 END) as Late,
                        SUM(CASE WHEN Status = 4 THEN 1 ELSE 0 END) as Excused
                    FROM Attendance
                    WHERE ProfessorId = @professorId 
                        AND Subject = @subject 
                        AND CAST(Date AS DATE) = CAST(@date AS DATE)";

                // Add branch filter based on professor's branch
                var professorBranchId = await BranchHelper.GetUserBranchIdAsync(currentProfessor.Id);
                if (professorBranchId > 0)
                {
                    statsQuery += " AND BranchId = @branchId";
                }

                using var command = new SqlCommand(statsQuery, connection);
                command.Parameters.AddWithValue("@professorId", currentProfessor.Id);
                command.Parameters.AddWithValue("@subject", subjectName);
                command.Parameters.AddWithValue("@date", dtpDate.Value.Date);
                
                if (professorBranchId > 0)
                {
                    command.Parameters.AddWithValue("@branchId", professorBranchId);
                }

                using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    int present = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);
                    int absent = reader.IsDBNull(1) ? 0 : reader.GetInt32(1);
                    int late = reader.IsDBNull(2) ? 0 : reader.GetInt32(2);
                    int excused = reader.IsDBNull(3) ? 0 : reader.GetInt32(3);

                    lblStats.Text = $"Attendance Statistics - Present: {present} | Absent: {absent} | Late: {late} | Excused: {excused}";
                }
                else
                {
                    lblStats.Text = "No attendance records found for this date.";
                }
            }
            catch (Exception ex)
            {
                lblStats.Text = $"Error loading statistics: {ex.Message}";
            }
        }
    }
}
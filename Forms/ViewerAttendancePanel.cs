using StudentReportInitial.Data;
using StudentReportInitial.Models;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Threading.Tasks;

namespace StudentReportInitial.Forms
{
    public partial class ViewerAttendancePanel : UserControl
    {
        private User currentUser;
        private DataGridView dgvAttendance;
        private ComboBox cmbStudent;
        private ComboBox cmbSubject;
        private DateTimePicker dtpFromDate;
        private DateTimePicker dtpToDate;
        private Button btnRefresh;
        private Label lblAttendanceSummary;
        private Panel pnlSummary;
        private Label? lblStudentInfo;
        private Student? linkedStudent;
        private bool studentContextResolved;
        private readonly List<Student> guardianStudents = new();
        private bool guardianStudentSelectorInitialized = false;

        public ViewerAttendancePanel(User user)
        {
            currentUser = user;
            InitializeComponent();
            ApplyModernStyling();
            _ = InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            if (!await EnsureStudentContextAsync())
            {
                HandleMissingStudentContext();
                return;
            }

            await LoadSubjectsAsync();
            await LoadAttendanceAsync();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // Main container
            this.Size = new Size(1000, 600);
            this.BackColor = Color.FromArgb(248, 250, 252);

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
                Text = "Attendance Report",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(51, 65, 85),
                AutoSize = true,
                Location = new Point(20, 20)
            };

            // Student selector (for guardians)
            lblStudentInfo = new Label
            {
                Text = "Student:",
                Location = new Point(20, 50),
                AutoSize = true,
                Visible = false
            };

            cmbStudent = new ComboBox
            {
                Location = new Point(90, 48),
                Size = new Size(220, 25),
                DropDownStyle = ComboBoxStyle.DropDownList,
                FormattingEnabled = true,
                Visible = false
            };
            cmbStudent.SelectedIndexChanged += CmbStudent_SelectedIndexChanged;

            // Subject filter
            var lblSubject = new Label
            {
                Text = "Subject:",
                Location = new Point(330, 50),
                AutoSize = true
            };

            cmbSubject = new ComboBox
            {
                Location = new Point(390, 48),
                Size = new Size(180, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbSubject.Items.Add("All Subjects");
            cmbSubject.SelectedIndex = 0;
            cmbSubject.SelectedIndexChanged += CmbSubject_SelectedIndexChanged;

            // Date range filters
            var lblFromDate = new Label
            {
                Text = "From:",
                Location = new Point(590, 50),
                AutoSize = true
            };

            dtpFromDate = new DateTimePicker
            {
                Location = new Point(630, 48),
                Size = new Size(120, 25),
                Value = DateTime.Today.AddDays(-30)
            };

            var lblToDate = new Label
            {
                Text = "To:",
                Location = new Point(770, 50),
                AutoSize = true
            };

            dtpToDate = new DateTimePicker
            {
                Location = new Point(800, 48),
                Size = new Size(120, 25),
                Value = DateTime.Today
            };

            btnRefresh = new Button
            {
                Text = "Refresh",
                Location = new Point(940, 47),
                Size = new Size(90, 27),
                BackColor = Color.FromArgb(59, 130, 246),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                
                Cursor = Cursors.Hand
            };
            btnRefresh.Click += BtnRefresh_Click;

            pnlHeader.Controls.AddRange(new Control[] { 
                lblTitle,
                lblStudentInfo!, cmbStudent,
                lblSubject, cmbSubject, lblFromDate, dtpFromDate, 
                lblToDate, dtpToDate, btnRefresh
            });

            // Summary panel
            pnlSummary = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.FromArgb(59, 130, 246),
                Padding = new Padding(20)
            };

            lblAttendanceSummary = new Label
            {
                Text = "Attendance Summary: --",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(20, 15)
            };

            pnlSummary.Controls.Add(lblAttendanceSummary);

            // Attendance grid
            dgvAttendance = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowHeadersVisible = false
            };

            this.Controls.Add(dgvAttendance);
            this.Controls.Add(pnlSummary);
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

            dgvAttendance.Font = font;
            dgvAttendance.ColumnHeadersDefaultCellStyle.Font = headerFont;
            dgvAttendance.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
            dgvAttendance.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(51, 65, 85);
            dgvAttendance.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
        }

        private void InitializeGuardianStudentSelector()
        {
            if (currentUser.Role != UserRole.Guardian || cmbStudent == null || lblStudentInfo == null)
            {
                return;
            }

            if (guardianStudentSelectorInitialized)
            {
                cmbStudent.Items.Clear();
            }
            else
            {
                cmbStudent.Format += (s, e) =>
                {
                    if (e.ListItem is Student sItem)
                    {
                        e.Value = $"{sItem.FirstName} {sItem.LastName} ({sItem.StudentId})";
                    }
                };
                guardianStudentSelectorInitialized = true;
            }

            foreach (var student in guardianStudents)
            {
                cmbStudent.Items.Add(student);
            }

            bool hasStudents = guardianStudents.Count > 0;
            cmbStudent.Visible = hasStudents;
            lblStudentInfo.Visible = hasStudents;

            if (hasStudents)
            {
                var index = guardianStudents.FindIndex(s => linkedStudent != null && s.Id == linkedStudent.Id);
                if (index >= 0 && index < cmbStudent.Items.Count)
                {
                    cmbStudent.SelectedIndex = index;
                }
                else
                {
                    cmbStudent.SelectedIndex = 0;
                    linkedStudent = guardianStudents[0];
                }

                lblStudentInfo.Text = $"Parent of: {linkedStudent!.FirstName} {linkedStudent.LastName} ({linkedStudent.StudentId})";
            }
            else
            {
                lblStudentInfo.Text = "No linked students found";
            }
        }

        private async Task<bool> EnsureStudentContextAsync()
        {
            if (studentContextResolved)
            {
                return linkedStudent != null;
            }

            studentContextResolved = true;

            if (currentUser.Role == UserRole.Guardian)
            {
                var students = await UserContextHelper.GetGuardianStudentsAsync(currentUser);
                guardianStudents.Clear();
                guardianStudents.AddRange(students);
                linkedStudent = guardianStudents.FirstOrDefault();
                InitializeGuardianStudentSelector();
            }
            else
            {
                linkedStudent = await UserContextHelper.GetLinkedStudentAsync(currentUser);
            }

            return linkedStudent != null;
        }

        private void HandleMissingStudentContext()
        {
            cmbSubject.Enabled = false;
            if (cmbStudent != null) cmbStudent.Enabled = false;
            dtpFromDate.Enabled = false;
            dtpToDate.Enabled = false;
            btnRefresh.Enabled = false;
            lblAttendanceSummary.Text = "Attendance Summary: No linked student record";
            dgvAttendance.DataSource = null;
        }

        private async Task LoadSubjectsAsync()
        {
            try
            {
                if (!await EnsureStudentContextAsync())
                {
                    HandleMissingStudentContext();
                    return;
                }

                cmbSubject.Items.Clear();
                cmbSubject.Items.Add("All Subjects");

                using var connection = DatabaseHelper.GetConnection();
                await connection.OpenAsync();

                var query = @"
                    SELECT DISTINCT a.Subject
                    FROM Attendance a
                    WHERE a.StudentId = @studentId
                    ORDER BY a.Subject";

                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@studentId", linkedStudent!.Id);

                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    cmbSubject.Items.Add(reader.GetString("Subject"));
                }
                
                if (cmbSubject.Items.Count > 0)
                {
                    cmbSubject.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading subjects: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task LoadAttendanceAsync()
        {
            try
            {
                if (!await EnsureStudentContextAsync())
                {
                    HandleMissingStudentContext();
                    return;
                }

                using var connection = DatabaseHelper.GetConnection();
                await connection.OpenAsync();

                var query = @"
                    SELECT a.Subject, a.Date, a.Status, a.Notes, a.RecordedDate,
                           u.FirstName + ' ' + u.LastName as ProfessorName
                    FROM Attendance a
                    INNER JOIN Users u ON a.ProfessorId = u.Id
                    WHERE a.StudentId = @studentId AND a.Date BETWEEN @fromDate AND @toDate";

                var parameters = new List<SqlParameter>
                {
                    new SqlParameter("@studentId", linkedStudent!.Id),
                    new SqlParameter("@fromDate", dtpFromDate.Value.Date),
                    new SqlParameter("@toDate", dtpToDate.Value.Date)
                };

                if (cmbSubject.SelectedIndex > 0)
                {
                    query += " AND a.Subject = @subject";
                    parameters.Add(new SqlParameter("@subject", cmbSubject.SelectedItem.ToString()));
                }

                query += " ORDER BY a.Date DESC";

                using var command = new SqlCommand(query, connection);
                command.Parameters.AddRange(parameters.ToArray());

                using var adapter = new SqlDataAdapter(command);
                var dataTable = new DataTable();
                adapter.Fill(dataTable);

                // Add StatusText column for display
                dataTable.Columns.Add("StatusText", typeof(string));
                
                // Convert status numbers to text in new column
                foreach (DataRow row in dataTable.Rows)
                {
                    if (row["Status"] != DBNull.Value)
                    {
                        var status = (AttendanceStatus)Convert.ToInt32(row["Status"]);
                        row["StatusText"] = status.ToString();
                    }
                    else
                    {
                        row["StatusText"] = "";
                    }
                }

                dgvAttendance.DataSource = dataTable;

                // Format columns
                if (dgvAttendance.Columns.Count > 0)
                {
                    // Hide the integer Status column and show the text version
                    dgvAttendance.Columns["Status"].Visible = false;
                    dgvAttendance.Columns["Subject"].HeaderText = "Subject";
                    dgvAttendance.Columns["Date"].HeaderText = "Date";
                    dgvAttendance.Columns["StatusText"].HeaderText = "Status";
                    dgvAttendance.Columns["Notes"].HeaderText = "Notes";
                    dgvAttendance.Columns["RecordedDate"].HeaderText = "Recorded Date";
                    dgvAttendance.Columns["ProfessorName"].HeaderText = "Professor";

                    dgvAttendance.Columns["Date"].DefaultCellStyle.Format = "MM/dd/yyyy";
                    dgvAttendance.Columns["RecordedDate"].DefaultCellStyle.Format = "MM/dd/yyyy HH:mm";

                    // Color code status
                    dgvAttendance.CellFormatting += DgvAttendance_CellFormatting;
                }

                // Calculate attendance summary
                CalculateAttendanceSummary(dataTable);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading attendance: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DgvAttendance_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dgvAttendance.Columns[e.ColumnIndex].Name == "StatusText")
            {
                var status = e.Value?.ToString();
                switch (status)
                {
                    case "Present":
                        e.CellStyle.BackColor = Color.FromArgb(220, 252, 231);
                        e.CellStyle.ForeColor = Color.FromArgb(22, 163, 74);
                        break;
                    case "Absent":
                        e.CellStyle.BackColor = Color.FromArgb(254, 226, 226);
                        e.CellStyle.ForeColor = Color.FromArgb(220, 38, 38);
                        break;
                    case "Late":
                        e.CellStyle.BackColor = Color.FromArgb(254, 249, 195);
                        e.CellStyle.ForeColor = Color.FromArgb(161, 98, 7);
                        break;
                    case "Excused":
                        e.CellStyle.BackColor = Color.FromArgb(219, 234, 254);
                        e.CellStyle.ForeColor = Color.FromArgb(37, 99, 235);
                        break;
                }
            }
        }

        private void CalculateAttendanceSummary(DataTable dataTable)
        {
            if (dataTable.Rows.Count == 0)
            {
                lblAttendanceSummary.Text = "Attendance Summary: No records found";
                return;
            }

            int present = 0, absent = 0, late = 0, excused = 0;

            foreach (DataRow row in dataTable.Rows)
            {
                // Status is stored as integer (1=Present, 2=Absent, 3=Late, 4=Excused)
                if (row["Status"] != DBNull.Value)
                {
                    var statusValue = Convert.ToInt32(row["Status"]);
                    switch (statusValue)
                    {
                        case 1: // Present
                            present++;
                            break;
                        case 2: // Absent
                            absent++;
                            break;
                        case 3: // Late
                            late++;
                            break;
                        case 4: // Excused
                            excused++;
                            break;
                    }
                }
            }

            var total = present + absent + late + excused;
            var attendanceRate = total > 0 ? (decimal)(present + excused) / total * 100 : 0;

            lblAttendanceSummary.Text = $"Attendance Summary: {attendanceRate:F1}% ({present + excused}/{total}) - Present: {present}, Absent: {absent}, Late: {late}, Excused: {excused}";
        }

        private async void CmbSubject_SelectedIndexChanged(object sender, EventArgs e)
        {
            await LoadAttendanceAsync();
        }

        private async void BtnRefresh_Click(object sender, EventArgs e)
        {
            await LoadAttendanceAsync();
        }

        private async void CmbStudent_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (cmbStudent?.SelectedItem is Student selected)
            {
                linkedStudent = selected;
                if (lblStudentInfo != null)
                {
                    lblStudentInfo.Text = $"Parent of: {selected.FirstName} {selected.LastName} ({selected.StudentId})";
                }

                await LoadSubjectsAsync();
                await LoadAttendanceAsync();
            }
        }
    }
}

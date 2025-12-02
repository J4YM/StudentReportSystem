using StudentReportInitial.Models;
using StudentReportInitial.Data;
using System.Data.SqlClient;
using System.Data;
using System.Collections.Generic;
using System.Linq;

namespace StudentReportInitial.Forms
{
    public partial class StudentSubjectEnrollment : UserControl
    {
        private DataGridView dgvStudents = null!;
        private DataGridView dgvSubjects = null!;
        private Button btnEnroll = null!;
        private Button btnUnenroll = null!;
        private Button btnRefresh = null!;
        private TextBox txtSearch = null!;
        private ComboBox cmbGradeFilter = null!;
        private ComboBox cmbSectionFilter = null!;
        private ComboBox cmbCourseFilter = null!;
        private Button btnClearFilters = null!;
        private Label lblSelectedStudent = null!;
        private DataTable? studentsTable = null;
        private Panel pnlEnrolledSubjects = null!;
        private DataGridView dgvEnrolledSubjects = null!;
        private int selectedStudentId = -1;
        private int? selectedStudentBranchId = null;
        private User? currentUser;
        private int? branchFilterId = null;

        public StudentSubjectEnrollment(User? user = null, int? branchId = null)
        {
            currentUser = user;
            branchFilterId = branchId;
            InitializeComponent();
            LoadStudents();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // Main container
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.FromArgb(248, 250, 252);
            this.Padding = new Padding(20);

            // Left panel - Student selection
            var pnlLeft = new Panel
            {
                Dock = DockStyle.Left,
                Width = 400,
                BackColor = Color.White,
                Padding = new Padding(10)
            };

            var lblStudents = new Label
            {
                Text = "Students",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(10, 10)
            };

            // Filter panel
            var pnlFilters = new Panel
            {
                Location = new Point(10, 40),
                Size = new Size(380, 100),
                BackColor = Color.FromArgb(248, 250, 252)
            };

            txtSearch = new TextBox
            {
                PlaceholderText = "Search students...",
                Size = new Size(180, 30),
                Location = new Point(0, 0)
            };
            txtSearch.TextChanged += TxtSearch_TextChanged;

            cmbGradeFilter = new ComboBox
            {
                Size = new Size(120, 30),
                Location = new Point(190, 0),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbGradeFilter.Items.Add("All Grades");
            cmbGradeFilter.SelectedIndex = 0;
            cmbGradeFilter.SelectedIndexChanged += CmbGradeFilter_SelectedIndexChanged;

            cmbSectionFilter = new ComboBox
            {
                Size = new Size(120, 30),
                Location = new Point(0, 35),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbSectionFilter.Items.Add("All Sections");
            cmbSectionFilter.SelectedIndex = 0;
            cmbSectionFilter.SelectedIndexChanged += (s, e) => ApplyFilters();

            cmbCourseFilter = new ComboBox
            {
                Size = new Size(120, 30),
                Location = new Point(130, 35),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbCourseFilter.Items.Add("All Courses");
            cmbCourseFilter.SelectedIndex = 0;
            cmbCourseFilter.SelectedIndexChanged += (s, e) => ApplyFilters();

            btnClearFilters = new Button
            {
                Text = "Clear Filters",
                Size = new Size(100, 30),
                Location = new Point(260, 35),
                BackColor = Color.FromArgb(241, 245, 249),
                ForeColor = Color.FromArgb(51, 65, 85),
                FlatStyle = FlatStyle.Flat,
                Enabled = false,
                Cursor = Cursors.Hand
            };
            btnClearFilters.Click += BtnClearFilters_Click;

            pnlFilters.Controls.AddRange(new Control[] { txtSearch, cmbGradeFilter, cmbSectionFilter, cmbCourseFilter, btnClearFilters });

            dgvStudents = new DataGridView
            {
                Location = new Point(10, 145),
                Size = new Size(380, 235),
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
            dgvStudents.SelectionChanged += DgvStudents_SelectionChanged;

            pnlLeft.Controls.AddRange(new Control[] { lblStudents, pnlFilters, dgvStudents });

            // Right panel - Subject management
            var pnlRight = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(10)
            };

            lblSelectedStudent = new Label
            {
                Text = "Select a student to manage subjects",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(10, 10)
            };

            // Available subjects
            var lblAvailableSubjects = new Label
            {
                Text = "Available Subjects",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(10, 40)
            };

            dgvSubjects = new DataGridView
            {
                Location = new Point(10, 70),
                Size = new Size(400, 200),
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowHeadersVisible = false,
                Enabled = false
            };

            // Buttons
            var pnlButtons = new Panel
            {
                Location = new Point(10, 280),
                Size = new Size(400, 30),
                BackColor = Color.White
            };

            // Enrolled subjects
            var lblEnrolledSubjects = new Label
            {
                Text = "Enrolled Subjects",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(10, 320)
            };

            dgvEnrolledSubjects = new DataGridView
            {
                Location = new Point(10, 350),
                Size = new Size(400, 200),
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowHeadersVisible = false,
                Enabled = false
            };

            btnEnroll = new Button
            {
                Text = "Enroll",
                Size = new Size(90, 28),
                Location = new Point(0, 0),
                BackColor = Color.FromArgb(34, 197, 94),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9),
                Cursor = Cursors.Hand,
                Enabled = false
            };
            btnEnroll.Click += BtnEnroll_Click;

            btnUnenroll = new Button
            {
                Text = "Unenroll",
                Size = new Size(90, 28),
                Location = new Point(100, 0),
                BackColor = Color.FromArgb(239, 68, 68),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9),
                Cursor = Cursors.Hand,
                Enabled = false
            };
            btnUnenroll.Click += BtnUnenroll_Click;

            btnRefresh = new Button
            {
                Text = "Refresh",
                Size = new Size(90, 28),
                Location = new Point(200, 0),
                BackColor = Color.FromArgb(59, 130, 246),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9),
                Cursor = Cursors.Hand,
                Enabled = false
            };
            btnRefresh.Click += BtnRefresh_Click;

            pnlButtons.Controls.AddRange(new Control[] { btnEnroll, btnUnenroll, btnRefresh });

            pnlRight.Controls.AddRange(new Control[] {
                lblSelectedStudent,
                lblAvailableSubjects,
                dgvSubjects,
                pnlButtons,
                lblEnrolledSubjects,
                dgvEnrolledSubjects
            });

            // Add panels to the form
            this.Controls.Add(pnlRight);
            this.Controls.Add(pnlLeft);

            ApplyModernStyling();

            this.ResumeLayout(false);
        }

        private void ApplyModernStyling()
        {
            var font = new Font("Segoe UI", 9);
            var headerFont = new Font("Segoe UI", 10, FontStyle.Bold);

            // Apply styling to DataGridViews
            foreach (var dgv in new[] { dgvStudents, dgvSubjects, dgvEnrolledSubjects })
            {
                if (dgv != null)
                {
                    dgv.Font = font;
                    dgv.ColumnHeadersDefaultCellStyle.Font = headerFont;
                    dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
                    dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(51, 65, 85);
                    dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
                    dgv.EnableHeadersVisualStyles = false;
                }
            }

            // Apply styling to buttons
            foreach (Control control in this.Controls)
            {
                if (control is Button button)
                {
                    button.FlatStyle = FlatStyle.Flat;
                    button.FlatAppearance.BorderSize = 0;
                    button.Font = font;
                }
            }
        }

        private async void LoadStudents()
        {
            try
            {
                using var connection = DatabaseHelper.GetConnection();
                await connection.OpenAsync();

                var query = @"
                    SELECT s.Id, s.StudentId, s.FirstName, s.LastName, s.GradeLevel, s.Section, s.BranchId
                    FROM Students s
                    WHERE s.IsActive = 1";

                var branchFilter = await ResolveBranchFilterAsync();
                if (branchFilter.HasValue)
                {
                    query += " AND s.BranchId = @branchId";
                }

                query += " ORDER BY s.LastName, s.FirstName";

                using var command = new SqlCommand(query, connection);

                if (branchFilter.HasValue)
                {
                    command.Parameters.AddWithValue("@branchId", branchFilter.Value);
                }

                using var adapter = new SqlDataAdapter(command);
                var dataTable = new DataTable();
                adapter.Fill(dataTable);

                // Enhance data table with Course column
                EnhanceStudentsDataTable(dataTable);
                studentsTable = dataTable;

                dgvStudents.DataSource = dataTable;

                if (dgvStudents.Columns.Count > 0)
                {
                    dgvStudents.Columns["Id"].Visible = false;
                    dgvStudents.Columns["StudentId"].HeaderText = "Student ID";
                    dgvStudents.Columns["FirstName"].HeaderText = "First Name";
                    dgvStudents.Columns["LastName"].HeaderText = "Last Name";
                    dgvStudents.Columns["GradeLevel"].HeaderText = "Grade";
                    dgvStudents.Columns["Section"].HeaderText = "Section";
                    if (dgvStudents.Columns.Contains("BranchId"))
                    {
                        dgvStudents.Columns["BranchId"].Visible = false;
                    }
                    if (dgvStudents.Columns.Contains("Course"))
                    {
                        dgvStudents.Columns["Course"].Visible = false; // Hidden column for filtering
                    }
                }

                // Populate filter options
                PopulateFilterOptions();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading students: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void LoadAvailableSubjects(string gradeLevel)
        {
            if (selectedStudentId <= 0) return;

            try
            {
                using var connection = DatabaseHelper.GetConnection();
                await connection.OpenAsync();

                var query = @"
                    SELECT s.Id, s.Code, s.Name, s.Section, 
                           COALESCE(u.FirstName + ' ' + u.LastName, 'Unassigned') as Professor
                    FROM Subjects s
                    LEFT JOIN Users u ON s.ProfessorId = u.Id
                    WHERE s.GradeLevel = @gradeLevel
                      AND s.IsActive = 1
                      AND s.Id NOT IN (
                        SELECT SubjectId 
                        FROM StudentSubjects 
                        WHERE StudentId = @studentId
                      )";

                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@gradeLevel", gradeLevel);
                command.Parameters.AddWithValue("@studentId", selectedStudentId);

                var branchFilter = selectedStudentBranchId ?? await ResolveBranchFilterAsync();
                if (branchFilter.HasValue)
                {
                    command.CommandText += " AND s.BranchId = @branchId";
                    command.Parameters.AddWithValue("@branchId", branchFilter.Value);
                }

                using var adapter = new SqlDataAdapter(command);
                var dataTable = new DataTable();
                adapter.Fill(dataTable);

                dgvSubjects.DataSource = dataTable;

                if (dgvSubjects.Columns.Count > 0)
                {
                    dgvSubjects.Columns["Id"].Visible = false;
                    dgvSubjects.Columns["Code"].HeaderText = "Code";
                    dgvSubjects.Columns["Name"].HeaderText = "Subject";
                    dgvSubjects.Columns["Section"].HeaderText = "Section";
                    dgvSubjects.Columns["Professor"].HeaderText = "Professor";
                }

                dgvSubjects.Enabled = true;
                btnEnroll.Enabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading available subjects: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void LoadEnrolledSubjects(int studentId)
        {
            if (studentId <= 0) return;

            try
            {
                using var connection = DatabaseHelper.GetConnection();
                await connection.OpenAsync();

                var query = @"
                    SELECT s.Id, s.Code, s.Name, s.Section, 
                           COALESCE(u.FirstName + ' ' + u.LastName, 'Unassigned') as Professor,
                           ss.EnrollmentDate
                    FROM Subjects s
                    INNER JOIN StudentSubjects ss ON s.Id = ss.SubjectId
                    LEFT JOIN Users u ON s.ProfessorId = u.Id
                    WHERE ss.StudentId = @studentId AND s.IsActive = 1";

                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@studentId", studentId);

                var branchFilter = selectedStudentBranchId ?? await ResolveBranchFilterAsync();
                if (branchFilter.HasValue)
                {
                    command.CommandText += " AND s.BranchId = @branchId";
                    command.Parameters.AddWithValue("@branchId", branchFilter.Value);
                }

                command.CommandText += " ORDER BY s.Name";

                using var adapter = new SqlDataAdapter(command);
                var dataTable = new DataTable();
                adapter.Fill(dataTable);

                dgvEnrolledSubjects.DataSource = dataTable;

                if (dgvEnrolledSubjects.Columns.Count > 0)
                {
                    dgvEnrolledSubjects.Columns["Id"].Visible = false;
                    dgvEnrolledSubjects.Columns["Code"].HeaderText = "Code";
                    dgvEnrolledSubjects.Columns["Name"].HeaderText = "Subject";
                    dgvEnrolledSubjects.Columns["Section"].HeaderText = "Section";
                    dgvEnrolledSubjects.Columns["Professor"].HeaderText = "Professor";
                    dgvEnrolledSubjects.Columns["EnrollmentDate"].HeaderText = "Enrolled On";
                    dgvEnrolledSubjects.Columns["EnrollmentDate"].DefaultCellStyle.Format = "MM/dd/yyyy";
                }

                dgvEnrolledSubjects.Enabled = true;
                btnUnenroll.Enabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading enrolled subjects: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task<int?> ResolveBranchFilterAsync()
        {
            // Branch selector on super admin takes precedence
            if (branchFilterId.HasValue)
            {
                return branchFilterId.Value;
            }

            if (currentUser == null)
            {
                return null;
            }

            var isSuperAdmin = await BranchHelper.IsSuperAdminAsync(currentUser.Id);
            if (isSuperAdmin)
            {
                // Super admins without a filter can view all branches
                return null;
            }

            var branchId = await BranchHelper.GetUserBranchIdAsync(currentUser.Id);
            return branchId > 0 ? branchId : (int?)null;
        }

        private void TxtSearch_TextChanged(object sender, EventArgs e)
        {
            ApplyFilters();
        }

        private void EnhanceStudentsDataTable(DataTable dataTable)
        {
            if (!dataTable.Columns.Contains("Course"))
            {
                dataTable.Columns.Add("Course", typeof(string));
            }

            foreach (DataRow row in dataTable.Rows)
            {
                row["Course"] = ExtractCourseFromSection(row["Section"]?.ToString());
            }
        }

        private static string ExtractCourseFromSection(string? sectionValue)
        {
            if (string.IsNullOrWhiteSpace(sectionValue))
            {
                return string.Empty;
            }

            var parts = sectionValue.Split('-', StringSplitOptions.RemoveEmptyEntries);
            return parts.Length > 0 ? parts[0] : sectionValue;
        }

        private void PopulateFilterOptions()
        {
            if (studentsTable == null) return;

            PopulateComboFromColumn(cmbSectionFilter, studentsTable, "Section");
            PopulateComboFromColumn(cmbCourseFilter, studentsTable, "Course");
            PopulateComboFromColumn(cmbGradeFilter, studentsTable, "GradeLevel", "All Grades");
        }

        private void PopulateComboFromColumn(ComboBox? comboBox, DataTable source, string columnName, string allLabel = "All")
        {
            if (comboBox == null || !source.Columns.Contains(columnName))
            {
                return;
            }

            var currentValue = comboBox.SelectedItem?.ToString();
            comboBox.Items.Clear();
            comboBox.Items.Add($"{allLabel} {(allLabel.Contains("All") ? columnName.Replace("GradeLevel", "Grades") : "")}");

            var values = source.AsEnumerable()
                .Select(row => row[columnName]?.ToString() ?? string.Empty)
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Distinct()
                .OrderBy(value => value)
                .ToList();

            foreach (var value in values)
            {
                comboBox.Items.Add(value);
            }

            if (!string.IsNullOrWhiteSpace(currentValue) && comboBox.Items.Contains(currentValue))
            {
                comboBox.SelectedItem = currentValue;
            }
            else
            {
                comboBox.SelectedIndex = 0;
            }
        }

        private void ApplyFilters()
        {
            if (dgvStudents.DataSource is DataTable dataTable)
            {
                var searchText = txtSearch.Text.Replace("'", "''");
                var filterParts = new List<string>();
                
                if (!string.IsNullOrWhiteSpace(searchText))
                {
                    filterParts.Add($"(FirstName LIKE '%{searchText}%' OR LastName LIKE '%{searchText}%' OR StudentId LIKE '%{searchText}%')");
                }

                if (cmbGradeFilter != null && cmbGradeFilter.SelectedIndex > 0)
                {
                    var grade = cmbGradeFilter.SelectedItem?.ToString()?.Replace("'", "''");
                    if (!string.IsNullOrEmpty(grade))
                    {
                        filterParts.Add($"GradeLevel = '{grade}'");
                    }
                }

                if (cmbSectionFilter != null && cmbSectionFilter.SelectedIndex > 0)
                {
                    var section = cmbSectionFilter.SelectedItem?.ToString()?.Replace("'", "''");
                    if (!string.IsNullOrEmpty(section))
                    {
                        filterParts.Add($"Section = '{section}'");
                    }
                }

                if (cmbCourseFilter != null && cmbCourseFilter.SelectedIndex > 0)
                {
                    var course = cmbCourseFilter.SelectedItem?.ToString()?.Replace("'", "''");
                    if (!string.IsNullOrEmpty(course))
                    {
                        filterParts.Add($"Course = '{course}'");
                    }
                }

                dataTable.DefaultView.RowFilter = filterParts.Count > 0 ? string.Join(" AND ", filterParts) : "";
                
                // Update Clear Filters button state
                if (btnClearFilters != null)
                {
                    btnClearFilters.Enabled = filterParts.Count > 0;
                }
            }
        }

        private void BtnClearFilters_Click(object? sender, EventArgs e)
        {
            txtSearch.Text = string.Empty;
            if (cmbGradeFilter != null) cmbGradeFilter.SelectedIndex = 0;
            if (cmbSectionFilter != null) cmbSectionFilter.SelectedIndex = 0;
            if (cmbCourseFilter != null) cmbCourseFilter.SelectedIndex = 0;
        }

        private void CmbGradeFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            ApplyFilters();
        }

        private void DgvStudents_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvStudents.SelectedRows.Count > 0)
            {
                var row = dgvStudents.SelectedRows[0];
                selectedStudentId = Convert.ToInt32(row.Cells["Id"].Value);
                if (row.Cells["BranchId"] != null && row.Cells["BranchId"].Value != null && row.Cells["BranchId"].Value != DBNull.Value)
                {
                    selectedStudentBranchId = Convert.ToInt32(row.Cells["BranchId"].Value);
                }
                else
                {
                    selectedStudentBranchId = null;
                }
                var studentName = $"{row.Cells["FirstName"].Value} {row.Cells["LastName"].Value}";
                var gradeLevel = row.Cells["GradeLevel"].Value.ToString();

                lblSelectedStudent.Text = $"Managing subjects for: {studentName} ({gradeLevel})";
                
                LoadAvailableSubjects(gradeLevel!);
                LoadEnrolledSubjects(selectedStudentId);
                btnRefresh.Enabled = true;
            }
            else
            {
                selectedStudentId = -1;
                selectedStudentBranchId = null;
                lblSelectedStudent.Text = "Select a student to manage subjects";
                dgvSubjects.DataSource = null;
                dgvEnrolledSubjects.DataSource = null;
                dgvSubjects.Enabled = false;
                dgvEnrolledSubjects.Enabled = false;
                btnEnroll.Enabled = false;
                btnUnenroll.Enabled = false;
                btnRefresh.Enabled = false;
            }
        }

        private async void BtnEnroll_Click(object sender, EventArgs e)
        {
            if (dgvSubjects.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a subject to enroll in.", "Warning",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var subjectId = Convert.ToInt32(dgvSubjects.SelectedRows[0].Cells["Id"].Value);
                var branchFilter = selectedStudentBranchId ?? await ResolveBranchFilterAsync();
                await SubjectEnrollmentHelper.EnrollStudentInSubjectAsync(selectedStudentId, subjectId, branchFilter);

                MessageBox.Show("Student enrolled successfully!", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                var gradeLevel = dgvStudents.SelectedRows[0].Cells["GradeLevel"].Value.ToString();
                LoadAvailableSubjects(gradeLevel!);
                LoadEnrolledSubjects(selectedStudentId);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error enrolling student: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void BtnUnenroll_Click(object sender, EventArgs e)
        {
            if (dgvEnrolledSubjects.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a subject to unenroll from.", "Warning",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var result = MessageBox.Show(
                "Are you sure you want to unenroll from this subject? This will remove all associated grades and attendance records.",
                "Confirm Unenroll",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                try
                {
                    var subjectId = Convert.ToInt32(dgvEnrolledSubjects.SelectedRows[0].Cells["Id"].Value);
                    var branchFilter = selectedStudentBranchId ?? await ResolveBranchFilterAsync();
                    await SubjectEnrollmentHelper.UnenrollStudentFromSubjectAsync(selectedStudentId, subjectId, branchFilter);

                    MessageBox.Show("Student unenrolled successfully!", "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);

                    var gradeLevel = dgvStudents.SelectedRows[0].Cells["GradeLevel"].Value.ToString();
                    LoadAvailableSubjects(gradeLevel!);
                    LoadEnrolledSubjects(selectedStudentId);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error unenrolling student: {ex.Message}", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            if (selectedStudentId > 0)
            {
                var gradeLevel = dgvStudents.SelectedRows[0].Cells["GradeLevel"].Value.ToString();
                LoadAvailableSubjects(gradeLevel!);
                LoadEnrolledSubjects(selectedStudentId);
            }
        }
    }
}

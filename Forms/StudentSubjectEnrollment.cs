using StudentReportInitial.Models;
using StudentReportInitial.Data;
using System.Data.SqlClient;
using System.Data;

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
        private Label lblSelectedStudent = null!;
        private Panel pnlEnrolledSubjects = null!;
        private DataGridView dgvEnrolledSubjects = null!;
        private int selectedStudentId = -1;

        public StudentSubjectEnrollment()
        {
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

            txtSearch = new TextBox
            {
                PlaceholderText = "Search students...",
                Size = new Size(200, 30),
                Location = new Point(10, 40)
            };
            txtSearch.TextChanged += TxtSearch_TextChanged;

            cmbGradeFilter = new ComboBox
            {
                Size = new Size(150, 30),
                Location = new Point(220, 40),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbGradeFilter.Items.AddRange(new[] { "All Grades", "Grade 1", "Grade 2", "Grade 3", "Grade 4", "Grade 5", "Grade 6", "Grade 7", "Grade 8", "Grade 9", "Grade 10", "Grade 11", "Grade 12" });
            cmbGradeFilter.SelectedIndex = 0;
            cmbGradeFilter.SelectedIndexChanged += CmbGradeFilter_SelectedIndexChanged;

            dgvStudents = new DataGridView
            {
                Location = new Point(10, 80),
                Size = new Size(380, 300),
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

            pnlLeft.Controls.AddRange(new Control[] { lblStudents, txtSearch, cmbGradeFilter, dgvStudents });

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
                    SELECT s.Id, s.StudentId, s.FirstName, s.LastName, s.GradeLevel, s.Section
                    FROM Students s
                    WHERE s.IsActive = 1
                    ORDER BY s.LastName, s.FirstName";

                using var command = new SqlCommand(query, connection);
                using var adapter = new SqlDataAdapter(command);
                var dataTable = new DataTable();
                adapter.Fill(dataTable);

                dgvStudents.DataSource = dataTable;

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
                    WHERE ss.StudentId = @studentId AND s.IsActive = 1
                    ORDER BY s.Name";

                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@studentId", studentId);

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

        private void TxtSearch_TextChanged(object sender, EventArgs e)
        {
            if (dgvStudents.DataSource is DataTable dataTable)
            {
                var filter = $"FirstName LIKE '%{txtSearch.Text}%' OR LastName LIKE '%{txtSearch.Text}%' OR StudentId LIKE '%{txtSearch.Text}%'";

                if (cmbGradeFilter.SelectedIndex > 0)
                {
                    var grade = cmbGradeFilter.SelectedItem.ToString()?.Replace("Grade ", "");
                    filter += $" AND GradeLevel = '{grade}'";
                }

                dataTable.DefaultView.RowFilter = filter;
            }
        }

        private void CmbGradeFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            TxtSearch_TextChanged(sender, e);
        }

        private void DgvStudents_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvStudents.SelectedRows.Count > 0)
            {
                var row = dgvStudents.SelectedRows[0];
                selectedStudentId = Convert.ToInt32(row.Cells["Id"].Value);
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
                await SubjectEnrollmentHelper.EnrollStudentInSubjectAsync(selectedStudentId, subjectId);

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
                    await SubjectEnrollmentHelper.UnenrollStudentFromSubjectAsync(selectedStudentId, subjectId);

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

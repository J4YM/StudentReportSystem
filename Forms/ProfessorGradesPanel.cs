using StudentReportInitial.Models;
using StudentReportInitial.Data;
using System.Data.SqlClient;
using System.Data;

namespace StudentReportInitial.Forms
{
    public partial class ProfessorGradesPanel : UserControl
    {
        private User currentProfessor;
        private DataGridView dgvGrades;
        private ComboBox cmbSubject;
        private ComboBox cmbAssignmentType;
        private TextBox txtAssignmentName;
        private DateTimePicker dtpDueDate;
        private NumericUpDown nudMaxScore;
        private Button btnAddGrade;
        private Button btnSaveGrades;
        private Button btnRefresh;
        private Panel pnlGradeForm;
        private List<Student> currentStudents = new();

        public ProfessorGradesPanel(User professor)
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
                Text = "Record Grades",
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

            // Assignment type
            var lblAssignmentType = new Label
            {
                Text = "Type:",
                Location = new Point(300, 50),
                AutoSize = true
            };

            cmbAssignmentType = new ComboBox
            {
                Location = new Point(340, 48),
                Size = new Size(120, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbAssignmentType.Items.AddRange(new[] { "Quiz", "Exam", "Project", "Assignment", "Participation", "Other" });

            // Assignment name
            var lblAssignmentName = new Label
            {
                Text = "Assignment:",
                Location = new Point(480, 50),
                AutoSize = true
            };

            txtAssignmentName = new TextBox
            {
                Location = new Point(560, 48),
                Size = new Size(150, 25)
            };

            // Due date
            var lblDueDate = new Label
            {
                Text = "Due Date:",
                Location = new Point(20, 75),
                AutoSize = true
            };

            dtpDueDate = new DateTimePicker
            {
                Location = new Point(90, 73),
                Size = new Size(120, 25),
                Value = DateTime.Today
            };

            // Max score
            var lblMaxScore = new Label
            {
                Text = "Max Score:",
                Location = new Point(230, 75),
                AutoSize = true
            };

            nudMaxScore = new NumericUpDown
            {
                Location = new Point(300, 73),
                Size = new Size(80, 25),
                Minimum = 1,
                Maximum = 1000,
                Value = 100
            };

            btnAddGrade = new Button
            {
                Text = "Add Grade Entry",
                Location = new Point(400, 72),
                Size = new Size(120, 27),
                BackColor = Color.FromArgb(34, 197, 94),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                
                Cursor = Cursors.Hand
            };
            btnAddGrade.Click += BtnAddGrade_Click;

            btnRefresh = new Button
            {
                Text = "Refresh",
                Location = new Point(540, 72),
                Size = new Size(80, 27),
                BackColor = Color.FromArgb(59, 130, 246),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                
                Cursor = Cursors.Hand
            };
            btnRefresh.Click += BtnRefresh_Click;

            pnlHeader.Controls.AddRange(new Control[] { 
                lblTitle, lblSubject, cmbSubject, lblAssignmentType, cmbAssignmentType,
                lblAssignmentName, txtAssignmentName, lblDueDate, dtpDueDate,
                lblMaxScore, nudMaxScore, btnAddGrade, btnRefresh
            });

            // Grades grid
            dgvGrades = new DataGridView
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

            // Add score column
            var scoreColumn = new DataGridViewTextBoxColumn
            {
                Name = "Score",
                HeaderText = "Score",
                DataPropertyName = "Score",
                Width = 80
            };
            dgvGrades.Columns.Add(scoreColumn);

            // Add comments column
            var commentsColumn = new DataGridViewTextBoxColumn
            {
                Name = "Comments",
                HeaderText = "Comments",
                DataPropertyName = "Comments",
                Width = 200
            };
            dgvGrades.Columns.Add(commentsColumn);

            // Action panel
            var pnlActions = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 60,
                BackColor = Color.White,
                Padding = new Padding(20)
            };

            btnSaveGrades = new Button
            {
                Text = "Save Grades",
                Size = new Size(150, 35),
                Location = new Point(20, 12),
                BackColor = Color.FromArgb(34, 197, 94),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            btnSaveGrades.Click += BtnSaveGrades_Click;

            pnlActions.Controls.Add(btnSaveGrades);

            this.Controls.Add(dgvGrades);
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
                else if (control is TextBox textBox)
                {
                    textBox.Font = font;
                    textBox.BorderStyle = BorderStyle.FixedSingle;
                }
                else if (control is DateTimePicker dateTimePicker)
                {
                    dateTimePicker.Font = font;
                }
                else if (control is NumericUpDown numericUpDown)
                {
                    numericUpDown.Font = font;
                }
            }

            dgvGrades.Font = font;
            dgvGrades.ColumnHeadersDefaultCellStyle.Font = headerFont;
            dgvGrades.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
            dgvGrades.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(51, 65, 85);
            dgvGrades.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
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

                // Add grade columns
                dataTable.Columns.Add("Score", typeof(decimal));
                dataTable.Columns.Add("Comments", typeof(string));

                // Set default values
                foreach (DataRow row in dataTable.Rows)
                {
                    row["Score"] = 0;
                    row["Comments"] = "";
                }

                dgvGrades.DataSource = dataTable;

                // Format columns
                if (dgvGrades.Columns.Count > 0)
                {
                    dgvGrades.Columns["Id"].Visible = false;
                    dgvGrades.Columns["StudentId"].HeaderText = "Student ID";
                    dgvGrades.Columns["FirstName"].HeaderText = "First Name";
                    dgvGrades.Columns["LastName"].HeaderText = "Last Name";
                    dgvGrades.Columns["GradeLevel"].HeaderText = "Grade";
                    dgvGrades.Columns["Section"].HeaderText = "Section";
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

        private void BtnAddGrade_Click(object sender, EventArgs e)
        {
            if (cmbSubject.SelectedIndex == -1)
            {
                MessageBox.Show("Please select a subject first.", "Warning", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtAssignmentName.Text))
            {
                MessageBox.Show("Please enter an assignment name.", "Warning", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (cmbAssignmentType.SelectedIndex == -1)
            {
                MessageBox.Show("Please select an assignment type.", "Warning", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            LoadStudents();
        }

        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            LoadStudents();
        }

        private async void BtnSaveGrades_Click(object sender, EventArgs e)
        {
            if (cmbSubject.SelectedIndex == -1)
            {
                MessageBox.Show("Please select a subject first.", "Warning", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtAssignmentName.Text))
            {
                MessageBox.Show("Please enter an assignment name.", "Warning", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var selectedSubject = cmbSubject.SelectedItem.ToString();
                var parts = selectedSubject.Split(" - ");
                var subjectName = parts[0];

                var gradeRecords = new List<Grade>();
                var maxScore = nudMaxScore.Value;

                foreach (DataGridViewRow row in dgvGrades.Rows)
                {
                    if (row.Cells["Id"].Value != null && row.Cells["Score"].Value != null)
                    {
                        var score = Convert.ToDecimal(row.Cells["Score"].Value);
                        if (score > 0) // Only save non-zero scores
                        {
                            var grade = new Grade
                            {
                                StudentId = Convert.ToInt32(row.Cells["Id"].Value),
                                ProfessorId = currentProfessor.Id,
                                Subject = subjectName,
                                AssignmentType = cmbAssignmentType.SelectedItem.ToString(),
                                AssignmentName = txtAssignmentName.Text,
                                Score = score,
                                MaxScore = maxScore,
                                Percentage = (score / maxScore) * 100,
                                Comments = row.Cells["Comments"].Value?.ToString(),
                                DateRecorded = DateTime.Now,
                                DueDate = dtpDueDate.Value.Date
                            };
                            gradeRecords.Add(grade);
                        }
                    }
                }

                if (gradeRecords.Count == 0)
                {
                    MessageBox.Show("No grades to save. Please enter at least one score.", "Warning", 
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                await SaveGradeRecordsAsync(gradeRecords);
                MessageBox.Show($"Grades saved successfully for {gradeRecords.Count} students!", "Success", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Clear form
                txtAssignmentName.Clear();
                cmbAssignmentType.SelectedIndex = -1;
                nudMaxScore.Value = 100;
                dtpDueDate.Value = DateTime.Today;
                dgvGrades.DataSource = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving grades: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task SaveGradeRecordsAsync(List<Grade> gradeRecords)
        {
            using var connection = DatabaseHelper.GetConnection();
            await connection.OpenAsync();

            var insertQuery = @"
                INSERT INTO Grades (StudentId, ProfessorId, Subject, AssignmentType, AssignmentName, 
                                  Score, MaxScore, Percentage, Comments, DateRecorded, DueDate)
                VALUES (@studentId, @professorId, @subject, @assignmentType, @assignmentName, 
                        @score, @maxScore, @percentage, @comments, @dateRecorded, @dueDate)";

            foreach (var grade in gradeRecords)
            {
                using var command = new SqlCommand(insertQuery, connection);
                command.Parameters.AddWithValue("@studentId", grade.StudentId);
                command.Parameters.AddWithValue("@professorId", grade.ProfessorId);
                command.Parameters.AddWithValue("@subject", grade.Subject);
                command.Parameters.AddWithValue("@assignmentType", grade.AssignmentType);
                command.Parameters.AddWithValue("@assignmentName", grade.AssignmentName);
                command.Parameters.AddWithValue("@score", grade.Score);
                command.Parameters.AddWithValue("@maxScore", grade.MaxScore);
                command.Parameters.AddWithValue("@percentage", grade.Percentage);
                command.Parameters.AddWithValue("@comments", grade.Comments ?? "");
                command.Parameters.AddWithValue("@dateRecorded", grade.DateRecorded);
                command.Parameters.AddWithValue("@dueDate", grade.DueDate);

                await command.ExecuteNonQueryAsync();
            }
        }
    }
}

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
        private ComboBox cmbQuarter;
        private ComboBox cmbComponentType;
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
                Height = 120,
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

            // Quarter selection
            var lblQuarter = new Label
            {
                Text = "Quarter:",
                Location = new Point(300, 50),
                AutoSize = true
            };

            cmbQuarter = new ComboBox
            {
                Location = new Point(360, 48),
                Size = new Size(100, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbQuarter.Items.AddRange(new[] { "Prelim", "Midterm", "PreFinal", "Final" });

            // Component type
            var lblComponentType = new Label
            {
                Text = "Component:",
                Location = new Point(480, 50),
                AutoSize = true
            };

            cmbComponentType = new ComboBox
            {
                Location = new Point(560, 48),
                Size = new Size(150, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbComponentType.Items.AddRange(new[] { "Quizzes", "PT/Activities", "Exam" });

            // Assignment name
            var lblAssignmentName = new Label
            {
                Text = "Assignment:",
                Location = new Point(730, 50),
                AutoSize = true
            };

            txtAssignmentName = new TextBox
            {
                Location = new Point(810, 48),
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
                lblTitle, lblSubject, cmbSubject, lblQuarter, cmbQuarter,
                lblComponentType, cmbComponentType, lblAssignmentName, txtAssignmentName, 
                lblDueDate, dtpDueDate, lblMaxScore, nudMaxScore, btnAddGrade, btnRefresh
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
            dgvGrades.CellValidating += DgvGrades_CellValidating;
            dgvGrades.CellEndEdit += DgvGrades_CellEndEdit;
            dgvGrades.CellDoubleClick += DgvGrades_CellDoubleClick;

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

            if (cmbQuarter.SelectedIndex == -1)
            {
                MessageBox.Show("Please select a quarter.", "Warning", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (cmbComponentType.SelectedIndex == -1)
            {
                MessageBox.Show("Please select a component type.", "Warning", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            LoadStudents();
        }

        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            LoadStudents();
        }

        private void DgvGrades_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            // Only validate the Score column
            if (e.ColumnIndex == dgvGrades.Columns["Score"].Index)
            {
                if (e.FormattedValue != null && !string.IsNullOrWhiteSpace(e.FormattedValue.ToString()))
                {
                    if (decimal.TryParse(e.FormattedValue.ToString(), out decimal score))
                    {
                        var maxScore = nudMaxScore.Value;
                        if (score > maxScore)
                        {
                            dgvGrades.Rows[e.RowIndex].ErrorText = $"Score will be capped at max score of {maxScore}";
                            MessageBox.Show($"Score cannot exceed the maximum score of {maxScore}. The score will be automatically capped at {maxScore}.", "Validation Warning",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            // Don't cancel - let CellEndEdit handle the capping
                        }
                        else if (score < 0)
                        {
                            dgvGrades.Rows[e.RowIndex].ErrorText = "Score will be set to 0";
                            MessageBox.Show("Score cannot be negative. The score will be set to 0.", "Validation Warning",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            // Don't cancel - let CellEndEdit handle the correction
                        }
                        else
                        {
                            dgvGrades.Rows[e.RowIndex].ErrorText = "";
                        }
                    }
                    else
                    {
                        e.Cancel = true;
                        dgvGrades.Rows[e.RowIndex].ErrorText = "Please enter a valid number";
                        MessageBox.Show("Please enter a valid number for the score.", "Validation Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
        }

        private void DgvGrades_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            // Auto-cap score at max value after editing
            if (e.ColumnIndex == dgvGrades.Columns["Score"].Index)
            {
                var cell = dgvGrades.Rows[e.RowIndex].Cells[e.ColumnIndex];
                if (cell.Value != null && decimal.TryParse(cell.Value.ToString(), out decimal score))
                {
                    var maxScore = nudMaxScore.Value;
                    if (score > maxScore)
                    {
                        cell.Value = maxScore;
                        dgvGrades.Rows[e.RowIndex].ErrorText = "";
                    }
                    else if (score < 0)
                    {
                        cell.Value = 0;
                        dgvGrades.Rows[e.RowIndex].ErrorText = "";
                    }
                }
            }
        }

        private void DgvGrades_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            // Only handle double-click on student rows (not header)
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;

            var row = dgvGrades.Rows[e.RowIndex];
            if (row.Cells["Id"].Value == null) return;

            // Get current score and comments
            var currentScore = row.Cells["Score"].Value != null 
                ? Convert.ToDecimal(row.Cells["Score"].Value) 
                : 0;
            var currentComments = row.Cells["Comments"].Value?.ToString() ?? "";
            var studentName = $"{row.Cells["FirstName"].Value} {row.Cells["LastName"].Value}";

            // Show grade entry form
            ShowGradeEntryForm(studentName, currentScore, currentComments, (newScore, newComments) =>
            {
                row.Cells["Score"].Value = newScore;
                row.Cells["Comments"].Value = newComments;
            });
        }

        private void ShowGradeEntryForm(string studentName, decimal currentScore, string currentComments, 
            Action<decimal, string> onSave)
        {
            var form = new Form
            {
                Text = $"Grade Entry - {studentName}",
                Size = new Size(500, 350),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false
            };

            var maxScore = nudMaxScore.Value;

            // Component type
            var lblComponentType = new Label 
            { 
                Text = "Component:", 
                Location = new Point(20, 20), 
                AutoSize = true 
            };
            var cmbFormComponentType = new ComboBox
            {
                Location = new Point(20, 40),
                Size = new Size(200, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbFormComponentType.Items.AddRange(new[] { "Quizzes", "PT/Activities", "Exam" });
            if (cmbComponentType.SelectedIndex >= 0)
            {
                cmbFormComponentType.SelectedIndex = cmbComponentType.SelectedIndex;
            }
            else
            {
                cmbFormComponentType.SelectedIndex = 0;
            }

            // Quarter
            var lblQuarter = new Label 
            { 
                Text = "Quarter:", 
                Location = new Point(240, 20), 
                AutoSize = true 
            };
            var cmbFormQuarter = new ComboBox
            {
                Location = new Point(240, 40),
                Size = new Size(200, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbFormQuarter.Items.AddRange(new[] { "Prelim", "Midterm", "PreFinal", "Final" });
            if (cmbQuarter.SelectedIndex >= 0)
            {
                cmbFormQuarter.SelectedIndex = cmbQuarter.SelectedIndex;
            }
            else
            {
                cmbFormQuarter.SelectedIndex = 0;
            }

            // Assignment name
            var lblAssignmentName = new Label 
            { 
                Text = "Assignment:", 
                Location = new Point(20, 70), 
                AutoSize = true 
            };
            var txtFormAssignmentName = new TextBox
            {
                Location = new Point(20, 90),
                Size = new Size(420, 25),
                Text = txtAssignmentName.Text
            };

            // Score
            var lblScore = new Label 
            { 
                Text = $"Score (Max: {maxScore}):", 
                Location = new Point(20, 120), 
                AutoSize = true 
            };
            var nudFormScore = new NumericUpDown
            {
                Location = new Point(20, 140),
                Size = new Size(200, 25),
                Minimum = 0,
                Maximum = maxScore,
                DecimalPlaces = 2,
                Value = currentScore > maxScore ? maxScore : (currentScore < 0 ? 0 : currentScore)
            };

            // Max score
            var lblMaxScore = new Label 
            { 
                Text = "Max Score:", 
                Location = new Point(240, 120), 
                AutoSize = true 
            };
            var nudFormMaxScore = new NumericUpDown
            {
                Location = new Point(240, 140),
                Size = new Size(200, 25),
                Minimum = 1,
                Maximum = 10000,
                DecimalPlaces = 2,
                Value = maxScore
            };
            nudFormMaxScore.ValueChanged += (s, e) =>
            {
                // Update score max when max score changes
                nudFormScore.Maximum = nudFormMaxScore.Value;
                lblScore.Text = $"Score (Max: {nudFormMaxScore.Value}):";
            };

            // Comments
            var lblComments = new Label 
            { 
                Text = "Comments:", 
                Location = new Point(20, 170), 
                AutoSize = true 
            };
            var txtFormComments = new TextBox
            {
                Location = new Point(20, 190),
                Size = new Size(420, 80),
                Multiline = true,
                Text = currentComments
            };

            // Buttons
            var btnSave = new Button
            {
                Text = "Save",
                Location = new Point(20, 280),
                Size = new Size(100, 30),
                BackColor = Color.FromArgb(34, 197, 94),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                DialogResult = DialogResult.OK
            };

            var btnCancel = new Button
            {
                Text = "Cancel",
                Location = new Point(130, 280),
                Size = new Size(100, 30),
                BackColor = Color.FromArgb(107, 114, 128),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                DialogResult = DialogResult.Cancel
            };

            btnSave.Click += (s, e) =>
            {
                if (nudFormScore.Value > nudFormMaxScore.Value)
                {
                    MessageBox.Show($"Score cannot exceed the maximum score of {nudFormMaxScore.Value}.", "Validation Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Update the form's max score if changed
                if (nudFormMaxScore.Value != nudMaxScore.Value)
                {
                    nudMaxScore.Value = nudFormMaxScore.Value;
                }

                // Update component type and quarter if changed
                if (cmbFormComponentType.SelectedIndex != cmbComponentType.SelectedIndex)
                {
                    cmbComponentType.SelectedIndex = cmbFormComponentType.SelectedIndex;
                }
                if (cmbFormQuarter.SelectedIndex != cmbQuarter.SelectedIndex)
                {
                    cmbQuarter.SelectedIndex = cmbFormQuarter.SelectedIndex;
                }
                if (txtFormAssignmentName.Text != txtAssignmentName.Text)
                {
                    txtAssignmentName.Text = txtFormAssignmentName.Text;
                }

                onSave(nudFormScore.Value, txtFormComments.Text);
                form.DialogResult = DialogResult.OK;
                form.Close();
            };

            btnCancel.Click += (s, e) => form.Close();

            form.Controls.AddRange(new Control[] 
            { 
                lblComponentType, cmbFormComponentType, lblQuarter, cmbFormQuarter,
                lblAssignmentName, txtFormAssignmentName,
                lblScore, nudFormScore, lblMaxScore, nudFormMaxScore,
                lblComments, txtFormComments, btnSave, btnCancel 
            });

            UIStyleHelper.ApplyRoundedButton(btnSave, 10);
            UIStyleHelper.ApplyRoundedButton(btnCancel, 10);

            form.ShowDialog();
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

            if (cmbQuarter.SelectedIndex == -1)
            {
                MessageBox.Show("Please select a quarter.", "Warning", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (cmbComponentType.SelectedIndex == -1)
            {
                MessageBox.Show("Please select a component type.", "Warning", 
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
                            // Map display text to ComponentType value
                            string componentTypeValue = MapComponentType(cmbComponentType.SelectedItem?.ToString() ?? "Quizzes");
                            
                            var grade = new Grade
                            {
                                StudentId = Convert.ToInt32(row.Cells["Id"].Value),
                                ProfessorId = currentProfessor.Id,
                                Subject = subjectName,
                                Quarter = cmbQuarter.SelectedItem.ToString() ?? "Prelim",
                                ComponentType = componentTypeValue,
                                AssignmentType = cmbComponentType.SelectedItem.ToString() ?? "Quizzes", // Legacy field
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

                var result = await SaveGradeRecordsAsync(gradeRecords);
                string message = $"Grades saved successfully for {result.SavedCount} student(s)!";
                if (result.SkippedCount > 0)
                {
                    message += $"\n\n{result.SkippedCount} duplicate grade entry(ies) skipped:\n" + 
                               string.Join("\n", result.SkippedGrades.Take(5));
                    if (result.SkippedCount > 5)
                    {
                        message += $"\n... and {result.SkippedCount - 5} more";
                    }
                }
                MessageBox.Show(message, "Save Complete", 
                    MessageBoxButtons.OK, result.SkippedCount > 0 ? MessageBoxIcon.Warning : MessageBoxIcon.Information);

                // Clear form
                txtAssignmentName.Clear();
                cmbQuarter.SelectedIndex = -1;
                cmbComponentType.SelectedIndex = -1;
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

        private static string MapComponentType(string? displayText)
        {
            return displayText switch
            {
                "Quizzes" => "QuizzesActivities",
                "PT/Activities" => "PerformanceTask",
                "Performance Task" => "PerformanceTask", // Legacy support
                "Quizzes/Activities" => "QuizzesActivities", // Legacy support
                "Exam" => "Exam",
                _ => "QuizzesActivities"
            };
        }

        private class SaveGradeResult
        {
            public int SavedCount { get; set; }
            public int SkippedCount { get; set; }
            public List<string> SkippedGrades { get; set; } = new();
        }

        private async Task<SaveGradeResult> SaveGradeRecordsAsync(List<Grade> gradeRecords)
        {
            using var connection = DatabaseHelper.GetConnection();
            await connection.OpenAsync();

            var checkDuplicateQuery = @"
                SELECT COUNT(*) 
                FROM Grades 
                WHERE StudentId = @studentId 
                  AND Subject = @subject 
                  AND Quarter = @quarter 
                  AND ComponentType = @componentType 
                  AND AssignmentName = @assignmentName";

            var insertQuery = @"
                INSERT INTO Grades (StudentId, ProfessorId, Subject, Quarter, ComponentType, AssignmentName, 
                                  Score, MaxScore, Percentage, Comments, DateRecorded, DueDate, BranchId)
                VALUES (@studentId, @professorId, @subject, @quarter, @componentType, @assignmentName, 
                        @score, @maxScore, @percentage, @comments, @dateRecorded, @dueDate, @branchId)";

            // Get professor name
            string professorName = $"{currentProfessor.FirstName} {currentProfessor.LastName}";
            var subjectName = gradeRecords.First().Subject;
            var assignmentType = gradeRecords.First().ComponentType;

            var skippedGrades = new List<string>();
            var savedCount = 0;

            foreach (var grade in gradeRecords)
            {
                // Get student's branch ID for the grade record
                var studentBranchQuery = "SELECT BranchId FROM Students WHERE Id = @studentId";
                int branchId = 0;
                using var studentBranchCommand = new SqlCommand(studentBranchQuery, connection);
                studentBranchCommand.Parameters.AddWithValue("@studentId", grade.StudentId);
                var branchResult = await studentBranchCommand.ExecuteScalarAsync();
                if (branchResult != null && branchResult != DBNull.Value)
                {
                    branchId = Convert.ToInt32(branchResult);
                }

                // Check for duplicate grade entry
                using var checkCommand = new SqlCommand(checkDuplicateQuery, connection);
                checkCommand.Parameters.AddWithValue("@studentId", grade.StudentId);
                checkCommand.Parameters.AddWithValue("@subject", grade.Subject);
                checkCommand.Parameters.AddWithValue("@quarter", grade.Quarter);
                checkCommand.Parameters.AddWithValue("@componentType", grade.ComponentType);
                checkCommand.Parameters.AddWithValue("@assignmentName", grade.AssignmentName);

                var duplicateCount = Convert.ToInt32(await checkCommand.ExecuteScalarAsync());
                
                if (duplicateCount > 0)
                {
                    // Get student name for error message
                    var studentQuery = "SELECT FirstName, LastName FROM Students WHERE Id = @studentId";
                    using var studentCommand = new SqlCommand(studentQuery, connection);
                    studentCommand.Parameters.AddWithValue("@studentId", grade.StudentId);
                    using var studentReader = await studentCommand.ExecuteReaderAsync();
                    string studentName = "Unknown";
                    if (await studentReader.ReadAsync())
                    {
                        studentName = $"{studentReader.GetString("FirstName")} {studentReader.GetString("LastName")}";
                    }
                    studentReader.Close();
                    
                    skippedGrades.Add($"{studentName} - {grade.AssignmentName} ({grade.ComponentType})");
                    continue;
                }

                using var command = new SqlCommand(insertQuery, connection);
                command.Parameters.AddWithValue("@studentId", grade.StudentId);
                command.Parameters.AddWithValue("@professorId", grade.ProfessorId);
                command.Parameters.AddWithValue("@subject", grade.Subject);
                command.Parameters.AddWithValue("@quarter", grade.Quarter);
                command.Parameters.AddWithValue("@componentType", grade.ComponentType);
                command.Parameters.AddWithValue("@assignmentName", grade.AssignmentName);
                command.Parameters.AddWithValue("@score", grade.Score);
                command.Parameters.AddWithValue("@maxScore", grade.MaxScore);
                command.Parameters.AddWithValue("@percentage", grade.Percentage);
                command.Parameters.AddWithValue("@comments", grade.Comments ?? "");
                command.Parameters.AddWithValue("@dateRecorded", grade.DateRecorded);
                command.Parameters.AddWithValue("@dueDate", grade.DueDate);
                command.Parameters.AddWithValue("@branchId", branchId);

                await command.ExecuteNonQueryAsync();
                savedCount++;

                // Send SMS notification to student/guardian
                try
                {
                    // Get student information including phone number
                    var studentQuery = @"
                        SELECT s.FirstName, s.LastName, s.Phone, 
                               u.Phone as GuardianPhone
                        FROM Students s
                        LEFT JOIN Users u ON s.GuardianId = u.Id
                        WHERE s.Id = @studentId";

                    using var studentCommand = new SqlCommand(studentQuery, connection);
                    studentCommand.Parameters.AddWithValue("@studentId", grade.StudentId);

                    using var studentReader = await studentCommand.ExecuteReaderAsync();
                    if (await studentReader.ReadAsync())
                    {
                        string studentName = $"{studentReader.GetString("FirstName")} {studentReader.GetString("LastName")}";
                        string phoneNumber = studentReader.IsDBNull("GuardianPhone") 
                            ? (studentReader.IsDBNull("Phone") ? "" : studentReader.GetString("Phone"))
                            : studentReader.GetString("GuardianPhone");

                        // Use student phone if guardian phone is not available
                        if (string.IsNullOrEmpty(phoneNumber) && !studentReader.IsDBNull("Phone"))
                        {
                            phoneNumber = studentReader.GetString("Phone");
                        }

                        if (!string.IsNullOrEmpty(phoneNumber) && PhoneValidator.IsValidPhilippinesMobile(phoneNumber))
                        {
                            await SmsService.SendGradeNotificationAsync(
                                phoneNumber,
                                studentName,
                                subjectName,
                                assignmentType,
                                grade.Score,
                                grade.MaxScore,
                                professorName
                            );
                        }
                    }
                    studentReader.Close();
                }
                catch (Exception smsEx)
                {
                    // Log SMS error but don't fail the grade save
                    System.Diagnostics.Debug.WriteLine($"SMS notification failed: {smsEx.Message}");
                }
            }

            return new SaveGradeResult
            {
                SavedCount = savedCount,
                SkippedCount = skippedGrades.Count,
                SkippedGrades = skippedGrades
            };
        }
    }
}

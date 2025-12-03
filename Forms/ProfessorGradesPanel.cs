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
        private TextBox txtAssignmentName;
        private DateTimePicker dtpDueDate;
        private NumericUpDown nudMaxScore;
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
            
            // Handle cleanup when control is being removed from parent
            this.ParentChanged += (s, e) =>
            {
                if (this.Parent == null)
                {
                    CleanupDataGridView();
                }
            };
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // Main container
            this.Size = new Size(1000, 600);
            this.AutoScroll = true;
            this.BackColor = Color.FromArgb(248, 250, 252);

            // Header panel - simplified to Subject and Quarter only
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
            cmbQuarter.SelectedIndexChanged += CmbQuarter_SelectedIndexChanged;

            // Assignment settings panel (for all components)
            var lblAssignmentName = new Label
            {
                Text = "Assignment Name:",
                Location = new Point(480, 50),
                AutoSize = true
            };

            txtAssignmentName = new TextBox
            {
                Location = new Point(600, 48),
                Size = new Size(150, 25)
            };

            // Due date
            var lblDueDate = new Label
            {
                Text = "Due Date:",
                Location = new Point(760, 50),
                AutoSize = true
            };

            dtpDueDate = new DateTimePicker
            {
                Location = new Point(840, 48),
                Size = new Size(120, 25),
                Value = DateTime.Today
            };

            // Max score
            var lblMaxScore = new Label
            {
                Text = "Max Score:",
                Location = new Point(970, 50),
                AutoSize = true
            };

            nudMaxScore = new NumericUpDown
            {
                Location = new Point(1050, 48),
                Size = new Size(80, 25),
                Minimum = 1,
                Maximum = 1000,
                Value = 100
            };

            btnRefresh = new Button
            {
                Text = "Refresh",
                Location = new Point(1140, 47),
                Size = new Size(80, 27),
                BackColor = Color.FromArgb(59, 130, 246),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnRefresh.Click += BtnRefresh_Click;

            pnlHeader.Controls.AddRange(new Control[] { 
                lblTitle, lblSubject, cmbSubject, lblQuarter, cmbQuarter,
                lblAssignmentName, txtAssignmentName, 
                lblDueDate, dtpDueDate, lblMaxScore, nudMaxScore, btnRefresh
            });

            // Grades grid (spreadsheet-like)
            dgvGrades = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                SelectionMode = DataGridViewSelectionMode.CellSelect,
                MultiSelect = true,
                ReadOnly = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowHeadersVisible = true,
                EditMode = DataGridViewEditMode.EditOnEnter,
                AutoGenerateColumns = true
            };
            dgvGrades.CellValidating += DgvGrades_CellValidating;
            dgvGrades.CellEndEdit += DgvGrades_CellEndEdit;
            
            // Handle cleanup when control is being removed
            this.HandleDestroyed += (s, e) => CleanupDataGridView();
            this.Disposed += (s, e) => CleanupDataGridView();

            // Core editable columns will be added dynamically along with the data source

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
                Text = "Save All",
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
            dgvGrades.EnableHeadersVisualStyles = false;
            dgvGrades.GridColor = Color.FromArgb(226, 232, 240);
            dgvGrades.DefaultCellStyle.SelectionBackColor = Color.FromArgb(219, 234, 254);
            dgvGrades.DefaultCellStyle.SelectionForeColor = Color.FromArgb(30, 64, 175);
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

                // Add component grade columns (one per component type)
                if (!dataTable.Columns.Contains("QuizzesActivities"))
                {
                    dataTable.Columns.Add("QuizzesActivities", typeof(decimal));
                }
                if (!dataTable.Columns.Contains("PerformanceTask"))
                {
                    dataTable.Columns.Add("PerformanceTask", typeof(decimal));
                }
                if (!dataTable.Columns.Contains("Exam"))
                {
                    dataTable.Columns.Add("Exam", typeof(decimal));
                }
                if (!dataTable.Columns.Contains("Comments"))
                {
                    dataTable.Columns.Add("Comments", typeof(string));
                }

                // Set default values
                foreach (DataRow row in dataTable.Rows)
                {
                    if (row["QuizzesActivities"] == DBNull.Value) row["QuizzesActivities"] = 0m;
                    if (row["PerformanceTask"] == DBNull.Value) row["PerformanceTask"] = 0m;
                    if (row["Exam"] == DBNull.Value) row["Exam"] = 0m;
                    if (row["Comments"] == DBNull.Value) row["Comments"] = "";
                }

                // Clear existing data source first to avoid column conflicts
                dgvGrades.DataSource = null;
                dgvGrades.Columns.Clear();

                // Set data source
                // Clear existing data source and unfreeze all columns first
                CleanupDataGridView();
                
                // Set data source
                dgvGrades.DataSource = dataTable;

                // Configure columns in spreadsheet-like order
                // Use BeginInvoke to ensure columns are fully generated
                this.BeginInvoke(new Action(() =>
                {
                    try
                    {
                        if (dgvGrades.Columns.Count > 0 && dgvGrades.DataSource != null)
                        {
                            // First, unfreeze all columns to avoid conflicts
                            foreach (DataGridViewColumn col in dgvGrades.Columns)
                            {
                                col.Frozen = false;
                            }

                            // Hide and configure all columns
                            if (dgvGrades.Columns.Contains("Id"))
                                dgvGrades.Columns["Id"].Visible = false;
                            
                            if (dgvGrades.Columns.Contains("StudentId"))
                                dgvGrades.Columns["StudentId"].HeaderText = "Student ID";
                            if (dgvGrades.Columns.Contains("FirstName"))
                                dgvGrades.Columns["FirstName"].HeaderText = "First Name";
                            if (dgvGrades.Columns.Contains("LastName"))
                                dgvGrades.Columns["LastName"].HeaderText = "Last Name";
                            if (dgvGrades.Columns.Contains("GradeLevel"))
                                dgvGrades.Columns["GradeLevel"].HeaderText = "Grade";
                            if (dgvGrades.Columns.Contains("Section"))
                                dgvGrades.Columns["Section"].HeaderText = "Section";

                            // Component columns
                            if (dgvGrades.Columns.Contains("QuizzesActivities"))
                                dgvGrades.Columns["QuizzesActivities"].HeaderText = "Quizzes";
                            if (dgvGrades.Columns.Contains("PerformanceTask"))
                                dgvGrades.Columns["PerformanceTask"].HeaderText = "PT/Activities";
                            if (dgvGrades.Columns.Contains("Exam"))
                                dgvGrades.Columns["Exam"].HeaderText = "Exam";
                            if (dgvGrades.Columns.Contains("Comments"))
                                dgvGrades.Columns["Comments"].HeaderText = "Comments";

                            // Set column widths
                            if (dgvGrades.Columns.Contains("QuizzesActivities"))
                                dgvGrades.Columns["QuizzesActivities"].Width = 100;
                            if (dgvGrades.Columns.Contains("PerformanceTask"))
                                dgvGrades.Columns["PerformanceTask"].Width = 120;
                            if (dgvGrades.Columns.Contains("Exam"))
                                dgvGrades.Columns["Exam"].Width = 100;
                            if (dgvGrades.Columns.Contains("Comments"))
                                dgvGrades.Columns["Comments"].Width = 200;

                            // Set display order
                            if (dgvGrades.Columns.Contains("StudentId"))
                                dgvGrades.Columns["StudentId"].DisplayIndex = 0;
                            if (dgvGrades.Columns.Contains("LastName"))
                                dgvGrades.Columns["LastName"].DisplayIndex = 1;
                            if (dgvGrades.Columns.Contains("FirstName"))
                                dgvGrades.Columns["FirstName"].DisplayIndex = 2;
                            if (dgvGrades.Columns.Contains("GradeLevel"))
                                dgvGrades.Columns["GradeLevel"].DisplayIndex = 3;
                            if (dgvGrades.Columns.Contains("Section"))
                                dgvGrades.Columns["Section"].DisplayIndex = 4;
                            if (dgvGrades.Columns.Contains("QuizzesActivities"))
                                dgvGrades.Columns["QuizzesActivities"].DisplayIndex = 5;
                            if (dgvGrades.Columns.Contains("PerformanceTask"))
                                dgvGrades.Columns["PerformanceTask"].DisplayIndex = 6;
                            if (dgvGrades.Columns.Contains("Exam"))
                                dgvGrades.Columns["Exam"].DisplayIndex = 7;
                            if (dgvGrades.Columns.Contains("Comments"))
                                dgvGrades.Columns["Comments"].DisplayIndex = 8;

                            // Note: Removed column freezing to avoid conflicts when switching panels
                            // Columns are still ordered correctly via DisplayIndex
                        }
                    }
                    catch (Exception ex)
                    {
                        // Silently handle column configuration errors
                        System.Diagnostics.Debug.WriteLine($"Error configuring columns: {ex.Message}");
                    }
                }));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading students: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CmbSubject_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbSubject.SelectedIndex >= 0 && cmbQuarter.SelectedIndex >= 0)
            {
                LoadStudents();
            }
        }

        private void CmbQuarter_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbSubject.SelectedIndex >= 0 && cmbQuarter.SelectedIndex >= 0)
            {
                LoadStudents();
            }
        }

        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            LoadStudents();
        }

        private void CleanupDataGridView()
        {
            try
            {
                if (dgvGrades != null && !dgvGrades.IsDisposed)
                {
                    // Unfreeze all columns before clearing to prevent conflicts
                    if (dgvGrades.Columns.Count > 0)
                    {
                        foreach (DataGridViewColumn col in dgvGrades.Columns)
                        {
                            try
                            {
                                col.Frozen = false;
                            }
                            catch
                            {
                                // Ignore individual column errors
                            }
                        }
                    }
                    
                    // Clear data source
                    dgvGrades.DataSource = null;
                    
                    // Clear columns if needed
                    if (dgvGrades.Columns.Count > 0)
                    {
                        dgvGrades.Columns.Clear();
                    }
                }
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
        
        protected override void OnHandleDestroyed(EventArgs e)
        {
            CleanupDataGridView();
            base.OnHandleDestroyed(e);
        }

        private void DgvGrades_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            // Validate component columns (QuizzesActivities, PerformanceTask, Exam)
            var columnName = dgvGrades.Columns[e.ColumnIndex].Name;
            if (columnName == "QuizzesActivities" || columnName == "PerformanceTask" || columnName == "Exam")
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
            // Auto-cap score at max value after editing for component columns
            var columnName = dgvGrades.Columns[e.ColumnIndex].Name;
            if (columnName == "QuizzesActivities" || columnName == "PerformanceTask" || columnName == "Exam")
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



        private async void BtnSaveGrades_Click(object sender, EventArgs e)
        {
            if (cmbSubject.SelectedIndex == -1)
            {
                MessageBox.Show("Please select a subject first.", "Warning", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Assignment name is now optional

            if (cmbQuarter.SelectedIndex == -1)
            {
                MessageBox.Show("Please select a quarter.", "Warning", 
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
                var quarter = cmbQuarter.SelectedItem.ToString() ?? "Prelim";
                var assignmentName = string.IsNullOrWhiteSpace(txtAssignmentName.Text) ? "Grade Entry" : txtAssignmentName.Text;

                // Iterate through each row and check each component column
                foreach (DataGridViewRow row in dgvGrades.Rows)
                {
                    if (row.Cells["Id"].Value == null) continue;

                    var studentId = Convert.ToInt32(row.Cells["Id"].Value);
                    var comments = row.Cells["Comments"].Value?.ToString() ?? "";

                    // Check QuizzesActivities column
                    if (row.Cells["QuizzesActivities"].Value != null)
                    {
                        var score = Convert.ToDecimal(row.Cells["QuizzesActivities"].Value);
                        if (score > 0)
                        {
                            gradeRecords.Add(new Grade
                            {
                                StudentId = studentId,
                                ProfessorId = currentProfessor.Id,
                                Subject = subjectName,
                                Quarter = quarter,
                                ComponentType = "QuizzesActivities",
                                AssignmentType = "Quizzes",
                                AssignmentName = assignmentName,
                                Score = score,
                                MaxScore = maxScore,
                                Percentage = (score / maxScore) * 100,
                                Comments = comments,
                                DateRecorded = DateTime.Now,
                                DueDate = dtpDueDate.Value.Date
                            });
                        }
                    }

                    // Check PerformanceTask column
                    if (row.Cells["PerformanceTask"].Value != null)
                    {
                        var score = Convert.ToDecimal(row.Cells["PerformanceTask"].Value);
                        if (score > 0)
                        {
                            gradeRecords.Add(new Grade
                            {
                                StudentId = studentId,
                                ProfessorId = currentProfessor.Id,
                                Subject = subjectName,
                                Quarter = quarter,
                                ComponentType = "PerformanceTask",
                                AssignmentType = "PT/Activities",
                                AssignmentName = assignmentName,
                                Score = score,
                                MaxScore = maxScore,
                                Percentage = (score / maxScore) * 100,
                                Comments = comments,
                                DateRecorded = DateTime.Now,
                                DueDate = dtpDueDate.Value.Date
                            });
                        }
                    }

                    // Check Exam column
                    if (row.Cells["Exam"].Value != null)
                    {
                        var score = Convert.ToDecimal(row.Cells["Exam"].Value);
                        if (score > 0)
                        {
                            gradeRecords.Add(new Grade
                            {
                                StudentId = studentId,
                                ProfessorId = currentProfessor.Id,
                                Subject = subjectName,
                                Quarter = quarter,
                                ComponentType = "Exam",
                                AssignmentType = "Exam",
                                AssignmentName = assignmentName,
                                Score = score,
                                MaxScore = maxScore,
                                Percentage = (score / maxScore) * 100,
                                Comments = comments,
                                DateRecorded = DateTime.Now,
                                DueDate = dtpDueDate.Value.Date
                            });
                        }
                    }
                }

                if (gradeRecords.Count == 0)
                {
                    MessageBox.Show("No grades to save. Please enter at least one score in any component column.", "Warning", 
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var result = await SaveGradeRecordsAsync(gradeRecords);
                string message = $"Grades saved successfully for {result.SavedCount} component(s)!";
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
                nudMaxScore.Value = 100;
                dtpDueDate.Value = DateTime.Today;
                LoadStudents(); // Reload to clear entered grades
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

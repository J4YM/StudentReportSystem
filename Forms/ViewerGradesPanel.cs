using StudentReportInitial.Data;
using StudentReportInitial.Models;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Threading.Tasks;
using System.Linq;

namespace StudentReportInitial.Forms
{
    public partial class ViewerGradesPanel : UserControl
    {
        private User currentUser;
        private DataGridView dgvGrades;
        private ComboBox cmbStudent;
        private ComboBox cmbSubject;
        private ComboBox cmbQuarter;
        private Button btnRefresh;
        private Label lblOverallAverage;
        private Label lblCurrentGWA;
        private Label lblCumulativeGWA;
        private Panel pnlSummary;
        private Label? lblStudentInfo;
        private Student? linkedStudent;
        private bool studentContextResolved;
        private readonly List<Student> guardianStudents = new();
        private bool guardianStudentSelectorInitialized = false;

        public ViewerGradesPanel(User user)
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
            await LoadGradesAsync();
            await CalculateGWAAsync();
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
            lblOverallAverage.Text = "Overall Average: No linked student record";
            lblCurrentGWA.Text = "Current GWA: No linked student record";
            lblCumulativeGWA.Text = "Cumulative GWA: No linked student record";
            cmbSubject.Enabled = false;
            cmbQuarter.Enabled = false;
            btnRefresh.Enabled = false;
            dgvGrades.DataSource = null;
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // Main container
            this.Size = new Size(1000, 600);
            this.BackColor = Color.FromArgb(248, 250, 252);
            this.AutoScroll = true;

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
                Text = "Grades Report",
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

            // Quarter filter
            var lblQuarter = new Label
            {
                Text = "Quarter:",
                Location = new Point(590, 50),
                AutoSize = true
            };

            cmbQuarter = new ComboBox
            {
                Location = new Point(650, 48),
                Size = new Size(130, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbQuarter.Items.AddRange(new[] { "All Quarters", "Prelim", "Midterm", "PreFinal", "Final" });
            cmbQuarter.SelectedIndex = 0;
            cmbQuarter.SelectedIndexChanged += CmbQuarter_SelectedIndexChanged;

            btnRefresh = new Button
            {
                Text = "Refresh",
                Location = new Point(800, 47),
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
                lblSubject, cmbSubject,
                lblQuarter, cmbQuarter,
                btnRefresh
            });

            // Summary panel
            pnlSummary = new Panel
            {
                Dock = DockStyle.Top,
                Height = 100,
                BackColor = Color.FromArgb(59, 130, 246),
                Padding = new Padding(20)
            };

            lblOverallAverage = new Label
            {
                Text = "Overall Average: --",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(20, 10)
            };

            lblCurrentGWA = new Label
            {
                Text = "Current GWA: --",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(20, 35)
            };

            lblCumulativeGWA = new Label
            {
                Text = "Cumulative GWA: --",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(20, 60)
            };

            pnlSummary.Controls.Add(lblOverallAverage);
            pnlSummary.Controls.Add(lblCurrentGWA);
            pnlSummary.Controls.Add(lblCumulativeGWA);

            // Grades grid
            dgvGrades = new DataGridView
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

            this.Controls.Add(dgvGrades);
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
            }

            dgvGrades.Font = font;
            dgvGrades.ColumnHeadersDefaultCellStyle.Font = headerFont;
            dgvGrades.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
            dgvGrades.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(51, 65, 85);
            dgvGrades.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
        }

        private void InitializeGuardianStudentSelector()
        {
            if (currentUser.Role != UserRole.Guardian || cmbStudent == null || lblStudentInfo == null)
            {
                return;
            }

            if (guardianStudentSelectorInitialized)
            {
                // Just refresh items
                cmbStudent.Items.Clear();
            }
            else
            {
                // Configure formatting once
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
                // Select the current linked student if present
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
                    SELECT DISTINCT g.Subject
                    FROM Grades g
                    WHERE g.StudentId = @studentId
                    ORDER BY g.Subject";

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

        private async Task LoadGradesAsync()
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
                    SELECT g.Subject, g.Quarter, g.ComponentType, g.AssignmentName, g.Score, g.MaxScore, 
                           g.Percentage, g.Comments, g.DateRecorded, g.DueDate,
                           u.FirstName + ' ' + u.LastName as ProfessorName
                    FROM Grades g
                    INNER JOIN Users u ON g.ProfessorId = u.Id
                    WHERE g.StudentId = @studentId";

                var parameters = new List<SqlParameter>
                {
                    new SqlParameter("@studentId", linkedStudent!.Id)
                };

                if (cmbSubject.SelectedIndex > 0)
                {
                    query += " AND g.Subject = @subject";
                    parameters.Add(new SqlParameter("@subject", cmbSubject.SelectedItem.ToString()));
                }

                if (cmbQuarter.SelectedIndex > 0)
                {
                    query += " AND g.Quarter = @quarter";
                    parameters.Add(new SqlParameter("@quarter", cmbQuarter.SelectedItem.ToString()));
                }

                query += " ORDER BY g.DateRecorded DESC";

                using var command = new SqlCommand(query, connection);
                command.Parameters.AddRange(parameters.ToArray());

                using var adapter = new SqlDataAdapter(command);
                var dataTable = new DataTable();
                adapter.Fill(dataTable);

                dgvGrades.DataSource = dataTable;

                // Format columns
                if (dgvGrades.Columns.Count > 0)
                {
                    dgvGrades.Columns["Subject"].HeaderText = "Subject";
                    dgvGrades.Columns["Quarter"].HeaderText = "Quarter";
                    dgvGrades.Columns["ComponentType"].HeaderText = "Component";
                    dgvGrades.Columns["AssignmentName"].HeaderText = "Assignment";
                    dgvGrades.Columns["Score"].HeaderText = "Score";
                    dgvGrades.Columns["MaxScore"].HeaderText = "Max Score";
                    dgvGrades.Columns["Percentage"].HeaderText = "Percentage";
                    dgvGrades.Columns["Comments"].HeaderText = "Comments";
                    dgvGrades.Columns["DateRecorded"].HeaderText = "Date Recorded";
                    dgvGrades.Columns["DueDate"].HeaderText = "Due Date";
                    dgvGrades.Columns["ProfessorName"].HeaderText = "Professor";

                    dgvGrades.Columns["DateRecorded"].DefaultCellStyle.Format = "MM/dd/yyyy";
                    dgvGrades.Columns["DueDate"].DefaultCellStyle.Format = "MM/dd/yyyy";
                    dgvGrades.Columns["Percentage"].DefaultCellStyle.Format = "0.0'%'";
                }

                // Calculate overall average using STI grading system
                CalculateOverallAverageSTI(dataTable);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading grades: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CalculateOverallAverageSTI(DataTable dataTable)
        {
            if (dataTable.Rows.Count == 0)
            {
                lblOverallAverage.Text = "Overall Average: No grades available";
                return;
            }

            try
            {
                // Group grades by subject and quarter
                var subjectGroups = dataTable.AsEnumerable()
                    .GroupBy(row => row.Field<string>("Subject") ?? "");

                var overallGrades = new List<double>();

                foreach (var subjectGroup in subjectGroups)
                {
                    var subjectName = subjectGroup.Key;
                    if (string.IsNullOrEmpty(subjectName)) continue;

                    // Group by quarter
                    var quarterGroups = subjectGroup.GroupBy(row => row.Field<string>("Quarter") ?? "");

                    var quarterGradeList = new List<(string Quarter, GradeCalculator.QuarterGrade Grade, double QuarterAverage)>();

                    foreach (var quarterGroup in quarterGroups)
                    {
                        var quarter = quarterGroup.Key;
                        if (string.IsNullOrEmpty(quarter)) continue;

                        // Group by component type
                        var componentGroups = quarterGroup.GroupBy(row => row.Field<string>("ComponentType") ?? "");

                        double quizzesAvg = 0, performanceAvg = 0, examAvg = 0;
                        int quizzesCount = 0, performanceCount = 0, examCount = 0;

                        foreach (var componentGroup in componentGroups)
                        {
                            var componentType = componentGroup.Key;
                            if (string.IsNullOrEmpty(componentType)) continue;

                            double totalPercentage = 0;
                            int count = 0;

                            foreach (var row in componentGroup)
                            {
                                if (row["Percentage"] != DBNull.Value)
                                {
                                    totalPercentage += Convert.ToDouble(row["Percentage"]);
                                    count++;
                                }
                            }

                            if (count > 0)
                            {
                                var avg = totalPercentage / count;
                                if (componentType == "QuizzesActivities")
                                {
                                    quizzesAvg = avg;
                                    quizzesCount = count;
                                }
                                else if (componentType == "PerformanceTask")
                                {
                                    performanceAvg = avg;
                                    performanceCount = count;
                                }
                                else if (componentType == "Exam")
                                {
                                    examAvg = avg;
                                    examCount = count;
                                }
                            }
                        }

                        // Only include quarters that have at least one component with data
                        // Calculate quarter average only if we have valid data
                        if (quizzesCount > 0 || performanceCount > 0 || examCount > 0)
                        {
                            var quarterGrade = new GradeCalculator.QuarterGrade
                            {
                                QuizzesActivities = quizzesAvg,
                                PerformanceTask = performanceAvg,
                                Exam = examAvg
                            };

                            // Calculate the quarter average
                            var quarterAverage = quarterGrade.CalculateQuarterGrade();
                            quarterGradeList.Add((quarter, quarterGrade, quarterAverage));
                        }
                    }

                    // Calculate overall grade for this subject using only quarters with data
                    if (quarterGradeList.Count > 0)
                    {
                        // Calculate weighted average based on available quarters
                        double totalWeight = 0;
                        double weightedSum = 0;

                        foreach (var (quarter, grade, quarterAvg) in quarterGradeList)
                        {
                            double weight = 0;
                            if (quarter == "Prelim") weight = GradeCalculator.PRELIM_WEIGHT;
                            else if (quarter == "Midterm") weight = GradeCalculator.MIDTERM_WEIGHT;
                            else if (quarter == "PreFinal") weight = GradeCalculator.PREFINAL_WEIGHT;
                            else if (quarter == "Final") weight = GradeCalculator.FINAL_WEIGHT;

                            if (weight > 0)
                            {
                                weightedSum += quarterAvg * weight;
                                totalWeight += weight;
                            }
                        }

                        // If we have quarters with data, calculate the overall grade
                        // Normalize by the total weight of available quarters
                        if (totalWeight > 0)
                        {
                            var overall = weightedSum / totalWeight;
                            overallGrades.Add(overall);
                        }
                    }
                }

                if (overallGrades.Count > 0)
                {
                    var overallAverage = overallGrades.Average();
                    var letterGrade = GradeCalculator.GetLetterGrade(overallAverage);
                    lblOverallAverage.Text = $"Overall Average: {overallAverage:F2}% ({letterGrade})";
                }
                else
                {
                    lblOverallAverage.Text = "Overall Average: No valid grades (STI format required)";
                }
            }
            catch (Exception ex)
            {
                lblOverallAverage.Text = $"Error calculating average: {ex.Message}";
            }
        }

        private async void CmbSubject_SelectedIndexChanged(object sender, EventArgs e)
        {
            await LoadGradesAsync();
        }

        private async void CmbQuarter_SelectedIndexChanged(object sender, EventArgs e)
        {
            await LoadGradesAsync();
            await CalculateGWAAsync();
        }

        private async void BtnRefresh_Click(object sender, EventArgs e)
        {
            await LoadGradesAsync();
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
                await LoadGradesAsync();
                await CalculateGWAAsync();
            }
        }

        private async Task CalculateGWAAsync()
        {
            try
            {
                if (!await EnsureStudentContextAsync())
                {
                    lblCurrentGWA.Text = "Current GWA: No linked student record";
                    lblCumulativeGWA.Text = "Cumulative GWA: No linked student record";
                    return;
                }

                using var connection = DatabaseHelper.GetConnection();
                await connection.OpenAsync();

                // Get all grades for cumulative GWA
                var allGradesQuery = @"
                    SELECT g.Subject, g.Quarter, g.ComponentType, g.Percentage
                    FROM Grades g
                    WHERE g.StudentId = @studentId";

                using var allGradesCommand = new SqlCommand(allGradesQuery, connection);
                allGradesCommand.Parameters.AddWithValue("@studentId", linkedStudent!.Id);

                using var allGradesAdapter = new SqlDataAdapter(allGradesCommand);
                var allGradesTable = new DataTable();
                allGradesAdapter.Fill(allGradesTable);

                // Calculate Cumulative GWA (all quarters, all subjects)
                var cumulativeGWA = CalculateGWAFromDataTable(allGradesTable);
                
                // Get selected quarter grades (or all quarters if "All Quarters" is selected)
                string selectedQuarter = cmbQuarter.SelectedIndex > 0 ? cmbQuarter.SelectedItem.ToString()! : "All Quarters";
                double? selectedQuarterGWA = null;
                
                if (selectedQuarter != "All Quarters")
                {
                    var selectedQuarterQuery = @"
                        SELECT g.Subject, g.Quarter, g.ComponentType, g.Percentage
                        FROM Grades g
                        WHERE g.StudentId = @studentId AND g.Quarter = @quarter";

                    using var selectedQuarterCommand = new SqlCommand(selectedQuarterQuery, connection);
                    selectedQuarterCommand.Parameters.AddWithValue("@studentId", linkedStudent!.Id);
                    selectedQuarterCommand.Parameters.AddWithValue("@quarter", selectedQuarter);

                    using var selectedQuarterAdapter = new SqlDataAdapter(selectedQuarterCommand);
                    var selectedQuarterTable = new DataTable();
                    selectedQuarterAdapter.Fill(selectedQuarterTable);

                    // Calculate GWA for selected quarter
                    selectedQuarterGWA = CalculateGWAFromDataTable(selectedQuarterTable);
                }

                // Update labels
                if (cumulativeGWA.HasValue)
                {
                    var cumulativeNumeric = GradeCalculator.GetGWANumericGrade(cumulativeGWA.Value);
                    var cumulativeLetter = GradeCalculator.GetGWALetterGrade(cumulativeGWA.Value);
                    lblCumulativeGWA.Text = $"Cumulative GWA: {cumulativeNumeric:F2} ({cumulativeLetter})";
                }
                else
                {
                    lblCumulativeGWA.Text = "Cumulative GWA: No grades available";
                }

                if (selectedQuarterGWA.HasValue)
                {
                    var selectedNumeric = GradeCalculator.GetGWANumericGrade(selectedQuarterGWA.Value);
                    var selectedLetter = GradeCalculator.GetGWALetterGrade(selectedQuarterGWA.Value);
                    lblCurrentGWA.Text = $"{selectedQuarter} GWA: {selectedNumeric:F2} ({selectedLetter})";
                }
                else
                {
                    lblCurrentGWA.Text = selectedQuarter != "All Quarters" 
                        ? $"{selectedQuarter} GWA: No grades available"
                        : "Select a quarter to view GWA";
                }
            }
            catch (Exception ex)
            {
                lblCurrentGWA.Text = $"Current GWA: Error - {ex.Message}";
                lblCumulativeGWA.Text = $"Cumulative GWA: Error - {ex.Message}";
            }
        }

        private double? CalculateGWAFromDataTable(DataTable dataTable)
        {
            if (dataTable.Rows.Count == 0)
            {
                return null;
            }

            try
            {
                // Group grades by subject and quarter
                var subjectGroups = dataTable.AsEnumerable()
                    .GroupBy(row => row.Field<string>("Subject") ?? "");

                var subjectFinalGrades = new List<double>();

                foreach (var subjectGroup in subjectGroups)
                {
                    var subjectName = subjectGroup.Key;
                    if (string.IsNullOrEmpty(subjectName)) continue;

                    // Group by quarter
                    var quarterGroups = subjectGroup.GroupBy(row => row.Field<string>("Quarter") ?? "");

                    var quarterGradeList = new List<(string Quarter, GradeCalculator.QuarterGrade Grade, double QuarterAverage)>();

                    foreach (var quarterGroup in quarterGroups)
                    {
                        var quarter = quarterGroup.Key;
                        if (string.IsNullOrEmpty(quarter)) continue;

                        // Group by component type
                        var componentGroups = quarterGroup.GroupBy(row => row.Field<string>("ComponentType") ?? "");

                        double quizzesAvg = 0, performanceAvg = 0, examAvg = 0;
                        int quizzesCount = 0, performanceCount = 0, examCount = 0;

                        foreach (var componentGroup in componentGroups)
                        {
                            var componentType = componentGroup.Key;
                            if (string.IsNullOrEmpty(componentType)) continue;

                            double totalPercentage = 0;
                            int count = 0;

                            foreach (var row in componentGroup)
                            {
                                if (row["Percentage"] != DBNull.Value)
                                {
                                    totalPercentage += Convert.ToDouble(row["Percentage"]);
                                    count++;
                                }
                            }

                            if (count > 0)
                            {
                                var avg = totalPercentage / count;
                                if (componentType == "QuizzesActivities")
                                {
                                    quizzesAvg = avg;
                                    quizzesCount = count;
                                }
                                else if (componentType == "PerformanceTask")
                                {
                                    performanceAvg = avg;
                                    performanceCount = count;
                                }
                                else if (componentType == "Exam")
                                {
                                    examAvg = avg;
                                    examCount = count;
                                }
                            }
                        }

                        // Only include quarters that have at least one component with data
                        if (quizzesCount > 0 || performanceCount > 0 || examCount > 0)
                        {
                            var quarterGrade = new GradeCalculator.QuarterGrade
                            {
                                QuizzesActivities = quizzesAvg,
                                PerformanceTask = performanceAvg,
                                Exam = examAvg
                            };

                            var quarterAverage = quarterGrade.CalculateQuarterGrade();
                            quarterGradeList.Add((quarter, quarterGrade, quarterAverage));
                        }
                    }

                    // Calculate overall grade for this subject using only quarters with data
                    if (quarterGradeList.Count > 0)
                    {
                        double totalWeight = 0;
                        double weightedSum = 0;

                        foreach (var (quarter, grade, quarterAvg) in quarterGradeList)
                        {
                            double weight = 0;
                            if (quarter == "Prelim") weight = GradeCalculator.PRELIM_WEIGHT;
                            else if (quarter == "Midterm") weight = GradeCalculator.MIDTERM_WEIGHT;
                            else if (quarter == "PreFinal") weight = GradeCalculator.PREFINAL_WEIGHT;
                            else if (quarter == "Final") weight = GradeCalculator.FINAL_WEIGHT;

                            if (weight > 0)
                            {
                                weightedSum += quarterAvg * weight;
                                totalWeight += weight;
                            }
                        }

                        if (totalWeight > 0)
                        {
                            var subjectFinal = weightedSum / totalWeight;
                            subjectFinalGrades.Add(subjectFinal);
                        }
                    }
                }

                if (subjectFinalGrades.Count > 0)
                {
                    return subjectFinalGrades.Average();
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

    }
}
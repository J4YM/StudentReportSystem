using System.Data;
using System.Data.SqlClient;
using System.Linq;
using StudentReportInitial.Data;
using StudentReportInitial.Models;

namespace StudentReportInitial.Forms
{
    /// <summary>
    /// Transcript-style read-only report for a single student.
    /// Launched from the professor past records panel and uses existing
    /// grading/attendance calculations (GradeCalculator, GWA logic).
    /// </summary>
    public class StudentReportForm : Form
    {
        private readonly int studentId;
        private readonly User currentProfessor;

        private Label lblStudentHeader = null!;
        private Label lblGwaSummary = null!;
        private DataGridView dgvGrades = null!;
        private DataGridView dgvAttendance = null!;
        private Button btnPrint = null!;
        private ComboBox cmbQuarterFilter = null!;
        private ComboBox cmbComponentFilter = null!;

        private DataTable? gradesTable;
        private DataTable? attendanceTable;
        private DataTable? allGradesTable; // Store unfiltered data

        public StudentReportForm(User professor, int studentId)
        {
            currentProfessor = professor;
            this.studentId = studentId;

            InitializeComponent();
            _ = LoadReportAsync();
        }

        private void InitializeComponent()
        {
            Text = "Student Academic Report";
            StartPosition = FormStartPosition.CenterParent;
            Size = new Size(1100, 700);
            MinimumSize = new Size(900, 600);

            var mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 4,
                Padding = new Padding(16),
            };
            mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // header (now taller with filters)
            mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // summary
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 55)); // grades
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 45)); // attendance

            // Header (student info + actions + filters)
            var headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 110,
            };

            lblStudentHeader = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 41, 59),
                Location = new Point(10, 10),
                MaximumSize = new Size(650, 0)
            };

            lblGwaSummary = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                ForeColor = Color.FromArgb(55, 65, 81),
                Location = new Point(10, 40),
                MaximumSize = new Size(650, 0)
            };

            // Filters
            var lblQuarterFilter = new Label
            {
                Text = "Quarter:",
                Location = new Point(10, 70),
                AutoSize = true,
                Font = new Font("Segoe UI", 9)
            };

            cmbQuarterFilter = new ComboBox
            {
                Location = new Point(70, 68),
                Size = new Size(120, 25),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9)
            };
            cmbQuarterFilter.Items.Add("All Quarters");
            cmbQuarterFilter.SelectedIndex = 0;
            cmbQuarterFilter.SelectedIndexChanged += CmbQuarterFilter_SelectedIndexChanged;

            var lblComponentFilter = new Label
            {
                Text = "Component:",
                Location = new Point(200, 70),
                AutoSize = true,
                Font = new Font("Segoe UI", 9)
            };

            cmbComponentFilter = new ComboBox
            {
                Location = new Point(280, 68),
                Size = new Size(150, 25),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9)
            };
            cmbComponentFilter.Items.Add("All Components");
            cmbComponentFilter.Items.AddRange(new[] { "QuizzesActivities", "PerformanceTask", "Exam" });
            cmbComponentFilter.SelectedIndex = 0;
            cmbComponentFilter.SelectedIndexChanged += CmbComponentFilter_SelectedIndexChanged;

            btnPrint = new Button
            {
                Text = "Print",
                Size = new Size(110, 32),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                BackColor = Color.FromArgb(34, 197, 94),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Location = new Point(Width - 280, 20)
            };
            btnPrint.Click += BtnPrint_Click;
            UIStyleHelper.ApplyRoundedButton(btnPrint, 10);

            headerPanel.Controls.Add(lblStudentHeader);
            headerPanel.Controls.Add(lblGwaSummary);
            headerPanel.Controls.Add(lblQuarterFilter);
            headerPanel.Controls.Add(cmbQuarterFilter);
            headerPanel.Controls.Add(lblComponentFilter);
            headerPanel.Controls.Add(cmbComponentFilter);
            headerPanel.Controls.Add(btnPrint);
            headerPanel.Resize += (_, _) =>
            {
                // Keep print button pinned to the right
                btnPrint.Left = headerPanel.ClientSize.Width - btnPrint.Width - 10;
            };

            // Grades section
            var gradesGroup = new GroupBox
            {
                Text = "Grades",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };

            dgvGrades = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                MultiSelect = false,
                RowHeadersVisible = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None
            };

            gradesGroup.Controls.Add(dgvGrades);

            // Attendance section
            var attendanceGroup = new GroupBox
            {
                Text = "Attendance",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };

            dgvAttendance = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                MultiSelect = false,
                RowHeadersVisible = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None
            };

            attendanceGroup.Controls.Add(dgvAttendance);

            mainLayout.Controls.Add(headerPanel, 0, 0);
            mainLayout.SetColumnSpan(headerPanel, 1);
            mainLayout.Controls.Add(new Panel { Height = 4, Dock = DockStyle.Top }, 0, 1); // spacer
            mainLayout.Controls.Add(gradesGroup, 0, 2);
            mainLayout.Controls.Add(attendanceGroup, 0, 3);

            Controls.Add(mainLayout);
        }

        private async Task LoadReportAsync()
        {
            try
            {
                using var connection = DatabaseHelper.GetConnection();
                await connection.OpenAsync();

                // Basic student info
                var studentQuery = @"
                    SELECT TOP 1 s.StudentId, s.FirstName, s.LastName, s.GradeLevel, s.Section
                    FROM Students s
                    WHERE s.Id = @studentId";

                using (var studentCommand = new SqlCommand(studentQuery, connection))
                {
                    studentCommand.Parameters.AddWithValue("@studentId", studentId);

                    using var reader = await studentCommand.ExecuteReaderAsync();
                    if (await reader.ReadAsync())
                    {
                        var fullName = $"{reader.GetString(reader.GetOrdinal("FirstName"))} {reader.GetString(reader.GetOrdinal("LastName"))}";
                        var studentNumber = reader.GetString(reader.GetOrdinal("StudentId"));
                        var gradeLevel = reader["GradeLevel"]?.ToString() ?? "";
                        var section = reader["Section"]?.ToString() ?? "";

                        lblStudentHeader.Text = $"{fullName} ({studentNumber}) - {gradeLevel} {section}";
                    }
                }

                // Load all grades for this student (reuse ViewerGradesPanel queries, but without user context)
                var gradesQuery = @"
                    SELECT g.Subject, g.Quarter, g.ComponentType, g.AssignmentName,
                           g.Score, g.MaxScore, g.Percentage, g.Comments,
                           g.DateRecorded, g.DueDate
                    FROM Grades g
                    WHERE g.StudentId = @studentId
                    ORDER BY g.Subject, g.Quarter, g.DateRecorded";

                using (var gradesCommand = new SqlCommand(gradesQuery, connection))
                {
                    gradesCommand.Parameters.AddWithValue("@studentId", studentId);
                    using var adapter = new SqlDataAdapter(gradesCommand);
                    allGradesTable = new DataTable();
                    adapter.Fill(allGradesTable);
                }

                // Populate quarter filter
                cmbQuarterFilter.Items.Clear();
                cmbQuarterFilter.Items.Add("All Quarters");
                var quarters = allGradesTable.AsEnumerable()
                    .Select(r => r.Field<string>("Quarter"))
                    .Where(q => !string.IsNullOrEmpty(q))
                    .Distinct()
                    .OrderBy(q => q)
                    .ToList();
                cmbQuarterFilter.Items.AddRange(quarters.ToArray());
                cmbQuarterFilter.SelectedIndex = 0;

                // Apply initial filter
                ApplyGradeFilters();
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
                    dgvGrades.Columns["DateRecorded"].HeaderText = "Recorded";
                    dgvGrades.Columns["DueDate"].HeaderText = "Due Date";

                    dgvGrades.Columns["DateRecorded"].DefaultCellStyle.Format = "MM/dd/yyyy";
                    dgvGrades.Columns["DueDate"].DefaultCellStyle.Format = "MM/dd/yyyy";
                    dgvGrades.Columns["Percentage"].DefaultCellStyle.Format = "0.0'%'";
                }

                // Attendance
                var attendanceQuery = @"
                    SELECT a.Subject, a.Date, a.Status, a.Notes, a.RecordedDate
                    FROM Attendance a
                    WHERE a.StudentId = @studentId
                    ORDER BY a.Date";

                using (var attCommand = new SqlCommand(attendanceQuery, connection))
                {
                    attCommand.Parameters.AddWithValue("@studentId", studentId);
                    using var adapter = new SqlDataAdapter(attCommand);
                    attendanceTable = new DataTable();
                    adapter.Fill(attendanceTable);
                }

                if (attendanceTable != null)
                {
                    // Add human-readable status
                    if (!attendanceTable.Columns.Contains("StatusText"))
                    {
                        attendanceTable.Columns.Add("StatusText", typeof(string));
                    }

                    foreach (DataRow row in attendanceTable.Rows)
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
                }

                dgvAttendance.DataSource = attendanceTable;
                if (dgvAttendance.Columns.Count > 0)
                {
                    dgvAttendance.Columns["Status"].Visible = false;
                    dgvAttendance.Columns["Subject"].HeaderText = "Subject";
                    dgvAttendance.Columns["Date"].HeaderText = "Date";
                    dgvAttendance.Columns["StatusText"].HeaderText = "Status";
                    dgvAttendance.Columns["Notes"].HeaderText = "Notes";
                    dgvAttendance.Columns["RecordedDate"].HeaderText = "Recorded";

                    dgvAttendance.Columns["Date"].DefaultCellStyle.Format = "MM/dd/yyyy";
                    dgvAttendance.Columns["RecordedDate"].DefaultCellStyle.Format = "MM/dd/yyyy HH:mm";
                }

                // Calculate GWA summary using same algorithm as viewer panels
                UpdateGwaSummary();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading student report: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CmbQuarterFilter_SelectedIndexChanged(object? sender, EventArgs e)
        {
            ApplyGradeFilters();
        }

        private void CmbComponentFilter_SelectedIndexChanged(object? sender, EventArgs e)
        {
            ApplyGradeFilters();
        }

        private void ApplyGradeFilters()
        {
            if (allGradesTable == null) return;

            var filterParts = new List<string>();

            // Quarter filter
            if (cmbQuarterFilter.SelectedIndex > 0)
            {
                var quarter = cmbQuarterFilter.SelectedItem?.ToString();
                if (!string.IsNullOrEmpty(quarter))
                {
                    filterParts.Add($"Quarter = '{quarter.Replace("'", "''")}'");
                }
            }

            // Component filter
            if (cmbComponentFilter.SelectedIndex > 0)
            {
                var component = cmbComponentFilter.SelectedItem?.ToString();
                if (!string.IsNullOrEmpty(component))
                {
                    filterParts.Add($"ComponentType = '{component.Replace("'", "''")}'");
                }
            }

            // Apply filter
            allGradesTable.DefaultView.RowFilter = filterParts.Count > 0
                ? string.Join(" AND ", filterParts)
                : string.Empty;

            // Create filtered table
            gradesTable = allGradesTable.DefaultView.ToTable();

            // Update grid
            dgvGrades.DataSource = gradesTable;

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
                dgvGrades.Columns["DateRecorded"].HeaderText = "Recorded";
                dgvGrades.Columns["DueDate"].HeaderText = "Due Date";

                dgvGrades.Columns["DateRecorded"].DefaultCellStyle.Format = "MM/dd/yyyy";
                dgvGrades.Columns["DueDate"].DefaultCellStyle.Format = "MM/dd/yyyy";
                dgvGrades.Columns["Percentage"].DefaultCellStyle.Format = "0.0'%'";
            }

            // Update GWA with filtered data
            UpdateGwaSummary();
        }

        private void UpdateGwaSummary()
        {
            // Use allGradesTable for GWA calculation (cumulative, not filtered)
            if (allGradesTable == null || allGradesTable.Rows.Count == 0)
            {
                lblGwaSummary.Text = "No grades available yet for GWA computation.";
                return;
            }

            try
            {
                var cumulative = CalculateGwaFromDataTable(allGradesTable);

                if (cumulative.HasValue)
                {
                    var gwaNumeric = GradeCalculator.GetGWANumericGrade(cumulative.Value);
                    var gwaLetter = GradeCalculator.GetGWALetterGrade(cumulative.Value);
                    lblGwaSummary.Text = $"Cumulative GWA: {gwaNumeric:F2} ({gwaLetter})";
                }
                else
                {
                    lblGwaSummary.Text = "Cumulative GWA: No valid grades (STI format required).";
                }
            }
            catch (Exception ex)
            {
                lblGwaSummary.Text = $"Error calculating GWA: {ex.Message}";
            }
        }

        /// <summary>
        /// GWA computation logic copied from ViewerGradesPanel.CalculateGWAFromDataTable
        /// so that we keep the same formulas without changing existing code.
        /// </summary>
        private static double? CalculateGwaFromDataTable(DataTable dataTable)
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

        private void BtnPrint_Click(object? sender, EventArgs e)
        {
            using var printDialog = new PrintDialog();
            using var printDoc = new System.Drawing.Printing.PrintDocument();

            printDoc.DocumentName = "Student Academic Report";
            printDoc.PrintPage += (s, ev) =>
            {
                // Simple print: draw the form as a bitmap
                using var bmp = new Bitmap(Width, Height);
                DrawToBitmap(bmp, new Rectangle(0, 0, bmp.Width, bmp.Height));
                ev.Graphics.DrawImage(bmp, ev.MarginBounds.Location);
            };

            printDialog.Document = printDoc;
            if (printDialog.ShowDialog(this) == DialogResult.OK)
            {
                printDoc.Print();
            }
        }

    }
}

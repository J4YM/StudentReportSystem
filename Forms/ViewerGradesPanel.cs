using StudentReportInitial.Data;
using StudentReportInitial.Models;
using System.Data.SqlClient;
using System.Data;
using System.Threading.Tasks;

namespace StudentReportInitial.Forms
{
    public partial class ViewerGradesPanel : UserControl
    {
        private User currentUser;
        private DataGridView dgvGrades;
        private ComboBox cmbSubject;
        private ComboBox cmbAssignmentType;
        private Button btnRefresh;
        private Label lblOverallAverage;
        private Panel pnlSummary;
        private Student? linkedStudent;
        private bool studentContextResolved;

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
        }

        private async Task<bool> EnsureStudentContextAsync()
        {
            if (studentContextResolved)
            {
                return linkedStudent != null;
            }

            studentContextResolved = true;
            linkedStudent = await UserContextHelper.GetLinkedStudentAsync(currentUser);
            return linkedStudent != null;
        }

        private void HandleMissingStudentContext()
        {
            lblOverallAverage.Text = "Overall Average: No linked student record";
            cmbSubject.Enabled = false;
            cmbAssignmentType.Enabled = false;
            btnRefresh.Enabled = false;
            dgvGrades.DataSource = null;
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
                Text = "Grades Report",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(51, 65, 85),
                AutoSize = true,
                Location = new Point(20, 20)
            };

            // Subject filter
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
            cmbSubject.Items.Add("All Subjects");
            cmbSubject.SelectedIndex = 0;
            cmbSubject.SelectedIndexChanged += CmbSubject_SelectedIndexChanged;

            // Assignment type filter
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
            cmbAssignmentType.Items.AddRange(new[] { "All Types", "Quiz", "Exam", "Project", "Assignment", "Participation", "Other" });
            cmbAssignmentType.SelectedIndex = 0;
            cmbAssignmentType.SelectedIndexChanged += CmbAssignmentType_SelectedIndexChanged;

            btnRefresh = new Button
            {
                Text = "Refresh",
                Location = new Point(480, 47),
                Size = new Size(80, 27),
                BackColor = Color.FromArgb(59, 130, 246),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                
                Cursor = Cursors.Hand
            };
            btnRefresh.Click += BtnRefresh_Click;

            pnlHeader.Controls.AddRange(new Control[] { 
                lblTitle, lblSubject, cmbSubject, lblAssignmentType, cmbAssignmentType, btnRefresh
            });

            // Summary panel
            pnlSummary = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.FromArgb(59, 130, 246),
                Padding = new Padding(20)
            };

            lblOverallAverage = new Label
            {
                Text = "Overall Average: --",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(20, 15)
            };

            pnlSummary.Controls.Add(lblOverallAverage);

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
                    SELECT g.Subject, g.AssignmentType, g.AssignmentName, g.Score, g.MaxScore, 
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

                if (cmbAssignmentType.SelectedIndex > 0)
                {
                    query += " AND g.AssignmentType = @assignmentType";
                    parameters.Add(new SqlParameter("@assignmentType", cmbAssignmentType.SelectedItem.ToString()));
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
                    dgvGrades.Columns["AssignmentType"].HeaderText = "Type";
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
                    dgvGrades.Columns["Percentage"].DefaultCellStyle.Format = "0.0%";
                }

                // Calculate overall average
                CalculateOverallAverage(dataTable);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading grades: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CalculateOverallAverage(DataTable dataTable)
        {
            if (dataTable.Rows.Count == 0)
            {
                lblOverallAverage.Text = "Overall Average: No grades available";
                return;
            }

            decimal totalPercentage = 0;
            int count = 0;

            foreach (DataRow row in dataTable.Rows)
            {
                if (row["Percentage"] != DBNull.Value)
                {
                    totalPercentage += Convert.ToDecimal(row["Percentage"]);
                    count++;
                }
            }

            if (count > 0)
            {
                var average = totalPercentage / count;
                var grade = GetLetterGrade(average);
                lblOverallAverage.Text = $"Overall Average: {average:F1}% ({grade})";
            }
            else
            {
                lblOverallAverage.Text = "Overall Average: No valid grades";
            }
        }

        private string GetLetterGrade(decimal percentage)
        {
            return percentage switch
            {
                >= 97 => "A+",
                >= 93 => "A",
                >= 90 => "A-",
                >= 87 => "B+",
                >= 83 => "B",
                >= 80 => "B-",
                >= 77 => "C+",
                >= 73 => "C",
                >= 70 => "C-",
                >= 67 => "D+",
                >= 63 => "D",
                >= 60 => "D-",
                _ => "F"
            };
        }

        private async void CmbSubject_SelectedIndexChanged(object sender, EventArgs e)
        {
            await LoadGradesAsync();
        }

        private async void CmbAssignmentType_SelectedIndexChanged(object sender, EventArgs e)
        {
            await LoadGradesAsync();
        }

        private async void BtnRefresh_Click(object sender, EventArgs e)
        {
            await LoadGradesAsync();
        }
    }
}

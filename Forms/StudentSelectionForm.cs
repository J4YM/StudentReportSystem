using StudentReportInitial.Models;
using StudentReportInitial.Data;
using System.Data.SqlClient;
using System.Data;
using System.Collections.Generic;
using System.Linq;

namespace StudentReportInitial.Forms
{
    public partial class StudentSelectionForm : Form
    {
        private DataGridView dgvStudents = null!;
        private ComboBox cmbYearFilter = null!;
        private ComboBox cmbCourseFilter = null!;
        private ComboBox cmbSectionFilter = null!;
        private Button btnClearFilters = null!;
        private Button btnSelect = null!;
        private Button btnCancel = null!;
        private Label lblSelectedCount = null!;
        private DataTable? studentsTable = null;
        private User? currentUser;
        private int? branchFilterId = null;
        private List<int> selectedStudentIds = new List<int>();

        public List<int> SelectedStudentIds => selectedStudentIds;

        public StudentSelectionForm(User? user = null, int? branchId = null)
        {
            currentUser = user;
            branchFilterId = branchId;
            InitializeComponent();
            LoadStudents();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            this.Text = "Select Students";
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.FromArgb(248, 250, 252);

            // Header
            var lblTitle = new Label
            {
                Text = "Select Students for Guardian Assignment",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(51, 65, 85),
                AutoSize = true,
                Location = new Point(20, 20)
            };

            // Filter panel
            var pnlFilters = new Panel
            {
                Location = new Point(20, 60),
                Size = new Size(740, 100),
                BackColor = Color.White,
                Padding = new Padding(10)
            };

            var lblYear = new Label { Text = "Year Level:", Location = new Point(10, 10), AutoSize = true };
            cmbYearFilter = new ComboBox
            {
                Location = new Point(10, 30),
                Size = new Size(150, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbYearFilter.Items.Add("All");
            cmbYearFilter.SelectedIndex = 0;
            cmbYearFilter.SelectedIndexChanged += (s, e) => ApplyFilters();

            var lblCourse = new Label { Text = "Course:", Location = new Point(180, 10), AutoSize = true };
            cmbCourseFilter = new ComboBox
            {
                Location = new Point(180, 30),
                Size = new Size(150, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbCourseFilter.Items.Add("All");
            cmbCourseFilter.SelectedIndex = 0;
            cmbCourseFilter.SelectedIndexChanged += (s, e) => ApplyFilters();

            var lblSection = new Label { Text = "Section:", Location = new Point(350, 10), AutoSize = true };
            cmbSectionFilter = new ComboBox
            {
                Location = new Point(350, 30),
                Size = new Size(150, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbSectionFilter.Items.Add("All");
            cmbSectionFilter.SelectedIndex = 0;
            cmbSectionFilter.SelectedIndexChanged += (s, e) => ApplyFilters();

            btnClearFilters = new Button
            {
                Text = "Clear Filters",
                Location = new Point(520, 30),
                Size = new Size(100, 25),
                BackColor = Color.FromArgb(241, 245, 249),
                ForeColor = Color.FromArgb(51, 65, 85),
                FlatStyle = FlatStyle.Flat,
                Enabled = false,
                Cursor = Cursors.Hand
            };
            btnClearFilters.Click += BtnClearFilters_Click;

            pnlFilters.Controls.AddRange(new Control[] {
                lblYear, cmbYearFilter,
                lblCourse, cmbCourseFilter,
                lblSection, cmbSectionFilter,
                btnClearFilters
            });

            // DataGridView
            dgvStudents = new DataGridView
            {
                Location = new Point(20, 170),
                Size = new Size(740, 320),
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = true,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowHeadersVisible = false
            };
            dgvStudents.SelectionChanged += DgvStudents_SelectionChanged;

            // Selected count label
            lblSelectedCount = new Label
            {
                Text = "Selected: 0",
                Location = new Point(20, 500),
                AutoSize = true,
                ForeColor = Color.FromArgb(100, 116, 139)
            };

            // Buttons
            btnSelect = new Button
            {
                Text = "Select",
                Location = new Point(580, 495),
                Size = new Size(80, 30),
                BackColor = Color.FromArgb(34, 197, 94),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Enabled = false
            };
            btnSelect.Click += BtnSelect_Click;

            btnCancel = new Button
            {
                Text = "Cancel",
                Location = new Point(680, 495),
                Size = new Size(80, 30),
                BackColor = Color.FromArgb(107, 114, 128),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnCancel.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };

            this.Controls.AddRange(new Control[] {
                lblTitle,
                pnlFilters,
                dgvStudents,
                lblSelectedCount,
                btnSelect,
                btnCancel
            });

            this.ResumeLayout(false);
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
                    dgvStudents.Columns["GradeLevel"].HeaderText = "Year Level";
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

            PopulateComboFromColumn(cmbYearFilter, studentsTable, "GradeLevel", "All");
            PopulateComboFromColumn(cmbCourseFilter, studentsTable, "Course", "All");
            PopulateComboFromColumn(cmbSectionFilter, studentsTable, "Section", "All");
        }

        private void PopulateComboFromColumn(ComboBox? comboBox, DataTable source, string columnName, string allLabel = "All")
        {
            if (comboBox == null || !source.Columns.Contains(columnName))
            {
                return;
            }

            var currentValue = comboBox.SelectedItem?.ToString();
            comboBox.Items.Clear();
            comboBox.Items.Add(allLabel);

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
                var filterParts = new List<string>();

                if (cmbYearFilter != null && cmbYearFilter.SelectedIndex > 0)
                {
                    var year = cmbYearFilter.SelectedItem?.ToString()?.Replace("'", "''");
                    if (!string.IsNullOrEmpty(year))
                    {
                        filterParts.Add($"GradeLevel = '{year}'");
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
            if (cmbYearFilter != null) cmbYearFilter.SelectedIndex = 0;
            if (cmbSectionFilter != null) cmbSectionFilter.SelectedIndex = 0;
            if (cmbCourseFilter != null) cmbCourseFilter.SelectedIndex = 0;
        }

        private void DgvStudents_SelectionChanged(object? sender, EventArgs e)
        {
            selectedStudentIds.Clear();
            foreach (DataGridViewRow row in dgvStudents.SelectedRows)
            {
                if (row.Cells["Id"].Value != null)
                {
                    selectedStudentIds.Add(Convert.ToInt32(row.Cells["Id"].Value));
                }
            }

            if (lblSelectedCount != null)
            {
                lblSelectedCount.Text = $"Selected: {selectedStudentIds.Count}";
            }

            if (btnSelect != null)
            {
                btnSelect.Enabled = selectedStudentIds.Count > 0;
            }
        }

        private void BtnSelect_Click(object? sender, EventArgs e)
        {
            if (selectedStudentIds.Count == 0)
            {
                MessageBox.Show("Please select at least one student.", "Validation",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult = DialogResult.OK;
            Close();
        }

        private async Task<int?> ResolveBranchFilterAsync()
        {
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
                return null;
            }

            var branchId = await BranchHelper.GetUserBranchIdAsync(currentUser.Id);
            return branchId > 0 ? branchId : (int?)null;
        }
    }
}


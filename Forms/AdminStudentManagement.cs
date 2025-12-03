using StudentReportInitial.Models;
using StudentReportInitial.Data;
using System.Data.SqlClient;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using StudentReportInitial.Theming;

namespace StudentReportInitial.Forms
{
    public partial class AdminStudentManagement : UserControl
    {
        private DataGridView dgvStudents = null!;
        private Button btnAddStudent = null!;
        private Button btnEditStudent = null!;
        private Button btnDeleteStudent = null!;
        private Panel pnlStudentForm = null!;
        private Panel? pnlStudentFilters;
        private bool isEditMode = false;
        private int selectedStudentId = -1;
        private User? currentUser;
        private int? branchFilterId = null;
        private ComboBox? cmbStudentSectionFilter;
        private ComboBox? cmbStudentCourseFilter;
        private ComboBox? cmbStudentYearFilter;
        private Button? btnStudentClearFilters;
        private Label? lblStudentActiveFilters;
        private DataTable? studentsTable;
        private const string FilterAllLabel = "All";

        public AdminStudentManagement(User? user = null, int? branchId = null)
        {
            currentUser = user;
            branchFilterId = branchId;
            InitializeComponent();
            ApplyModernStyling();
            ThemeManager.ApplyTheme(this);
            LoadStudentsAsync();
            LoadGuardians();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // Main container
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.FromArgb(248, 250, 252);
            this.AutoScroll = true;

            // Search panel removed

            // Action buttons panel
            var pnlActions = new Panel
            {
                Dock = DockStyle.Top,
                Height = 50,
                BackColor = Color.White,
                Padding = new Padding(20, 10, 20, 10)
            };

            btnAddStudent = new Button
            {
                Text = "Add Student",
                Size = new Size(90, 28),
                Location = new Point(20, 10),
                BackColor = Color.FromArgb(34, 197, 94),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9),
                Cursor = Cursors.Hand
            };
            btnAddStudent.Click += BtnAddStudent_Click;

            btnEditStudent = new Button
            {
                Text = "Edit Student",
                Size = new Size(90, 28),
                Location = new Point(120, 10),
                BackColor = Color.FromArgb(59, 130, 246),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9),
                Cursor = Cursors.Hand
            };
            btnEditStudent.Click += BtnEditStudent_Click;

            btnDeleteStudent = new Button
            {
                Text = "Delete Student",
                Size = new Size(90, 28),
                Location = new Point(220, 10),
                BackColor = Color.FromArgb(239, 68, 68),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9),
                Cursor = Cursors.Hand
            };
            btnDeleteStudent.Click += BtnDeleteStudent_Click;

            pnlActions.Controls.AddRange(new Control[] { btnAddStudent, btnEditStudent, btnDeleteStudent });

            var pnlFilters = BuildStudentFiltersPanel();

            // Data grid view
            dgvStudents = new DataGridView
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
            dgvStudents.SelectionChanged += DgvStudents_SelectionChanged;

            // Student form panel (initially hidden)
            pnlStudentForm = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Visible = false,
                Padding = new Padding(20),
                AutoScroll = true
            };

            pnlStudentFilters = pnlFilters;

            this.Controls.Add(dgvStudents);
            this.Controls.Add(pnlStudentForm);
            this.Controls.Add(pnlFilters);
            this.Controls.Add(pnlActions);

            UIStyleHelper.ApplyRoundedButton(btnAddStudent, 10);
            UIStyleHelper.ApplyRoundedButton(btnEditStudent, 10);
            UIStyleHelper.ApplyRoundedButton(btnDeleteStudent, 10);

            this.ResumeLayout(false);
        }

        private Panel BuildStudentFiltersPanel()
        {
            var panel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 95,
                BackColor = Color.White,
                Padding = new Padding(20, 8, 20, 8)
            };

            var flow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoSize = false,
                WrapContents = false,
                FlowDirection = FlowDirection.LeftToRight,
                AutoScroll = true
            };

            cmbStudentSectionFilter = CreateFilterComboBox();
            cmbStudentCourseFilter = CreateFilterComboBox();
            cmbStudentYearFilter = CreateFilterComboBox();

            btnStudentClearFilters = new Button
            {
                Text = "Clear Filters",
                Width = 120,
                Height = 32,
                BackColor = Color.FromArgb(241, 245, 249),
                ForeColor = Color.FromArgb(51, 65, 85),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Enabled = false
            };
            btnStudentClearFilters.FlatAppearance.BorderColor = Color.FromArgb(226, 232, 240);
            btnStudentClearFilters.Click += BtnStudentClearFilters_Click;

            flow.Controls.Add(CreateFilterGroup("Section", cmbStudentSectionFilter));
            flow.Controls.Add(CreateFilterGroup("Course", cmbStudentCourseFilter));
            flow.Controls.Add(CreateFilterGroup("Year Level", cmbStudentYearFilter));
            flow.Controls.Add(btnStudentClearFilters);

            lblStudentActiveFilters = new Label
            {
                Dock = DockStyle.Bottom,
                Height = 20,
                Text = "Active filters: None",
                ForeColor = Color.FromArgb(100, 116, 139)
            };

            panel.Controls.Add(flow);
            panel.Controls.Add(lblStudentActiveFilters);

            InitializeFilterCombo(cmbStudentSectionFilter);
            InitializeFilterCombo(cmbStudentCourseFilter);
            InitializeFilterCombo(cmbStudentYearFilter);

            return panel;
        }

        private static ComboBox CreateFilterComboBox()
        {
            return new ComboBox
            {
                Width = 170,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9)
            };
        }

        private Control CreateFilterGroup(string labelText, ComboBox? comboBox)
        {
            var wrapper = new Panel
            {
                Width = 190,
                Height = 60,
                Margin = new Padding(0, 0, 16, 0)
            };

            var label = new Label
            {
                Text = labelText,
                Dock = DockStyle.Top,
                Height = 18,
                ForeColor = Color.FromArgb(71, 85, 105)
            };

            if (comboBox != null)
            {
                comboBox.Location = new Point(0, 25);
                comboBox.Width = wrapper.Width;
                comboBox.SelectedIndexChanged += (s, e) => ApplyStudentFilters();
            }

            wrapper.Controls.Add(comboBox ?? new ComboBox());
            wrapper.Controls.Add(label);

            return wrapper;
        }

        private void InitializeFilterCombo(ComboBox? comboBox)
        {
            if (comboBox == null)
            {
                return;
            }

            comboBox.Items.Clear();
            comboBox.Items.Add(FilterAllLabel);
            comboBox.SelectedIndex = 0;
        }

        private void BtnStudentClearFilters_Click(object? sender, EventArgs e)
        {
            if (cmbStudentSectionFilter != null) cmbStudentSectionFilter.SelectedIndex = 0;
            if (cmbStudentCourseFilter != null) cmbStudentCourseFilter.SelectedIndex = 0;
            if (cmbStudentYearFilter != null) cmbStudentYearFilter.SelectedIndex = 0;
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
                else if (control is TextBox textBox)
                {
                    textBox.Font = font;
                    textBox.BorderStyle = BorderStyle.FixedSingle;
                }
                else if (control is ComboBox comboBox)
                {
                    comboBox.Font = font;
                }
            }

            dgvStudents.Font = font;
            dgvStudents.ColumnHeadersDefaultCellStyle.Font = headerFont;
            dgvStudents.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
            dgvStudents.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(51, 65, 85);
            dgvStudents.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
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

        private void PopulateStudentFilterOptions()
        {
            if (studentsTable == null)
            {
                return;
            }

            PopulateComboFromColumn(cmbStudentSectionFilter, studentsTable, "Section");
            PopulateComboFromColumn(cmbStudentCourseFilter, studentsTable, "Course");
            PopulateComboFromColumn(cmbStudentYearFilter, studentsTable, "GradeLevel");
        }

        private void PopulateComboFromColumn(ComboBox? comboBox, DataTable table, string columnName)
        {
            if (comboBox == null || !table.Columns.Contains(columnName))
            {
                return;
            }

            var currentValue = comboBox.SelectedItem?.ToString();
            comboBox.Items.Clear();
            comboBox.Items.Add(FilterAllLabel);

            var values = table.AsEnumerable()
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

        private void ApplyStudentFilters()
        {
            if (studentsTable == null)
            {
                return;
            }

            var filterParts = new List<string>();

            if (cmbStudentSectionFilter != null && cmbStudentSectionFilter.SelectedIndex > 0)
            {
                filterParts.Add($"Section = '{EscapeRowFilterValue(cmbStudentSectionFilter.SelectedItem?.ToString())}'");
            }

            if (cmbStudentCourseFilter != null && cmbStudentCourseFilter.SelectedIndex > 0)
            {
                filterParts.Add($"Course = '{EscapeRowFilterValue(cmbStudentCourseFilter.SelectedItem?.ToString())}'");
            }

            if (cmbStudentYearFilter != null && cmbStudentYearFilter.SelectedIndex > 0)
            {
                filterParts.Add($"GradeLevel = '{EscapeRowFilterValue(cmbStudentYearFilter.SelectedItem?.ToString())}'");
            }

            studentsTable.DefaultView.RowFilter = filterParts.Count > 0
                ? string.Join(" AND ", filterParts)
                : string.Empty;

            UpdateStudentFilterState(filterParts.Count > 0);
        }

        private void UpdateStudentFilterState(bool hasFilters)
        {
            var activeFilters = new List<string>();

            if (cmbStudentSectionFilter != null && cmbStudentSectionFilter.SelectedIndex > 0)
            {
                activeFilters.Add($"Section={cmbStudentSectionFilter.SelectedItem}");
            }

            if (cmbStudentCourseFilter != null && cmbStudentCourseFilter.SelectedIndex > 0)
            {
                activeFilters.Add($"Course={cmbStudentCourseFilter.SelectedItem}");
            }

            if (cmbStudentYearFilter != null && cmbStudentYearFilter.SelectedIndex > 0)
            {
                activeFilters.Add($"Year={cmbStudentYearFilter.SelectedItem}");
            }

            if (lblStudentActiveFilters != null)
            {
                lblStudentActiveFilters.Text = activeFilters.Count == 0
                    ? "Active filters: None"
                    : $"Active filters: {string.Join(" | ", activeFilters)}";
            }

            if (btnStudentClearFilters != null)
            {
                btnStudentClearFilters.Enabled = hasFilters;
            }
        }

        private static string EscapeRowFilterValue(string? value)
        {
            return (value ?? string.Empty).Replace("'", "''");
        }

        private void SetStudentFormVisibility(bool showForm)
        {
            pnlStudentForm.Visible = showForm;
            dgvStudents.Visible = !showForm;
            if (pnlStudentFilters != null)
            {
                pnlStudentFilters.Visible = !showForm;
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

        private async Task LoadStudentsAsync()
        {
            try
            {
                using var connection = DatabaseHelper.GetConnection();
                await connection.OpenAsync();

                var query = @"
                    SELECT s.Id, s.StudentId, s.FirstName, s.LastName, s.DateOfBirth, s.Gender, 
                           s.GradeLevel, s.Section, s.Email, s.Phone, s.EnrollmentDate, s.IsActive,
                           u.FirstName + ' ' + u.LastName as GuardianName,
                           b.Name as BranchName
                    FROM Students s
                    INNER JOIN Users u ON s.GuardianId = u.Id
                    LEFT JOIN Branches b ON s.BranchId = b.Id
                    WHERE s.IsActive = 1";

                // Add branch filter if not Super Admin
                if (currentUser != null)
                {
                    var isSuperAdmin = await BranchHelper.IsSuperAdminAsync(currentUser.Id);
                    if (!isSuperAdmin)
                    {
                        var branchId = await BranchHelper.GetUserBranchIdAsync(currentUser.Id);
                        if (branchId > 0)
                        {
                            query += " AND s.BranchId = @branchId";
                        }
                    }
                    else if (branchFilterId.HasValue)
                    {
                        // Super Admin with branch filter selected
                        query += " AND s.BranchId = @branchId";
                    }
                }

                query += " ORDER BY s.EnrollmentDate DESC";

                using var command = new SqlCommand(query, connection);
                
                // Add branch parameter if needed
                if (currentUser != null)
                {
                    var isSuperAdmin = await BranchHelper.IsSuperAdminAsync(currentUser.Id);
                    if (!isSuperAdmin)
                    {
                        var branchId = await BranchHelper.GetUserBranchIdAsync(currentUser.Id);
                        if (branchId > 0)
                        {
                            command.Parameters.AddWithValue("@branchId", branchId);
                        }
                    }
                    else if (branchFilterId.HasValue)
                    {
                        command.Parameters.AddWithValue("@branchId", branchFilterId.Value);
                    }
                }

                using var adapter = new SqlDataAdapter(command);
                var dataTable = new DataTable();
                adapter.Fill(dataTable);

                EnhanceStudentsDataTable(dataTable);
                studentsTable = dataTable;
                dgvStudents.DataSource = studentsTable;
                PopulateStudentFilterOptions();
                ApplyStudentFilters();

                // Format columns
                if (dgvStudents.Columns.Count > 0)
                {
                    dgvStudents.Columns["Id"].Visible = false;
                    dgvStudents.Columns["StudentId"].HeaderText = "Student ID";
                    dgvStudents.Columns["FirstName"].HeaderText = "First Name";
                    dgvStudents.Columns["LastName"].HeaderText = "Last Name";
                    dgvStudents.Columns["DateOfBirth"].HeaderText = "Date of Birth";
                    dgvStudents.Columns["Gender"].HeaderText = "Gender";
                    dgvStudents.Columns["GradeLevel"].HeaderText = "Grade";
                    dgvStudents.Columns["Section"].HeaderText = "Section";
                    dgvStudents.Columns["Email"].HeaderText = "Email";
                    dgvStudents.Columns["Phone"].HeaderText = "Phone";
                    dgvStudents.Columns["EnrollmentDate"].HeaderText = "Enrollment Date";
                    dgvStudents.Columns["GuardianName"].HeaderText = "Guardian";
                    dgvStudents.Columns["IsActive"].HeaderText = "Active";
                    if (dgvStudents.Columns.Contains("BranchName"))
                    {
                        dgvStudents.Columns["BranchName"].HeaderText = "Branch";
                    }

                    dgvStudents.Columns["DateOfBirth"].DefaultCellStyle.Format = "MM/dd/yyyy";
                    dgvStudents.Columns["EnrollmentDate"].DefaultCellStyle.Format = "MM/dd/yyyy";
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading students: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void LoadGuardians()
        {
            try
            {
                using var connection = DatabaseHelper.GetConnection();
                await connection.OpenAsync();

                var query = "SELECT Id, FirstName + ' ' + LastName as FullName FROM Users WHERE Role = 3 AND IsActive = 1";
                using var command = new SqlCommand(query, connection);
                using var reader = await command.ExecuteReaderAsync();

                // Store guardians for later use in forms
                // This would be used when creating the student form
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading guardians: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }




        private void DgvStudents_SelectionChanged(object sender, EventArgs e)
        {
            btnEditStudent.Enabled = btnDeleteStudent.Enabled = dgvStudents.SelectedRows.Count > 0;
        }

        private void BtnAddStudent_Click(object sender, EventArgs e)
        {
            ShowStudentForm();
        }

        private void BtnEditStudent_Click(object sender, EventArgs e)
        {
            if (dgvStudents.SelectedRows.Count > 0)
            {
                selectedStudentId = Convert.ToInt32(dgvStudents.SelectedRows[0].Cells["Id"].Value);
                ShowStudentForm(selectedStudentId);
            }
        }

        private async void BtnDeleteStudent_Click(object sender, EventArgs e)
        {
            if (dgvStudents.SelectedRows.Count == 0)
            {
                return;
            }

            var selectedRow = dgvStudents.SelectedRows[0];
            var studentId = Convert.ToInt32(selectedRow.Cells["Id"].Value);
            var studentName = $"{selectedRow.Cells["FirstName"].Value} {selectedRow.Cells["LastName"].Value}";
            var studentIdValue = selectedRow.Cells["StudentId"].Value?.ToString() ?? "";

            var confirmMessage = $"WARNING: You are about to delete student '{studentName}' (ID: {studentIdValue}).\n\n" +
                               "This will:\n" +
                               "• Deactivate the student account\n" +
                               "• Deactivate the associated user login\n" +
                               "• This action cannot be undone\n\n" +
                               "Are you absolutely sure you want to proceed?";

            var result = MessageBox.Show(confirmMessage, "Confirm Student Deletion", 
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                try
                {
                    await DeleteStudentAsync(studentId);
                    await LoadStudentsAsync();
                    MessageBox.Show("Student deleted successfully. Statistics will be updated when you refresh the System Reports panel.", 
                        "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting student: {ex.Message}", "Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private async void ShowStudentForm(int studentId = -1)
        {
            pnlStudentForm.Controls.Clear();
            SetStudentFormVisibility(true);

            isEditMode = studentId > 0;
            selectedStudentId = studentId;

            // Create a scrollable panel for the form content
            var scrollPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(10)
            };

            var lblTitle = new Label
            {
                Text = isEditMode ? "Edit Student" : "Add New Student",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(51, 65, 85),
                AutoSize = true,
                Location = new Point(20, 20)
            };

            var yPos = 50;
            var spacing = 40;

            // Student ID
            var lblStudentId = new Label { Text = "Student ID:", Location = new Point(20, yPos), AutoSize = true };
            var txtStudentId = new TextBox { Location = new Point(20, yPos + 20), Size = new Size(250, 25) };
            
            // Auto-generate student ID for new students
            if (!isEditMode)
            {
                txtStudentId.Text = GenerateStudentId();
                txtStudentId.ReadOnly = false; // Allow manual override
            }
            yPos += spacing;

            // First Name
            var lblFirstName = new Label { Text = "First Name:", Location = new Point(20, yPos), AutoSize = true };
            var txtFirstName = new TextBox { Location = new Point(20, yPos + 20), Size = new Size(250, 25) };
            yPos += spacing;

            // Last Name
            var lblLastName = new Label { Text = "Last Name:", Location = new Point(20, yPos), AutoSize = true };
            var txtLastName = new TextBox { Location = new Point(20, yPos + 20), Size = new Size(250, 25) };
            yPos += spacing;

            // Date of Birth
            var lblDateOfBirth = new Label { Text = "Date of Birth:", Location = new Point(20, yPos), AutoSize = true };
            var dtpDateOfBirth = new DateTimePicker { Location = new Point(20, yPos + 20), Size = new Size(250, 25) };
            yPos += spacing;

            // Gender
            var lblGender = new Label { Text = "Gender:", Location = new Point(20, yPos), AutoSize = true };
            var cmbGender = new ComboBox { Location = new Point(20, yPos + 20), Size = new Size(250, 25), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbGender.Items.AddRange(new[] { "Male", "Female", "Other" });
            yPos += spacing;

			// Grade Level
			var lblGradeLevel = new Label { Text = "Year Level:", Location = new Point(20, yPos), AutoSize = true };
			var cmbGradeLevel = new ComboBox { Location = new Point(20, yPos + 20), Size = new Size(250, 25), DropDownStyle = ComboBoxStyle.DropDownList };
			cmbGradeLevel.Items.AddRange(new[] { "1st Year", "2nd Year", "3rd Year", "4th Year" });
			yPos += spacing;

			// Course
			var lblCourse = new Label { Text = "Course:", Location = new Point(20, yPos), AutoSize = true };
			var cmbCourse = new ComboBox { Location = new Point(20, yPos + 20), Size = new Size(250, 25), DropDownStyle = ComboBoxStyle.DropDownList };
			cmbCourse.Items.AddRange(new[] { "BSIT", "BSHM", "BSTM", "BSBA" });
			yPos += spacing;

			// Section Code (dependent)
			var lblSection = new Label { Text = "Section:", Location = new Point(20, yPos), AutoSize = true };
			var cmbSectionCode = new ComboBox { Location = new Point(20, yPos + 20), Size = new Size(250, 25), DropDownStyle = ComboBoxStyle.DropDownList };
			yPos += spacing;

			void PopulateSectionCodes()
			{
				cmbSectionCode.Items.Clear();
				if (cmbGradeLevel.SelectedItem == null) return;
				var yearIdx = cmbGradeLevel.SelectedIndex + 1;
				cmbSectionCode.Items.AddRange(new[] { $"{yearIdx}A", $"{yearIdx}B", $"{yearIdx}C" });
				if (cmbSectionCode.Items.Count > 0) cmbSectionCode.SelectedIndex = 0;
			}

			cmbGradeLevel.SelectedIndexChanged += (s, e) => PopulateSectionCodes();
			if (cmbGradeLevel.Items.Count > 0) cmbGradeLevel.SelectedIndex = 0;
			if (cmbCourse.Items.Count > 0) cmbCourse.SelectedIndex = 0;

            // Branch selection (for Super Admin when managing all branches)
            ComboBox? cmbBranch = null;
            Label? lblBranch = null;
            if (currentUser != null && !isEditMode)
            {
                var isSuperAdmin = await BranchHelper.IsSuperAdminAsync(currentUser.Id);
                if (isSuperAdmin && !branchFilterId.HasValue) // Only show when managing all branches
                {
                    lblBranch = new Label { Text = "Branch (Required):", Location = new Point(20, yPos), AutoSize = true };
                    cmbBranch = new ComboBox { Location = new Point(20, yPos + 20), Size = new Size(250, 25), DropDownStyle = ComboBoxStyle.DropDownList };
                    
                    var branches = await BranchHelper.GetAllBranchesAsync();
                    foreach (var branch in branches)
                    {
                        cmbBranch.Items.Add(branch);
                    }
                    if (cmbBranch.Items.Count > 0)
                    {
                        cmbBranch.SelectedIndex = 0;
                    }
                    yPos += spacing;
                }
            }

            // Email
            var lblEmail = new Label { Text = "Email:", Location = new Point(20, yPos), AutoSize = true };
            var txtEmail = new TextBox { Location = new Point(20, yPos + 20), Size = new Size(250, 25) };
            yPos += spacing;

            // Phone
            var lblPhone = new Label { Text = "Phone (International format):", Location = new Point(20, yPos), AutoSize = true };
            var txtPhone = new TextBox { Location = new Point(20, yPos + 20), Size = new Size(250, 25), PlaceholderText="+1234567890 or 0XXXXXXXXX" };
            var lblPhoneError = new Label 
            { 
                Text = "", 
                Location = new Point(20, yPos + 48), 
                AutoSize = true,
                ForeColor = Color.FromArgb(239, 68, 68),
                Font = new Font("Segoe UI", 8F),
                Visible = false
            };
            
            // Phone validation on leave
            txtPhone.Leave += (s, e) =>
            {
                string phone = txtPhone.Text.Trim();
                if (!string.IsNullOrEmpty(phone))
                {
                    string errorMsg = PhoneValidator.GetValidationMessage(phone);
                    if (!string.IsNullOrEmpty(errorMsg))
                    {
                        lblPhoneError.Text = errorMsg;
                        lblPhoneError.Visible = true;
                        txtPhone.BackColor = Color.FromArgb(254, 242, 242);
                    }
                    else
                    {
                        lblPhoneError.Visible = false;
                        txtPhone.BackColor = Color.White;
                        // Auto-format valid number
                        txtPhone.Text = PhoneValidator.FormatPhoneNumber(phone);
                    }
                }
                else
                {
                    lblPhoneError.Visible = false;
                    txtPhone.BackColor = Color.White;
                }
            };

            txtPhone.TextChanged += (s, e) =>
            {
                // Clear error when user starts typing
                if (lblPhoneError.Visible)
                {
                    lblPhoneError.Visible = false;
                    txtPhone.BackColor = Color.White;
                }
            };
            
            yPos += spacing + 25;

            // Address
            var lblAddress = new Label { Text = "Address:", Location = new Point(20, yPos), AutoSize = true };
            var txtAddress = new TextBox { Location = new Point(20, yPos + 20), Size = new Size(250, 25) };
            yPos += spacing;

            // Guardian assignment
            var lblGuardian = new Label { Text = "Guardian:", Location = new Point(20, yPos), AutoSize = true };
            yPos += 20;
            
            var btnSelectGuardian = new Button
            {
                Text = "Select Existing Guardian",
                Location = new Point(20, yPos),
                Size = new Size(180, 30),
                BackColor = Color.FromArgb(59, 130, 246),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9),
                Cursor = Cursors.Hand
            };
            
            int? selectedGuardianId = null;
            string? selectedGuardianName = null;
            var lblGuardianStatus = new Label
            {
                Text = "No guardian selected - new guardian will be created",
                Location = new Point(210, yPos + 5),
                Size = new Size(300, 20),
                ForeColor = Color.FromArgb(107, 114, 128),
                Font = new Font("Segoe UI", 9, FontStyle.Italic),
                AutoSize = false
            };
            
            btnSelectGuardian.Click += async (s, e) =>
            {
                using var guardianForm = new GuardianSelectionForm(currentUser, branchFilterId);
                if (guardianForm.ShowDialog() == DialogResult.OK && guardianForm.SelectedGuardianId.HasValue)
                {
                    selectedGuardianId = guardianForm.SelectedGuardianId.Value;
                    selectedGuardianName = guardianForm.SelectedGuardianName;
                    lblGuardianStatus.Text = $"Selected: {selectedGuardianName}";
                    lblGuardianStatus.ForeColor = Color.FromArgb(34, 197, 94);
                    lblGuardianStatus.Font = new Font("Segoe UI", 9, FontStyle.Regular);
                }
            };
            
            yPos += spacing + 10;

            // Buttons
            var btnSave = new Button
            {
                Text = "Save",
                Location = new Point(20, yPos + 20),
                Size = new Size(90, 28),
                BackColor = Color.FromArgb(34, 197, 94),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9),
                Cursor = Cursors.Hand
            };

            var btnCancel = new Button
            {
                Text = "Cancel",
                Location = new Point(120, yPos + 20),
                Size = new Size(90, 28),
                BackColor = Color.FromArgb(107, 114, 128),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9),
                Cursor = Cursors.Hand
            };

            btnCancel.Click += (s, e) => {
                SetStudentFormVisibility(false);
            };
            btnSave.Click += async (s, e) =>
            {
                try
                {
                    // Validate phone number (mandatory)
                    string phone = txtPhone.Text.Trim();
                    if (string.IsNullOrEmpty(phone))
                    {
                        lblPhoneError.Text = "Phone number is required.";
                        lblPhoneError.Visible = true;
                        txtPhone.BackColor = Color.FromArgb(254, 242, 242);
                        txtPhone.Focus();
                        return;
                    }

                    string phoneError = PhoneValidator.GetValidationMessage(phone);
                    if (!string.IsNullOrEmpty(phoneError))
                    {
                        lblPhoneError.Text = phoneError;
                        lblPhoneError.Visible = true;
                        txtPhone.BackColor = Color.FromArgb(254, 242, 242);
                        txtPhone.Focus();
                        return;
                    }
                    // Format phone number before saving
                    phone = PhoneValidator.FormatPhoneNumber(phone);

                    // For new students, verify phone number with OTP
                    if (!isEditMode)
                    {
                        string otpCode = SmsService.GenerateOtp();
                        bool otpSent = await SmsService.SendOtpAsync(phone, otpCode);

                        if (!otpSent)
                        {
                            MessageBox.Show("Failed to send verification code. Please check the phone number and try again.",
                                "Verification Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }

                        using var otpForm = new OtpVerificationForm(phone, otpCode);
                        if (otpForm.ShowDialog() != DialogResult.OK || !otpForm.IsVerified)
                        {
                            MessageBox.Show("Phone number verification is required to continue.",
                                "Verification Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                    }

                    int selectedGuardianIdValue = selectedGuardianId ?? 0;

                    // Determine branch assignment
                    int assignedBranchId = 0;
                    if (currentUser != null)
                    {
                        var isSuperAdmin = await BranchHelper.IsSuperAdminAsync(currentUser.Id);
                        if (isSuperAdmin)
                        {
                            // When managing all branches, use branch from form dropdown
                            if (cmbBranch != null && cmbBranch.SelectedItem is Branch selectedBranch)
                            {
                                assignedBranchId = selectedBranch.Id;
                            }
                            // When specific branch is selected in filter, use that
                            else if (branchFilterId.HasValue)
                            {
                                assignedBranchId = branchFilterId.Value;
                            }
                            else
                            {
                                MessageBox.Show("Please select a branch for this student.", "Validation Error",
                                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                return;
                            }
                        }
                        else
                        {
                            var branchId = await BranchHelper.GetUserBranchIdAsync(currentUser.Id);
                            if (branchId > 0)
                            {
                                assignedBranchId = branchId;
                            }
                        }
                    }
                    
                    if (assignedBranchId == 0)
                    {
                        MessageBox.Show("Unable to determine branch assignment. Please ensure a branch is selected.", "Validation Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    var student = new Student
                    {
                        StudentId = txtStudentId.Text,
                        FirstName = txtFirstName.Text,
                        LastName = txtLastName.Text,
                        DateOfBirth = dtpDateOfBirth.Value,
                        Gender = cmbGender.SelectedItem?.ToString() ?? "",
						GradeLevel = cmbGradeLevel.SelectedItem?.ToString() ?? "",
						Section = (cmbCourse.SelectedItem != null && cmbSectionCode.SelectedItem != null) ? $"{cmbCourse.SelectedItem}-{cmbSectionCode.SelectedItem}" : "",
                        Email = txtEmail.Text,
                        Phone = phone,
                        Address = txtAddress.Text,
                        GuardianId = selectedGuardianIdValue, // Use selected guardian or 0 to create new
                        BranchId = assignedBranchId,
                        IsActive = true
                    };

                    if (isEditMode)
                    {
                        student.Id = selectedStudentId;
                        await UpdateStudentAsync(student);
                    }
                    else
                    {
                        await AddStudentAsync(student);
                    }

                    SetStudentFormVisibility(false);
                    await LoadStudentsAsync();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error saving student: {ex.Message}", "Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            // Add all controls to the scrollable panel
            var controlsList = new List<Control> {
                lblTitle, lblStudentId, txtStudentId, lblFirstName, txtFirstName, lblLastName, txtLastName,
				lblDateOfBirth, dtpDateOfBirth, lblGender, cmbGender, lblGradeLevel, cmbGradeLevel,
				lblCourse, cmbCourse, lblSection, cmbSectionCode, lblEmail, txtEmail, lblPhone, txtPhone, lblPhoneError, lblAddress, txtAddress,
                lblGuardian, btnSelectGuardian, lblGuardianStatus, btnSave, btnCancel
            };
            
            // Add branch controls if they exist
            if (lblBranch != null && cmbBranch != null)
            {
                controlsList.Add(lblBranch);
                controlsList.Add(cmbBranch);
            }
            
			scrollPanel.Controls.AddRange(controlsList.ToArray());

            // Add the scrollable panel to the main form panel
            pnlStudentForm.Controls.Add(scrollPanel);

            ThemeManager.ApplyTheme(pnlStudentForm);

            // Load student data if editing
            if (isEditMode)
            {
				var (guardianId, guardianName) = await LoadStudentDataAsync(selectedStudentId, txtStudentId, txtFirstName, txtLastName, dtpDateOfBirth, 
					cmbGender, cmbGradeLevel, cmbSectionCode, txtEmail, txtPhone, txtAddress, lblGuardianStatus);
                selectedGuardianId = guardianId;
                selectedGuardianName = guardianName;
            }
        }


		private async Task<(int? guardianId, string? guardianName)> LoadStudentDataAsync(int studentId, TextBox txtStudentId, TextBox txtFirstName, 
			TextBox txtLastName, DateTimePicker dtpDateOfBirth, ComboBox cmbGender, ComboBox cmbGradeLevel,
			ComboBox cmbSectionCode, TextBox txtEmail, TextBox txtPhone, TextBox txtAddress, Label lblGuardianStatus)
        {
            int? guardianId = null;
            string? guardianName = null;
            
            try
            {
                using var connection = DatabaseHelper.GetConnection();
                await connection.OpenAsync();

                var query = @"
                    SELECT s.*, u.FirstName + ' ' + u.LastName as GuardianName
                    FROM Students s
                    LEFT JOIN Users u ON s.GuardianId = u.Id
                    WHERE s.Id = @id";
                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@id", studentId);

                using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    txtStudentId.Text = reader.GetString("StudentId");
                    txtFirstName.Text = reader.GetString("FirstName");
                    txtLastName.Text = reader.GetString("LastName");
                    dtpDateOfBirth.Value = reader.GetDateTime("DateOfBirth");
                    cmbGender.SelectedItem = reader.GetString("Gender");
					cmbGradeLevel.SelectedItem = reader.GetString("GradeLevel");
					var sectionValue = reader.GetString("Section"); // e.g., IT-1A
					var coursePart = sectionValue.Contains('-') ? sectionValue.Split('-')[0] : "";
					var codePart = sectionValue.Contains('-') ? sectionValue.Split('-')[1] : sectionValue;
					// Ensure section codes reflect grade
					if (cmbGradeLevel.SelectedIndex >= 0)
					{
						var yearIdx = cmbGradeLevel.SelectedIndex + 1;
						cmbSectionCode.Items.Clear();
						cmbSectionCode.Items.AddRange(new[] { $"{yearIdx}A", $"{yearIdx}B", $"{yearIdx}C" });
					}
					if (cmbSectionCode.Items.Contains(codePart))
					{
						cmbSectionCode.SelectedItem = codePart;
					}
                    txtEmail.Text = reader.IsDBNull("Email") ? "" : reader.GetString("Email");
                    txtPhone.Text = reader.IsDBNull("Phone") ? "" : reader.GetString("Phone");
                    txtAddress.Text = reader.IsDBNull("Address") ? "" : reader.GetString("Address");
                    
                    // Set guardian selection
                    if (!reader.IsDBNull("GuardianId"))
                    {
                        guardianId = reader.GetInt32("GuardianId");
                        guardianName = reader.IsDBNull("GuardianName") ? null : reader.GetString("GuardianName");
                        if (guardianName != null)
                        {
                            lblGuardianStatus.Text = $"Selected: {guardianName}";
                            lblGuardianStatus.ForeColor = Color.FromArgb(34, 197, 94);
                            lblGuardianStatus.Font = new Font("Segoe UI", 9, FontStyle.Regular);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading student data: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            
            return (guardianId, guardianName);
        }

        private async Task AddStudentAsync(Student student)
        {
            using var connection = DatabaseHelper.GetConnection();
            await connection.OpenAsync();
            using var transaction = connection.BeginTransaction();

            try
            {
                // Ensure student has branch assigned
                if (student.BranchId == 0)
                {
                    // Get branch from current user or use default
                    if (currentUser != null)
                    {
                        var isSuperAdmin = await BranchHelper.IsSuperAdminAsync(currentUser.Id);
                        if (!isSuperAdmin)
                        {
                            student.BranchId = await BranchHelper.GetUserBranchIdAsync(currentUser.Id);
                        }
                        else if (branchFilterId.HasValue)
                        {
                            student.BranchId = branchFilterId.Value;
                        }
                    }
                    
                    // Default to Baliuag if still 0
                    if (student.BranchId == 0)
                    {
                        var defaultBranchQuery = "SELECT TOP 1 Id FROM Branches WHERE Code = 'BALIUAG'";
                        using var defaultBranchCommand = new SqlCommand(defaultBranchQuery, connection, transaction);
                        var defaultBranchResult = await defaultBranchCommand.ExecuteScalarAsync();
                        if (defaultBranchResult != null)
                        {
                            student.BranchId = Convert.ToInt32(defaultBranchResult);
                        }
                    }
                }

                // Generate student credentials with branch-specific email
                var studentUsername = await GenerateStudentUsernameAsync(student);
                var studentPassword = GenerateStudentPassword(student);
                PasswordHasher.CreatePasswordHash(studentPassword, out string studentPasswordHash, out string studentPasswordSalt);

                // First, create the student account in the Users table
                var studentUserId = await AccountHelper.CreateStudentAccountAsync(connection, transaction, student);

                // Create guardian account only if no guardian was selected (GuardianId is 0)
                int guardianId = student.GuardianId;
                string guardianUsername = "";
                string guardianPassword = "";
                
                if (guardianId == 0)
                {
                    guardianId = await CreateGuardianAccountAsync(connection, transaction, student);
                    guardianUsername = await GenerateGuardianUsernameAsync(student);
                    guardianPassword = GenerateGuardianPassword(student);
                }
                else
                {
                    // Get existing guardian info for display
                    var guardianQuery = "SELECT Username FROM Users WHERE Id = @id";
                    using var guardianCommand = new SqlCommand(guardianQuery, connection, transaction);
                    guardianCommand.Parameters.AddWithValue("@id", guardianId);
                    var result = await guardianCommand.ExecuteScalarAsync();
                    guardianUsername = result?.ToString() ?? "N/A";
                }

                // Finally, create the student record in the Students table
                var query = @"
                    INSERT INTO Students (StudentId, FirstName, LastName, DateOfBirth, Gender, GradeLevel, 
                                        Section, Email, Phone, Address, GuardianId, BranchId, IsActive, EnrollmentDate,
                                        Username, PasswordHash, PasswordSalt)
                    VALUES (@studentId, @firstName, @lastName, @dateOfBirth, @gender, @gradeLevel, 
                            @section, @email, @phone, @address, @guardianId, @branchId, @isActive, @enrollmentDate,
                            @username, @passwordHash, @passwordSalt)";

                using var command = new SqlCommand(query, connection, transaction);
                command.Parameters.AddWithValue("@studentId", student.StudentId);
                command.Parameters.AddWithValue("@firstName", student.FirstName);
                command.Parameters.AddWithValue("@lastName", student.LastName);
                command.Parameters.AddWithValue("@dateOfBirth", student.DateOfBirth);
                command.Parameters.AddWithValue("@gender", student.Gender);
                command.Parameters.AddWithValue("@gradeLevel", student.GradeLevel);
                command.Parameters.AddWithValue("@section", student.Section);
                command.Parameters.AddWithValue("@email", student.Email);
                command.Parameters.AddWithValue("@phone", student.Phone);
                command.Parameters.AddWithValue("@address", student.Address);
                command.Parameters.AddWithValue("@guardianId", guardianId);
                command.Parameters.AddWithValue("@branchId", student.BranchId);
                command.Parameters.AddWithValue("@isActive", student.IsActive);
                command.Parameters.AddWithValue("@enrollmentDate", DateTime.Now);
                command.Parameters.AddWithValue("@username", studentUsername);
                command.Parameters.AddWithValue("@passwordHash", studentPasswordHash);
                command.Parameters.AddWithValue("@passwordSalt", studentPasswordSalt);

                await command.ExecuteNonQueryAsync();

                transaction.Commit();

                // Show success message
                string message = $"Student added successfully!\n\n" +
                    $"Student account created:\n" +
                    $"Username: {studentUsername}\n" +
                    $"Password: {studentPassword}\n\n";
                
                if (student.GuardianId == 0)
                {
                    message += $"Guardian account created:\n" +
                        $"Username: {guardianUsername}\n" +
                        $"Password: {guardianPassword}\n\n" +
                        "Please provide these credentials to both the student and guardian.";
                }
                else
                {
                    message += $"Assigned to existing guardian: {guardianUsername}";
                }
                
                MessageBox.Show(message, "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                throw new Exception($"Error adding student: {ex.Message}", ex);
            }
        }

        private async Task UpdateStudentAsync(Student student)
        {
            using var connection = DatabaseHelper.GetConnection();
            await connection.OpenAsync();

            // Validate branch access before updating
            if (currentUser != null)
            {
                var isSuperAdmin = await BranchHelper.IsSuperAdminAsync(currentUser.Id);
                if (!isSuperAdmin)
                {
                    // Check if student being updated belongs to current admin's branch
                    var checkQuery = "SELECT BranchId FROM Students WHERE Id = @id";
                    using var checkCommand = new SqlCommand(checkQuery, connection);
                    checkCommand.Parameters.AddWithValue("@id", student.Id);
                    var studentBranchId = await checkCommand.ExecuteScalarAsync();
                    
                    var adminBranchId = await BranchHelper.GetUserBranchIdAsync(currentUser.Id);
                    if (studentBranchId == null || studentBranchId == DBNull.Value || 
                        Convert.ToInt32(studentBranchId) != adminBranchId)
                    {
                        throw new UnauthorizedAccessException("You can only update students from your own branch.");
                    }
                }
                else if (branchFilterId.HasValue)
                {
                    // Super Admin with branch filter - ensure student belongs to selected branch
                    var checkQuery = "SELECT BranchId FROM Students WHERE Id = @id";
                    using var checkCommand = new SqlCommand(checkQuery, connection);
                    checkCommand.Parameters.AddWithValue("@id", student.Id);
                    var studentBranchId = await checkCommand.ExecuteScalarAsync();
                    
                    if (studentBranchId == null || studentBranchId == DBNull.Value || 
                        Convert.ToInt32(studentBranchId) != branchFilterId.Value)
                    {
                        throw new UnauthorizedAccessException("Student does not belong to the selected branch.");
                    }
                }
            }

            var query = @"
                UPDATE Students 
                SET StudentId = @studentId, FirstName = @firstName, LastName = @lastName, 
                    DateOfBirth = @dateOfBirth, Gender = @gender, GradeLevel = @gradeLevel,
                    Section = @section, Email = @email, Phone = @phone, Address = @address
                WHERE Id = @id";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@id", student.Id);
            command.Parameters.AddWithValue("@studentId", student.StudentId);
            command.Parameters.AddWithValue("@firstName", student.FirstName);
            command.Parameters.AddWithValue("@lastName", student.LastName);
            command.Parameters.AddWithValue("@dateOfBirth", student.DateOfBirth);
            command.Parameters.AddWithValue("@gender", student.Gender);
            command.Parameters.AddWithValue("@gradeLevel", student.GradeLevel);
            command.Parameters.AddWithValue("@section", student.Section);
            command.Parameters.AddWithValue("@email", student.Email);
            command.Parameters.AddWithValue("@phone", student.Phone);
            command.Parameters.AddWithValue("@address", student.Address);

            await command.ExecuteNonQueryAsync();
        }

        private async Task DeleteStudentAsync(int studentId)
        {
            using var connection = DatabaseHelper.GetConnection();
            await connection.OpenAsync();
            using var transaction = connection.BeginTransaction();

            try
            {
                // Validate branch access before deleting
                if (currentUser != null)
                {
                    var isSuperAdmin = await BranchHelper.IsSuperAdminAsync(currentUser.Id);
                    if (!isSuperAdmin)
                    {
                        // Check if student being deleted belongs to current admin's branch
                        var checkQuery = "SELECT BranchId FROM Students WHERE Id = @id";
                        using var checkCommand = new SqlCommand(checkQuery, connection, transaction);
                        checkCommand.Parameters.AddWithValue("@id", studentId);
                        var studentBranchId = await checkCommand.ExecuteScalarAsync();
                        
                        var adminBranchId = await BranchHelper.GetUserBranchIdAsync(currentUser.Id);
                        if (studentBranchId == null || studentBranchId == DBNull.Value || 
                            Convert.ToInt32(studentBranchId) != adminBranchId)
                        {
                            transaction.Rollback();
                            throw new UnauthorizedAccessException("You can only delete students from your own branch.");
                        }
                    }
                    else if (branchFilterId.HasValue)
                    {
                        // Super Admin with branch filter - ensure student belongs to selected branch
                        var checkQuery = "SELECT BranchId FROM Students WHERE Id = @id";
                        using var checkCommand = new SqlCommand(checkQuery, connection, transaction);
                        checkCommand.Parameters.AddWithValue("@id", studentId);
                        var studentBranchId = await checkCommand.ExecuteScalarAsync();
                        
                        if (studentBranchId == null || studentBranchId == DBNull.Value || 
                            Convert.ToInt32(studentBranchId) != branchFilterId.Value)
                        {
                            transaction.Rollback();
                            throw new UnauthorizedAccessException("Student does not belong to the selected branch.");
                        }
                    }
                }

                // Get student info to find associated user account
                var studentQuery = "SELECT StudentId, Email FROM Students WHERE Id = @id";
                using var studentCommand = new SqlCommand(studentQuery, connection, transaction);
                studentCommand.Parameters.AddWithValue("@id", studentId);
                
                string? studentIdValue = null;
                string? studentEmail = null;
                using (var reader = await studentCommand.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        studentIdValue = reader.GetString("StudentId");
                        if (!reader.IsDBNull("Email"))
                        {
                            studentEmail = reader.GetString("Email");
                        }
                    }
                }

                // Deactivate the student record
                var deleteStudentQuery = "UPDATE Students SET IsActive = 0 WHERE Id = @id";
                using var deleteStudentCommand = new SqlCommand(deleteStudentQuery, connection, transaction);
                deleteStudentCommand.Parameters.AddWithValue("@id", studentId);
                await deleteStudentCommand.ExecuteNonQueryAsync();

                // Also deactivate the associated user account (student login account)
                // Try matching by username first (format: studentnumber@baliuag.sti.edu.ph)
                if (!string.IsNullOrEmpty(studentIdValue))
                {
                    var studentNumber = studentIdValue.Replace("-", "");
                    var studentUsername = $"{studentNumber}@baliuag.sti.edu.ph";

                    var deleteUserQuery = "UPDATE Users SET IsActive = 0 WHERE (Username = @username OR Email = @email) AND Role = 4";
                    using var deleteUserCommand = new SqlCommand(deleteUserQuery, connection, transaction);
                    deleteUserCommand.Parameters.AddWithValue("@username", studentUsername);
                    deleteUserCommand.Parameters.AddWithValue("@email", studentEmail ?? studentUsername);
                    await deleteUserCommand.ExecuteNonQueryAsync();
                }

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        private async Task<int> CreateGuardianAccountAsync(SqlConnection connection, SqlTransaction transaction, Student student)
        {
            var guardianUsername = await GenerateGuardianUsernameAsync(student);
            var guardianPassword = GenerateGuardianPassword(student);
            var guardianFirstName = $"Guardian of {student.FirstName}";
            var guardianLastName = student.LastName;
            var guardianEmail = await EmailDomainHelper.GenerateGuardianEmailAsync(student.StudentId, student.BranchId);

            // Hash the password
            string passwordHash, passwordSalt;
            PasswordHasher.CreatePasswordHash(guardianPassword, out passwordHash, out passwordSalt);

            var query = @"
                INSERT INTO Users (Username, PasswordHash, PasswordSalt, FirstName, LastName, Email, Phone, Role, BranchId, CreatedDate, IsActive)
                VALUES (@username, @passwordHash, @passwordSalt, @firstName, @lastName, @email, @phone, @role, @branchId, @createdDate, @isActive);
                SELECT SCOPE_IDENTITY();";

            using var command = new SqlCommand(query, connection, transaction);
            command.Parameters.AddWithValue("@username", guardianUsername);
            command.Parameters.AddWithValue("@passwordHash", passwordHash);
            command.Parameters.AddWithValue("@passwordSalt", passwordSalt);
            command.Parameters.AddWithValue("@firstName", guardianFirstName);
            command.Parameters.AddWithValue("@lastName", guardianLastName);
            command.Parameters.AddWithValue("@email", guardianEmail);
            command.Parameters.AddWithValue("@phone", student.Phone);
            command.Parameters.AddWithValue("@role", 3); // Guardian
            command.Parameters.AddWithValue("@branchId", student.BranchId); // Assign guardian to same branch as student
            command.Parameters.AddWithValue("@createdDate", DateTime.Now);
            command.Parameters.AddWithValue("@isActive", true);

            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }


        private async Task<string> GenerateGuardianUsernameAsync(Student student)
        {
            // Generate username like: guardian.[studentID]@[branch-domain].sti.edu.ph
            return await EmailDomainHelper.GenerateGuardianEmailAsync(student.StudentId, student.BranchId);
        }

        private string GenerateGuardianPassword(Student student)
        {
            // Generate password like: Guardian@STU001!2024
            var year = DateTime.Now.Year;
            var studentId = student.StudentId.Replace(" ", "");
            return $"Guardian@{studentId}!{year}";
        }

        private async Task<string> GenerateStudentUsernameAsync(Student student)
        {
            // Format: studentnumber@[branch-domain].sti.edu.ph
            var studentNumber = student.StudentId.Replace("-", ""); // Remove dashes from student ID
            var email = await EmailDomainHelper.GenerateStudentEmailAsync(studentNumber, student.BranchId);
            return email;
        }

        private string GenerateStudentPassword(Student student)
        {
            // Format: studentnumber@LastName
            return $"{student.StudentId}@{student.LastName}";
        }

        private string GenerateStudentId()
        {
            // Generate student ID like: 24-0001 (year-sequence)
            var year = DateTime.Now.Year.ToString().Substring(2);
            var sequence = GetNextStudentSequence();
            return $"{year}-{sequence:D4}";
        }

        private int GetNextStudentSequence()
        {
            try
            {
                using var connection = DatabaseHelper.GetConnection();
                connection.Open();
                
                var query = "SELECT COUNT(*) FROM Students WHERE YEAR(EnrollmentDate) = @year";
                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@year", DateTime.Now.Year);
                
                var count = Convert.ToInt32(command.ExecuteScalar());
                return count + 1;
            }
            catch
            {
                return 1; // Default to 1 if there's an error
            }
        }
    }
}

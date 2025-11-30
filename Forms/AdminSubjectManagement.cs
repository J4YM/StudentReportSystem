using StudentReportInitial.Models;
using StudentReportInitial.Data;
using System.Data.SqlClient;
using System.Data;

namespace StudentReportInitial.Forms
{
    public partial class AdminSubjectManagement : UserControl
    {
        private DataGridView dgvSubjects;
        private Button btnAddSubject;
        private Button btnEditSubject;
        private Button btnDeleteSubject;
        private Button btnRefresh;
        private Panel pnlSubjectForm;
        private bool isEditMode = false;
        private int selectedSubjectId = -1;
		private ComboBox? cmbSubjectsGradeFilter;
		private ComboBox? cmbSubjectsSectionFilter;
        private User? currentUser;
        private int? branchFilterId = null;

        public AdminSubjectManagement(User? user = null, int? branchId = null)
        {
            currentUser = user;
            branchFilterId = branchId;
            InitializeComponent();
            ApplyModernStyling();
            LoadSubjects();
            LoadProfessors();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // Main container
            this.Size = new Size(1000, 600);
            this.BackColor = Color.FromArgb(248, 250, 252);

			// Action buttons panel
            var pnlActions = new Panel
            {
                Dock = DockStyle.Top,
				Height = 90,
                BackColor = Color.White,
                Padding = new Padding(20, 10, 20, 10)
            };

            btnAddSubject = new Button
            {
                Text = "Add Subject",
                Size = new Size(100, 30),
                Location = new Point(20, 10),
                BackColor = Color.FromArgb(34, 197, 94),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                
                Cursor = Cursors.Hand
            };
            btnAddSubject.Click += BtnAddSubject_Click;

            btnEditSubject = new Button
            {
                Text = "Edit Subject",
                Size = new Size(100, 30),
                Location = new Point(130, 10),
                BackColor = Color.FromArgb(59, 130, 246),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                
                Cursor = Cursors.Hand
            };
            btnEditSubject.Click += BtnEditSubject_Click;

            btnDeleteSubject = new Button
            {
                Text = "Delete Subject",
                Size = new Size(100, 30),
                Location = new Point(240, 10),
                BackColor = Color.FromArgb(239, 68, 68),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                
                Cursor = Cursors.Hand
            };
            btnDeleteSubject.Click += BtnDeleteSubject_Click;

            btnRefresh = new Button
            {
                Text = "Refresh",
                Size = new Size(80, 30),
                Location = new Point(350, 10),
                BackColor = Color.FromArgb(59, 130, 246),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                
                Cursor = Cursors.Hand
            };
            btnRefresh.Click += BtnRefresh_Click;

			// Filters row
			var lblFilterGrade = new Label { Text = "Grade:", Location = new Point(450, 15), AutoSize = true };
			cmbSubjectsGradeFilter = new ComboBox { Location = new Point(500, 12), Size = new Size(130, 25), DropDownStyle = ComboBoxStyle.DropDownList };
			cmbSubjectsGradeFilter.Items.AddRange(new[] { "All", "1st Year", "2nd Year", "3rd Year", "4th Year" });
			cmbSubjectsGradeFilter.SelectedIndex = 0;
			cmbSubjectsGradeFilter.SelectedIndexChanged += (s, e) => ApplySubjectsGridFilter();

			var lblFilterSection = new Label { Text = "Section:", Location = new Point(650, 15), AutoSize = true };
			cmbSubjectsSectionFilter = new ComboBox { Location = new Point(715, 12), Size = new Size(150, 25), DropDownStyle = ComboBoxStyle.DropDownList };
			cmbSubjectsSectionFilter.Items.AddRange(new[] { "All", "1A", "1B", "1C", "2A", "2B", "2C", "3A", "3B", "3C", "4A", "4B", "4C" });
			cmbSubjectsSectionFilter.SelectedIndex = 0;
			cmbSubjectsSectionFilter.SelectedIndexChanged += (s, e) => ApplySubjectsGridFilter();

			pnlActions.Controls.AddRange(new Control[] { btnAddSubject, btnEditSubject, btnDeleteSubject, btnRefresh, lblFilterGrade, cmbSubjectsGradeFilter, lblFilterSection, cmbSubjectsSectionFilter });

            // Data grid view
            dgvSubjects = new DataGridView
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
            dgvSubjects.SelectionChanged += DgvSubjects_SelectionChanged;

            // Subject form panel (initially hidden)
            pnlSubjectForm = new Panel
            {
                Dock = DockStyle.Right,
                Width = 400,
                BackColor = Color.White,
                Visible = false,
                Padding = new Padding(20)
            };

            this.Controls.Add(dgvSubjects);
            this.Controls.Add(pnlSubjectForm);
            this.Controls.Add(pnlActions);

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
            }

            dgvSubjects.Font = font;
            dgvSubjects.ColumnHeadersDefaultCellStyle.Font = headerFont;
            dgvSubjects.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
            dgvSubjects.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(51, 65, 85);
            dgvSubjects.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
        }

        private async void LoadSubjects()
        {
            try
            {
                using var connection = DatabaseHelper.GetConnection();
                await connection.OpenAsync();

                var query = @"
                    SELECT s.Id, s.Name, s.Code, s.Description, s.GradeLevel, s.Section, s.IsActive,
                           COALESCE(u.FirstName + ' ' + u.LastName, 'Unassigned') as ProfessorName,
                           b.Name as BranchName
                    FROM Subjects s
                    LEFT JOIN Users u ON s.ProfessorId = u.Id
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

                query += " ORDER BY s.Name";

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

                dgvSubjects.DataSource = dataTable;

                // Format columns
                if (dgvSubjects.Columns.Count > 0)
                {
                    dgvSubjects.Columns["Id"].Visible = false;
                    dgvSubjects.Columns["Name"].HeaderText = "Subject Name";
                    dgvSubjects.Columns["Code"].HeaderText = "Code";
                    dgvSubjects.Columns["Description"].HeaderText = "Description";
                    dgvSubjects.Columns["GradeLevel"].HeaderText = "Grade Level";
                    dgvSubjects.Columns["Section"].HeaderText = "Section";
                    dgvSubjects.Columns["ProfessorName"].HeaderText = "Professor";
                    dgvSubjects.Columns["IsActive"].HeaderText = "Active";
                    if (dgvSubjects.Columns.Contains("BranchName"))
                    {
                        dgvSubjects.Columns["BranchName"].HeaderText = "Branch";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading subjects: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void LoadProfessors()
        {
            // This will be used when creating the subject form
        }

        private void DgvSubjects_SelectionChanged(object sender, EventArgs e)
        {
            btnEditSubject.Enabled = btnDeleteSubject.Enabled = dgvSubjects.SelectedRows.Count > 0;
        }

        private void BtnAddSubject_Click(object sender, EventArgs e)
        {
            ShowSubjectForm();
        }

        private void BtnEditSubject_Click(object sender, EventArgs e)
        {
            if (dgvSubjects.SelectedRows.Count > 0)
            {
                selectedSubjectId = Convert.ToInt32(dgvSubjects.SelectedRows[0].Cells["Id"].Value);
                ShowSubjectForm(selectedSubjectId);
            }
        }

        private async void BtnDeleteSubject_Click(object sender, EventArgs e)
        {
            if (dgvSubjects.SelectedRows.Count == 0)
            {
                return;
            }

            var selectedRow = dgvSubjects.SelectedRows[0];
            var subjectId = Convert.ToInt32(selectedRow.Cells["Id"].Value);
            var subjectName = selectedRow.Cells["Name"].Value?.ToString() ?? "Unknown";
            var subjectCode = selectedRow.Cells["Code"].Value?.ToString() ?? "";

            var confirmMessage = $"WARNING: You are about to delete subject '{subjectName}' ({subjectCode}).\n\n" +
                               "This will:\n" +
                               "• Deactivate the subject\n" +
                               "• Students will no longer be able to enroll in this subject\n" +
                               "• This action cannot be undone\n\n" +
                               "Are you absolutely sure you want to proceed?";

            var result = MessageBox.Show(confirmMessage, "Confirm Subject Deletion", 
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                try
                {
                    await DeleteSubjectAsync(subjectId);
                    LoadSubjects();
                    MessageBox.Show("Subject deleted successfully.", 
                        "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting subject: {ex.Message}", "Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            LoadSubjects();
        }

        private void ShowSubjectForm(int subjectId = -1)
        {
            pnlSubjectForm.Controls.Clear();
            pnlSubjectForm.Visible = true;

            isEditMode = subjectId > 0;
            selectedSubjectId = subjectId;

            var lblTitle = new Label
            {
                Text = isEditMode ? "Edit Subject" : "Add New Subject",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(51, 65, 85),
                AutoSize = true,
                Location = new Point(20, 20)
            };

            var yPos = 60;
            var spacing = 50;

            // Subject Name
            var lblName = new Label { Text = "Subject Name:", Location = new Point(20, yPos), AutoSize = true };
            var txtName = new TextBox { Location = new Point(20, yPos + 20), Size = new Size(300, 25) };
            yPos += spacing;

            // Code
            var lblCode = new Label { Text = "Code:", Location = new Point(20, yPos), AutoSize = true };
            var txtCode = new TextBox { Location = new Point(20, yPos + 20), Size = new Size(300, 25) };
            yPos += spacing;

            // Description
            var lblDescription = new Label { Text = "Description:", Location = new Point(20, yPos), AutoSize = true };
            var txtDescription = new TextBox { Location = new Point(20, yPos + 20), Size = new Size(300, 25) };
            yPos += spacing;

			// Grade Level
			var lblGradeLevel = new Label { Text = "Year Level:", Location = new Point(20, yPos), AutoSize = true };
			var cmbGradeLevel = new ComboBox { Location = new Point(20, yPos + 20), Size = new Size(300, 25), DropDownStyle = ComboBoxStyle.DropDownList };
			cmbGradeLevel.Items.AddRange(new[] { "1st Year", "2nd Year", "3rd Year", "4th Year" });
			yPos += spacing;

			// Course
			var lblCourse = new Label { Text = "Course:", Location = new Point(20, yPos), AutoSize = true };
			var cmbCourse = new ComboBox { Location = new Point(20, yPos + 20), Size = new Size(300, 25), DropDownStyle = ComboBoxStyle.DropDownList };
			cmbCourse.Items.AddRange(new[] { "BSIT", "BSHM", "BSTM", "BSBA" });
			yPos += spacing;

			// Section Code (dependent on year level)
			var lblSection = new Label { Text = "Section:", Location = new Point(20, yPos), AutoSize = true };
			var cmbSectionCode = new ComboBox { Location = new Point(20, yPos + 20), Size = new Size(300, 25), DropDownStyle = ComboBoxStyle.DropDownList };
			yPos += spacing;

			void PopulateSectionCodes()
			{
				cmbSectionCode.Items.Clear();
				if (cmbGradeLevel.SelectedItem == null)
				{
					return;
				}
				var yearIdx = cmbGradeLevel.SelectedIndex + 1; // 1..4
				var options = new[] { $"{yearIdx}A", $"{yearIdx}B", $"{yearIdx}C" };
				cmbSectionCode.Items.AddRange(options);
				if (cmbSectionCode.Items.Count > 0) cmbSectionCode.SelectedIndex = 0;
			}

			cmbGradeLevel.SelectedIndexChanged += (s, e) => PopulateSectionCodes();
			if (cmbGradeLevel.Items.Count > 0) { cmbGradeLevel.SelectedIndex = 0; }
			if (cmbCourse.Items.Count > 0) { cmbCourse.SelectedIndex = 0; }

            // Professor
            var lblProfessor = new Label { Text = "Professor:", Location = new Point(20, yPos), AutoSize = true };
            var cmbProfessor = new ComboBox { Location = new Point(20, yPos + 20), Size = new Size(300, 25), DropDownStyle = ComboBoxStyle.DropDownList };
            yPos += spacing;

            // Load professors
            LoadProfessorsForComboBox(cmbProfessor);

            // Buttons
            var btnSave = new Button
            {
                Text = "Save",
                Location = new Point(20, yPos + 20),
                Size = new Size(100, 30),
                BackColor = Color.FromArgb(34, 197, 94),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                
                Cursor = Cursors.Hand
            };

            var btnCancel = new Button
            {
                Text = "Cancel",
                Location = new Point(130, yPos + 20),
                Size = new Size(100, 30),
                BackColor = Color.FromArgb(107, 114, 128),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                
                Cursor = Cursors.Hand
            };

            btnCancel.Click += (s, e) => pnlSubjectForm.Visible = false;
			btnSave.Click += async (s, e) =>
            {
                try
                {
					var composedSection = string.Empty;
					if (cmbCourse.SelectedItem != null && cmbSectionCode.SelectedItem != null)
					{
						composedSection = $"{cmbCourse.SelectedItem}-{cmbSectionCode.SelectedItem}";
					}
                    // Determine branch assignment
                    int assignedBranchId = 0;
                    if (currentUser != null)
                    {
                        var isSuperAdmin = await BranchHelper.IsSuperAdminAsync(currentUser.Id);
                        if (isSuperAdmin && branchFilterId.HasValue)
                        {
                            assignedBranchId = branchFilterId.Value;
                        }
                        else if (!isSuperAdmin)
                        {
                            var branchId = await BranchHelper.GetUserBranchIdAsync(currentUser.Id);
                            if (branchId > 0)
                            {
                                assignedBranchId = branchId;
                            }
                        }
                    }

                    var subject = new Subject
                    {
                        Name = txtName.Text,
                        Code = txtCode.Text,
                        Description = txtDescription.Text,
						GradeLevel = cmbGradeLevel.SelectedItem?.ToString() ?? "",
						Section = composedSection,
                        ProfessorId = Convert.ToInt32(cmbProfessor.SelectedValue),
                        BranchId = assignedBranchId,
                        IsActive = true
                    };

                    if (isEditMode)
                    {
                        subject.Id = selectedSubjectId;
                        await UpdateSubjectAsync(subject);
                    }
                    else
                    {
                        await AddSubjectAsync(subject);
                    }

                    pnlSubjectForm.Visible = false;
                    LoadSubjects();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error saving subject: {ex.Message}", "Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

			pnlSubjectForm.Controls.AddRange(new Control[] {
				lblTitle, lblName, txtName, lblCode, txtCode, lblDescription, txtDescription,
				lblGradeLevel, cmbGradeLevel, lblCourse, cmbCourse, lblSection, cmbSectionCode, lblProfessor, cmbProfessor,
				btnSave, btnCancel
			});

            // Load subject data if editing
            if (isEditMode)
            {
				LoadSubjectData(selectedSubjectId, txtName, txtCode, txtDescription, cmbGradeLevel, cmbSectionCode, cmbProfessor, cmbCourse);
            }
        }

        private async void LoadProfessorsForComboBox(ComboBox cmbProfessor)
        {
            try
            {
                using var connection = DatabaseHelper.GetConnection();
                await connection.OpenAsync();

                var query = "SELECT Id, FirstName + ' ' + LastName as FullName FROM Users WHERE Role = 2 AND IsActive = 1";
                using var command = new SqlCommand(query, connection);
                using var adapter = new SqlDataAdapter(command);
                var dataTable = new DataTable();
                adapter.Fill(dataTable);

                cmbProfessor.DataSource = dataTable;
                cmbProfessor.DisplayMember = "FullName";
                cmbProfessor.ValueMember = "Id";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading professors: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

		private async void LoadSubjectData(int subjectId, TextBox txtName, TextBox txtCode, TextBox txtDescription,
			ComboBox cmbGradeLevel, ComboBox cmbSectionCode, ComboBox cmbProfessor, ComboBox cmbCourse)
        {
            try
            {
                using var connection = DatabaseHelper.GetConnection();
                await connection.OpenAsync();

                var query = "SELECT * FROM Subjects WHERE Id = @id";
                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@id", subjectId);

                using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    txtName.Text = reader.GetString("Name");
                    txtCode.Text = reader.GetString("Code");
                    txtDescription.Text = reader.IsDBNull("Description") ? "" : reader.GetString("Description");
					cmbGradeLevel.SelectedItem = reader.GetString("GradeLevel");
					var sectionVal = reader.GetString("Section");
					// Expecting format COURSE-<nX>
					var coursePart = sectionVal.Contains('-') ? sectionVal.Split('-')[0] : "";
					var codePart = sectionVal.Contains('-') ? sectionVal.Split('-')[1] : sectionVal;
					// Ensure section codes reflect current grade selection
					if (cmbGradeLevel.SelectedIndex >= 0)
					{
						// repopulate codes based on grade
						// same helper as in form setup
						var yearIdx = cmbGradeLevel.SelectedIndex + 1;
						cmbSectionCode.Items.Clear();
						cmbSectionCode.Items.AddRange(new[] { $"{yearIdx}A", $"{yearIdx}B", $"{yearIdx}C" });
					}
					if (!string.IsNullOrEmpty(coursePart))
					{
						cmbCourse.SelectedItem = coursePart;
					}
					if (!string.IsNullOrEmpty(codePart) && cmbSectionCode.Items.Contains(codePart))
					{
						cmbSectionCode.SelectedItem = codePart;
					}
                    cmbProfessor.SelectedValue = reader.GetInt32("ProfessorId");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading subject data: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

		private void ApplySubjectsGridFilter()
		{
			if (dgvSubjects.DataSource is DataTable dataTable)
			{
				var filters = new List<string>();
				if (cmbSubjectsGradeFilter != null && cmbSubjectsGradeFilter.SelectedIndex > 0)
				{
					var grade = cmbSubjectsGradeFilter.SelectedItem?.ToString() ?? string.Empty;
					filters.Add($"GradeLevel = '{grade.Replace("'", "''")}'");
				}
				if (cmbSubjectsSectionFilter != null && cmbSubjectsSectionFilter.SelectedIndex > 0)
				{
					var sectionCode = cmbSubjectsSectionFilter.SelectedItem?.ToString() ?? string.Empty;
					// Section stored like COURSE-1A, so match endswith section code
					filters.Add($"Section LIKE '%-{sectionCode.Replace("'", "''")}'");
				}
				dataTable.DefaultView.RowFilter = string.Join(" AND ", filters);
			}
		}

        private async Task AddSubjectAsync(Subject subject)
        {
            using var connection = DatabaseHelper.GetConnection();
            await connection.OpenAsync();

            var query = @"
                INSERT INTO Subjects (Name, Code, Description, GradeLevel, Section, ProfessorId, BranchId, IsActive)
                VALUES (@name, @code, @description, @gradeLevel, @section, @professorId, @branchId, @isActive)";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@name", subject.Name);
            command.Parameters.AddWithValue("@code", subject.Code);
            command.Parameters.AddWithValue("@description", subject.Description);
            command.Parameters.AddWithValue("@gradeLevel", subject.GradeLevel);
            command.Parameters.AddWithValue("@section", subject.Section);
            command.Parameters.AddWithValue("@professorId", subject.ProfessorId);
            command.Parameters.AddWithValue("@branchId", subject.BranchId);
            command.Parameters.AddWithValue("@isActive", subject.IsActive);

            await command.ExecuteNonQueryAsync();
        }

        private async Task UpdateSubjectAsync(Subject subject)
        {
            using var connection = DatabaseHelper.GetConnection();
            await connection.OpenAsync();

            var query = @"
                UPDATE Subjects 
                SET Name = @name, Code = @code, Description = @description, 
                    GradeLevel = @gradeLevel, Section = @section, ProfessorId = @professorId
                WHERE Id = @id";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@id", subject.Id);
            command.Parameters.AddWithValue("@name", subject.Name);
            command.Parameters.AddWithValue("@code", subject.Code);
            command.Parameters.AddWithValue("@description", subject.Description);
            command.Parameters.AddWithValue("@gradeLevel", subject.GradeLevel);
            command.Parameters.AddWithValue("@section", subject.Section);
            command.Parameters.AddWithValue("@professorId", subject.ProfessorId);

            await command.ExecuteNonQueryAsync();
        }

        private async Task DeleteSubjectAsync(int subjectId)
        {
            using var connection = DatabaseHelper.GetConnection();
            await connection.OpenAsync();

            var query = "UPDATE Subjects SET IsActive = 0 WHERE Id = @id";
            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@id", subjectId);

            await command.ExecuteNonQueryAsync();
        }
    }
}

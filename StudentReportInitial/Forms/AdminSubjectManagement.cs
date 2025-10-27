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

        public AdminSubjectManagement()
        {
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
                Height = 50,
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

            pnlActions.Controls.AddRange(new Control[] { btnAddSubject, btnEditSubject, btnDeleteSubject, btnRefresh });

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
                           u.FirstName + ' ' + u.LastName as ProfessorName
                    FROM Subjects s
                    INNER JOIN Users u ON s.ProfessorId = u.Id
                    ORDER BY s.Name";

                using var command = new SqlCommand(query, connection);
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
            if (dgvSubjects.SelectedRows.Count > 0)
            {
                var result = MessageBox.Show("Are you sure you want to delete this subject?", "Confirm Delete", 
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    try
                    {
                        var subjectId = Convert.ToInt32(dgvSubjects.SelectedRows[0].Cells["Id"].Value);
                        await DeleteSubjectAsync(subjectId);
                        LoadSubjects();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error deleting subject: {ex.Message}", "Error", 
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
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
            var lblGradeLevel = new Label { Text = "Grade Level:", Location = new Point(20, yPos), AutoSize = true };
            var cmbGradeLevel = new ComboBox { Location = new Point(20, yPos + 20), Size = new Size(300, 25), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbGradeLevel.Items.AddRange(new[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12" });
            yPos += spacing;

            // Section
            var lblSection = new Label { Text = "Section:", Location = new Point(20, yPos), AutoSize = true };
            var txtSection = new TextBox { Location = new Point(20, yPos + 20), Size = new Size(300, 25) };
            yPos += spacing;

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
                    var subject = new Subject
                    {
                        Name = txtName.Text,
                        Code = txtCode.Text,
                        Description = txtDescription.Text,
                        GradeLevel = cmbGradeLevel.SelectedItem?.ToString() ?? "",
                        Section = txtSection.Text,
                        ProfessorId = Convert.ToInt32(cmbProfessor.SelectedValue),
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
                lblGradeLevel, cmbGradeLevel, lblSection, txtSection, lblProfessor, cmbProfessor,
                btnSave, btnCancel
            });

            // Load subject data if editing
            if (isEditMode)
            {
                LoadSubjectData(selectedSubjectId, txtName, txtCode, txtDescription, cmbGradeLevel, txtSection, cmbProfessor);
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
            ComboBox cmbGradeLevel, TextBox txtSection, ComboBox cmbProfessor)
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
                    txtSection.Text = reader.GetString("Section");
                    cmbProfessor.SelectedValue = reader.GetInt32("ProfessorId");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading subject data: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task AddSubjectAsync(Subject subject)
        {
            using var connection = DatabaseHelper.GetConnection();
            await connection.OpenAsync();

            var query = @"
                INSERT INTO Subjects (Name, Code, Description, GradeLevel, Section, ProfessorId, IsActive)
                VALUES (@name, @code, @description, @gradeLevel, @section, @professorId, @isActive)";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@name", subject.Name);
            command.Parameters.AddWithValue("@code", subject.Code);
            command.Parameters.AddWithValue("@description", subject.Description);
            command.Parameters.AddWithValue("@gradeLevel", subject.GradeLevel);
            command.Parameters.AddWithValue("@section", subject.Section);
            command.Parameters.AddWithValue("@professorId", subject.ProfessorId);
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

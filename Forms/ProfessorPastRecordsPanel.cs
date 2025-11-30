using StudentReportInitial.Models;
using StudentReportInitial.Data;
using System.Data.SqlClient;
using System.Data;

namespace StudentReportInitial.Forms
{
    public partial class ProfessorPastRecordsPanel : UserControl
    {
        private User currentProfessor;
        private DataGridView dgvRecords;
        private ComboBox cmbRecordType;
        private ComboBox cmbSubject;
        private ComboBox cmbQuarter;
        private ComboBox cmbStudent;
        private ComboBox cmbCourse;
        private ComboBox cmbSection;
        private Button btnRefresh;
        private Button btnEdit;
        private Button btnDelete;
        private TabControl tabControl;

        public ProfessorPastRecordsPanel(User professor)
        {
            currentProfessor = professor;
            InitializeComponent();
            ApplyModernStyling();
            LoadSubjects();
            LoadStudents();
            LoadCourses();
            LoadSections();
            LoadRecords();
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
                Height = 160,
                BackColor = Color.White,
                Padding = new Padding(20)
            };

            var lblTitle = new Label
            {
                Text = "Past Records Management",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(51, 65, 85),
                AutoSize = true,
                Location = new Point(20, 20)
            };

            // Record type filter
            var lblRecordType = new Label
            {
                Text = "Record Type:",
                Location = new Point(20, 50),
                AutoSize = true
            };

            cmbRecordType = new ComboBox
            {
                Location = new Point(120, 48),
                Size = new Size(150, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbRecordType.Items.AddRange(new[] { "Grades", "Attendance" });
            cmbRecordType.SelectedIndex = 0;
            cmbRecordType.SelectedIndexChanged += CmbRecordType_SelectedIndexChanged;

            // Subject filter
            var lblSubject = new Label
            {
                Text = "Subject:",
                Location = new Point(290, 50),
                AutoSize = true
            };

            cmbSubject = new ComboBox
            {
                Location = new Point(350, 48),
                Size = new Size(200, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbSubject.Items.Add("All Subjects");
            cmbSubject.SelectedIndex = 0;
            cmbSubject.SelectedIndexChanged += CmbSubject_SelectedIndexChanged;

            // Quarter filter (for grades)
            var lblQuarter = new Label
            {
                Text = "Quarter:",
                Location = new Point(570, 50),
                AutoSize = true
            };

            cmbQuarter = new ComboBox
            {
                Location = new Point(630, 48),
                Size = new Size(100, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbQuarter.Items.Add("All Quarters");
            cmbQuarter.Items.AddRange(new[] { "Prelim", "Midterm", "PreFinal", "Final" });
            cmbQuarter.SelectedIndex = 0;
            cmbQuarter.SelectedIndexChanged += CmbQuarter_SelectedIndexChanged;

            // Student filter
            var lblStudent = new Label
            {
                Text = "Student:",
                Location = new Point(20, 80),
                AutoSize = true
            };

            cmbStudent = new ComboBox
            {
                Location = new Point(80, 78),
                Size = new Size(200, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbStudent.Items.Add("All Students");
            cmbStudent.SelectedIndex = 0;
            cmbStudent.SelectedIndexChanged += CmbStudent_SelectedIndexChanged;

            // Course filter
            var lblCourse = new Label
            {
                Text = "Course:",
                Location = new Point(300, 80),
                AutoSize = true
            };

            cmbCourse = new ComboBox
            {
                Location = new Point(360, 78),
                Size = new Size(150, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbCourse.Items.Add("All Courses");
            cmbCourse.SelectedIndex = 0;
            cmbCourse.SelectedIndexChanged += CmbCourse_SelectedIndexChanged;

            // Section filter
            var lblSection = new Label
            {
                Text = "Section:",
                Location = new Point(530, 80),
                AutoSize = true
            };

            cmbSection = new ComboBox
            {
                Location = new Point(590, 78),
                Size = new Size(100, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbSection.Items.Add("All Sections");
            cmbSection.SelectedIndex = 0;
            cmbSection.SelectedIndexChanged += CmbSection_SelectedIndexChanged;

            btnRefresh = new Button
            {
                Text = "Refresh",
                Location = new Point(750, 77),
                Size = new Size(80, 27),
                BackColor = Color.FromArgb(59, 130, 246),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnRefresh.Click += BtnRefresh_Click;

            pnlHeader.Controls.AddRange(new Control[] {
                lblTitle, lblRecordType, cmbRecordType, lblSubject, cmbSubject,
                lblQuarter, cmbQuarter, lblStudent, cmbStudent, lblCourse, cmbCourse,
                lblSection, cmbSection, btnRefresh
            });

            // Action buttons panel
            var pnlActions = new Panel
            {
                Dock = DockStyle.Top,
                Height = 50,
                BackColor = Color.White,
                Padding = new Padding(20, 10, 20, 10)
            };

            btnEdit = new Button
            {
                Text = "Edit",
                Size = new Size(90, 30),
                Location = new Point(20, 10),
                BackColor = Color.FromArgb(59, 130, 246),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnEdit.Click += BtnEdit_Click;

            btnDelete = new Button
            {
                Text = "Delete",
                Size = new Size(90, 30),
                Location = new Point(120, 10),
                BackColor = Color.FromArgb(239, 68, 68),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnDelete.Click += BtnDelete_Click;

            pnlActions.Controls.AddRange(new Control[] { btnEdit, btnDelete });

            // Records grid
            dgvRecords = new DataGridView
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
            dgvRecords.SelectionChanged += DgvRecords_SelectionChanged;

            this.Controls.Add(dgvRecords);
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
            }

            dgvRecords.Font = font;
            dgvRecords.ColumnHeadersDefaultCellStyle.Font = headerFont;
            dgvRecords.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
            dgvRecords.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(51, 65, 85);
            dgvRecords.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
        }

        private async void LoadSubjects()
        {
            try
            {
                cmbSubject.Items.Clear();
                cmbSubject.Items.Add("All Subjects");

                using var connection = DatabaseHelper.GetConnection();
                await connection.OpenAsync();

                var query = @"
                    SELECT DISTINCT s.Name
                    FROM Subjects s
                    WHERE s.ProfessorId = @professorId AND s.IsActive = 1
                    ORDER BY s.Name";

                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@professorId", currentProfessor.Id);

                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    cmbSubject.Items.Add(reader.GetString("Name"));
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

        private async void LoadStudents()
        {
            try
            {
                cmbStudent.Items.Clear();
                cmbStudent.Items.Add("All Students");

                using var connection = DatabaseHelper.GetConnection();
                await connection.OpenAsync();

                var query = @"
                    SELECT DISTINCT s.Id, s.FirstName + ' ' + s.LastName as StudentName
                    FROM Students s
                    INNER JOIN Grades g ON s.Id = g.StudentId
                    WHERE g.ProfessorId = @professorId AND s.IsActive = 1
                    UNION
                    SELECT DISTINCT s.Id, s.FirstName + ' ' + s.LastName as StudentName
                    FROM Students s
                    INNER JOIN Attendance a ON s.Id = a.StudentId
                    WHERE a.ProfessorId = @professorId AND s.IsActive = 1
                    ORDER BY StudentName";

                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@professorId", currentProfessor.Id);

                using var adapter = new SqlDataAdapter(command);
                var dataTable = new DataTable();
                adapter.Fill(dataTable);

                // Add "All Students" row at the beginning
                var allStudentsRow = dataTable.NewRow();
                allStudentsRow["Id"] = 0;
                allStudentsRow["StudentName"] = "All Students";
                dataTable.Rows.InsertAt(allStudentsRow, 0);

                cmbStudent.DisplayMember = "StudentName";
                cmbStudent.ValueMember = "Id";
                cmbStudent.DataSource = dataTable;
                cmbStudent.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading students: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void LoadCourses()
        {
            try
            {
                cmbCourse.Items.Clear();
                cmbCourse.Items.Add("All Courses");

                using var connection = DatabaseHelper.GetConnection();
                await connection.OpenAsync();

                var query = @"
                    SELECT DISTINCT s.GradeLevel
                    FROM Students s
                    INNER JOIN Grades g ON s.Id = g.StudentId
                    WHERE g.ProfessorId = @professorId AND s.IsActive = 1
                    UNION
                    SELECT DISTINCT s.GradeLevel
                    FROM Students s
                    INNER JOIN Attendance a ON s.Id = a.StudentId
                    WHERE a.ProfessorId = @professorId AND s.IsActive = 1
                    ORDER BY s.GradeLevel";

                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@professorId", currentProfessor.Id);

                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    if (!reader.IsDBNull("GradeLevel"))
                    {
                        cmbCourse.Items.Add(reader.GetString("GradeLevel"));
                    }
                }

                if (cmbCourse.Items.Count > 0)
                {
                    cmbCourse.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading courses: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void LoadSections()
        {
            try
            {
                cmbSection.Items.Clear();
                cmbSection.Items.Add("All Sections");

                using var connection = DatabaseHelper.GetConnection();
                await connection.OpenAsync();

                var query = @"
                    SELECT DISTINCT s.Section
                    FROM Students s
                    INNER JOIN Grades g ON s.Id = g.StudentId
                    WHERE g.ProfessorId = @professorId AND s.IsActive = 1 AND s.Section IS NOT NULL AND s.Section != ''
                    UNION
                    SELECT DISTINCT s.Section
                    FROM Students s
                    INNER JOIN Attendance a ON s.Id = a.StudentId
                    WHERE a.ProfessorId = @professorId AND s.IsActive = 1 AND s.Section IS NOT NULL AND s.Section != ''
                    ORDER BY s.Section";

                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@professorId", currentProfessor.Id);

                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    if (!reader.IsDBNull("Section"))
                    {
                        cmbSection.Items.Add(reader.GetString("Section"));
                    }
                }

                if (cmbSection.Items.Count > 0)
                {
                    cmbSection.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading sections: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void LoadRecords()
        {
            try
            {
                using var connection = DatabaseHelper.GetConnection();
                await connection.OpenAsync();

                DataTable dataTable;

                if (cmbRecordType.SelectedItem?.ToString() == "Grades")
                {
                    dataTable = await LoadGradesAsync(connection);
                }
                else
                {
                    dataTable = await LoadAttendanceAsync(connection);
                }

                dgvRecords.DataSource = dataTable;
                FormatDataGridColumns();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading records: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task<DataTable> LoadGradesAsync(SqlConnection connection)
        {
            var query = @"
                SELECT g.Id, g.StudentId, s.FirstName + ' ' + s.LastName as StudentName,
                       s.GradeLevel, s.Section,
                       g.Subject, g.Quarter, g.ComponentType, g.AssignmentName,
                       g.Score, g.MaxScore, g.Percentage, g.Comments,
                       g.DateRecorded, g.DueDate
                FROM Grades g
                INNER JOIN Students s ON g.StudentId = s.Id
                WHERE g.ProfessorId = @professorId AND s.IsActive = 1";

            var parameters = new List<SqlParameter>
            {
                new SqlParameter("@professorId", currentProfessor.Id)
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

            if (cmbStudent.SelectedIndex > 0 && cmbStudent.SelectedValue != null)
            {
                query += " AND g.StudentId = @studentId";
                parameters.Add(new SqlParameter("@studentId", cmbStudent.SelectedValue));
            }

            if (cmbCourse.SelectedIndex > 0)
            {
                query += " AND s.GradeLevel = @course";
                parameters.Add(new SqlParameter("@course", cmbCourse.SelectedItem.ToString()));
            }

            if (cmbSection.SelectedIndex > 0)
            {
                query += " AND s.Section = @section";
                parameters.Add(new SqlParameter("@section", cmbSection.SelectedItem.ToString()));
            }

            query += " ORDER BY g.DateRecorded DESC";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddRange(parameters.ToArray());

            using var adapter = new SqlDataAdapter(command);
            var dataTable = new DataTable();
            adapter.Fill(dataTable);

            return dataTable;
        }

        private async Task<DataTable> LoadAttendanceAsync(SqlConnection connection)
        {
            var query = @"
                SELECT a.Id, a.StudentId, s.FirstName + ' ' + s.LastName as StudentName,
                       s.GradeLevel, s.Section,
                       a.Subject, a.Date, a.Status, a.Notes, a.RecordedDate
                FROM Attendance a
                INNER JOIN Students s ON a.StudentId = s.Id
                WHERE a.ProfessorId = @professorId AND s.IsActive = 1";

            var parameters = new List<SqlParameter>
            {
                new SqlParameter("@professorId", currentProfessor.Id)
            };

            if (cmbSubject.SelectedIndex > 0)
            {
                query += " AND a.Subject = @subject";
                parameters.Add(new SqlParameter("@subject", cmbSubject.SelectedItem.ToString()));
            }

            if (cmbStudent.SelectedIndex > 0 && cmbStudent.SelectedValue != null)
            {
                query += " AND a.StudentId = @studentId";
                parameters.Add(new SqlParameter("@studentId", cmbStudent.SelectedValue));
            }

            if (cmbCourse.SelectedIndex > 0)
            {
                query += " AND s.GradeLevel = @course";
                parameters.Add(new SqlParameter("@course", cmbCourse.SelectedItem.ToString()));
            }

            if (cmbSection.SelectedIndex > 0)
            {
                query += " AND s.Section = @section";
                parameters.Add(new SqlParameter("@section", cmbSection.SelectedItem.ToString()));
            }

            query += " ORDER BY a.Date DESC";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddRange(parameters.ToArray());

            using var adapter = new SqlDataAdapter(command);
            var dataTable = new DataTable();
            adapter.Fill(dataTable);

            return dataTable;
        }

        private void FormatDataGridColumns()
        {
            if (dgvRecords.Columns.Count == 0) return;

            dgvRecords.Columns["Id"].Visible = false;
            dgvRecords.Columns["StudentId"].Visible = false;

            if (cmbRecordType.SelectedItem?.ToString() == "Grades")
            {
                dgvRecords.Columns["StudentName"].HeaderText = "Student";
                dgvRecords.Columns["Subject"].HeaderText = "Subject";
                dgvRecords.Columns["Quarter"].HeaderText = "Quarter";
                dgvRecords.Columns["ComponentType"].HeaderText = "Component";
                dgvRecords.Columns["AssignmentName"].HeaderText = "Assignment";
                dgvRecords.Columns["Score"].HeaderText = "Score";
                dgvRecords.Columns["MaxScore"].HeaderText = "Max Score";
                dgvRecords.Columns["Percentage"].HeaderText = "Percentage";
                dgvRecords.Columns["Comments"].HeaderText = "Comments";
                dgvRecords.Columns["DateRecorded"].HeaderText = "Date Recorded";
                dgvRecords.Columns["DueDate"].HeaderText = "Due Date";

                dgvRecords.Columns["DateRecorded"].DefaultCellStyle.Format = "MM/dd/yyyy";
                dgvRecords.Columns["DueDate"].DefaultCellStyle.Format = "MM/dd/yyyy";
                dgvRecords.Columns["Percentage"].DefaultCellStyle.Format = "0.0'%'";
            }
            else
            {
                dgvRecords.Columns["StudentName"].HeaderText = "Student";
                dgvRecords.Columns["Subject"].HeaderText = "Subject";
                dgvRecords.Columns["Date"].HeaderText = "Date";
                dgvRecords.Columns["Status"].HeaderText = "Status";
                dgvRecords.Columns["Notes"].HeaderText = "Notes";
                dgvRecords.Columns["RecordedDate"].HeaderText = "Recorded Date";

                dgvRecords.Columns["Date"].DefaultCellStyle.Format = "MM/dd/yyyy hh:mm tt";
                dgvRecords.Columns["RecordedDate"].DefaultCellStyle.Format = "MM/dd/yyyy hh:mm tt";
            }
        }

        private void CmbRecordType_SelectedIndexChanged(object sender, EventArgs e)
        {
            cmbQuarter.Visible = cmbRecordType.SelectedItem?.ToString() == "Grades";
            LoadRecords();
        }

        private void CmbSubject_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadRecords();
        }

        private void CmbQuarter_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadRecords();
        }

        private void CmbStudent_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadRecords();
        }

        private void CmbCourse_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadRecords();
        }

        private void CmbSection_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadRecords();
        }

        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            LoadRecords();
        }

        private void DgvRecords_SelectionChanged(object sender, EventArgs e)
        {
            btnEdit.Enabled = btnDelete.Enabled = dgvRecords.SelectedRows.Count > 0;
        }

        private async void BtnEdit_Click(object sender, EventArgs e)
        {
            if (dgvRecords.SelectedRows.Count == 0) return;

            try
            {
                var recordId = Convert.ToInt32(dgvRecords.SelectedRows[0].Cells["Id"].Value);
                var recordType = cmbRecordType.SelectedItem?.ToString();

                if (recordType == "Grades")
                {
                    await EditGradeAsync(recordId);
                }
                else
                {
                    await EditAttendanceAsync(recordId);
                }

                LoadRecords();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error editing record: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void BtnDelete_Click(object sender, EventArgs e)
        {
            if (dgvRecords.SelectedRows.Count == 0) return;

            var selectedRow = dgvRecords.SelectedRows[0];
            var recordType = cmbRecordType.SelectedItem?.ToString() ?? "record";
            var recordId = Convert.ToInt32(selectedRow.Cells["Id"].Value);
            
            string recordInfo = "";
            if (recordType == "Grades")
            {
                var studentName = selectedRow.Cells["StudentName"].Value?.ToString() ?? "Unknown";
                var assignmentName = selectedRow.Cells["AssignmentName"].Value?.ToString() ?? "";
                recordInfo = $"Grade record for '{studentName}' - {assignmentName}";
            }
            else
            {
                var studentName = selectedRow.Cells["StudentName"].Value?.ToString() ?? "Unknown";
                var date = selectedRow.Cells["Date"].Value?.ToString() ?? "";
                recordInfo = $"Attendance record for '{studentName}' on {date}";
            }

            var confirmMessage = $"WARNING: You are about to permanently delete this {recordType.ToLower()}.\n\n" +
                               $"Record: {recordInfo}\n\n" +
                               "This action cannot be undone. Are you absolutely sure?";

            var result = MessageBox.Show(confirmMessage, "Confirm Record Deletion",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (result != DialogResult.Yes) return;

            try
            {
                using var connection = DatabaseHelper.GetConnection();
                await connection.OpenAsync();

                if (recordType == "Grades")
                {
                    var query = "DELETE FROM Grades WHERE Id = @id AND ProfessorId = @professorId";
                    using var command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@id", recordId);
                    command.Parameters.AddWithValue("@professorId", currentProfessor.Id);
                    await command.ExecuteNonQueryAsync();
                }
                else
                {
                    var query = "DELETE FROM Attendance WHERE Id = @id AND ProfessorId = @professorId";
                    using var command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@id", recordId);
                    command.Parameters.AddWithValue("@professorId", currentProfessor.Id);
                    await command.ExecuteNonQueryAsync();
                }

                MessageBox.Show("Record deleted successfully.", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadRecords();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting record: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task EditGradeAsync(int gradeId)
        {
            using var connection = DatabaseHelper.GetConnection();
            await connection.OpenAsync();

            var query = "SELECT * FROM Grades WHERE Id = @id AND ProfessorId = @professorId";
            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@id", gradeId);
            command.Parameters.AddWithValue("@professorId", currentProfessor.Id);

            using var reader = await command.ExecuteReaderAsync();
            if (!await reader.ReadAsync())
            {
                MessageBox.Show("Grade record not found or you don't have permission to edit it.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var form = new Form
            {
                Text = "Edit Grade",
                Size = new Size(500, 400),
                StartPosition = FormStartPosition.CenterParent
            };

            var lblScore = new Label { Text = "Score:", Location = new Point(20, 20), AutoSize = true };
            var nudScore = new NumericUpDown
            {
                Location = new Point(20, 40),
                Size = new Size(200, 25),
                Minimum = 0,
                Maximum = 10000,
                DecimalPlaces = 2,
                Value = Convert.ToDecimal(reader["Score"])
            };

            var lblMaxScore = new Label { Text = "Max Score:", Location = new Point(240, 20), AutoSize = true };
            var nudMaxScore = new NumericUpDown
            {
                Location = new Point(240, 40),
                Size = new Size(200, 25),
                Minimum = 1,
                Maximum = 10000,
                DecimalPlaces = 2,
                Value = Convert.ToDecimal(reader["MaxScore"])
            };

            var lblComments = new Label { Text = "Comments:", Location = new Point(20, 80), AutoSize = true };
            var txtComments = new TextBox
            {
                Location = new Point(20, 100),
                Size = new Size(420, 100),
                Multiline = true,
                Text = reader["Comments"]?.ToString() ?? ""
            };

            var btnSave = new Button
            {
                Text = "Save",
                Location = new Point(20, 220),
                Size = new Size(100, 30),
                BackColor = Color.FromArgb(34, 197, 94),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };

            var btnCancel = new Button
            {
                Text = "Cancel",
                Location = new Point(130, 220),
                Size = new Size(100, 30),
                BackColor = Color.FromArgb(107, 114, 128),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };

            btnSave.Click += async (s, e) =>
            {
                try
                {
                    var newScore = nudScore.Value;
                    var newMaxScore = nudMaxScore.Value;
                    var newPercentage = (newScore / newMaxScore) * 100;

                    reader.Close();

                    var updateQuery = @"
                        UPDATE Grades 
                        SET Score = @score, MaxScore = @maxScore, Percentage = @percentage, Comments = @comments
                        WHERE Id = @id AND ProfessorId = @professorId";

                    using var updateCommand = new SqlCommand(updateQuery, connection);
                    updateCommand.Parameters.AddWithValue("@score", newScore);
                    updateCommand.Parameters.AddWithValue("@maxScore", newMaxScore);
                    updateCommand.Parameters.AddWithValue("@percentage", newPercentage);
                    updateCommand.Parameters.AddWithValue("@comments", txtComments.Text);
                    updateCommand.Parameters.AddWithValue("@id", gradeId);
                    updateCommand.Parameters.AddWithValue("@professorId", currentProfessor.Id);

                    await updateCommand.ExecuteNonQueryAsync();
                    MessageBox.Show("Grade updated successfully.", "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    form.DialogResult = DialogResult.OK;
                    form.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error updating grade: {ex.Message}", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            btnCancel.Click += (s, e) => form.Close();

            form.Controls.AddRange(new Control[] { lblScore, nudScore, lblMaxScore, nudMaxScore, lblComments, txtComments, btnSave, btnCancel });
            form.ShowDialog();
        }

        private async Task EditAttendanceAsync(int attendanceId)
        {
            using var connection = DatabaseHelper.GetConnection();
            await connection.OpenAsync();

            var query = "SELECT * FROM Attendance WHERE Id = @id AND ProfessorId = @professorId";
            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@id", attendanceId);
            command.Parameters.AddWithValue("@professorId", currentProfessor.Id);

            using var reader = await command.ExecuteReaderAsync();
            if (!await reader.ReadAsync())
            {
                MessageBox.Show("Attendance record not found or you don't have permission to edit it.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var form = new Form
            {
                Text = "Edit Attendance",
                Size = new Size(500, 350),
                StartPosition = FormStartPosition.CenterParent
            };

            var lblDate = new Label { Text = "Date:", Location = new Point(20, 20), AutoSize = true };
            var dtpDate = new DateTimePicker
            {
                Location = new Point(20, 40),
                Size = new Size(200, 25),
                Value = reader.GetDateTime("Date")
            };

            var lblStatus = new Label { Text = "Status:", Location = new Point(240, 20), AutoSize = true };
            var cmbStatus = new ComboBox
            {
                Location = new Point(240, 40),
                Size = new Size(200, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbStatus.Items.AddRange(new[] { "Present", "Absent", "Late", "Excused" });
            var statusValue = reader.GetInt32("Status");
            cmbStatus.SelectedIndex = statusValue - 1;

            var lblNotes = new Label { Text = "Notes:", Location = new Point(20, 80), AutoSize = true };
            var txtNotes = new TextBox
            {
                Location = new Point(20, 100),
                Size = new Size(420, 100),
                Multiline = true,
                Text = reader["Notes"]?.ToString() ?? ""
            };

            var btnSave = new Button
            {
                Text = "Save",
                Location = new Point(20, 220),
                Size = new Size(100, 30),
                BackColor = Color.FromArgb(34, 197, 94),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };

            var btnCancel = new Button
            {
                Text = "Cancel",
                Location = new Point(130, 220),
                Size = new Size(100, 30),
                BackColor = Color.FromArgb(107, 114, 128),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };

            btnSave.Click += async (s, e) =>
            {
                try
                {
                    reader.Close();

                    var updateQuery = @"
                        UPDATE Attendance 
                        SET Date = @date, Status = @status, Notes = @notes
                        WHERE Id = @id AND ProfessorId = @professorId";

                    using var updateCommand = new SqlCommand(updateQuery, connection);
                    updateCommand.Parameters.AddWithValue("@date", dtpDate.Value);
                    updateCommand.Parameters.AddWithValue("@status", cmbStatus.SelectedIndex + 1);
                    updateCommand.Parameters.AddWithValue("@notes", txtNotes.Text);
                    updateCommand.Parameters.AddWithValue("@id", attendanceId);
                    updateCommand.Parameters.AddWithValue("@professorId", currentProfessor.Id);

                    await updateCommand.ExecuteNonQueryAsync();
                    MessageBox.Show("Attendance updated successfully.", "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    form.DialogResult = DialogResult.OK;
                    form.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error updating attendance: {ex.Message}", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            btnCancel.Click += (s, e) => form.Close();

            form.Controls.AddRange(new Control[] { lblDate, dtpDate, lblStatus, cmbStatus, lblNotes, txtNotes, btnSave, btnCancel });
            form.ShowDialog();
        }
    }
}


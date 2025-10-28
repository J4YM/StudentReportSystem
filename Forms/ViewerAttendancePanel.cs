using StudentReportInitial.Models;
using StudentReportInitial.Data;
using System.Data.SqlClient;
using System.Data;

namespace StudentReportInitial.Forms
{
    public partial class ViewerAttendancePanel : UserControl
    {
        private User currentUser;
        private DataGridView dgvAttendance;
        private ComboBox cmbSubject;
        private DateTimePicker dtpFromDate;
        private DateTimePicker dtpToDate;
        private Button btnRefresh;
        private Label lblAttendanceSummary;
        private Panel pnlSummary;

        public ViewerAttendancePanel(User user)
        {
            currentUser = user;
            InitializeComponent();
            ApplyModernStyling();
            LoadSubjects();
            LoadAttendance();
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
                Text = "Attendance Report",
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

            // Date range filters
            var lblFromDate = new Label
            {
                Text = "From:",
                Location = new Point(300, 50),
                AutoSize = true
            };

            dtpFromDate = new DateTimePicker
            {
                Location = new Point(340, 48),
                Size = new Size(120, 25),
                Value = DateTime.Today.AddDays(-30)
            };

            var lblToDate = new Label
            {
                Text = "To:",
                Location = new Point(480, 50),
                AutoSize = true
            };

            dtpToDate = new DateTimePicker
            {
                Location = new Point(510, 48),
                Size = new Size(120, 25),
                Value = DateTime.Today
            };

            btnRefresh = new Button
            {
                Text = "Refresh",
                Location = new Point(650, 47),
                Size = new Size(80, 27),
                BackColor = Color.FromArgb(59, 130, 246),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                
                Cursor = Cursors.Hand
            };
            btnRefresh.Click += BtnRefresh_Click;

            pnlHeader.Controls.AddRange(new Control[] { 
                lblTitle, lblSubject, cmbSubject, lblFromDate, dtpFromDate, 
                lblToDate, dtpToDate, btnRefresh
            });

            // Summary panel
            pnlSummary = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.FromArgb(59, 130, 246),
                Padding = new Padding(20)
            };

            lblAttendanceSummary = new Label
            {
                Text = "Attendance Summary: --",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(20, 15)
            };

            pnlSummary.Controls.Add(lblAttendanceSummary);

            // Attendance grid
            dgvAttendance = new DataGridView
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

            this.Controls.Add(dgvAttendance);
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
                else if (control is DateTimePicker dateTimePicker)
                {
                    dateTimePicker.Font = font;
                }
            }

            dgvAttendance.Font = font;
            dgvAttendance.ColumnHeadersDefaultCellStyle.Font = headerFont;
            dgvAttendance.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
            dgvAttendance.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(51, 65, 85);
            dgvAttendance.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
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
                    SELECT DISTINCT a.Subject
                    FROM Attendance a
                    INNER JOIN Students s ON a.StudentId = s.Id
                    WHERE s.Username = @username
                    ORDER BY a.Subject";

                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@username", currentUser.Username);

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

        private async void LoadAttendance()
        {
            try
            {
                using var connection = DatabaseHelper.GetConnection();
                await connection.OpenAsync();

                var query = @"
                    SELECT a.Subject, a.Date, a.Status, a.Notes, a.RecordedDate,
                           u.FirstName + ' ' + u.LastName as ProfessorName
                    FROM Attendance a
                    INNER JOIN Students s ON a.StudentId = s.Id
                    INNER JOIN Users u ON a.ProfessorId = u.Id
                    WHERE s.Username = @username AND a.Date BETWEEN @fromDate AND @toDate";

                var parameters = new List<SqlParameter>
                {
                    new SqlParameter("@username", currentUser.Username),
                    new SqlParameter("@fromDate", dtpFromDate.Value.Date),
                    new SqlParameter("@toDate", dtpToDate.Value.Date)
                };

                if (cmbSubject.SelectedIndex > 0)
                {
                    query += " AND a.Subject = @subject";
                    parameters.Add(new SqlParameter("@subject", cmbSubject.SelectedItem.ToString()));
                }

                query += " ORDER BY a.Date DESC";

                using var command = new SqlCommand(query, connection);
                command.Parameters.AddRange(parameters.ToArray());

                using var adapter = new SqlDataAdapter(command);
                var dataTable = new DataTable();
                adapter.Fill(dataTable);

                // Add StatusText column for display
                dataTable.Columns.Add("StatusText", typeof(string));
                
                // Convert status numbers to text in new column
                foreach (DataRow row in dataTable.Rows)
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

                dgvAttendance.DataSource = dataTable;

                // Format columns
                if (dgvAttendance.Columns.Count > 0)
                {
                    // Hide the integer Status column and show the text version
                    dgvAttendance.Columns["Status"].Visible = false;
                    dgvAttendance.Columns["Subject"].HeaderText = "Subject";
                    dgvAttendance.Columns["Date"].HeaderText = "Date";
                    dgvAttendance.Columns["StatusText"].HeaderText = "Status";
                    dgvAttendance.Columns["Notes"].HeaderText = "Notes";
                    dgvAttendance.Columns["RecordedDate"].HeaderText = "Recorded Date";
                    dgvAttendance.Columns["ProfessorName"].HeaderText = "Professor";

                    dgvAttendance.Columns["Date"].DefaultCellStyle.Format = "MM/dd/yyyy";
                    dgvAttendance.Columns["RecordedDate"].DefaultCellStyle.Format = "MM/dd/yyyy HH:mm";

                    // Color code status
                    dgvAttendance.CellFormatting += DgvAttendance_CellFormatting;
                }

                // Calculate attendance summary
                CalculateAttendanceSummary(dataTable);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading attendance: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DgvAttendance_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dgvAttendance.Columns[e.ColumnIndex].Name == "StatusText")
            {
                var status = e.Value?.ToString();
                switch (status)
                {
                    case "Present":
                        e.CellStyle.BackColor = Color.FromArgb(220, 252, 231);
                        e.CellStyle.ForeColor = Color.FromArgb(22, 163, 74);
                        break;
                    case "Absent":
                        e.CellStyle.BackColor = Color.FromArgb(254, 226, 226);
                        e.CellStyle.ForeColor = Color.FromArgb(220, 38, 38);
                        break;
                    case "Late":
                        e.CellStyle.BackColor = Color.FromArgb(254, 249, 195);
                        e.CellStyle.ForeColor = Color.FromArgb(161, 98, 7);
                        break;
                    case "Excused":
                        e.CellStyle.BackColor = Color.FromArgb(219, 234, 254);
                        e.CellStyle.ForeColor = Color.FromArgb(37, 99, 235);
                        break;
                }
            }
        }

        private void CalculateAttendanceSummary(DataTable dataTable)
        {
            if (dataTable.Rows.Count == 0)
            {
                lblAttendanceSummary.Text = "Attendance Summary: No records found";
                return;
            }

            int present = 0, absent = 0, late = 0, excused = 0;

            foreach (DataRow row in dataTable.Rows)
            {
                var status = row["Status"]?.ToString();
                switch (status)
                {
                    case "Present":
                        present++;
                        break;
                    case "Absent":
                        absent++;
                        break;
                    case "Late":
                        late++;
                        break;
                    case "Excused":
                        excused++;
                        break;
                }
            }

            var total = present + absent + late + excused;
            var attendanceRate = total > 0 ? (decimal)(present + excused) / total * 100 : 0;

            lblAttendanceSummary.Text = $"Attendance Summary: {attendanceRate:F1}% ({present + excused}/{total}) - Present: {present}, Absent: {absent}, Late: {late}, Excused: {excused}";
        }

        private void CmbSubject_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadAttendance();
        }

        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            LoadAttendance();
        }
    }
}

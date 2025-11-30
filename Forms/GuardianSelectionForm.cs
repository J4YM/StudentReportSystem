using StudentReportInitial.Models;
using StudentReportInitial.Data;
using System.Data.SqlClient;
using System.Data;
using System.Windows.Forms;

namespace StudentReportInitial.Forms
{
    public partial class GuardianSelectionForm : Form
    {
        private DataGridView dgvGuardians;
        private TextBox txtSearch;
        private Button btnSelect;
        private Button btnCancel;
        private User? currentUser;
        private int? branchFilterId;
        public int? SelectedGuardianId { get; private set; }
        public string? SelectedGuardianName { get; private set; }

        public GuardianSelectionForm(User? user = null, int? branchId = null)
        {
            currentUser = user;
            branchFilterId = branchId;
            InitializeComponent();
        }

        private async void InitializeComponent()
        {
            this.Text = "Select Existing Guardian";
            this.Size = new Size(600, 550);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.FromArgb(248, 250, 252);
            this.Padding = new Padding(10);

            var yPos = 20;

            // Title
            var lblTitle = new Label
            {
                Text = "Select Existing Guardian",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 41, 59),
                Location = new Point(20, yPos),
                AutoSize = true
            };
            yPos += 40;

            // Search box
            var lblSearch = new Label
            {
                Text = "Search:",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(51, 65, 85),
                Location = new Point(20, yPos),
                AutoSize = true
            };
            yPos += 25;

            txtSearch = new TextBox
            {
                Location = new Point(20, yPos),
                Size = new Size(540, 25),
                PlaceholderText = "Search by name, email, or phone..."
            };
            txtSearch.TextChanged += (s, e) => FilterGuardians();
            yPos += 35;

            // DataGridView - reduced height to make room for buttons
            dgvGuardians = new DataGridView
            {
                Location = new Point(20, yPos),
                Size = new Size(540, 280),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                ReadOnly = true,
                AllowUserToAddRows = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                RowHeadersVisible = false
            };
            dgvGuardians.SelectionChanged += (s, e) => UpdateSelectButton();
            yPos += 290;

            // Buttons - positioned with proper spacing from bottom
            btnSelect = new Button
            {
                Text = "Select",
                Location = new Point(380, yPos),
                Size = new Size(100, 35),
                BackColor = Color.FromArgb(34, 197, 94),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Enabled = false
            };
            btnSelect.Click += BtnSelect_Click;

            btnCancel = new Button
            {
                Text = "Cancel",
                Location = new Point(490, yPos),
                Size = new Size(100, 35),
                BackColor = Color.FromArgb(107, 114, 128),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9),
                Cursor = Cursors.Hand
            };
            btnCancel.Click += (s, e) => this.Close();

            UIStyleHelper.ApplyRoundedButton(btnSelect, 8);
            UIStyleHelper.ApplyRoundedButton(btnCancel, 8);

            this.Controls.AddRange(new Control[] {
                lblTitle, lblSearch, txtSearch, dgvGuardians, btnSelect, btnCancel
            });

            await LoadGuardiansAsync();
        }

        private async Task LoadGuardiansAsync()
        {
            try
            {
                using var connection = DatabaseHelper.GetConnection();
                await connection.OpenAsync();

                var query = @"
                    SELECT u.Id, 
                           u.FirstName + ' ' + u.LastName as FullName,
                           u.Email,
                           u.Phone,
                           b.Name as BranchName
                    FROM Users u
                    LEFT JOIN Branches b ON u.BranchId = b.Id
                    WHERE u.Role = 3 AND u.IsActive = 1";

                // Add branch filter if not Super Admin
                if (currentUser != null)
                {
                    var isSuperAdmin = await BranchHelper.IsSuperAdminAsync(currentUser.Id);
                    if (!isSuperAdmin)
                    {
                        var branchId = await BranchHelper.GetUserBranchIdAsync(currentUser.Id);
                        if (branchId > 0)
                        {
                            query += " AND u.BranchId = @branchId";
                        }
                    }
                    else if (branchFilterId.HasValue)
                    {
                        query += " AND u.BranchId = @branchId";
                    }
                }

                query += " ORDER BY u.LastName, u.FirstName";

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

                dgvGuardians.DataSource = dataTable;
                dgvGuardians.Columns["Id"].Visible = false;
                dgvGuardians.Columns["FullName"].HeaderText = "Name";
                dgvGuardians.Columns["FullName"].Width = 200;
                dgvGuardians.Columns["Email"].HeaderText = "Email";
                dgvGuardians.Columns["Email"].Width = 180;
                dgvGuardians.Columns["Phone"].HeaderText = "Phone";
                dgvGuardians.Columns["Phone"].Width = 120;
                dgvGuardians.Columns["BranchName"].HeaderText = "Branch";
                dgvGuardians.Columns["BranchName"].Width = 100;

                // Store original data source for filtering
                dgvGuardians.Tag = dataTable;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading guardians: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void FilterGuardians()
        {
            if (dgvGuardians.Tag is DataTable dataTable)
            {
                var searchText = txtSearch.Text.Trim().ToLower();
                if (string.IsNullOrEmpty(searchText))
                {
                    dgvGuardians.DataSource = dataTable;
                }
                else
                {
                    var filteredTable = dataTable.Clone();
                    foreach (DataRow row in dataTable.Rows)
                    {
                        var fullName = row["FullName"]?.ToString()?.ToLower() ?? "";
                        var email = row["Email"]?.ToString()?.ToLower() ?? "";
                        var phone = row["Phone"]?.ToString()?.ToLower() ?? "";

                        if (fullName.Contains(searchText) || email.Contains(searchText) || phone.Contains(searchText))
                        {
                            filteredTable.ImportRow(row);
                        }
                    }
                    dgvGuardians.DataSource = filteredTable;
                }

                // Reapply column settings
                if (dgvGuardians.Columns.Count > 0)
                {
                    dgvGuardians.Columns["Id"].Visible = false;
                    dgvGuardians.Columns["FullName"].HeaderText = "Name";
                    dgvGuardians.Columns["Email"].HeaderText = "Email";
                    dgvGuardians.Columns["Phone"].HeaderText = "Phone";
                    dgvGuardians.Columns["BranchName"].HeaderText = "Branch";
                }
            }
        }

        private void UpdateSelectButton()
        {
            btnSelect.Enabled = dgvGuardians.SelectedRows.Count > 0;
        }

        private void BtnSelect_Click(object? sender, EventArgs e)
        {
            if (dgvGuardians.SelectedRows.Count > 0)
            {
                var selectedRow = dgvGuardians.SelectedRows[0];
                SelectedGuardianId = Convert.ToInt32(selectedRow.Cells["Id"].Value);
                SelectedGuardianName = selectedRow.Cells["FullName"].Value?.ToString();
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }
    }
}


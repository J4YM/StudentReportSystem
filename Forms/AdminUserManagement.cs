using StudentReportInitial.Models;
using StudentReportInitial.Data;
using System.Data.SqlClient;
using System.Data;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using StudentReportInitial.Theming;

namespace StudentReportInitial.Forms
{
    public partial class AdminUserManagement : UserControl
    {
        private DataGridView dgvUsers;
        private Button btnAddUser;
        private Button btnEditUser;
        private Button btnDeleteUser;
        private ComboBox? cmbRoleFilter;
        private ComboBox? cmbSectionFilter;
        private ComboBox? cmbCourseFilter;
        private ComboBox? cmbYearFilter;
        private Button? btnClearFilters;
        private Label? lblActiveFilters;
        private Button? btnRequestAdmin;
        private Panel pnlUserForm;
        private Panel? pnlUserFilters;
        private bool isEditMode = false;
        private int selectedUserId = -1;
        private User? currentUser;
        private int? branchFilterId = null;
        private DataTable? usersTable;
        private bool? isCurrentUserSuperAdmin;
        private const string FilterAllLabel = "All";

        public AdminUserManagement(User? user = null, int? branchId = null)
        {
            currentUser = user;
            branchFilterId = branchId;
            InitializeComponent();
            ApplyModernStyling();
            ThemeManager.ApplyTheme(this);
            LoadUsersAsync();
        }

        private void EnhanceUsersDataTable(DataTable dataTable)
        {
            if (!dataTable.Columns.Contains("Course"))
            {
                dataTable.Columns.Add("Course", typeof(string));
            }

            foreach (DataRow row in dataTable.Rows)
            {
                var sectionValue = row.Table.Columns.Contains("StudentSection")
                    ? row["StudentSection"]?.ToString()
                    : string.Empty;
                row["Course"] = ExtractCourseFromSection(sectionValue);
            }
        }

        private void PopulateUserFilterOptions()
        {
            if (usersTable == null)
            {
                return;
            }

            PopulateComboFromColumn(cmbSectionFilter, usersTable, "StudentSection");
            PopulateComboFromColumn(cmbCourseFilter, usersTable, "Course");
            PopulateComboFromColumn(cmbYearFilter, usersTable, "StudentYearLevel");
        }

        private void PopulateComboFromColumn(ComboBox? comboBox, DataTable source, string columnName)
        {
            if (comboBox == null || !source.Columns.Contains(columnName))
            {
                return;
            }

            var currentValue = comboBox.SelectedItem?.ToString();
            comboBox.Items.Clear();
            comboBox.Items.Add(FilterAllLabel);

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

        private void ApplyUserFilters()
        {
            if (usersTable == null)
            {
                return;
            }

            var filterParts = new List<string>();

            if (cmbRoleFilter != null && cmbRoleFilter.SelectedIndex > 0)
            {
                var roleValue = MapRoleNameToValue(cmbRoleFilter.SelectedItem?.ToString());
                if (roleValue.HasValue)
                {
                    filterParts.Add($"Role = {roleValue.Value}");
                }
            }

            if (cmbSectionFilter != null && cmbSectionFilter.SelectedIndex > 0)
            {
                var value = EscapeRowFilterValue(cmbSectionFilter.SelectedItem?.ToString());
                filterParts.Add($"StudentSection = '{value}'");
            }

            if (cmbCourseFilter != null && cmbCourseFilter.SelectedIndex > 0)
            {
                var value = EscapeRowFilterValue(cmbCourseFilter.SelectedItem?.ToString());
                filterParts.Add($"Course = '{value}'");
            }

            if (cmbYearFilter != null && cmbYearFilter.SelectedIndex > 0)
            {
                var value = EscapeRowFilterValue(cmbYearFilter.SelectedItem?.ToString());
                filterParts.Add($"StudentYearLevel = '{value}'");
            }

            usersTable.DefaultView.RowFilter = filterParts.Count > 0
                ? string.Join(" AND ", filterParts)
                : string.Empty;

            UpdateUserFilterState();
        }

        private void UpdateUserFilterState()
        {
            var activeFilters = new List<string>();

            if (cmbRoleFilter != null && cmbRoleFilter.SelectedIndex > 0)
            {
                activeFilters.Add($"Role={cmbRoleFilter.SelectedItem}");
            }

            if (cmbSectionFilter != null && cmbSectionFilter.SelectedIndex > 0)
            {
                activeFilters.Add($"Section={cmbSectionFilter.SelectedItem}");
            }

            if (cmbCourseFilter != null && cmbCourseFilter.SelectedIndex > 0)
            {
                activeFilters.Add($"Course={cmbCourseFilter.SelectedItem}");
            }

            if (cmbYearFilter != null && cmbYearFilter.SelectedIndex > 0)
            {
                activeFilters.Add($"Year={cmbYearFilter.SelectedItem}");
            }

            if (lblActiveFilters != null)
            {
                lblActiveFilters.Text = activeFilters.Count == 0
                    ? "Active filters: None"
                    : $"Active filters: {string.Join(" | ", activeFilters)}";
            }

            if (btnClearFilters != null)
            {
                btnClearFilters.Enabled = activeFilters.Count > 0;
            }
        }

        private static string EscapeRowFilterValue(string? value)
        {
            return (value ?? string.Empty).Replace("'", "''");
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

        private static int? MapRoleNameToValue(string? roleName)
        {
            return roleName switch
            {
                "Super Admin" => (int)UserRole.SuperAdmin,
                "Admin" => (int)UserRole.Admin,
                "Professor" => (int)UserRole.Professor,
                "Guardian" => (int)UserRole.Guardian,
                "Student" => (int)UserRole.Student,
                _ => null
            };
        }

        private static void ConfigureRoleComboBox(ComboBox comboBox, bool canManageAdmins)
        {
            var roles = new List<UserRole>();
            if (canManageAdmins)
            {
                roles.Add(UserRole.Admin);
            }
            roles.Add(UserRole.Professor);
            roles.Add(UserRole.Guardian);
            //roles.Add(UserRole.Student);

            comboBox.Items.Clear();
            comboBox.Tag = roles;

            foreach (var role in roles)
            {
                comboBox.Items.Add(GetRoleDisplayName(role));
            }

            if (comboBox.Items.Count > 0)
            {
                comboBox.SelectedIndex = 0;
            }
        }

        private static UserRole? GetComboSelectedRole(ComboBox comboBox)
        {
            if (comboBox.Tag is List<UserRole> roles &&
                comboBox.SelectedIndex >= 0 &&
                comboBox.SelectedIndex < roles.Count)
            {
                return roles[comboBox.SelectedIndex];
            }

            return null;
        }

        private static void SetComboSelectedRole(ComboBox comboBox, UserRole role)
        {
            if (comboBox.Tag is List<UserRole> roles)
            {
                var index = roles.FindIndex(r => r == role);
                if (index >= 0)
                {
                    comboBox.SelectedIndex = index;
                }
            }
        }

        private static string GetRoleDisplayName(UserRole role)
        {
            return role switch
            {
                UserRole.SuperAdmin => "Super Admin",
                UserRole.Admin => "Admin",
                UserRole.Professor => "Professor",
                UserRole.Guardian => "Guardian",
                UserRole.Student => "Student",
                _ => role.ToString()
            };
        }

        private void SetUserFormVisibility(bool showForm)
        {
            pnlUserForm.Visible = showForm;
            dgvUsers.Visible = !showForm;
            if (pnlUserFilters != null)
            {
                pnlUserFilters.Visible = !showForm;
            }
        }

        private async Task<bool> IsCurrentUserSuperAdminAsync()
        {
            if (currentUser == null)
            {
                return true;
            }

            if (!isCurrentUserSuperAdmin.HasValue)
            {
                isCurrentUserSuperAdmin = await BranchHelper.IsSuperAdminAsync(currentUser.Id);
            }

            return isCurrentUserSuperAdmin.Value;
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

            btnAddUser = new Button
            {
                Text = "Add User",
                Size = new Size(90, 28),
                Location = new Point(15, 10),
                BackColor = Color.FromArgb(34, 197, 94),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9),
                Cursor = Cursors.Hand
            };
            btnAddUser.Click += BtnAddUser_Click;

            btnEditUser = new Button
            {
                Text = "Edit User",
                Size = new Size(90, 28),
                Location = new Point(115, 10),
                BackColor = Color.FromArgb(59, 130, 246),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9),
                Cursor = Cursors.Hand
            };
            btnEditUser.Click += BtnEditUser_Click;

            btnDeleteUser = new Button
            {
                Text = "Delete User",
                Size = new Size(90, 28),
                Location = new Point(215, 10),
                BackColor = Color.FromArgb(239, 68, 68),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9),
                Cursor = Cursors.Hand
            };
            btnDeleteUser.Click += BtnDeleteUser_Click;

            btnRequestAdmin = new Button
            {
                Text = "Request Admin Access",
                Size = new Size(170, 28),
                Location = new Point(315, 10),
                BackColor = Color.FromArgb(248, 250, 252),
                ForeColor = Color.FromArgb(51, 65, 85),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9),
                Cursor = Cursors.Hand,
                Visible = currentUser != null && currentUser.Role != UserRole.SuperAdmin
            };
            btnRequestAdmin.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);
            btnRequestAdmin.Click += BtnRequestAdmin_Click;

            pnlActions.Controls.AddRange(new Control[] { btnAddUser, btnEditUser, btnDeleteUser, btnRequestAdmin });

            var pnlFilters = BuildUserFiltersPanel();

            // Data grid view
            dgvUsers = new DataGridView
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
            dgvUsers.SelectionChanged += DgvUsers_SelectionChanged;

            // User form panel (initially hidden)
            pnlUserForm = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Visible = false,
                Padding = new Padding(20),
                AutoScroll = true
            };

            pnlUserFilters = pnlFilters;

            this.Controls.Add(dgvUsers);
            this.Controls.Add(pnlUserForm);
            this.Controls.Add(pnlFilters);
            this.Controls.Add(pnlActions);

            UIStyleHelper.ApplyRoundedButton(btnAddUser, 10);
            UIStyleHelper.ApplyRoundedButton(btnEditUser, 10);
            UIStyleHelper.ApplyRoundedButton(btnDeleteUser, 10);

            this.ResumeLayout(false);
        }

        private Panel BuildUserFiltersPanel()
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

            cmbRoleFilter = CreateFilterComboBox();
            cmbSectionFilter = CreateFilterComboBox();
            cmbCourseFilter = CreateFilterComboBox();
            cmbYearFilter = CreateFilterComboBox();
            btnClearFilters = new Button
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
            btnClearFilters.FlatAppearance.BorderColor = Color.FromArgb(226, 232, 240);
            btnClearFilters.Click += BtnClearUserFilters_Click;

            flow.Controls.Add(CreateFilterGroup("Role", cmbRoleFilter));
            flow.Controls.Add(CreateFilterGroup("Section", cmbSectionFilter));
            flow.Controls.Add(CreateFilterGroup("Course", cmbCourseFilter));
            flow.Controls.Add(CreateFilterGroup("Year Level", cmbYearFilter));
            flow.Controls.Add(btnClearFilters);

            lblActiveFilters = new Label
            {
                Dock = DockStyle.Bottom,
                Height = 20,
                Text = "Active filters: None",
                ForeColor = Color.FromArgb(100, 116, 139)
            };

            panel.Controls.Add(flow);
            panel.Controls.Add(lblActiveFilters);

            InitializeFilterCombo(cmbRoleFilter);
            InitializeFilterCombo(cmbSectionFilter);
            InitializeFilterCombo(cmbCourseFilter);
            InitializeFilterCombo(cmbYearFilter);
            PopulateRoleFilterItems();

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
                comboBox.SelectedIndexChanged += HandleUserFilterChanged;
                comboBox.Location = new Point(0, 25);
                comboBox.Width = wrapper.Width;
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

        private void PopulateRoleFilterItems()
        {
            if (cmbRoleFilter == null)
            {
                return;
            }

            cmbRoleFilter.Items.Clear();
            cmbRoleFilter.Items.Add(FilterAllLabel);
            cmbRoleFilter.Items.Add("Admin");
            cmbRoleFilter.Items.Add("Professor");
            cmbRoleFilter.Items.Add("Guardian");
            cmbRoleFilter.Items.Add("Student");
            cmbRoleFilter.SelectedIndex = 0;
        }

        private void BtnClearUserFilters_Click(object? sender, EventArgs e)
        {
            if (cmbRoleFilter != null) cmbRoleFilter.SelectedIndex = 0;
            if (cmbSectionFilter != null) cmbSectionFilter.SelectedIndex = 0;
            if (cmbCourseFilter != null) cmbCourseFilter.SelectedIndex = 0;
            if (cmbYearFilter != null) cmbYearFilter.SelectedIndex = 0;
        }

        private void HandleUserFilterChanged(object? sender, EventArgs e)
        {
            ApplyUserFilters();
        }


        private void ApplyModernStyling()
        {
            // Apply modern styling to all controls
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

            dgvUsers.Font = font;
            dgvUsers.ColumnHeadersDefaultCellStyle.Font = headerFont;
            dgvUsers.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
            dgvUsers.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(51, 65, 85);
            dgvUsers.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
        }

        private async Task LoadUsersAsync()
        {
            try
            {
                using var connection = DatabaseHelper.GetConnection();
                await connection.OpenAsync();

                var query = @"
                    SELECT u.Id, u.Username, u.FirstName, u.LastName, u.Email, u.Phone, u.Role, u.CreatedDate, u.IsActive,
                           b.Name as BranchName,
                           s.Section as StudentSection,
                           s.GradeLevel as StudentYearLevel
                    FROM Users u
                    LEFT JOIN Branches b ON u.BranchId = b.Id
                    LEFT JOIN Students s ON s.Username = u.Username AND s.IsActive = 1
                    WHERE u.IsActive = 1";

                // Add branch filter if not Super Admin
                if (currentUser != null)
                {
                    var isSuperAdmin = await IsCurrentUserSuperAdminAsync();
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
                        // Super Admin with branch filter selected
                        query += " AND u.BranchId = @branchId";
                    }
                }

                query += " ORDER BY u.Role, u.CreatedDate ASC";

                using var command = new SqlCommand(query, connection);
                
                // Add branch parameter if needed
                if (currentUser != null)
                {
                    var isSuperAdmin = await IsCurrentUserSuperAdminAsync();
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

                EnhanceUsersDataTable(dataTable);
                usersTable = dataTable;
                dgvUsers.DataSource = usersTable;
                PopulateUserFilterOptions();
                ApplyUserFilters();

                // Format columns
                if (dgvUsers.Columns.Count > 0)
                {
                    dgvUsers.Columns["Id"].Visible = false;
                    dgvUsers.Columns["Username"].HeaderText = "Username";
                    dgvUsers.Columns["FirstName"].HeaderText = "First Name";
                    dgvUsers.Columns["LastName"].HeaderText = "Last Name";
                    dgvUsers.Columns["Email"].HeaderText = "Email";
                    dgvUsers.Columns["Phone"].HeaderText = "Phone";
                    dgvUsers.Columns["Role"].HeaderText = "Role";
                    dgvUsers.Columns["CreatedDate"].HeaderText = "Created Date";
                    dgvUsers.Columns["IsActive"].HeaderText = "Active";
                    if (dgvUsers.Columns.Contains("StudentSection"))
                    {
                        dgvUsers.Columns["StudentSection"].HeaderText = "Section";
                    }
                    if (dgvUsers.Columns.Contains("StudentYearLevel"))
                    {
                        dgvUsers.Columns["StudentYearLevel"].HeaderText = "Year Level";
                    }
                    if (dgvUsers.Columns.Contains("Course"))
                    {
                        dgvUsers.Columns["Course"].HeaderText = "Course";
                    }

                    // Format role column
                    dgvUsers.Columns["Role"].DefaultCellStyle.Format = "0";
                    dgvUsers.Columns["CreatedDate"].DefaultCellStyle.Format = "MM/dd/yyyy";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading users: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void DgvUsers_SelectionChanged(object sender, EventArgs e)
        {
            var hasSelection = dgvUsers.SelectedRows.Count > 0;
            btnEditUser.Enabled = hasSelection;
            
            if (hasSelection)
            {
                var selectedRow = dgvUsers.SelectedRows[0];
                var userId = Convert.ToInt32(selectedRow.Cells["Id"].Value);
                var userRole = Convert.ToInt32(selectedRow.Cells["Role"].Value);
                var username = selectedRow.Cells["Username"].Value?.ToString() ?? "";
                bool isSuperAdminAccount = userRole == (int)UserRole.SuperAdmin;
                bool isAdminAccount = userRole == (int)UserRole.Admin || isSuperAdminAccount;
                bool canManageAdmins = await IsCurrentUserSuperAdminAsync();
                btnEditUser.Enabled = hasSelection && (!isAdminAccount || canManageAdmins);
                
                // Check if this is the primary admin account
                bool isPrimaryAdmin = await IsPrimaryAdminAccountAsync(userId, username);
                bool isAdmin = userRole == (int)UserRole.Admin;
                
                // Disable delete for ALL admin accounts
                bool canDelete = !isAdmin && !isSuperAdminAccount;
                
                // Additional check: also disable for primary admin (even if not admin role, though unlikely)
                if (isPrimaryAdmin)
                {
                    canDelete = false;
                }
                
                btnDeleteUser.Enabled = canDelete;
                
                // Add tooltip for disabled delete button
                if (!canDelete)
                {
                    var tooltip = new ToolTip();
                    if (isPrimaryAdmin)
                    {
                        tooltip.SetToolTip(btnDeleteUser, "Cannot delete the primary admin account. This account is required for system access.");
                    }
                    else if (isSuperAdminAccount)
                    {
                        tooltip.SetToolTip(btnDeleteUser, "The Super Admin account cannot be deleted.");
                    }
                    else if (isAdmin)
                    {
                        tooltip.SetToolTip(btnDeleteUser, "Admin accounts cannot be deleted. Use the deactivate option instead.");
                    }
                }
            }
            else
            {
                btnDeleteUser.Enabled = false;
            }
        }
        
        private async Task<bool> IsPrimaryAdminAccountAsync(int userId, string username)
        {
            try
            {
                using var connection = DatabaseHelper.GetConnection();
                await connection.OpenAsync();
                
                // Primary admin is either the "admin" username or the first admin created
                var query = @"
                    SELECT TOP 1 Id FROM Users 
                    WHERE Role = 1 
                    ORDER BY Id ASC";
                
                using var command = new SqlCommand(query, connection);
                var result = await command.ExecuteScalarAsync();
                
                if (result != null)
                {
                    int firstAdminId = Convert.ToInt32(result);
                    return userId == firstAdminId || username.ToLower() == "admin";
                }
                
                return false;
            }
            catch
            {
                return false;
            }
        }
        
        private async Task<bool> IsLastActiveAdminAsync(int excludeUserId)
        {
            try
            {
                using var connection = DatabaseHelper.GetConnection();
                await connection.OpenAsync();
                
                var query = @"
                    SELECT COUNT(*) FROM Users 
                    WHERE Role = 1 AND IsActive = 1 AND Id != @excludeId";
                
                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@excludeId", excludeUserId);
                var count = Convert.ToInt32(await command.ExecuteScalarAsync());
                
                return count == 0; // If no other active admins, this is the last one
            }
            catch
            {
                return false;
            }
        }

        private async void BtnAddUser_Click(object sender, EventArgs e)
        {
            await ShowUserForm();
        }

        private void BtnRequestAdmin_Click(object? sender, EventArgs e)
        {
            if (currentUser == null)
            {
                MessageBox.Show("Please sign in to submit a request.", "Authentication Required",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using var requestForm = new AdminRequestForm(currentUser);
            requestForm.ShowDialog();
        }

        private async void BtnEditUser_Click(object sender, EventArgs e)
        {
            if (dgvUsers.SelectedRows.Count > 0)
            {
                selectedUserId = Convert.ToInt32(dgvUsers.SelectedRows[0].Cells["Id"].Value);
                var selectedRole = Convert.ToInt32(dgvUsers.SelectedRows[0].Cells["Role"].Value);
                if (selectedRole == (int)UserRole.Admin && !await IsCurrentUserSuperAdminAsync())
                {
                    MessageBox.Show("Only Super Admin can modify admin accounts.", "Access Restricted",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                await ShowUserForm(selectedUserId);
            }
        }

        private async void BtnDeleteUser_Click(object sender, EventArgs e)
        {
            if (dgvUsers.SelectedRows.Count == 0 || !btnDeleteUser.Enabled)
            {
                return;
            }

            var selectedRow = dgvUsers.SelectedRows[0];
            var userId = Convert.ToInt32(selectedRow.Cells["Id"].Value);
            var username = selectedRow.Cells["Username"].Value?.ToString() ?? "";
            var userRole = Convert.ToInt32(selectedRow.Cells["Role"].Value);
            var isSuperAdmin = userRole == (int)UserRole.SuperAdmin;
            var isAdmin = userRole == (int)UserRole.Admin;

            if (isSuperAdmin)
            {
                MessageBox.Show("The Super Admin account cannot be deleted.", "Access Restricted",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Double confirmation for admin accounts
            var confirmMessage = isAdmin 
                ? $"WARNING: You are about to delete an Admin account ({username}).\n\nThis action cannot be undone. Are you absolutely sure?"
                : $"Are you sure you want to delete user '{username}'?\n\nThis action cannot be undone.";

            var result = MessageBox.Show(confirmMessage, "Confirm Delete", 
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                // Second confirmation for admin accounts
                if (isAdmin)
                {
                    var secondConfirm = MessageBox.Show(
                        "Final confirmation: Delete this Admin account?",
                        "Final Confirmation",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Exclamation);
                    
                    if (secondConfirm != DialogResult.Yes)
                    {
                        return;
                    }
                }

                try
                {
                    // at least one active admin remains
                    if (isAdmin && await IsLastActiveAdminAsync(userId))
                    {
                        MessageBox.Show("Cannot delete the last active admin account. At least one admin must remain active for system access.",
                            "Deletion Prevented", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    await DeleteUserAsync(userId);
                    await LoadUsersAsync();
                    MessageBox.Show("User deleted successfully. Statistics will be updated when you refresh the System Reports panel.", 
                        "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting user: {ex.Message}", "Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private async Task ShowUserForm(int userId = -1)
        {
            try
            {
            pnlUserForm.Controls.Clear();
            SetUserFormVisibility(true);

            isEditMode = userId > 0;
                selectedUserId = userId;
            var canManageAdmins = await IsCurrentUserSuperAdminAsync();
            var editedUserIsAdmin = false;
            var editedUserIsSuperAdmin = false;
            var editingOwnSuperAdmin = false;

            var lblTitle = new Label
            {
                Text = isEditMode ? "Edit User" : "Add New User",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(51, 65, 85),
                AutoSize = true,
                Location = new Point(20, 20)
            };

            var yPos = 50;
            var spacing = 40;

            // Username
            var lblUsername = new Label { Text = "Username:", Location = new Point(20, yPos), AutoSize = true };
            var txtUsername = new TextBox { Location = new Point(20, yPos + 20), Size = new Size(250, 25) };
            yPos += spacing;

            // Password
            var lblPassword = new Label { Text = "Password:", Location = new Point(20, yPos), AutoSize = true };
            var txtPassword = new TextBox
            {
                Location = new Point(20, yPos + 20),
                Size = new Size(250, 25),
                UseSystemPasswordChar = true
            };
            yPos += spacing;

            // Confirm Password
            var lblConfirmPassword = new Label { Text = "Confirm Password:", Location = new Point(20, yPos), AutoSize = true };
            var txtConfirmPassword = new TextBox
            {
                Location = new Point(20, yPos + 20),
                Size = new Size(250, 25),
                UseSystemPasswordChar = true
            };
            yPos += spacing;

            // First Name
            var lblFirstName = new Label { Text = "First Name:", Location = new Point(20, yPos), AutoSize = true };
            var txtFirstName = new TextBox { Location = new Point(20, yPos + 20), Size = new Size(250, 25) };
            yPos += spacing;

            // Last Name
            var lblLastName = new Label { Text = "Last Name:", Location = new Point(20, yPos), AutoSize = true };
            var txtLastName = new TextBox { Location = new Point(20, yPos + 20), Size = new Size(250, 25) };
            yPos += spacing;

            // Email
            var lblEmail = new Label { Text = "Email:", Location = new Point(20, yPos), AutoSize = true };
            var txtEmail = new TextBox { Location = new Point(20, yPos + 20), Size = new Size(250, 25) };
            yPos += spacing;

            // Phone
            var lblPhone = new Label { Text = "Phone (International format):", Location = new Point(20, yPos), AutoSize = true };
            var txtPhone = new TextBox { Location = new Point(20, yPos + 20), Size = new Size(250, 25), PlaceholderText="+1234567890" };
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

            // Role
            var lblRole = new Label { Text = "Role:", Location = new Point(20, yPos), AutoSize = true };
            var cmbRole = new ComboBox { Location = new Point(20, yPos + 20), Size = new Size(250, 25), DropDownStyle = ComboBoxStyle.DropDownList };
            ConfigureRoleComboBox(cmbRole, canManageAdmins);
            var lblRoleWarning = new Label 
            { 
                Text = "", 
                Location = new Point(20, yPos + 48), 
                AutoSize = true,
                ForeColor = Color.FromArgb(239, 68, 68),
                Font = new Font("Segoe UI", 8F),
                Visible = false
            };
            
            // Function to show/hide phone field based on role
            Action updatePhoneFieldVisibility = () =>
            {
                var selectedRole = GetComboSelectedRole(cmbRole);
                bool showPhone = selectedRole != UserRole.Admin;
                
                lblPhone.Visible = showPhone;
                txtPhone.Visible = showPhone;
                lblPhoneError.Visible = showPhone && lblPhoneError.Visible;
                
                if (!showPhone)
                {
                    // Clear phone field when hidden
                    txtPhone.Text = "";
                    lblPhoneError.Visible = false;
                }
            };
            
            // Update phone visibility when role changes
            cmbRole.SelectedIndexChanged += (s, e) => updatePhoneFieldVisibility();
            
            yPos += spacing + 25;

            // Branch selection (only for Super Admin creating Admin users, or when creating users that need branch assignment)
            ComboBox? cmbBranch = null;
            Label? lblBranch = null;
            bool allowBranchSelection = true;
            var editingOwnSuperAdminCandidate = isEditMode && currentUser != null && currentUser.Id == userId && currentUser.Role == UserRole.SuperAdmin;
            if (currentUser != null && canManageAdmins)
            {
                lblBranch = new Label { Text = "Branch:", Location = new Point(20, yPos), AutoSize = true };
                cmbBranch = new ComboBox { Location = new Point(20, yPos + 20), Size = new Size(250, 25), DropDownStyle = ComboBoxStyle.DropDownList };

                if (editingOwnSuperAdminCandidate)
                {
                    allowBranchSelection = false;
                    lblBranch.Visible = false;
                    cmbBranch.Visible = false;
                    cmbBranch.Enabled = false;
                }
                
                var branches = await BranchHelper.GetAllBranchesAsync();
                if (branches.Count > 0)
                {
                    foreach (var branch in branches)
                    {
                        cmbBranch.Items.Add(branch);
                    }
                    if (cmbBranch.Items.Count > 0)
                    {
                        cmbBranch.SelectedIndex = 0;
                    }
                }
                
                // Update branch visibility when role changes
                cmbRole.SelectedIndexChanged += (s, e) =>
                {
                    if (lblBranch != null && cmbBranch != null)
                    {
                        if (!allowBranchSelection)
                        {
                            lblBranch.Visible = false;
                            cmbBranch.Visible = false;
                            cmbBranch.Enabled = false;
                            return;
                        }

                        var selectedRole = GetComboSelectedRole(cmbRole);
                        var showBranch = !branchFilterId.HasValue && selectedRole.HasValue;
                        lblBranch.Visible = showBranch;
                        cmbBranch.Visible = showBranch;
                        if (!showBranch && cmbBranch.Items.Count > 0)
                        {
                            cmbBranch.SelectedIndex = 0; // Reset selection
                        }
                    }
                };
                
                // Initial visibility
                if (lblBranch != null && cmbBranch != null)
                {
                    if (!allowBranchSelection)
                    {
                        lblBranch.Visible = false;
                        cmbBranch.Visible = false;
                        cmbBranch.Enabled = false;
                    }
                    else
                    {
                        var selectedRole = GetComboSelectedRole(cmbRole);
                        var showBranch = !branchFilterId.HasValue && selectedRole.HasValue;
                        lblBranch.Visible = showBranch;
                        cmbBranch.Visible = showBranch;
                    }
                }
                
                yPos += spacing;
            }

            // Student selection for Guardian role
            Button? btnSelectStudents = null;
            Label? lblStudents = null;
            Label? lblSelectedStudents = null;
            Label? lblStudentsError = null;
            List<int> selectedStudentIds = new List<int>();
            Dictionary<int, string> studentIdToNameMap = new Dictionary<int, string>();
            
            // Show student selection when Guardian role is selected
            cmbRole.SelectedIndexChanged += (s, e) =>
            {
                var selectedRole = GetComboSelectedRole(cmbRole);
                var isGuardian = selectedRole == UserRole.Guardian;
                
                if (lblStudents != null && btnSelectStudents != null && lblSelectedStudents != null && lblStudentsError != null)
                {
                    lblStudents.Visible = isGuardian && !isEditMode;
                    btnSelectStudents.Visible = isGuardian && !isEditMode;
                    lblSelectedStudents.Visible = isGuardian && !isEditMode;
                    lblStudentsError.Visible = false;
                    
                    if (!isGuardian)
                    {
                        selectedStudentIds.Clear();
                        studentIdToNameMap.Clear();
                        if (lblSelectedStudents != null)
                        {
                            lblSelectedStudents.Text = "No students selected";
                        }
                    }
                }
            };
            
            if (!isEditMode) // Only show student selection when creating new guardian
            {
                lblStudents = new Label 
                { 
                    Text = "Select Students (Required):", 
                    Location = new Point(20, yPos), 
                    AutoSize = true,
                    Visible = false
                };
                
                btnSelectStudents = new Button
                {
                    Text = "Select Existing Student(s)",
                    Location = new Point(20, yPos + 20),
                    Size = new Size(200, 30),
                    BackColor = Color.FromArgb(59, 130, 246),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Cursor = Cursors.Hand,
                    Visible = false
                };
                btnSelectStudents.Click += async (s, e) =>
                {
                    using var selectionForm = new StudentSelectionForm(currentUser, branchFilterId);
                    if (selectionForm.ShowDialog() == DialogResult.OK)
                    {
                        selectedStudentIds = selectionForm.SelectedStudentIds.ToList();
                        
                        // Load student names for display
                        studentIdToNameMap.Clear();
                        if (selectedStudentIds.Count > 0)
                        {
                            try
                            {
                                using var connection = DatabaseHelper.GetConnection();
                                await connection.OpenAsync();
                                
                                var ids = string.Join(",", selectedStudentIds);
                                var query = $@"
                                    SELECT Id, FirstName, LastName, StudentId, GradeLevel
                                    FROM Students
                                    WHERE Id IN ({ids}) AND IsActive = 1";
                                
                                using var command = new SqlCommand(query, connection);
                                using var reader = await command.ExecuteReaderAsync();
                                while (await reader.ReadAsync())
                                {
                                    var id = reader.GetInt32("Id");
                                    var name = $"{reader.GetString("FirstName")} {reader.GetString("LastName")} ({reader.GetString("StudentId")}) - {reader.GetString("GradeLevel")}";
                                    studentIdToNameMap[id] = name;
                                }
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show($"Error loading student names: {ex.Message}", "Error",
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                        
                        // Update display
                        if (lblSelectedStudents != null)
                        {
                            if (selectedStudentIds.Count == 0)
                            {
                                lblSelectedStudents.Text = "No students selected";
                            }
                            else
                            {
                                var names = selectedStudentIds
                                    .Where(id => studentIdToNameMap.ContainsKey(id))
                                    .Select(id => studentIdToNameMap[id])
                                    .ToList();
                                lblSelectedStudents.Text = $"Selected ({selectedStudentIds.Count}): {string.Join(", ", names)}";
                            }
                        }
                        
                        if (lblStudentsError != null)
                        {
                            lblStudentsError.Visible = false;
                        }
                    }
                };
                
                lblSelectedStudents = new Label
                {
                    Text = "No students selected",
                    Location = new Point(20, yPos + 55),
                    Size = new Size(500, 40),
                    AutoSize = false,
                    ForeColor = Color.FromArgb(100, 116, 139),
                    Font = new Font("Segoe UI", 9F),
                    Visible = false
                };
                
                lblStudentsError = new Label
                {
                    Text = "At least one student must be selected",
                    Location = new Point(20, yPos + 100),
                    AutoSize = true,
                    ForeColor = Color.FromArgb(239, 68, 68),
                    Font = new Font("Segoe UI", 8F),
                    Visible = false
                };
                
                yPos += spacing + 100;
            }

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

            btnCancel.Click += (s, e) => {
                SetUserFormVisibility(false);
            };

            btnSave.Click += async (s, e) =>
            {
                try
                {
                    bool requiresPasswordValidation = !isEditMode ||
                        !string.IsNullOrWhiteSpace(txtPassword.Text) ||
                        !string.IsNullOrWhiteSpace(txtConfirmPassword.Text);

                    if (requiresPasswordValidation)
                    {
                        if (string.IsNullOrWhiteSpace(txtPassword.Text))
                        {
                            MessageBox.Show("Password is required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            txtPassword.Focus();
                            return;
                        }

                        if (string.IsNullOrWhiteSpace(txtConfirmPassword.Text))
                        {
                            MessageBox.Show("Please confirm the password.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            txtConfirmPassword.Focus();
                            return;
                        }

                        if (txtPassword.Text != txtConfirmPassword.Text)
                        {
                            MessageBox.Show("Passwords do not match.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            txtConfirmPassword.Focus();
                            return;
                        }
                    }

                    // Validate email
                    string email = txtEmail.Text.Trim();
                    if (string.IsNullOrWhiteSpace(email) || !IsValidEmail(email))
                    {
                        MessageBox.Show("Please enter a valid email address.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        txtEmail.Focus();
                        return;
                    }

                    // Validate phone number (not required for Admin)
                    string phone = "";
                    var selectedRole = GetComboSelectedRole(cmbRole) ?? UserRole.Professor;
                    bool isAdmin = selectedRole == UserRole.Admin;
                    if (isAdmin && !canManageAdmins)
                    {
                        MessageBox.Show("Only the Super Admin can create new admin accounts. Please submit a request instead.",
                            "Access Restricted", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    
                    if (!isAdmin)
                    {
                        phone = txtPhone.Text.Trim();
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
                        if (string.IsNullOrEmpty(phone))
                        {
                            lblPhoneError.Text = "Invalid phone number format.";
                            lblPhoneError.Visible = true;
                            txtPhone.BackColor = Color.FromArgb(254, 242, 242);
                            txtPhone.Focus();
                            return;
                        }
                    }

            // Check if editing an admin account
                    bool isEditingAdmin = false;
                    UserRole originalRole = UserRole.Admin;
                    if (isEditMode)
                    {
                        using var checkConnection = DatabaseHelper.GetConnection();
                        await checkConnection.OpenAsync();
                        var checkQuery = "SELECT Role FROM Users WHERE Id = @id";
                        using var checkCommand = new SqlCommand(checkQuery, checkConnection);
                        checkCommand.Parameters.AddWithValue("@id", selectedUserId);
                        var roleResult = await checkCommand.ExecuteScalarAsync();
                        if (roleResult != null)
                        {
                            originalRole = (UserRole)Convert.ToInt32(roleResult);
                            isEditingAdmin = originalRole == UserRole.Admin;
                        }
                    }

                    string verificationMethodUsed = string.Empty;

            bool isSuperAdminRole = selectedRole == UserRole.SuperAdmin;

            // For Admin creation, require admin password instead of phone verification
                    if (!isEditMode && isAdmin)
                    {
                        const string adminPassword = "admin@sti123";
                        var verified = await ShowAdminPasswordBypassDialogAsync(
                            "Admin Account Creation - Security Verification",
                            "Enter admin password to create an admin account:",
                            input => Task.FromResult(input == adminPassword),
                            "Invalid admin password. Admin account creation cancelled.");
                        if (!verified)
                        {
                            return;
                        }
                    }
                    // For non-Admin users or editing admin accounts, verify phone number with OTP
            else if (!isSuperAdminRole && (!isAdmin || (isEditMode && isEditingAdmin)))
                    {
                        string otpCode = SmsService.GenerateOtp();
                        bool otpSent = await SmsService.SendOtpAsync(phone, otpCode);

                        if (!otpSent)
                        {
                            MessageBox.Show("Failed to send verification code. Please check the phone number and try again.",
                                "Verification Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }

                        var otpMessage = isEditingAdmin 
                            ? "Admin account modification requires OTP verification. Please enter the code sent to your phone."
                            : "Phone number verification is required to continue.";
                        
                        using var otpForm = new OtpVerificationForm(phone, otpCode);
                        if (otpForm.ShowDialog() != DialogResult.OK || !otpForm.IsVerified)
                        {
                            MessageBox.Show(otpMessage,
                                "Verification Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                    }

            if (isEditMode && editedUserIsSuperAdmin)
            {
                const string adminPassword = "admin@sti123";
                var bypassVerified = await ShowAdminPasswordBypassDialogAsync(
                    "Admin Password Bypass",
                    "Enter the admin bypass password to modify Super Admin credentials:",
                    input => Task.FromResult(input == adminPassword),
                    "Invalid admin password. Super Admin update cancelled.");
                if (!bypassVerified)
                {
                    return;
                }

                verificationMethodUsed = "Admin Password Bypass";

                if (currentUser != null && currentUser.Id == selectedUserId)
                {
                    var accountVerified = await ShowAdminPasswordBypassDialogAsync(
                        "Super Admin Security Verification",
                        "Enter your current Super Admin password to confirm changes:",
                        VerifyCurrentSuperAdminPasswordAsync,
                        "Invalid password. Super Admin credentials were not updated.");
                    if (!accountVerified)
                    {
                        return;
                    }

                    verificationMethodUsed = "Admin Password Bypass + Account Password";
                }
            }
                    
                    // Prevent role downgrade for admin accounts
                    var newRole = GetComboSelectedRole(cmbRole) ?? UserRole.Student;
                    if (isEditMode && isEditingAdmin && newRole != UserRole.Admin)
                    {
                        MessageBox.Show("Cannot change Admin role to a lower permission level. This is a security restriction.",
                            "Role Change Restricted", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        SetComboSelectedRole(cmbRole, UserRole.Admin);
                        return;
                    }

                    // Determine branch assignment
                    int? assignedBranchId = null;
                    if (currentUser != null)
                    {
                        var isSuperAdmin = await BranchHelper.IsSuperAdminAsync(currentUser.Id);
                        
                        if (isSuperAdmin)
                        {
                            // Super Admin can assign branch to Admin and Professor
                            if ((selectedRole == UserRole.Admin || selectedRole == UserRole.Professor))
                            {
                                if (cmbBranch != null && cmbBranch.SelectedItem is Branch selectedBranch)
                                {
                                    assignedBranchId = selectedBranch.Id;
                                }
                                else
                                {
                                    MessageBox.Show("Please select a branch for this user.", "Validation Error",
                                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                    return;
                                }
                            }
                            else if (selectedRole == UserRole.Guardian || selectedRole == UserRole.Student)
                            {
                                // For Guardian and Student when managing all branches, use branch from form dropdown if available
                                // Otherwise, they will be assigned to a branch when their associated student is created
                                if (cmbBranch != null && cmbBranch.SelectedItem is Branch selectedBranch)
                                {
                                    assignedBranchId = selectedBranch.Id;
                                }
                            }
                        }
                        else
                        {
                            // Regular admin assigns users to their own branch
                            var branchId = await BranchHelper.GetUserBranchIdAsync(currentUser.Id);
                            if (branchId > 0)
                            {
                                assignedBranchId = branchId;
                            }
                        }
                    }

                    var user = new User
                    {
                        Username = txtUsername.Text,
                        Password = txtPassword.Text,
                        FirstName = txtFirstName.Text,
                        LastName = txtLastName.Text,
                        Email = email,
                        Phone = phone,
                        Role = GetComboSelectedRole(cmbRole) ?? UserRole.Student,
                        BranchId = assignedBranchId,
                        IsActive = true,
                    };
                    if (user.Role == UserRole.SuperAdmin)
                    {
                        user.BranchId = null;
                    }

                    var passwordChanged = !string.IsNullOrWhiteSpace(user.Password);

                    // Validate guardian student selection
                    if (user.Role == UserRole.Guardian && !isEditMode)
                    {
                        if (selectedStudentIds == null || selectedStudentIds.Count == 0)
                        {
                            if (lblStudentsError != null)
                            {
                                lblStudentsError.Visible = true;
                            }
                            MessageBox.Show("Please select at least one student for the guardian.", "Validation Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                        
                        if (lblStudentsError != null)
                        {
                            lblStudentsError.Visible = false;
                        }
                    }

                    // Prepare student IDs for guardian
                    List<int>? studentIdsForGuardian = null;
                    if (user.Role == UserRole.Guardian && !isEditMode && selectedStudentIds != null && selectedStudentIds.Count > 0)
                    {
                        studentIdsForGuardian = selectedStudentIds;
                    }

                    if (isEditMode)
                    {
                        user.Id = selectedUserId;
                        
                        // Ensure at least one active admin remains if deactivating an admin
                        if (isEditingAdmin && !user.IsActive)
                        {
                            if (await IsLastActiveAdminAsync(selectedUserId))
                            {
                                MessageBox.Show("Cannot deactivate the last active admin account. At least one admin must remain active.",
                                    "Operation Prevented", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                return;
                            }
                        }
                        
                        await UpdateUserAsync(user);

                        if (isEditMode && editedUserIsSuperAdmin && currentUser != null && !string.IsNullOrEmpty(verificationMethodUsed))
                        {
                            var details = $"PasswordChanged={passwordChanged}; Email={user.Email}; Phone={(string.IsNullOrWhiteSpace(user.Phone) ? "N/A" : user.Phone)}";
                            await SecurityAuditLogger.LogSuperAdminChangeAsync(currentUser.Id, "Credential Update", verificationMethodUsed, details);
                            MessageBox.Show("Super Admin credentials updated successfully.", "Success",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                    else
                    {
                        await AddUserAsync(user, studentIdsForGuardian);
                    }

                    SetUserFormVisibility(false);
                    await LoadUsersAsync();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error saving user: {ex.Message}", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            UIStyleHelper.ApplyRoundedButton(btnSave, 10);
            UIStyleHelper.ApplyRoundedButton(btnCancel, 10);

            var controlsList = new List<Control> {
                lblTitle, lblUsername, txtUsername, lblPassword, txtPassword,
                lblConfirmPassword, txtConfirmPassword,
                lblFirstName, txtFirstName, lblLastName, txtLastName,
                lblEmail, txtEmail, lblPhone, txtPhone, lblPhoneError, lblRole, cmbRole, lblRoleWarning,
                btnSave, btnCancel
            };
            
            // Add branch controls if they exist
            if (lblBranch != null && cmbBranch != null)
            {
                controlsList.Add(lblBranch);
                controlsList.Add(cmbBranch);
            }
            
            // Add student selection controls if they exist (for Guardian role)
            if (!isEditMode && lblStudents != null && btnSelectStudents != null && lblSelectedStudents != null && lblStudentsError != null)
            {
                controlsList.Add(lblStudents);
                controlsList.Add(btnSelectStudents);
                controlsList.Add(lblSelectedStudents);
                controlsList.Add(lblStudentsError);
            }
            
            pnlUserForm.Controls.AddRange(controlsList.ToArray());

            // Set initial phone field visibility based on role
            if (GetComboSelectedRole(cmbRole).HasValue)
            {
                updatePhoneFieldVisibility();
            }

            ThemeManager.ApplyTheme(pnlUserForm);

            // Load user data if editing
            if (isEditMode)
            {
                await LoadUserDataAsync(selectedUserId, txtUsername, txtPassword, txtConfirmPassword, txtFirstName, txtLastName, txtEmail, txtPhone, cmbRole, lblRoleWarning, cmbBranch);
                // Update phone visibility after loading (in case role changed)
                updatePhoneFieldVisibility();
                
                // Check if editing admin account
                var currentRole = GetComboSelectedRole(cmbRole);
                editedUserIsAdmin = currentRole == UserRole.Admin;
                editedUserIsSuperAdmin = currentRole == UserRole.SuperAdmin;
                editingOwnSuperAdmin = editedUserIsSuperAdmin && currentUser != null && currentUser.Id == selectedUserId;
                if (editedUserIsSuperAdmin)
                {
                    allowBranchSelection = false;
                    if (lblBranch != null)
                    {
                        lblBranch.Visible = false;
                    }
                    if (cmbBranch != null)
                    {
                        cmbBranch.Visible = false;
                        cmbBranch.Enabled = false;
                    }
                }
                if (editingOwnSuperAdmin)
                {
                    if (lblBranch != null)
                    {
                        lblBranch.Visible = false;
                    }
                    if (cmbBranch != null)
                    {
                        cmbBranch.Visible = false;
                        cmbBranch.Enabled = false;
                    }
                    cmbRole.Enabled = false;
                    lblRoleWarning.Text = "Super Admin role cannot be changed.";
                    lblRoleWarning.Visible = true;
                }
                if (currentRole == UserRole.Admin) // Admin role
                {
                    // Disable role change for admin accounts (prevent downgrade)
                    cmbRole.Enabled = false;
                    lblRoleWarning.Text = "Admin role cannot be changed. This prevents security issues.";
                    lblRoleWarning.Visible = true;
                }
            }
            
            // Monitor role changes to prevent admin downgrade
            cmbRole.SelectedIndexChanged += (s, e) =>
            {
                var newSelection = GetComboSelectedRole(cmbRole);
                if (isEditMode && editedUserIsAdmin && newSelection != UserRole.Admin)
                {
                    MessageBox.Show("Cannot change Admin role to a lower permission level. This is a security restriction.",
                        "Role Change Restricted", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    SetComboSelectedRole(cmbRole, UserRole.Admin);
                }
                if (isEditMode && editedUserIsSuperAdmin && newSelection != UserRole.SuperAdmin)
                {
                    MessageBox.Show("Super Admin role cannot be changed.", "Role Change Restricted",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    SetComboSelectedRole(cmbRole, UserRole.SuperAdmin);
                }
            };
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error showing user form: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task<bool> ShowAdminPasswordBypassDialogAsync(string title, string promptMessage, Func<string, Task<bool>> validator, string failureMessage)
        {
            using var passwordForm = new Form
            {
                Text = title,
                Size = new Size(420, 170),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                ShowInTaskbar = false
            };

            var lblPrompt = new Label
            {
                Text = promptMessage,
                Location = new Point(20, 20),
                AutoSize = true
            };

            var txtPassword = new TextBox
            {
                Location = new Point(20, 45),
                Size = new Size(360, 25),
                PasswordChar = '*',
                UseSystemPasswordChar = true
            };

            var btnOk = new Button
            {
                Text = "OK",
                Location = new Point(220, 85),
                Size = new Size(80, 30),
                DialogResult = DialogResult.OK
            };

            var btnCancel = new Button
            {
                Text = "Cancel",
                Location = new Point(310, 85),
                Size = new Size(80, 30),
                DialogResult = DialogResult.Cancel
            };

            passwordForm.Controls.AddRange(new Control[] { lblPrompt, txtPassword, btnOk, btnCancel });
            passwordForm.AcceptButton = btnOk;
            passwordForm.CancelButton = btnCancel;

            if (passwordForm.ShowDialog() != DialogResult.OK)
            {
                return false;
            }

            var input = txtPassword.Text.Trim();
            if (!await validator(input))
            {
                MessageBox.Show(failureMessage, "Authentication Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }

        private async Task<bool> VerifyCurrentSuperAdminPasswordAsync(string passwordInput)
        {
            if (currentUser == null || string.IsNullOrWhiteSpace(passwordInput))
            {
                return false;
            }

            using var connection = DatabaseHelper.GetConnection();
            await connection.OpenAsync();

            var query = "SELECT PasswordHash, PasswordSalt FROM Users WHERE Id = @id";
            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@id", currentUser.Id);

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                var hash = reader.GetString("PasswordHash");
                var salt = reader.GetString("PasswordSalt");
                return PasswordHasher.VerifyPasswordHash(passwordInput, hash, salt);
            }

            return false;
        }

        private async Task LoadUserDataAsync(int userId, TextBox txtUsername, TextBox txtPassword, TextBox txtConfirmPassword,
            TextBox txtFirstName, TextBox txtLastName, TextBox txtEmail, TextBox txtPhone, ComboBox cmbRole, Label lblRoleWarning, ComboBox? cmbBranch)
        {
            try
            {
                using var connection = DatabaseHelper.GetConnection();
                await connection.OpenAsync();

                var query = "SELECT * FROM Users WHERE Id = @id";
                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@id", userId);

                using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    txtUsername.Text = reader.GetString("Username");
                    // Clear password fields in edit mode
                    txtPassword.Text = "";
                    txtConfirmPassword.Text = "";
                    txtFirstName.Text = reader.GetString("FirstName");
                    txtLastName.Text = reader.GetString("LastName");
                    txtEmail.Text = reader.GetString("Email");
                    txtPhone.Text = reader.IsDBNull("Phone") ? "" : reader.GetString("Phone");
                    var role = (UserRole)reader.GetInt32("Role");
                    SetComboSelectedRole(cmbRole, role);
                    
                    // Load branch if available
                    if (cmbBranch != null && !reader.IsDBNull("BranchId"))
                    {
                        var branchId = reader.GetInt32("BranchId");
                        // Try to set by SelectedValue first (when using DataSource)
                        try
                        {
                            cmbBranch.SelectedValue = branchId;
                        }
                        catch
                        {
                            // Fallback: iterate through items
                            for (int i = 0; i < cmbBranch.Items.Count; i++)
                            {
                                if (cmbBranch.Items[i] is Branch branch && branch.Id == branchId)
                                {
                                    cmbBranch.SelectedIndex = i;
                                    break;
                                }
                            }
                        }
                    }
                    
                    // Check if this is an admin account
                    if (role == UserRole.Admin) // Admin
                    {
                        var username = reader.GetString("Username");
                        bool isPrimaryAdmin = await IsPrimaryAdminAccountAsync(userId, username);
                        
                        if (isPrimaryAdmin)
                        {
                            lblRoleWarning.Text = "Primary Admin Account - Role cannot be changed";
                            lblRoleWarning.Visible = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading user data: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task AddUserAsync(User user, List<int>? studentIds = null)
        {
            using var connection = DatabaseHelper.GetConnection();
            await connection.OpenAsync();
            using var transaction = connection.BeginTransaction();

            try
            {
                // Hash the password (declared only ONCE)
                PasswordHasher.CreatePasswordHash(user.Password, out string passwordHash, out string passwordSalt);

                var query = @"
                    INSERT INTO Users (Username, PasswordHash, PasswordSalt, FirstName, LastName, Email, Phone, Role, BranchId, CreatedDate, IsActive)
                    VALUES (@username, @passwordHash, @passwordSalt, @firstName, @lastName, @email, @phone, @role, @branchId, @createdDate, @isActive);
                    SELECT SCOPE_IDENTITY();";

                using var command = new SqlCommand(query, connection, transaction);
                command.Parameters.AddWithValue("@username", user.Username);
                command.Parameters.AddWithValue("@passwordHash", passwordHash);
                command.Parameters.AddWithValue("@passwordSalt", passwordSalt);
                command.Parameters.AddWithValue("@firstName", user.FirstName);
                command.Parameters.AddWithValue("@lastName", user.LastName);
                command.Parameters.AddWithValue("@email", user.Email);
                command.Parameters.AddWithValue("@phone", user.Phone);
                command.Parameters.AddWithValue("@role", (int)user.Role);
                command.Parameters.AddWithValue("@branchId", user.BranchId.HasValue ? (object)user.BranchId.Value : DBNull.Value);
                command.Parameters.AddWithValue("@createdDate", DateTime.Now);
                command.Parameters.AddWithValue("@isActive", user.IsActive);

                var userId = Convert.ToInt32(await command.ExecuteScalarAsync());

                // If guardian, link to selected students
                if (user.Role == UserRole.Guardian && studentIds != null && studentIds.Count > 0)
                {
                    var linkQuery = @"
                        INSERT INTO GuardianStudents (GuardianId, StudentId, Relationship, CreatedDate)
                        VALUES (@guardianId, @studentId, @relationship, @createdDate)";
                    
                    foreach (var studentId in studentIds)
                    {
                        using var linkCommand = new SqlCommand(linkQuery, connection, transaction);
                        linkCommand.Parameters.AddWithValue("@guardianId", userId);
                        linkCommand.Parameters.AddWithValue("@studentId", studentId);
                        linkCommand.Parameters.AddWithValue("@relationship", "Parent");
                        linkCommand.Parameters.AddWithValue("@createdDate", DateTime.Now);
                        await linkCommand.ExecuteNonQueryAsync();
                    }
                }

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        private async Task UpdateUserAsync(User user)
        {
            using var connection = DatabaseHelper.GetConnection();
            await connection.OpenAsync();

            // Validate branch access before updating
            if (currentUser != null)
            {
                var isSuperAdmin = await BranchHelper.IsSuperAdminAsync(currentUser.Id);
                if (!isSuperAdmin)
                {
                    // Check if user being updated belongs to current admin's branch
                    var checkQuery = "SELECT BranchId FROM Users WHERE Id = @id";
                    using var checkCommand = new SqlCommand(checkQuery, connection);
                    checkCommand.Parameters.AddWithValue("@id", user.Id);
                    var userBranchId = await checkCommand.ExecuteScalarAsync();
                    
                    var adminBranchId = await BranchHelper.GetUserBranchIdAsync(currentUser.Id);
                    if (userBranchId == null || userBranchId == DBNull.Value || 
                        Convert.ToInt32(userBranchId) != adminBranchId)
                    {
                        throw new UnauthorizedAccessException("You can only update users from your own branch.");
                    }
                }
                else if (branchFilterId.HasValue)
                {
                    // Super Admin with branch filter - ensure user belongs to selected branch
                    var checkQuery = "SELECT BranchId FROM Users WHERE Id = @id";
                    using var checkCommand = new SqlCommand(checkQuery, connection);
                    checkCommand.Parameters.AddWithValue("@id", user.Id);
                    var userBranchId = await checkCommand.ExecuteScalarAsync();
                    
                    if (userBranchId == null || userBranchId == DBNull.Value || 
                        Convert.ToInt32(userBranchId) != branchFilterId.Value)
                    {
                        throw new UnauthorizedAccessException("User does not belong to the selected branch.");
                    }
                }
            }

            // Check if password was changed
            bool passwordChanged = !string.IsNullOrWhiteSpace(user.Password);
            
            string query;
            if (passwordChanged)
            {
                query = @"
                    UPDATE Users 
                    SET Username = @username, PasswordHash = @passwordHash, PasswordSalt = @passwordSalt,
                        FirstName = @firstName, LastName = @lastName, Email = @email, Phone = @phone, Role = @role, BranchId = @branchId
                    WHERE Id = @id";
            }
            else
            {
                query = @"
                    UPDATE Users 
                    SET Username = @username, FirstName = @firstName, 
                        LastName = @lastName, Email = @email, Phone = @phone, Role = @role, BranchId = @branchId
                    WHERE Id = @id";
            }

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@id", user.Id);
            command.Parameters.AddWithValue("@username", user.Username);
            command.Parameters.AddWithValue("@firstName", user.FirstName);
            command.Parameters.AddWithValue("@lastName", user.LastName);
            command.Parameters.AddWithValue("@email", user.Email);
            command.Parameters.AddWithValue("@phone", user.Phone);
            command.Parameters.AddWithValue("@role", (int)user.Role);
            command.Parameters.AddWithValue("@branchId", user.BranchId.HasValue ? (object)user.BranchId.Value : DBNull.Value);
            
            if (passwordChanged)
            {
                // Hash the new password
                PasswordHasher.CreatePasswordHash(user.Password, out string passwordHash, out string passwordSalt);
                command.Parameters.AddWithValue("@passwordHash", passwordHash);
                command.Parameters.AddWithValue("@passwordSalt", passwordSalt);
            }

            await command.ExecuteNonQueryAsync();
        }

        private async Task DeleteUserAsync(int userId)
        {
            using var connection = DatabaseHelper.GetConnection();
            await connection.OpenAsync();

            // Validate branch access before deleting
            if (currentUser != null)
            {
                var isSuperAdmin = await BranchHelper.IsSuperAdminAsync(currentUser.Id);
                if (!isSuperAdmin)
                {
                    // Check if user being deleted belongs to current admin's branch
                    var checkQuery = "SELECT BranchId FROM Users WHERE Id = @id";
                    using var checkCommand = new SqlCommand(checkQuery, connection);
                    checkCommand.Parameters.AddWithValue("@id", userId);
                    var userBranchId = await checkCommand.ExecuteScalarAsync();
                    
                    var adminBranchId = await BranchHelper.GetUserBranchIdAsync(currentUser.Id);
                    if (userBranchId == null || userBranchId == DBNull.Value || 
                        Convert.ToInt32(userBranchId) != adminBranchId)
                    {
                        throw new UnauthorizedAccessException("You can only delete users from your own branch.");
                    }
                }
                else if (branchFilterId.HasValue)
                {
                    // Super Admin with branch filter - ensure user belongs to selected branch
                    var checkQuery = "SELECT BranchId FROM Users WHERE Id = @id";
                    using var checkCommand = new SqlCommand(checkQuery, connection);
                    checkCommand.Parameters.AddWithValue("@id", userId);
                    var userBranchId = await checkCommand.ExecuteScalarAsync();
                    
                    if (userBranchId == null || userBranchId == DBNull.Value || 
                        Convert.ToInt32(userBranchId) != branchFilterId.Value)
                    {
                        throw new UnauthorizedAccessException("User does not belong to the selected branch.");
                    }
                }
            }

            var query = "UPDATE Users SET IsActive = 0 WHERE Id = @id";
            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@id", userId);

            await command.ExecuteNonQueryAsync();
        }

        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return false;
            }

            return Regex.IsMatch(email.Trim(), @"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase);
        }
    }
}

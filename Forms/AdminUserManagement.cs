using StudentReportInitial.Models;
using StudentReportInitial.Data;
using System.Data.SqlClient;
using System.Data;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace StudentReportInitial.Forms
{
    public partial class AdminUserManagement : UserControl
    {
        private DataGridView dgvUsers;
        private Button btnAddUser;
        private Button btnEditUser;
        private Button btnDeleteUser;
        private Button btnRefresh;
        private TextBox txtSearch;
        private ComboBox cmbRoleFilter;
        private Panel pnlUserForm;
        private bool isEditMode = false;
        private int selectedUserId = -1;

        public AdminUserManagement()
        {
            InitializeComponent();
            ApplyModernStyling();
            LoadUsers();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // Main container
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.FromArgb(248, 250, 252);

            // Search and filter panel
            var pnlSearch = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.White,
                Padding = new Padding(20)
            };

            txtSearch = new TextBox
            {
                PlaceholderText = "Search users...",
                Size = new Size(200, 30),
                Location = new Point(20, 15)
            };
            txtSearch.TextChanged += TxtSearch_TextChanged;

            cmbRoleFilter = new ComboBox
            {
                Size = new Size(170, 30),
                Location = new Point(240, 15),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbRoleFilter.Items.AddRange(new[] { "All Roles", "Admin", "Professor", "Guardian", "Student" });
            cmbRoleFilter.SelectedIndex = 0;
            cmbRoleFilter.SelectedIndexChanged += CmbRoleFilter_SelectedIndexChanged;

            btnRefresh = new Button
            {
                Text = "Refresh",
                Size = new Size(90, 32),
                Location = new Point(420, 14),
                BackColor = Color.FromArgb(59, 130, 246),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9),
                Cursor = Cursors.Hand
            };
            btnRefresh.Click += BtnRefresh_Click;

            pnlSearch.Controls.AddRange(new Control[] { txtSearch, cmbRoleFilter, btnRefresh });

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

            pnlActions.Controls.AddRange(new Control[] { btnAddUser, btnEditUser, btnDeleteUser });

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
                Padding = new Padding(20)
            };

            this.Controls.Add(dgvUsers);
            this.Controls.Add(pnlUserForm);
            this.Controls.Add(pnlActions);
            this.Controls.Add(pnlSearch);

            UIStyleHelper.ApplyRoundedButton(btnRefresh, 10);
            UIStyleHelper.ApplyRoundedButton(btnAddUser, 10);
            UIStyleHelper.ApplyRoundedButton(btnEditUser, 10);
            UIStyleHelper.ApplyRoundedButton(btnDeleteUser, 10);

            this.ResumeLayout(false);
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

        private async void LoadUsers()
        {
            try
            {
                using var connection = DatabaseHelper.GetConnection();
                await connection.OpenAsync();

                var query = @"
                    SELECT Id, Username, FirstName, LastName, Email, Phone, Role, CreatedDate, IsActive
                    FROM Users 
                    WHERE IsActive = 1
                    ORDER BY Role, CreatedDate ASC";

                using var command = new SqlCommand(query, connection);
                using var adapter = new SqlDataAdapter(command);
                var dataTable = new DataTable();
                adapter.Fill(dataTable);

                dgvUsers.DataSource = dataTable;

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

        private void TxtSearch_TextChanged(object sender, EventArgs e)
        {
            // Implement search functionality
            if (dgvUsers.DataSource is DataTable dataTable)
            {
                var searchText = txtSearch.Text.Replace("'", "''");
                var filterParts = new List<string>();
                
                if (!string.IsNullOrWhiteSpace(searchText))
                {
                    filterParts.Add($"(FirstName LIKE '%{searchText}%' OR LastName LIKE '%{searchText}%' OR Username LIKE '%{searchText}%' OR Email LIKE '%{searchText}%')");
                }
                
                if (cmbRoleFilter.SelectedIndex > 0)
                {
                    filterParts.Add($"Role = {cmbRoleFilter.SelectedIndex}");
                }

                dataTable.DefaultView.RowFilter = filterParts.Count > 0 ? string.Join(" AND ", filterParts) : "";
            }
        }

        private void CmbRoleFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            ApplyFilters();
        }

        private void ApplyFilters()
        {
            TxtSearch_TextChanged(null, null);
        }

        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            LoadUsers();
        }

        private async void DgvUsers_SelectionChanged(object sender, EventArgs e)
        {
            btnEditUser.Enabled = dgvUsers.SelectedRows.Count > 0;
            
            if (dgvUsers.SelectedRows.Count > 0)
            {
                var selectedRow = dgvUsers.SelectedRows[0];
                var userId = Convert.ToInt32(selectedRow.Cells["Id"].Value);
                var userRole = Convert.ToInt32(selectedRow.Cells["Role"].Value);
                var username = selectedRow.Cells["Username"].Value?.ToString() ?? "";
                
                // Check if this is the primary admin account
                bool isPrimaryAdmin = await IsPrimaryAdminAccountAsync(userId, username);
                bool isAdmin = userRole == 1; // Role 1 = Admin
                
                // Disable delete for primary admin or if it's the last active admin
                bool canDelete = !isPrimaryAdmin && !(isAdmin && await IsLastActiveAdminAsync(userId));
                
                btnDeleteUser.Enabled = canDelete;
                
                // Add tooltip for disabled delete button
                if (!canDelete)
                {
                    var tooltip = new ToolTip();
                    if (isPrimaryAdmin)
                    {
                        tooltip.SetToolTip(btnDeleteUser, "Cannot delete the primary admin account. This account is required for system access.");
                    }
                    else if (isAdmin)
                    {
                        tooltip.SetToolTip(btnDeleteUser, "Cannot delete the last active admin account. At least one admin must remain active.");
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

        private async void BtnEditUser_Click(object sender, EventArgs e)
        {
            if (dgvUsers.SelectedRows.Count > 0)
            {
                selectedUserId = Convert.ToInt32(dgvUsers.SelectedRows[0].Cells["Id"].Value);
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
            var isAdmin = userRole == 1;

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
                    LoadUsers();
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
                pnlUserForm.Visible = true;
                dgvUsers.Visible = false;

                isEditMode = userId > 0;
                selectedUserId = userId;

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
            cmbRole.Items.AddRange(new[] { "Admin", "Professor", "Guardian", "Student" });
            var lblRoleWarning = new Label 
            { 
                Text = "", 
                Location = new Point(20, yPos + 48), 
                AutoSize = true,
                ForeColor = Color.FromArgb(239, 68, 68),
                Font = new Font("Segoe UI", 8F),
                Visible = false
            };
            yPos += spacing + 25;

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
                pnlUserForm.Visible = false;
                dgvUsers.Visible = true;
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

                    // Validate phone number
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
                    if (string.IsNullOrEmpty(phone))
                    {
                        lblPhoneError.Text = "Invalid phone number format.";
                        lblPhoneError.Visible = true;
                        txtPhone.BackColor = Color.FromArgb(254, 242, 242);
                        txtPhone.Focus();
                        return;
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

                    // For new users or when editing admin accounts, verify phone number with OTP
                    if (!isEditMode || isEditingAdmin)
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
                    
                    // Prevent role downgrade for admin accounts
                    var newRole = (UserRole)(cmbRole.SelectedIndex + 1);
                    if (isEditMode && isEditingAdmin && newRole != UserRole.Admin)
                    {
                        MessageBox.Show("Cannot change Admin role to a lower permission level. This is a security restriction.",
                            "Role Change Restricted", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        cmbRole.SelectedIndex = 0; // Reset to Admin
                        return;
                    }

                    var user = new User
                    {
                        Username = txtUsername.Text,
                        Password = txtPassword.Text,
                        FirstName = txtFirstName.Text,
                        LastName = txtLastName.Text,
                        Email = email,
                        Phone = phone,
                        Role = (UserRole)(cmbRole.SelectedIndex + 1),
                        IsActive = true,
                    };

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
                    }
                    else
                    {
                        await AddUserAsync(user);
                    }

                    pnlUserForm.Visible = false;
                    dgvUsers.Visible = true;
                    LoadUsers();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error saving user: {ex.Message}", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            UIStyleHelper.ApplyRoundedButton(btnSave, 10);
            UIStyleHelper.ApplyRoundedButton(btnCancel, 10);

            pnlUserForm.Controls.AddRange(new Control[] {
                lblTitle, lblUsername, txtUsername, lblPassword, txtPassword,
                lblConfirmPassword, txtConfirmPassword,
                lblFirstName, txtFirstName, lblLastName, txtLastName,
                lblEmail, txtEmail, lblPhone, txtPhone, lblPhoneError, lblRole, cmbRole, lblRoleWarning,
                btnSave, btnCancel
            });

            // Load user data if editing
            if (isEditMode)
            {
                await LoadUserDataAsync(selectedUserId, txtUsername, txtPassword, txtConfirmPassword, txtFirstName, txtLastName, txtEmail, txtPhone, cmbRole, lblRoleWarning);
                
                // Check if editing admin account
                var currentRole = cmbRole.SelectedIndex + 1;
                if (currentRole == 1) // Admin role
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
                if (isEditMode && cmbRole.SelectedIndex + 1 < 1) // Trying to change from Admin
                {
                    MessageBox.Show("Cannot change Admin role to a lower permission level. This is a security restriction.",
                        "Role Change Restricted", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    cmbRole.SelectedIndex = 0; // Reset to Admin
                }
            };
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error showing user form: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task LoadUserDataAsync(int userId, TextBox txtUsername, TextBox txtPassword, TextBox txtConfirmPassword,
            TextBox txtFirstName, TextBox txtLastName, TextBox txtEmail, TextBox txtPhone, ComboBox cmbRole, Label lblRoleWarning)
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
                    var role = reader.GetInt32("Role");
                    cmbRole.SelectedIndex = role - 1;
                    
                    // Check if this is an admin account
                    if (role == 1) // Admin
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

        private async Task AddUserAsync(User user)
        {
            using var connection = DatabaseHelper.GetConnection();
            await connection.OpenAsync();

            // Hash the password (declared only ONCE)
            PasswordHasher.CreatePasswordHash(user.Password, out string passwordHash, out string passwordSalt);

            var query = @"
        INSERT INTO Users (Username, PasswordHash, PasswordSalt, FirstName, LastName, Email, Phone, Role, CreatedDate, IsActive)
        VALUES (@username, @passwordHash, @passwordSalt, @firstName, @lastName, @email, @phone, @role, @createdDate, @isActive)";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@username", user.Username);
            command.Parameters.AddWithValue("@passwordHash", passwordHash);
            command.Parameters.AddWithValue("@passwordSalt", passwordSalt);
            command.Parameters.AddWithValue("@firstName", user.FirstName);
            command.Parameters.AddWithValue("@lastName", user.LastName);
            command.Parameters.AddWithValue("@email", user.Email);
            command.Parameters.AddWithValue("@phone", user.Phone);
            command.Parameters.AddWithValue("@role", (int)user.Role);
            command.Parameters.AddWithValue("@createdDate", DateTime.Now);
            command.Parameters.AddWithValue("@isActive", user.IsActive);

            await command.ExecuteNonQueryAsync();
        }

        private async Task UpdateUserAsync(User user)
        {
            using var connection = DatabaseHelper.GetConnection();
            await connection.OpenAsync();

            // Check if password was changed
            bool passwordChanged = !string.IsNullOrWhiteSpace(user.Password);
            
            string query;
            if (passwordChanged)
            {
                query = @"
                    UPDATE Users 
                    SET Username = @username, PasswordHash = @passwordHash, PasswordSalt = @passwordSalt,
                        FirstName = @firstName, LastName = @lastName, Email = @email, Phone = @phone, Role = @role
                    WHERE Id = @id";
            }
            else
            {
                query = @"
                    UPDATE Users 
                    SET Username = @username, FirstName = @firstName, 
                        LastName = @lastName, Email = @email, Phone = @phone, Role = @role
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

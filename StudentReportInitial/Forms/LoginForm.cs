using StudentReportInitial.Models;
using StudentReportInitial.Data;
using System.Data.SqlClient;
using System.Windows.Forms;
using System.Drawing;

namespace StudentReportInitial.Forms
{
    public partial class LoginForm : Form
    {
        public User? CurrentUser { get; private set; }

        public LoginForm()
        {
            InitializeComponent();
            ApplyModernStyling();
        }

        private void ApplyModernStyling()
        {
            this.BackColor = Color.FromArgb(240, 244, 248);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Size = new Size(400, 500);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Text = "Student Report System - Login";

            // Title label
            lblTitle.ForeColor = Color.FromArgb(51, 65, 85);
            lblTitle.Font = new Font("Segoe UI", 24F, FontStyle.Bold);
            lblTitle.TextAlign = ContentAlignment.MiddleCenter;

            // Subtitle label
            lblSubtitle.ForeColor = Color.FromArgb(100, 116, 139);
            lblSubtitle.Font = new Font("Segoe UI", 10F);
            lblSubtitle.TextAlign = ContentAlignment.MiddleCenter;

            // Username label and textbox
            lblUsername.ForeColor = Color.FromArgb(51, 65, 85);
            lblUsername.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            txtUsername.Font = new Font("Segoe UI", 11F);
            txtUsername.BorderStyle = BorderStyle.FixedSingle;
            txtUsername.BackColor = Color.White;
            txtUsername.ForeColor = Color.FromArgb(51, 65, 85);

            // Password label and textbox
            lblPassword.ForeColor = Color.FromArgb(51, 65, 85);
            lblPassword.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            txtPassword.Font = new Font("Segoe UI", 11F);
            txtPassword.BorderStyle = BorderStyle.FixedSingle;
            txtPassword.BackColor = Color.White;
            txtPassword.ForeColor = Color.FromArgb(51, 65, 85);

            // Login button
            btnLogin.BackColor = Color.FromArgb(59, 130, 246);
            btnLogin.ForeColor = Color.White;
            btnLogin.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            btnLogin.FlatStyle = FlatStyle.Flat;
            btnLogin.FlatAppearance.BorderSize = 0;
            btnLogin.Cursor = Cursors.Hand;

            // Error label
            lblError.ForeColor = Color.FromArgb(239, 68, 68);
            lblError.Font = new Font("Segoe UI", 9F);
            lblError.TextAlign = ContentAlignment.MiddleCenter;
            lblError.Visible = false;
        }

        private async void btnLogin_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtUsername.Text) || string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                ShowError("Please enter both username and password.");
                return;
            }

            btnLogin.Enabled = false;
            btnLogin.Text = "Logging in...";

            try
            {
                CurrentUser = await AuthenticateUserAsync(txtUsername.Text, txtPassword.Text);
                if (CurrentUser != null)
                {
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    ShowError("Invalid username or password.");
                }
            }
            catch (Exception ex)
            {
                ShowError($"Login failed: {ex.Message}");
            }
            finally
            {
                btnLogin.Enabled = true;
                btnLogin.Text = "Login";
            }
        }

        private async Task<User?> AuthenticateUserAsync(string username, string password)
        {
            using var connection = DatabaseHelper.GetConnection();
            await connection.OpenAsync();

            var query = @"
                SELECT Id, Username, PasswordHash, PasswordSalt, FirstName, LastName, Email, Phone, Role, CreatedDate, IsActive
                FROM Users 
                WHERE Username = @username AND IsActive = 1";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@username", username);

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                var storedHash = reader.GetString(reader.GetOrdinal("PasswordHash"));
                var storedSalt = reader.GetString(reader.GetOrdinal("PasswordSalt"));

                // Verify the password
                if (PasswordHasher.VerifyPasswordHash(password, storedHash, storedSalt))
                {
                    var user = new User();
                    user.Id = reader.GetInt32(reader.GetOrdinal("Id"));
                    user.Username = reader.GetString(reader.GetOrdinal("Username"));
                    user.Password = ""; // Don't store the password in memory
                    user.FirstName = reader.GetString(reader.GetOrdinal("FirstName"));
                    user.LastName = reader.GetString(reader.GetOrdinal("LastName"));
                    user.Email = reader.GetString(reader.GetOrdinal("Email"));
                    user.Phone = reader.IsDBNull(reader.GetOrdinal("Phone")) ? "" : reader.GetString(reader.GetOrdinal("Phone"));
                    user.Role = (UserRole)reader.GetInt32(reader.GetOrdinal("Role"));
                    user.CreatedDate = reader.GetDateTime(reader.GetOrdinal("CreatedDate"));
                    user.IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive"));
                    return user;
                }
            }

            return null;
        }

        private void ShowError(string message)
        {
            lblError.Text = message;
            lblError.Visible = true;
            timerError.Stop();
            timerError.Start();
        }

        private void timerError_Tick(object sender, EventArgs e)
        {
            lblError.Visible = false;
            timerError.Stop();
        }

        private void txtPassword_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                btnLogin_Click(sender, e);
            }
        }
    }
}

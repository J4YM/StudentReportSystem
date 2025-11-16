using StudentReportInitial.Models;
using StudentReportInitial.Data;
using System.Data.SqlClient;
using System.Windows.Forms;
using System.Drawing;
using System.IO;

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
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(700, 420);
            MinimumSize = new Size(700, 420);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = false;
            Text = "STI College Baliuag - AimONE - Login";
            AcceptButton = btnLogin;

            // Set background image - try multiple formats
            try
            {
                string imagesFolder = Path.Combine(Application.StartupPath, "Images");
                string[] imageExtensions = { ".jpg", ".jpeg", ".png", ".bmp" };
                string imageName = "sti_baliuag_building";
                string? imagePath = null;

                // Try to find the image with any supported extension
                foreach (var ext in imageExtensions)
                {
                    string testPath = Path.Combine(imagesFolder, imageName + ext);
                    if (File.Exists(testPath))
                    {
                        imagePath = testPath;
                        break;
                    }
                }

                if (imagePath != null && File.Exists(imagePath))
                {
                    BackgroundImage = Image.FromFile(imagePath);
                    BackgroundImageLayout = ImageLayout.Stretch;
                    BackColor = Color.FromArgb(245, 247, 250); // Fallback color
                }
                else
                {
                    // If image not found, use solid color
                    BackColor = Color.FromArgb(245, 247, 250);
                }
            }
            catch
            {
                // If image loading fails, use solid color
                BackColor = Color.FromArgb(245, 247, 250);
            }

            // Hero panel styling with semi-transparent overlay for better text visibility
            pnlHero.Padding = new Padding(32, 56, 32, 32);
            // Use custom paint for semi-transparent overlay
            pnlHero.Paint += (s, e) =>
            {
                using (var brush = new SolidBrush(Color.FromArgb(200, 37, 99, 235)))
                {
                    e.Graphics.FillRectangle(brush, pnlHero.ClientRectangle);
                }
            };
            pnlHero.BackColor = Color.Transparent; // Make panel transparent so custom paint shows
            lblTitle.Font = new Font("Segoe UI", 20F, FontStyle.Bold);
            lblTitle.AutoSize = true;
            lblTitle.MaximumSize = new Size(200, 0);
            lblTitle.TextAlign = ContentAlignment.TopLeft;
            lblTitle.ForeColor = Color.White;
            lblTitle.BackColor = Color.Transparent;
            lblHeroSubtitle.Font = new Font("Segoe UI", 10F);
            lblHeroSubtitle.MaximumSize = new Size(200, 0);
            lblHeroSubtitle.ForeColor = Color.White;
            lblHeroSubtitle.BackColor = Color.Transparent;

            // Login card styling - make it semi-transparent to show background
            pnlLogin.BackColor = Color.Transparent;
            pnlLoginCard.BackColor = Color.White;
            pnlLoginCard.BorderStyle = BorderStyle.None;
            UIStyleHelper.ApplyRoundedCorners(pnlLoginCard, 18, drawBorder: true);

            lblLoginHeading.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            lblLoginHeading.Text = "Welcome back";
            lblSubtitle.Font = new Font("Segoe UI", 10F);
            lblSubtitle.ForeColor = Color.FromArgb(100, 116, 139);

            lblUsername.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblUsername.ForeColor = Color.FromArgb(51, 65, 85);
            txtUsername.Font = new Font("Segoe UI", 10F);
            txtUsername.BorderStyle = BorderStyle.FixedSingle;
            txtUsername.BackColor = Color.White;
            txtUsername.ForeColor = Color.FromArgb(51, 65, 85);
            txtUsername.PlaceholderText = "Enter your username";

            lblPassword.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblPassword.ForeColor = Color.FromArgb(51, 65, 85);
            txtPassword.Font = new Font("Segoe UI", 10F);
            txtPassword.BorderStyle = BorderStyle.FixedSingle;
            txtPassword.BackColor = Color.White;
            txtPassword.ForeColor = Color.FromArgb(51, 65, 85);
            txtPassword.PlaceholderText = "Enter your password";

            btnLogin.BackColor = Color.FromArgb(37, 99, 235);
            btnLogin.ForeColor = Color.White;
            btnLogin.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            btnLogin.FlatStyle = FlatStyle.Flat;
            btnLogin.FlatAppearance.BorderSize = 0;
            btnLogin.Cursor = Cursors.Hand;
            btnLogin.FlatAppearance.MouseOverBackColor = Color.FromArgb(29, 78, 216);
            btnLogin.FlatAppearance.MouseDownBackColor = Color.FromArgb(30, 64, 175);
            UIStyleHelper.ApplyRoundedButton(btnLogin, 14);

            lblError.ForeColor = Color.FromArgb(239, 68, 68);
            lblError.Font = new Font("Segoe UI", 9F);
            lblError.TextAlign = ContentAlignment.MiddleLeft;
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

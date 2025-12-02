using StudentReportInitial.Models;
using StudentReportInitial.Data;
using System.Data.SqlClient;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using StudentReportInitial.Theming;
using System;
using System.Threading.Tasks;

namespace StudentReportInitial.Forms
{
    public partial class LoginForm : Form
    {
        public User? CurrentUser { get; private set; }
        private bool passwordVisible = false;

        public LoginForm()
        {
            InitializeComponent();
            ApplyModernStyling();
            ThemeManager.ApplyTheme(this);
        }

        private void ApplyModernStyling()
        {
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(1100, 650);
            MinimumSize = new Size(1100, 650);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = false;
            Text = "STI College - AimONE - Login";
            AcceptButton = btnLogin;
            DoubleBuffered = true;

            // Left illustration section typography
            pnlHero.BackColor = Color.Transparent;
            lblTitle.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblTitle.Text = "STI College Login Portal";
            lblTitle.ForeColor = Color.White;
            lblTitle.TextAlign = ContentAlignment.MiddleCenter;
            lblTitle.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lblHeroSubtitle.Font = new Font("Segoe UI", 10F, FontStyle.Regular);
            lblHeroSubtitle.ForeColor = Color.White;
            lblHeroSubtitle.Text = string.Empty;

            // Right login section styling
            pnlLogin.BackColor = Color.Transparent;
            pnlLoginCard.BackColor = Color.Transparent;
            pnlLoginCard.BorderStyle = BorderStyle.None;

            lblLoginHeading.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblLoginHeading.Text = "STI Student Portal Login";
            lblLoginHeading.ForeColor = ColorTranslator.FromHtml("#002B5C");

            lblSubtitle.Font = new Font("Segoe UI", 11F, FontStyle.Regular);
            lblSubtitle.ForeColor = Color.FromArgb(100, 116, 139);

            lblUsername.Font = new Font("Segoe UI", 10.5F, FontStyle.Regular);
            lblUsername.ForeColor = Color.FromArgb(51, 65, 85);
            txtUsername.Font = new Font("Segoe UI", 11F, FontStyle.Regular);
            txtUsername.BorderStyle = BorderStyle.FixedSingle;
            txtUsername.BackColor = Color.White;
            txtUsername.ForeColor = Color.FromArgb(31, 41, 55);
            txtUsername.PlaceholderText = "Enter your STI email";

            lblPassword.Font = new Font("Segoe UI", 10.5F, FontStyle.Regular);
            lblPassword.ForeColor = Color.FromArgb(51, 65, 85);
            txtPassword.Font = new Font("Segoe UI", 11F, FontStyle.Regular);
            txtPassword.BorderStyle = BorderStyle.FixedSingle;
            txtPassword.BackColor = Color.White;
            txtPassword.ForeColor = Color.FromArgb(31, 41, 55);
            txtPassword.PlaceholderText = "Enter your password";
            txtPassword.UseSystemPasswordChar = true;

            btnTogglePassword.FlatStyle = FlatStyle.Flat;
            btnTogglePassword.FlatAppearance.BorderSize = 0;
            btnTogglePassword.BackColor = Color.FromArgb(241, 245, 249);
            btnTogglePassword.ForeColor = Color.FromArgb(51, 65, 85);
            btnTogglePassword.Cursor = Cursors.Hand;
            btnTogglePassword.Font = new Font("Segoe UI", 9F);
            UIStyleHelper.ApplyRoundedButton(btnTogglePassword, 10);

            btnLogin.BackColor = ColorTranslator.FromHtml("#FFDD00");
            btnLogin.ForeColor = ColorTranslator.FromHtml("#002B5C");
            btnLogin.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold);
            btnLogin.FlatStyle = FlatStyle.Flat;
            btnLogin.FlatAppearance.BorderSize = 0;
            btnLogin.Cursor = Cursors.Hand;
            UIStyleHelper.ApplyRoundedButton(btnLogin, 10);

            lblError.ForeColor = Color.FromArgb(239, 68, 68);
            lblError.Font = new Font("Segoe UI", 9F);
            lblError.TextAlign = ContentAlignment.MiddleLeft;
            lblError.Visible = false;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            var rect = ClientRectangle;
            if (rect.Width <= 0 || rect.Height <= 0)
            {
                return;
            }

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            using (var brush = new LinearGradientBrush(
                       rect,
                       ColorTranslator.FromHtml("#004A99"),
                       ColorTranslator.FromHtml("#0074CC"),
                       LinearGradientMode.Horizontal))
            {
                e.Graphics.FillRectangle(brush, rect);
            }

            // Soft shadow behind main card panel (if present)
            if (pnlLogin != null)
            {
                var bounds = pnlLogin.Bounds;
                bounds.Offset(10, 14);
                using var shadowBrush = new SolidBrush(Color.FromArgb(50, 0, 0, 0));
                e.Graphics.FillRectangle(shadowBrush, bounds);
            }
        }

        private void pnlHero_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            var bounds = pnlHero.ClientRectangle;
            if (bounds.Width <= 0 || bounds.Height <= 0)
            {
                return;
            }

            // Large circular gradient
            int circleSize = Math.Min(bounds.Width, bounds.Height) - 80;
            var circleRect = new Rectangle(
                bounds.Left + (bounds.Width - circleSize) / 2,
                bounds.Top + 80,
                circleSize,
                circleSize);

            using (var circleBrush = new LinearGradientBrush(
                       circleRect,
                       Color.FromArgb(180, 127, 184, 255),
                       Color.FromArgb(220, 224, 242, 255),
                       LinearGradientMode.ForwardDiagonal))
            {
                g.FillEllipse(circleBrush, circleRect);
            }

            // Graduation cap icon
            var capCenter = new Point(circleRect.Left + circleRect.Width / 2, circleRect.Top + circleRect.Height / 2);
            var capWidth = circleRect.Width / 2;
            var capHeight = capWidth / 3;

            Point[] capTop =
            {
                new Point(capCenter.X - capWidth / 2, capCenter.Y),
                new Point(capCenter.X, capCenter.Y - capHeight),
                new Point(capCenter.X + capWidth / 2, capCenter.Y),
                new Point(capCenter.X, capCenter.Y + capHeight)
            };

            using (var capBrush = new SolidBrush(Color.White))
            using (var capPen = new Pen(Color.FromArgb(0, 43, 92), 2))
            {
                g.FillPolygon(capBrush, capTop);
                g.DrawPolygon(capPen, capTop);

                // Tassel
                var tasselStart = new Point(capCenter.X + capWidth / 2, capCenter.Y);
                var tasselEnd = new Point(tasselStart.X + 18, tasselStart.Y + 28);
                g.DrawLine(capPen, tasselStart, tasselEnd);
                g.FillEllipse(capBrush, tasselEnd.X - 3, tasselEnd.Y - 3, 6, 6);
            }

            // Decorative dots and triangles
            using var yellowBrush = new SolidBrush(ColorTranslator.FromHtml("#FFDD00"));
            using var blueBrush = new SolidBrush(ColorTranslator.FromHtml("#7FB8FF"));

            g.FillEllipse(yellowBrush, circleRect.Left - 12, circleRect.Top + 20, 8, 8);
            g.FillEllipse(yellowBrush, circleRect.Right + 6, circleRect.Top + 40, 10, 10);
            g.FillEllipse(blueBrush, circleRect.Left + 30, circleRect.Bottom + 6, 10, 10);

            Point[] triangle1 =
            {
                new Point(circleRect.Left - 20, circleRect.Bottom - 10),
                new Point(circleRect.Left - 4, circleRect.Bottom - 2),
                new Point(circleRect.Left - 12, circleRect.Bottom + 10)
            };
            g.FillPolygon(blueBrush, triangle1);

            // Simple book icon near circle
            var bookRect = new Rectangle(circleRect.Right - 80, circleRect.Bottom - 60, 48, 34);
            using var bookBrush = new SolidBrush(Color.FromArgb(250, 250, 250));
            using var bookPen = new Pen(ColorTranslator.FromHtml("#FFDD00"), 2);
            g.FillRectangle(bookBrush, bookRect);
            g.DrawRectangle(bookPen, bookRect);
            g.DrawLine(bookPen, bookRect.Left + bookRect.Width / 2, bookRect.Top,
                bookRect.Left + bookRect.Width / 2, bookRect.Bottom);
        }

        private void pnlLoginCard_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Email icon near username textbox
            var centerEmail = new Point(txtUsername.Left - 30, txtUsername.Top + txtUsername.Height / 2);
            DrawEnvelopeIcon(g, centerEmail);

            // Lock icon near password textbox
            var centerLock = new Point(txtPassword.Left - 30, txtPassword.Top + txtPassword.Height / 2);
            DrawLockIcon(g, centerLock);
        }

        private static void DrawEnvelopeIcon(Graphics g, Point center)
        {
            var size = 16;
            var rect = new Rectangle(center.X - size / 2, center.Y - size / 2, size, size);
            using var pen = new Pen(Color.White, 1.5f);
            using var brush = new SolidBrush(Color.FromArgb(255, 221, 0));

            g.FillRectangle(brush, rect);
            g.DrawRectangle(pen, rect);

            // Envelope flap
            g.DrawLine(pen, rect.Left, rect.Top, rect.Left + rect.Width / 2, rect.Top + rect.Height / 2);
            g.DrawLine(pen, rect.Right, rect.Top, rect.Left + rect.Width / 2, rect.Top + rect.Height / 2);
        }

        private static void DrawLockIcon(Graphics g, Point center)
        {
            var size = 16;
            var bodyRect = new Rectangle(center.X - size / 2, center.Y - size / 4, size, size / 2 + 4);
            using var brush = new SolidBrush(Color.FromArgb(0, 43, 92));
            using var pen = new Pen(Color.White, 1.5f);

            // Body
            g.FillRectangle(brush, bodyRect);
            g.DrawRectangle(pen, bodyRect);

            // Shackle
            var shackleRect = new Rectangle(center.X - size / 4, center.Y - size / 2, size / 2, size / 2);
            g.DrawArc(pen, shackleRect, 200, 140);
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

        private void btnTogglePassword_Click(object sender, EventArgs e)
        {
            passwordVisible = !passwordVisible;
            txtPassword.UseSystemPasswordChar = !passwordVisible;
            if (passwordVisible)
            {
                txtPassword.PasswordChar = '\0';
                btnTogglePassword.Text = "Hide";
            }
            else
            {
                txtPassword.PasswordChar = '*';
                btnTogglePassword.Text = "Show";
            }
        }

        private async void lnkForgotPassword_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            // Forgot Password flow restricted to non-admin roles with OTP verification
            try
            {
                var identifier = PromptForUsernameOrEmail();
                if (string.IsNullOrWhiteSpace(identifier))
                {
                    return;
                }

                int userId = 0;
                string? username = null;
                string? roleName = null;
                string? phone = null;
                UserRole? role = null;

                using var connection = DatabaseHelper.GetConnection();
                await connection.OpenAsync();

                var lookupQuery = @"
                    SELECT Id, Username, Email, Phone, Role
                    FROM Users
                    WHERE (Username = @identifier OR Email = @identifier) AND IsActive = 1";

                using (var lookupCommand = new SqlCommand(lookupQuery, connection))
                {
                    lookupCommand.Parameters.AddWithValue("@identifier", identifier);

                    using var reader = await lookupCommand.ExecuteReaderAsync();
                    if (!await reader.ReadAsync())
                    {
                        await SecurityAuditLogger.LogSuperAdminChangeAsync(
                            0,
                            "Password Reset Attempt (Unknown User)",
                            "Forgot Password",
                            $"Identifier={identifier}");

                        MessageBox.Show(
                            "Account not found. If you believe this is an error, please contact the system administrator.",
                            "Account Not Found",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);
                        return;
                    }

                    userId = reader.GetInt32(reader.GetOrdinal("Id"));
                    username = reader.GetString(reader.GetOrdinal("Username"));
                    phone = reader.IsDBNull(reader.GetOrdinal("Phone"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("Phone"));
                    var roleValue = reader.GetInt32(reader.GetOrdinal("Role"));
                    role = (UserRole)roleValue;
                    roleName = role.ToString();
                }

                bool isAdminRole = role == UserRole.Admin || role == UserRole.SuperAdmin;
                if (isAdminRole)
                {
                    await SecurityAuditLogger.LogSuperAdminChangeAsync(
                        userId,
                        "Password Reset Attempt (Blocked - Admin Role)",
                        "Forgot Password",
                        $"Username={username}; Identifier={identifier}; Role={roleName}");

                    MessageBox.Show(
                        "Admin password reset not allowed. Contact system administrator.",
                        "Not Allowed",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    return;
                }

                if (string.IsNullOrWhiteSpace(phone))
                {
                    await SecurityAuditLogger.LogSuperAdminChangeAsync(
                        userId,
                        "Password Reset Attempt (Failed - Missing Phone)",
                        "Forgot Password",
                        $"Username={username}; Identifier={identifier}; Role={roleName}");

                    MessageBox.Show(
                        "Phone number is required for password reset. Please contact system administrator.",
                        "Verification Required",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                // Send OTP using existing SMS service
                string otpCode = SmsService.GenerateOtp();
                bool otpSent = await SmsService.SendOtpAsync(phone, otpCode);

                if (!otpSent)
                {
                    await SecurityAuditLogger.LogSuperAdminChangeAsync(
                        userId,
                        "Password Reset Attempt (Failed - OTP Send)",
                        "Forgot Password",
                        $"Username={username}; Identifier={identifier}; Role={roleName}");

                    MessageBox.Show(
                        "Failed to send verification code. Please try again later.",
                        "Verification Failed",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    return;
                }

                using (var otpForm = new OtpVerificationForm(phone!, otpCode))
                {
                    if (otpForm.ShowDialog(this) != DialogResult.OK || !otpForm.IsVerified)
                    {
                        await SecurityAuditLogger.LogSuperAdminChangeAsync(
                            userId,
                            "Password Reset Attempt (Failed - OTP Verification)",
                            "Forgot Password + OTP",
                            $"Username={username}; Identifier={identifier}; Role={roleName}");

                        MessageBox.Show(
                            "OTP verification is required to reset password.",
                            "Verification Required",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);
                        return;
                    }
                }

                var newPassword = PromptForNewPassword();
                if (string.IsNullOrWhiteSpace(newPassword))
                {
                    await SecurityAuditLogger.LogSuperAdminChangeAsync(
                        userId,
                        "Password Reset Attempt (Cancelled - New Password Entry)",
                        "Forgot Password + OTP",
                        $"Username={username}; Identifier={identifier}; Role={roleName}");
                    return;
                }

                PasswordHasher.CreatePasswordHash(newPassword, out string newPasswordHash, out string newPasswordSalt);

                var updateQuery = @"
                    UPDATE Users
                    SET PasswordHash = @passwordHash,
                        PasswordSalt = @passwordSalt
                    WHERE Id = @id";

                using (var updateCommand = new SqlCommand(updateQuery, connection))
                {
                    updateCommand.Parameters.AddWithValue("@passwordHash", newPasswordHash);
                    updateCommand.Parameters.AddWithValue("@passwordSalt", newPasswordSalt);
                    updateCommand.Parameters.AddWithValue("@id", userId);
                    await updateCommand.ExecuteNonQueryAsync();
                }

                await SecurityAuditLogger.LogSuperAdminChangeAsync(
                    userId,
                    "Password Reset Attempt (Success)",
                    "Forgot Password + OTP",
                    $"Username={username}; Identifier={identifier}; Role={roleName}");

                MessageBox.Show(
                    "Your password has been reset successfully. You can now log in with your new password.",
                    "Password Reset Successful",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error processing password reset: {ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private string? PromptForUsernameOrEmail()
        {
            using var form = new Form();
            form.Text = "Forgot Password";
            form.StartPosition = FormStartPosition.CenterParent;
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.MaximizeBox = false;
            form.MinimizeBox = false;
            form.ClientSize = new Size(420, 170);
            form.BackColor = Color.FromArgb(248, 250, 252);

            var lbl = new Label
            {
                Text = "Enter your username or email:",
                AutoSize = false,
                Location = new Point(20, 20),
                Size = new Size(380, 20),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.FromArgb(51, 65, 85)
            };

            var txt = new TextBox
            {
                Location = new Point(20, 50),
                Size = new Size(380, 25)
            };

            var btnOk = new Button
            {
                Text = "Continue",
                DialogResult = DialogResult.OK,
                Location = new Point(210, 100),
                Size = new Size(90, 30),
                BackColor = Color.FromArgb(37, 99, 235),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            var btnCancel = new Button
            {
                Text = "Cancel",
                DialogResult = DialogResult.Cancel,
                Location = new Point(310, 100),
                Size = new Size(90, 30),
                BackColor = Color.FromArgb(107, 114, 128),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };

            btnOk.FlatAppearance.BorderSize = 0;
            btnCancel.FlatAppearance.BorderSize = 0;
            UIStyleHelper.ApplyRoundedButton(btnOk, 8);
            UIStyleHelper.ApplyRoundedButton(btnCancel, 8);

            form.Controls.Add(lbl);
            form.Controls.Add(txt);
            form.Controls.Add(btnOk);
            form.Controls.Add(btnCancel);

            form.AcceptButton = btnOk;
            form.CancelButton = btnCancel;

            var result = form.ShowDialog(this);
            if (result == DialogResult.OK)
            {
                return txt.Text.Trim();
            }

            return null;
        }

        private string? PromptForNewPassword()
        {
            using var form = new Form();
            form.Text = "Reset Password";
            form.StartPosition = FormStartPosition.CenterParent;
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.MaximizeBox = false;
            form.MinimizeBox = false;
            form.ClientSize = new Size(420, 220);
            form.BackColor = Color.FromArgb(248, 250, 252);

            var lblNew = new Label
            {
                Text = "New Password",
                AutoSize = false,
                Location = new Point(20, 20),
                Size = new Size(380, 20),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.FromArgb(51, 65, 85)
            };
            var txtNew = new TextBox
            {
                Location = new Point(20, 40),
                Size = new Size(380, 25),
                UseSystemPasswordChar = true
            };

            var lblConfirm = new Label
            {
                Text = "Confirm Password",
                AutoSize = false,
                Location = new Point(20, 80),
                Size = new Size(380, 20),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.FromArgb(51, 65, 85)
            };
            var txtConfirm = new TextBox
            {
                Location = new Point(20, 100),
                Size = new Size(380, 25),
                UseSystemPasswordChar = true
            };

            string? passwordToReturn = null;

            var btnOk = new Button
            {
                Text = "Reset Password",
                Location = new Point(190, 150),
                Size = new Size(120, 30),
                BackColor = Color.FromArgb(34, 197, 94),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            var btnCancel = new Button
            {
                Text = "Cancel",
                DialogResult = DialogResult.Cancel,
                Location = new Point(320, 150),
                Size = new Size(80, 30),
                BackColor = Color.FromArgb(107, 114, 128),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };

            btnOk.FlatAppearance.BorderSize = 0;
            btnCancel.FlatAppearance.BorderSize = 0;
            UIStyleHelper.ApplyRoundedButton(btnOk, 8);
            UIStyleHelper.ApplyRoundedButton(btnCancel, 8);

            btnOk.Click += (s, e) =>
            {
                var newPassword = txtNew.Text;
                var confirmPassword = txtConfirm.Text;

                if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 6)
                {
                    MessageBox.Show(
                        "New password must be at least 6 characters long.",
                        "Validation",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    form.DialogResult = DialogResult.None;
                    return;
                }

                if (newPassword != confirmPassword)
                {
                    MessageBox.Show(
                        "New password and confirm password do not match.",
                        "Validation",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    form.DialogResult = DialogResult.None;
                    return;
                }

                passwordToReturn = newPassword;
                form.DialogResult = DialogResult.OK;
            };

            form.Controls.AddRange(new Control[]
            {
                lblNew, txtNew, lblConfirm, txtConfirm, btnOk, btnCancel
            });

            form.AcceptButton = btnOk;
            form.CancelButton = btnCancel;

            var result = form.ShowDialog(this);
            return result == DialogResult.OK ? passwordToReturn : null;
        }

        private void lblHeroSubtitle_Click(object sender, EventArgs e)
        {

        }
    }
}

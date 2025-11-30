using StudentReportInitial.Data;
using System.Windows.Forms;

namespace StudentReportInitial.Forms
{
    public partial class OtpVerificationForm : Form
    {
        private TextBox txtOtp;
        private Button btnVerify;
        private Button btnResend;
        private Button btnCancel;
        private Label lblMessage;
        private string phoneNumber;
        private string otpCode;
        private System.Windows.Forms.Timer countdownTimer;
        private int remainingSeconds = 600; // 10 minutes
        private Label lblCountdown;

        public bool IsVerified { get; private set; } = false;

        public OtpVerificationForm(string phoneNumber, string otpCode)
        {
            this.phoneNumber = phoneNumber;
            this.otpCode = otpCode;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Verify Phone Number";
            this.Size = new Size(400, 320); // Increased height to show all buttons
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.FromArgb(248, 250, 252);
            this.Padding = new Padding(10); // Add padding to prevent content from touching edges

            var yPos = 20;

            // Title
            var lblTitle = new Label
            {
                Text = "Phone Number Verification",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 41, 59),
                Location = new Point(20, yPos),
                AutoSize = true
            };
            yPos += 40;

            // Message
            lblMessage = new Label
            {
                Text = $"We've sent a verification code to:\n{PhoneValidator.FormatPhoneNumber(phoneNumber)}\n\nPlease enter the 6-digit code below:",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(51, 65, 85),
                Location = new Point(20, yPos),
                Size = new Size(340, 60),
                AutoSize = false
            };
            yPos += 70;

            // OTP Input
            var lblOtp = new Label
            {
                Text = "Verification Code:",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.FromArgb(51, 65, 85),
                Location = new Point(20, yPos),
                AutoSize = true
            };
            yPos += 25;

            txtOtp = new TextBox
            {
                Location = new Point(20, yPos),
                Size = new Size(340, 30),
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                TextAlign = HorizontalAlignment.Center,
                MaxLength = 6,
                PlaceholderText = "000000"
            };
            txtOtp.KeyPress += (s, e) =>
            {
                if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
                {
                    e.Handled = true;
                }
            };
            yPos += 45;

            // Countdown label
            lblCountdown = new Label
            {
                Text = "Code expires in: 10:00",
                Font = new Font("Segoe UI", 8),
                ForeColor = Color.FromArgb(107, 114, 128),
                Location = new Point(20, yPos),
                AutoSize = true
            };
            yPos += 25;

            // Buttons
            btnVerify = new Button
            {
                Text = "Verify",
                Location = new Point(20, yPos),
                Size = new Size(100, 35),
                BackColor = Color.FromArgb(34, 197, 94),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnVerify.Click += BtnVerify_Click;

            btnResend = new Button
            {
                Text = "Resend Code",
                Location = new Point(130, yPos),
                Size = new Size(110, 35),
                BackColor = Color.FromArgb(59, 130, 246),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9),
                Cursor = Cursors.Hand
            };
            btnResend.Click += BtnResend_Click;

            btnCancel = new Button
            {
                Text = "Cancel",
                Location = new Point(250, yPos),
                Size = new Size(110, 35),
                BackColor = Color.FromArgb(107, 114, 128),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9),
                Cursor = Cursors.Hand
            };
            btnCancel.Click += (s, e) => this.Close();

            UIStyleHelper.ApplyRoundedButton(btnVerify, 8);
            UIStyleHelper.ApplyRoundedButton(btnResend, 8);
            UIStyleHelper.ApplyRoundedButton(btnCancel, 8);

            this.Controls.AddRange(new Control[] {
                lblTitle, lblMessage, lblOtp, txtOtp, lblCountdown,
                btnVerify, btnResend, btnCancel
            });

            // Start countdown timer
            countdownTimer = new System.Windows.Forms.Timer();
            countdownTimer.Interval = 1000; // 1 second
            countdownTimer.Tick += CountdownTimer_Tick;
            countdownTimer.Start();

            // Focus on OTP input
            txtOtp.Focus();
        }

        private void CountdownTimer_Tick(object? sender, EventArgs e)
        {
            remainingSeconds--;
            if (remainingSeconds <= 0)
            {
                countdownTimer.Stop();
                lblCountdown.Text = "Code expired";
                lblCountdown.ForeColor = Color.FromArgb(239, 68, 68);
                btnVerify.Enabled = false;
                txtOtp.Enabled = false;
            }
            else
            {
                int minutes = remainingSeconds / 60;
                int seconds = remainingSeconds % 60;
                lblCountdown.Text = $"Code expires in: {minutes:D2}:{seconds:D2}";
            }
        }

        private async void BtnVerify_Click(object? sender, EventArgs e)
        {
            if (txtOtp.Text.Trim() != otpCode)
            {
                MessageBox.Show("Invalid verification code. Please try again.", "Verification Failed",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtOtp.Clear();
                txtOtp.Focus();
                return;
            }

            IsVerified = true;
            countdownTimer.Stop();
            MessageBox.Show("Phone number verified successfully!", "Verification Success",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private async void BtnResend_Click(object? sender, EventArgs e)
        {
            try
            {
                btnResend.Enabled = false;
                btnResend.Text = "Sending...";

                otpCode = SmsService.GenerateOtp();
                bool sent = await SmsService.SendOtpAsync(phoneNumber, otpCode);

                if (sent)
                {
                    remainingSeconds = 600; // Reset to 10 minutes
                    lblCountdown.ForeColor = Color.FromArgb(107, 114, 128);
                    lblCountdown.Text = "Code expires in: 10:00";
                    txtOtp.Clear();
                    txtOtp.Focus();
                    MessageBox.Show("Verification code has been resent.", "Code Resent",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Failed to resend verification code. Please check the phone number and try again.",
                        "Resend Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error resending code: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnResend.Enabled = true;
                btnResend.Text = "Resend Code";
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            countdownTimer?.Stop();
            countdownTimer?.Dispose();
            base.OnFormClosing(e);
        }
    }
}


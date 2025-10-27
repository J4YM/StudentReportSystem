using StudentReportInitial.Data;
using System.Data.SqlClient;

namespace StudentReportInitial.Forms
{
    public partial class AddAdminForm : Form
    {
        private TextBox txtUsername;
        private TextBox txtPassword;
        private TextBox txtFirstName;
        private TextBox txtLastName;
        private TextBox txtEmail;
        private Button btnSave;
        private Button btnCancel;

        public AddAdminForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Add Admin User";
            this.Size = new Size(400, 400);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            var yPos = 20;
            var spacing = 60;

            // Username
            var lblUsername = new Label
            {
                Text = "Username:",
                Location = new Point(20, yPos),
                AutoSize = true
            };

            txtUsername = new TextBox
            {
                Location = new Point(20, yPos + 20),
                Size = new Size(340, 25)
            };
            yPos += spacing;

            // Password
            var lblPassword = new Label
            {
                Text = "Password:",
                Location = new Point(20, yPos),
                AutoSize = true
            };

            txtPassword = new TextBox
            {
                Location = new Point(20, yPos + 20),
                Size = new Size(340, 25),
                UseSystemPasswordChar = true
            };
            yPos += spacing;

            // First Name
            var lblFirstName = new Label
            {
                Text = "First Name:",
                Location = new Point(20, yPos),
                AutoSize = true
            };

            txtFirstName = new TextBox
            {
                Location = new Point(20, yPos + 20),
                Size = new Size(340, 25)
            };
            yPos += spacing;

            // Last Name
            var lblLastName = new Label
            {
                Text = "Last Name:",
                Location = new Point(20, yPos),
                AutoSize = true
            };

            txtLastName = new TextBox
            {
                Location = new Point(20, yPos + 20),
                Size = new Size(340, 25)
            };
            yPos += spacing;

            // Email
            var lblEmail = new Label
            {
                Text = "Email:",
                Location = new Point(20, yPos),
                AutoSize = true
            };

            txtEmail = new TextBox
            {
                Location = new Point(20, yPos + 20),
                Size = new Size(340, 25)
            };
            yPos += spacing;

            // Buttons
            btnSave = new Button
            {
                Text = "Save",
                Location = new Point(180, yPos),
                Size = new Size(80, 30),
                BackColor = Color.FromArgb(34, 197, 94),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnSave.Click += BtnSave_Click;

            btnCancel = new Button
            {
                Text = "Cancel",
                Location = new Point(280, yPos),
                Size = new Size(80, 30),
                BackColor = Color.FromArgb(107, 114, 128),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnCancel.Click += (s, e) => this.Close();

            this.Controls.AddRange(new Control[] {
                lblUsername, txtUsername,
                lblPassword, txtPassword,
                lblFirstName, txtFirstName,
                lblLastName, txtLastName,
                lblEmail, txtEmail,
                btnSave, btnCancel
            });
        }

        private async void BtnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtUsername.Text) ||
                string.IsNullOrWhiteSpace(txtPassword.Text) ||
                string.IsNullOrWhiteSpace(txtFirstName.Text) ||
                string.IsNullOrWhiteSpace(txtLastName.Text) ||
                string.IsNullOrWhiteSpace(txtEmail.Text))
            {
                MessageBox.Show("Please fill in all fields.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                await AdminHelper.CreateAdminUser(
                    txtUsername.Text,
                    txtPassword.Text,
                    txtFirstName.Text,
                    txtLastName.Text,
                    txtEmail.Text
                );

                MessageBox.Show("Admin user created successfully!", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating admin user: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}

using System;
using StudentReportInitial.Data;
using StudentReportInitial.Models;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StudentReportInitial.Forms
{
    public class AdminRequestForm : Form
    {
        private readonly User requester;
        private TextBox txtFullName = null!;
        private TextBox txtEmail = null!;
        private TextBox txtReason = null!;
        private Button btnSubmit = null!;
        private Button btnCancel = null!;

        public AdminRequestForm(User requesterUser)
        {
            requester = requesterUser;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            Text = "Request New Admin Account";
            Size = new Size(420, 360);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;

            var lblIntro = new Label
            {
                Text = "Branch admins cannot create admin accounts directly. Submit a request for Super Admin approval.",
                Location = new Point(20, 15),
                Size = new Size(360, 40),
                ForeColor = Color.FromArgb(71, 85, 105)
            };

            var lblFullName = new Label { Text = "Proposed Admin Full Name", Location = new Point(20, 60), AutoSize = true };
            txtFullName = new TextBox { Location = new Point(20, 80), Size = new Size(360, 25) };

            var lblEmail = new Label { Text = "Work Email", Location = new Point(20, 115), AutoSize = true };
            txtEmail = new TextBox { Location = new Point(20, 135), Size = new Size(360, 25) };

            var lblReason = new Label { Text = "Justification / Notes", Location = new Point(20, 170), AutoSize = true };
            txtReason = new TextBox
            {
                Location = new Point(20, 190),
                Size = new Size(360, 70),
                Multiline = true,
                ScrollBars = ScrollBars.Vertical
            };

            btnSubmit = new Button
            {
                Text = "Submit Request",
                Location = new Point(20, 275),
                Size = new Size(150, 32),
                BackColor = Color.FromArgb(59, 130, 246),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnSubmit.Click += async (s, e) => await SubmitRequestAsync();

            btnCancel = new Button
            {
                Text = "Cancel",
                Location = new Point(190, 275),
                Size = new Size(120, 32),
                BackColor = Color.FromArgb(148, 163, 184),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnCancel.Click += (s, e) => DialogResult = DialogResult.Cancel;

            Controls.AddRange(new Control[]
            {
                lblIntro, lblFullName, txtFullName, lblEmail, txtEmail, lblReason, txtReason, btnSubmit, btnCancel
            });
        }

        private async Task SubmitRequestAsync()
        {
            if (string.IsNullOrWhiteSpace(txtFullName.Text))
            {
                MessageBox.Show("Please provide the proposed admin's full name.", "Validation",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtFullName.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtEmail.Text))
            {
                MessageBox.Show("Please provide the proposed admin's email.", "Validation",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtEmail.Focus();
                return;
            }

            try
            {
                var branchId = await BranchHelper.GetUserBranchIdAsync(requester.Id);
                await AdminRequestHelper.SubmitRequestAsync(
                    requester.Id,
                    branchId > 0 ? branchId : (int?)null,
                    txtFullName.Text.Trim(),
                    txtEmail.Text.Trim(),
                    txtReason.Text.Trim());

                MessageBox.Show("Request submitted. The Super Admin will review and respond.", "Request Sent",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to submit request: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}


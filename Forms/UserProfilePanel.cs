using StudentReportInitial.Data;
using StudentReportInitial.Models;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;

namespace StudentReportInitial.Forms
{
    public class UserProfilePanel : UserControl
    {
        private readonly User currentUser;
        private PictureBox picProfile = null!;
        private Label lblFullName = null!;
        private Label lblRole = null!;
        private TextBox txtFirstName = null!;
        private TextBox txtLastName = null!;
        private TextBox txtEmail = null!;
        private TextBox txtPhone = null!;
        private Label lblStatus = null!;
        private Panel pnlStudentCard = null!;
        private Label lblStudentNameValue = null!;
        private Label lblStudentIdValue = null!;
        private Label lblStudentSectionValue = null!;
        private Button btnSaveChanges = null!;
        private Button btnChangePhoto = null!;
        private Student? linkedStudent;

        public event Action<User>? ProfileUpdated;

        public UserProfilePanel(User user)
        {
            currentUser = user;
            InitializeComponent();
            ApplyModernStyling();
            _ = LoadProfileAsync();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.FromArgb(248, 250, 252);

            var lblTitle = new Label
            {
                Text = "My Profile",
                Font = new Font("Segoe UI", 22, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 41, 59),
                AutoSize = true,
                Location = new Point(24, 24)
            };

            var profileCard = new Panel
            {
                Location = new Point(24, 80),
                Size = new Size(300, 360),
                BackColor = Color.White,
                Padding = new Padding(20)
            };

            picProfile = new PictureBox
            {
                Size = new Size(160, 160),
                Location = new Point(70, 10),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.FromArgb(248, 250, 252)
            };

            lblFullName = new Label
            {
                Text = "Full Name",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 41, 59),
                AutoSize = true,
                Location = new Point(20, 180),
                TextAlign = ContentAlignment.MiddleCenter
            };

            lblRole = new Label
            {
                Text = "Role",
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                ForeColor = Color.FromArgb(100, 116, 139),
                AutoSize = true,
                Location = new Point(20, 210),
                TextAlign = ContentAlignment.MiddleCenter
            };

            btnChangePhoto = new Button
            {
                Text = "Change Photo",
                Location = new Point(70, 250),
                Size = new Size(160, 36),
                BackColor = Color.FromArgb(59, 130, 246),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnChangePhoto.Click += BtnChangePhoto_Click;

            profileCard.Controls.Add(picProfile);
            profileCard.Controls.Add(lblFullName);
            profileCard.Controls.Add(lblRole);
            profileCard.Controls.Add(btnChangePhoto);

            var infoCard = new Panel
            {
                Location = new Point(344, 80),
                Size = new Size(620, 360),
                BackColor = Color.White,
                Padding = new Padding(24)
            };

            txtFirstName = CreateTextBox(new Point(20, 60), "First Name");
            txtLastName = CreateTextBox(new Point(320, 60), "Last Name");
            txtEmail = CreateTextBox(new Point(20, 160), "Email");
            txtPhone = CreateTextBox(new Point(320, 160), "Phone");

            btnSaveChanges = new Button
            {
                Text = "Save Changes",
                Location = new Point(20, 260),
                Size = new Size(160, 40),
                BackColor = Color.FromArgb(34, 197, 94),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnSaveChanges.Click += BtnSaveChanges_Click;

            infoCard.Controls.Add(CreateFieldLabel("First Name", new Point(20, 30)));
            infoCard.Controls.Add(txtFirstName);
            infoCard.Controls.Add(CreateFieldLabel("Last Name", new Point(320, 30)));
            infoCard.Controls.Add(txtLastName);
            infoCard.Controls.Add(CreateFieldLabel("Email", new Point(20, 130)));
            infoCard.Controls.Add(txtEmail);
            infoCard.Controls.Add(CreateFieldLabel("Phone", new Point(320, 130)));
            infoCard.Controls.Add(txtPhone);
            infoCard.Controls.Add(btnSaveChanges);

            pnlStudentCard = new Panel
            {
                Location = new Point(24, 460),
                Size = new Size(940, 160),
                BackColor = Color.White,
                Padding = new Padding(20),
                Visible = false
            };

            var lblStudentCardTitle = new Label
            {
                Text = "Linked Student",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 41, 59),
                AutoSize = true,
                Location = new Point(20, 10)
            };

            lblStudentNameValue = CreateInfoValueLabel(new Point(20, 60));
            lblStudentIdValue = CreateInfoValueLabel(new Point(320, 60));
            lblStudentSectionValue = CreateInfoValueLabel(new Point(620, 60));

            pnlStudentCard.Controls.Add(lblStudentCardTitle);
            pnlStudentCard.Controls.Add(CreateFieldLabel("Student Name", new Point(20, 40)));
            pnlStudentCard.Controls.Add(lblStudentNameValue);
            pnlStudentCard.Controls.Add(CreateFieldLabel("Student ID", new Point(320, 40)));
            pnlStudentCard.Controls.Add(lblStudentIdValue);
            pnlStudentCard.Controls.Add(CreateFieldLabel("Year & Section", new Point(620, 40)));
            pnlStudentCard.Controls.Add(lblStudentSectionValue);

            lblStatus = new Label
            {
                Location = new Point(24, 640),
                AutoSize = true,
                ForeColor = Color.FromArgb(34, 197, 94),
                Font = new Font("Segoe UI", 9, FontStyle.Italic)
            };

            this.Controls.Add(lblTitle);
            this.Controls.Add(profileCard);
            this.Controls.Add(infoCard);
            this.Controls.Add(pnlStudentCard);
            this.Controls.Add(lblStatus);

            this.ResumeLayout(false);
        }

        private void ApplyModernStyling()
        {
            UIStyleHelper.ApplyRoundedButton(btnChangePhoto, 12);
            UIStyleHelper.ApplyRoundedButton(btnSaveChanges, 12);
            UIStyleHelper.ApplyRoundedCorners(picProfile, 100);
        }

        private async Task LoadProfileAsync()
        {
            try
            {
                using var connection = DatabaseHelper.GetConnection();
                await connection.OpenAsync();

                var query = "SELECT FirstName, LastName, Email, Phone FROM Users WHERE Id = @id";
                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@id", currentUser.Id);

                using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    var firstNameOrdinal = reader.GetOrdinal("FirstName");
                    var lastNameOrdinal = reader.GetOrdinal("LastName");
                    var emailOrdinal = reader.GetOrdinal("Email");
                    var phoneOrdinal = reader.GetOrdinal("Phone");

                    currentUser.FirstName = reader.GetString(firstNameOrdinal);
                    currentUser.LastName = reader.GetString(lastNameOrdinal);
                    currentUser.Email = reader.GetString(emailOrdinal);
                    currentUser.Phone = reader.IsDBNull(phoneOrdinal) ? string.Empty : reader.GetString(phoneOrdinal);
                }

                txtFirstName.Text = currentUser.FirstName;
                txtLastName.Text = currentUser.LastName;
                txtEmail.Text = currentUser.Email;
                txtPhone.Text = currentUser.Phone;

                UpdateHeaderText();
                LoadProfilePhoto();

                if (currentUser.Role == UserRole.Student || currentUser.Role == UserRole.Guardian)
                {
                    linkedStudent = await UserContextHelper.GetLinkedStudentAsync(currentUser);
                    UpdateStudentSummary();
                }
                else
                {
                    pnlStudentCard.Visible = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading profile information: {ex.Message}", "Profile",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateHeaderText()
        {
            lblFullName.Text = $"{currentUser.FirstName} {currentUser.LastName}".Trim();
            lblRole.Text = GetRoleDisplay(currentUser.Role);
        }

        private void UpdateStudentSummary()
        {
            if (linkedStudent == null)
            {
                pnlStudentCard.Visible = false;
                return;
            }

            pnlStudentCard.Visible = true;
            lblStudentNameValue.Text = $"{linkedStudent.FirstName} {linkedStudent.LastName}";
            lblStudentIdValue.Text = linkedStudent.StudentId;
            lblStudentSectionValue.Text = $"{linkedStudent.GradeLevel} - {linkedStudent.Section}";
        }

        private async void BtnSaveChanges_Click(object? sender, EventArgs e)
        {
            if (!ValidateInputs(out var firstName, out var lastName, out var email, out var phone))
            {
                return;
            }

            try
            {
                using var connection = DatabaseHelper.GetConnection();
                await connection.OpenAsync();

                var query = @"
                    UPDATE Users
                    SET FirstName = @firstName,
                        LastName = @lastName,
                        Email = @email,
                        Phone = @phone
                    WHERE Id = @id";

                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@firstName", firstName);
                command.Parameters.AddWithValue("@lastName", lastName);
                command.Parameters.AddWithValue("@email", email);
                command.Parameters.AddWithValue("@phone", string.IsNullOrWhiteSpace(phone) ? (object)DBNull.Value : phone);
                command.Parameters.AddWithValue("@id", currentUser.Id);

                await command.ExecuteNonQueryAsync();

                currentUser.FirstName = firstName;
                currentUser.LastName = lastName;
                currentUser.Email = email;
                currentUser.Phone = phone;

                UpdateHeaderText();
                ProfileUpdated?.Invoke(currentUser);

                lblStatus.ForeColor = Color.FromArgb(34, 197, 94);
                lblStatus.Text = "Profile updated successfully.";
            }
            catch (Exception ex)
            {
                lblStatus.ForeColor = Color.FromArgb(239, 68, 68);
                lblStatus.Text = $"Failed to update profile: {ex.Message}";
            }
        }

        private void BtnChangePhoto_Click(object? sender, EventArgs e)
        {
            ShowChangePhotoDialog();
        }

        private void ShowChangePhotoDialog()
        {
            string? selectedFile = null;
            Exception? dialogException = null;

            // Create a new STA thread for the dialog
            var dialogThread = new Thread(() =>
            {
                try
                {
                    using var dialog = new OpenFileDialog
                    {
                        Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp",
                        Title = "Select Profile Picture"
                    };

                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        selectedFile = dialog.FileName;
                    }
                }
                catch (Exception ex)
                {
                    dialogException = ex;
                }
            });

            dialogThread.SetApartmentState(ApartmentState.STA);
            dialogThread.Start();
            dialogThread.Join(); // Wait for the dialog to close

            // Back on the calling thread, handle the result
            if (dialogException != null)
            {
                lblStatus.ForeColor = Color.FromArgb(239, 68, 68);
                lblStatus.Text = $"Failed to open file dialog: {dialogException.Message}";
                return;
            }

            if (string.IsNullOrEmpty(selectedFile))
            {
                return; // User cancelled
            }

            // Process the selected file
            try
            {
                SaveProfilePhoto(selectedFile);
                LoadProfilePhoto();
                lblStatus.ForeColor = Color.FromArgb(34, 197, 94);
                lblStatus.Text = "Profile picture updated.";
            }
            catch (Exception ex)
            {
                lblStatus.ForeColor = Color.FromArgb(239, 68, 68);
                lblStatus.Text = $"Failed to update photo: {ex.Message}";
            }
        }

        private void SaveProfilePhoto(string sourcePath)
        {
            Directory.CreateDirectory(ProfilePhotosDirectory);
            var destinationPath = GetProfilePhotoPath();

            using var sourceImage = Image.FromFile(sourcePath);
            using var resized = new Bitmap(sourceImage, new Size(256, 256));
            resized.Save(destinationPath, ImageFormat.Png);
        }

        private bool ValidateInputs(out string firstName, out string lastName, out string email, out string phone)
        {
            firstName = txtFirstName.Text.Trim();
            lastName = txtLastName.Text.Trim();
            email = txtEmail.Text.Trim();
            phone = txtPhone.Text.Trim();

            if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
            {
                MessageBox.Show("First name and last name are required.", "Validation",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (!IsValidEmail(email))
            {
                MessageBox.Show("Please enter a valid email address.", "Validation",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (!string.IsNullOrWhiteSpace(phone))
            {
                var errorMessage = PhoneValidator.GetValidationMessage(phone);
                if (!string.IsNullOrEmpty(errorMessage))
                {
                    MessageBox.Show(errorMessage, "Validation",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                phone = PhoneValidator.FormatPhoneNumber(phone);
            }

            return true;
        }

        private void LoadProfilePhoto()
        {
            try
            {
                Image? imageToUse = null;
                var profilePhotoPath = GetProfilePhotoPath();

                if (File.Exists(profilePhotoPath))
                {
                    using var fs = new FileStream(profilePhotoPath, FileMode.Open, FileAccess.Read);
                    imageToUse = Image.FromStream(fs);
                }
                else
                {
                    imageToUse = CreateInitialsAvatar();
                }

                SetPicture(imageToUse);
            }
            catch
            {
                // Fallback silently
            }
        }

        private void SetPicture(Image image)
        {
            var previous = picProfile.Image;
            picProfile.Image = (Image)image.Clone();
            previous?.Dispose();
            image.Dispose();
        }

        private Image CreateInitialsAvatar()
        {
            var initials = $"{GetInitial(currentUser.FirstName)}{GetInitial(currentUser.LastName)}";
            var bitmap = new Bitmap(256, 256);

            using var graphics = Graphics.FromImage(bitmap);
            using var brush = new SolidBrush(GetAvatarColor(currentUser.Id));
            using var textBrush = new SolidBrush(Color.White);
            using var format = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };

            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.FillEllipse(brush, 0, 0, bitmap.Width, bitmap.Height);
            graphics.DrawString(initials, new Font("Segoe UI", 64, FontStyle.Bold), textBrush,
                new RectangleF(0, 0, bitmap.Width, bitmap.Height), format);

            return bitmap;
        }

        private static string GetInitial(string text)
        {
            return string.IsNullOrWhiteSpace(text) ? string.Empty : text.Substring(0, 1).ToUpper();
        }

        private static Color GetAvatarColor(int seed)
        {
            var colors = new[]
            {
                Color.FromArgb(59, 130, 246),
                Color.FromArgb(99, 102, 241),
                Color.FromArgb(139, 92, 246),
                Color.FromArgb(236, 72, 153),
                Color.FromArgb(248, 113, 113),
                Color.FromArgb(34, 197, 94),
                Color.FromArgb(14, 165, 233)
            };

            return colors[Math.Abs(seed) % colors.Length];
        }

        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return false;
            }

            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private Label CreateFieldLabel(string text, Point location)
        {
            return new Label
            {
                Text = text,
                Location = location,
                AutoSize = true,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.FromArgb(100, 116, 139)
            };
        }

        private TextBox CreateTextBox(Point location, string placeholder)
        {
            return new TextBox
            {
                Location = location,
                Size = new Size(250, 28),
                PlaceholderText = placeholder
            };
        }

        private Label CreateInfoValueLabel(Point location)
        {
            return new Label
            {
                Location = location,
                AutoSize = true,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 41, 59)
            };
        }

        private static string GetRoleDisplay(UserRole role)
        {
            return role switch
            {
                UserRole.Admin => "Administrator",
                UserRole.Professor => "Professor",
                UserRole.Guardian => "Guardian",
                UserRole.Student => "Student",
                _ => role.ToString()
            };
        }

        private string ProfilePhotosDirectory =>
            Path.Combine(Application.StartupPath, "Images", "ProfilePhotos");

        private string GetProfilePhotoPath()
        {
            return Path.Combine(ProfilePhotosDirectory, $"user_{currentUser.Id}.png");
        }

        private sealed class WindowWrapper : IWin32Window, IDisposable
        {
            public WindowWrapper(IntPtr handle)
            {
                Handle = handle;
            }

            public IntPtr Handle { get; }

            public void Dispose()
            {
                // Nothing to dispose, required for 'using' pattern compatibility
            }
        }
    }
}


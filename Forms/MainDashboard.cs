using System;
using System.Drawing;
using System.Windows.Forms;
using StudentReportInitial.Models;
using StudentReportInitial.Forms;
using StudentReportInitial.Data;
using System.ComponentModel;
using System.Drawing.Design;
using System.Data.SqlClient;
using System.Data;



namespace StudentReportInitial.Forms
{
    public partial class MainDashboard : Form
    {
        private User currentUser;
        private Panel? mainContentPanel;

        public MainDashboard(User user)
        {
            currentUser = user;
            InitializeComponent();
            ApplyModernStyling();
            LoadUserInterface();
        }

        private void ApplyModernStyling()
        {
            BackColor = Color.FromArgb(248, 250, 252);
            StartPosition = FormStartPosition.CenterScreen;
            Size = new Size(1280, 720);
            Text = $"STI College Baliuag - AimONE - Welcome {currentUser.FirstName} {currentUser.LastName}";
            WindowState = FormWindowState.Maximized;

            // Header panel
            pnlHeader.BackColor = Color.White;
            pnlHeader.Padding = new Padding(32, 0, 32, 0);
            pnlHeader.Height = 80;

            lblAppTitle.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            lblAppTitle.ForeColor = Color.FromArgb(30, 41, 59);
            lblAppTitle.Text = "STI College Baliuag - AimONE";

            // User info label
            lblUserInfo.ForeColor = Color.FromArgb(30, 64, 175);
            lblUserInfo.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblUserInfo.TextAlign = ContentAlignment.MiddleRight;
            lblUserInfo.AutoSize = false;
            lblUserInfo.MaximumSize = new Size(240, 0);
            lblUserInfo.UseMnemonic = false;
            lblUserInfo.Text = FormatUserInfo();

            // Logout button
            btnLogout.BackColor = Color.FromArgb(239, 68, 68);
            btnLogout.ForeColor = Color.White;
            btnLogout.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnLogout.FlatStyle = FlatStyle.Flat;
            btnLogout.FlatAppearance.BorderSize = 0;
            btnLogout.Cursor = Cursors.Hand;
            btnLogout.Width = 100;
            btnLogout.FlatAppearance.MouseOverBackColor = Color.FromArgb(220, 38, 38);
            btnLogout.FlatAppearance.MouseDownBackColor = Color.FromArgb(185, 28, 28);
            UIStyleHelper.ApplyRoundedButton(btnLogout, 12);

            // Sidebar panel
            pnlSidebar.BackColor = Color.White;
            pnlSidebar.Width = 260;
            pnlSidebar.Padding = new Padding(0, 0, 0, 24);
            pnlSidebar.BorderStyle = BorderStyle.None;

            pnlSidebarHeader.BackColor = Color.White;
            lblSidebarTitle.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblSidebarTitle.ForeColor = Color.FromArgb(51, 65, 85);
            lblSidebarTitle.Text = $"{GetRoleDisplayName(currentUser.Role)} Dashboard";

            flpSidebarButtons.BackColor = Color.White;
            flpSidebarButtons.FlowDirection = FlowDirection.TopDown;
            flpSidebarButtons.WrapContents = false;
            flpSidebarButtons.AutoScroll = true;
            flpSidebarButtons.Padding = new Padding(20, 10, 20, 10);
            flpSidebarButtons.SizeChanged += (_, _) => UpdateSidebarButtonWidths();

            // Main content panel
            mainContentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(248, 250, 252),
                Padding = new Padding(24)
            };

            Controls.Add(mainContentPanel);
            Controls.SetChildIndex(mainContentPanel, Controls.Count - 1);
            mainContentPanel.BringToFront();
        }

        private void LoadUserInterface()
        {
            switch (currentUser.Role)
            {
                case UserRole.Admin:
                    LoadAdminInterface();
                    break;
                case UserRole.Professor:
                    LoadProfessorInterface();
                    break;
                case UserRole.Guardian:
                case UserRole.Student:
                    LoadViewerInterface();
                    break;
            }
        }

        private void LoadAdminInterface()
        {
            ClearSidebarButtons();

            // Admin sidebar buttons
            var btnManageUsers = CreateSidebarButton("Manage Users", 0);
            var btnManageStudents = CreateSidebarButton("Manage Students", 1);
            var btnManageSubjects = CreateSidebarButton("Manage Subjects", 2);
            var btnEnrollStudents = CreateSidebarButton("Student Enrollment", 3);
            var btnReports = CreateSidebarButton("System Reports", 4);

            btnManageUsers.Click += (s, e) => LoadAdminPanel("users");
            btnManageStudents.Click += (s, e) => LoadAdminPanel("students");
            btnManageSubjects.Click += (s, e) => LoadAdminPanel("subjects");
            btnEnrollStudents.Click += (s, e) => LoadAdminPanel("enrollment");
            btnReports.Click += (s, e) => LoadAdminPanel("reports");

            // Load default panel
            LoadAdminPanel("users");
        }

        private void LoadProfessorInterface()
        {
            // Professor sidebar buttons
            ClearSidebarButtons();
            var btnMySubjects = CreateSidebarButton("My Subjects", 0);
            var btnAttendance = CreateSidebarButton("Record Attendance", 1);
            var btnGrades = CreateSidebarButton("Record Grades", 2);
            var btnProfile = CreateSidebarButton("My Profile", 3);

            btnMySubjects.Click += (s, e) => LoadProfessorPanel("subjects");
            btnAttendance.Click += (s, e) => LoadProfessorPanel("attendance");
            btnGrades.Click += (s, e) => LoadProfessorPanel("grades");
            btnProfile.Click += (s, e) => LoadProfessorPanel("profile");

            // Load default panel
            LoadProfessorPanel("subjects");
        }

        private void LoadViewerInterface()
        {
            // Guardian/Student sidebar buttons
            ClearSidebarButtons();
            var btnGrades = CreateSidebarButton("View Grades", 0);
            var btnAttendance = CreateSidebarButton("View Attendance", 1);
            var btnProfile = CreateSidebarButton("Profile", 2);

            btnGrades.Click += (s, e) => LoadViewerPanel("grades");
            btnAttendance.Click += (s, e) => LoadViewerPanel("attendance");
            btnProfile.Click += (s, e) => LoadViewerPanel("profile");

            // Load default panel
            LoadViewerPanel("grades");
        }

        private Button CreateSidebarButton(string text, int index)
        {
            var button = new Button
            {
                Text = text,
                AutoSize = false,
                Width = flpSidebarButtons.ClientSize.Width - flpSidebarButtons.Padding.Horizontal,
                Height = 48,
                BackColor = Color.FromArgb(249, 250, 251),
                ForeColor = Color.FromArgb(51, 65, 85),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                TextAlign = ContentAlignment.MiddleCenter,
                Margin = new Padding(0, index == 0 ? 0 : 8, 0, 0)
            };
            button.FlatAppearance.BorderSize = 0;
            button.FlatAppearance.MouseOverBackColor = Color.FromArgb(219, 234, 254);
            button.FlatAppearance.MouseDownBackColor = Color.FromArgb(191, 219, 254);

            button.MouseEnter += (s, e) => button.BackColor = Color.FromArgb(219, 234, 254);
            button.MouseLeave += (s, e) => button.BackColor = Color.FromArgb(249, 250, 251);

            flpSidebarButtons.Controls.Add(button);
            UpdateSidebarButtonWidths();
            UIStyleHelper.ApplyRoundedButton(button, 12);

            return button;
        }

        private void LoadAdminPanel(string panelType)
        {
            if (mainContentPanel != null)
                mainContentPanel.Controls.Clear();

            switch (panelType)
            {
                case "users":
                    LoadUserManagementPanel();
                    break;
                case "students":
                    LoadStudentManagementPanel();
                    break;
                case "subjects":
                    LoadSubjectManagementPanel();
                    break;
                case "enrollment":
                    LoadStudentEnrollmentPanel();
                    break;
                case "reports":
                    LoadSystemReportsPanel();
                    break;
            }
        }

        private void LoadProfessorPanel(string panelType)
        {
            if (mainContentPanel != null)
                mainContentPanel.Controls.Clear();

            switch (panelType)
            {
                case "subjects":
                    LoadProfessorSubjectsPanel();
                    break;
                case "attendance":
                    LoadAttendancePanel();
                    break;
                case "grades":
                    LoadGradesPanel();
                    break;
                case "profile":
                    LoadProfilePanel();
                    break;
            }
        }

        private void LoadViewerPanel(string panelType)
        {
            if (mainContentPanel != null)
                mainContentPanel.Controls.Clear();

            switch (panelType)
            {
                case "grades":
                    LoadViewerGradesPanel();
                    break;
                case "attendance":
                    LoadViewerAttendancePanel();
                    break;
                case "profile":
                    LoadProfilePanel();
                    break;
            }
        }

        // Panel loading methods
        private void LoadUserManagementPanel()
        {
            if (mainContentPanel != null)
                mainContentPanel.Controls.Clear();
            var userManagement = new AdminUserManagement();
            userManagement.Dock = DockStyle.Fill;
            if (mainContentPanel != null)
                mainContentPanel.Controls.Add(userManagement);
        }

        private void LoadStudentManagementPanel()
        {
            if (mainContentPanel != null)
                mainContentPanel.Controls.Clear();
            var studentManagement = new AdminStudentManagement();
            studentManagement.Dock = DockStyle.Fill;
            if (mainContentPanel != null)
                mainContentPanel.Controls.Add(studentManagement);
        }

        private void LoadSubjectManagementPanel()
        {
            if (mainContentPanel != null)
            {
                mainContentPanel.Controls.Clear();
                var subjectManagement = new AdminSubjectManagement();
                subjectManagement.Dock = DockStyle.Fill;
                mainContentPanel.Controls.Add(subjectManagement);
                mainContentPanel.BringToFront(); // Ensure the panel is visible
            }
        }

        private async void LoadSystemReportsPanel()
        {
            if (mainContentPanel == null) return;

            mainContentPanel.Controls.Clear();
            mainContentPanel.BackColor = Color.FromArgb(248, 250, 252);
            mainContentPanel.Padding = new Padding(24);

            // Title
            var lblTitle = new Label
            {
                Text = "System Reports & Statistics",
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 41, 59),
                AutoSize = true,
                Location = new Point(24, 24)
            };
            mainContentPanel.Controls.Add(lblTitle);

            // Loading indicator
            var lblLoading = new Label
            {
                Text = "Loading statistics...",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.FromArgb(107, 114, 128),
                AutoSize = true,
                Location = new Point(24, 60)
            };
            mainContentPanel.Controls.Add(lblLoading);

            // Refresh the panel to show loading
            mainContentPanel.Refresh();

            // Load statistics asynchronously
            var stats = await StatisticsHelper.GetSystemStatisticsAsync();

            // Remove loading label
            mainContentPanel.Controls.Remove(lblLoading);

            // Create statistics cards
            int cardWidth = 280;
            int cardHeight = 140;
            int spacing = 20;
            int startX = 24;
            int startY = 80;
            int cardsPerRow = 3;

            // Statistics cards with colors
            var cards = new[]
            {
                new { Title = "Total Students", Value = stats.TotalStudents, Color = Color.FromArgb(59, 130, 246), Icon = "👥" },
                new { Title = "Total Professors", Value = stats.TotalProfessors, Color = Color.FromArgb(34, 197, 94), Icon = "👨‍🏫" },
                new { Title = "Total Users", Value = stats.TotalUsers, Color = Color.FromArgb(168, 85, 247), Icon = "👤" },
                new { Title = "Total Subjects", Value = stats.TotalSubjects, Color = Color.FromArgb(236, 72, 153), Icon = "📚" },
                new { Title = "Total Grades", Value = stats.TotalGrades, Color = Color.FromArgb(251, 146, 60), Icon = "📊" },
                new { Title = "Attendance Records", Value = stats.TotalAttendanceRecords, Color = Color.FromArgb(14, 165, 233), Icon = "✅" }
            };

            for (int i = 0; i < cards.Length; i++)
            {
                int row = i / cardsPerRow;
                int col = i % cardsPerRow;
                int x = startX + col * (cardWidth + spacing);
                int y = startY + row * (cardHeight + spacing);

                var card = CreateStatisticsCard(cards[i].Title, cards[i].Value, cards[i].Color, cards[i].Icon, x, y, cardWidth, cardHeight);
                mainContentPanel.Controls.Add(card);
            }

            // Create charts section
            int chartY = startY + 2 * (cardHeight + spacing) + 20;
            var lblChartsTitle = new Label
            {
                Text = "Visual Statistics",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 41, 59),
                AutoSize = true,
                Location = new Point(24, chartY)
            };
            mainContentPanel.Controls.Add(lblChartsTitle);

            // Create bar chart panel
            int chartPanelY = chartY + 40;
            var chartPanel = new Panel
            {
                Location = new Point(24, chartPanelY),
                Size = new Size(mainContentPanel.Width - 48, 300),
                BackColor = Color.White,
                BorderStyle = BorderStyle.None
            };
            UIStyleHelper.ApplyRoundedCorners(chartPanel, 12, drawBorder: true);
            mainContentPanel.Controls.Add(chartPanel);

            // Draw bar chart
            DrawBarChart(chartPanel, stats);

            // Add refresh button
            var btnRefresh = new Button
            {
                Text = "🔄 Refresh Statistics",
                Location = new Point(mainContentPanel.Width - 200, 24),
                Size = new Size(170, 36),
                BackColor = Color.FromArgb(59, 130, 246),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnRefresh.FlatAppearance.BorderSize = 0;
            btnRefresh.Click += async (s, e) => LoadSystemReportsPanel();
            UIStyleHelper.ApplyRoundedButton(btnRefresh, 10);
            mainContentPanel.Controls.Add(btnRefresh);
        }

        private Panel CreateStatisticsCard(string title, int value, Color accentColor, string icon, int x, int y, int width, int height)
        {
            var card = new Panel
            {
                Location = new Point(x, y),
                Size = new Size(width, height),
                BackColor = Color.White,
                BorderStyle = BorderStyle.None
            };
            UIStyleHelper.ApplyRoundedCorners(card, 12, drawBorder: true);

            // Accent line at top (add first so it's behind other elements)
            var accentLine = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(width, 4),
                BackColor = accentColor
            };
            card.Controls.Add(accentLine);

            // Icon label - positioned at top right, no background
            var lblIcon = new Label
            {
                Text = icon,
                Font = new Font("Segoe UI", 24),
                Location = new Point(width - 50, 15),
                Size = new Size(40, 40),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent,
                ForeColor = accentColor
            };
            card.Controls.Add(lblIcon);

            // Value label - positioned below accent line
            var lblValue = new Label
            {
                Text = value.ToString("N0"),
                Font = new Font("Segoe UI", 32, FontStyle.Bold),
                ForeColor = accentColor,
                Location = new Point(20, 20),
                AutoSize = true,
                BackColor = Color.Transparent
            };
            card.Controls.Add(lblValue);

            // Title label - positioned at bottom
            var lblTitle = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.FromArgb(107, 114, 128),
                Location = new Point(20, height - 35),
                Size = new Size(width - 40, 20),
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleLeft
            };
            card.Controls.Add(lblTitle);

            return card;
        }

        private void DrawBarChart(Panel panel, SystemStatistics stats)
        {
            panel.Paint += (sender, e) =>
            {
                // Validate panel dimensions
                if (panel.Width <= 0 || panel.Height <= 0)
                {
                    return; // Panel not sized yet, skip drawing
                }

                var g = e.Graphics;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                int padding = 40;
                int chartWidth = Math.Max(0, panel.Width - 2 * padding);
                int chartHeight = Math.Max(0, panel.Height - 2 * padding);
                
                // Ensure minimum chart dimensions
                if (chartWidth < 100 || chartHeight < 100)
                {
                    return; // Panel too small to draw chart
                }

                int barWidth = 80;
                int spacing = 30;
                int startX = padding + 20;
                int baseY = panel.Height - padding;

                // Find max value for scaling
                int maxValue = Math.Max(stats.TotalStudents, Math.Max(stats.TotalProfessors, 
                    Math.Max(stats.TotalUsers, Math.Max(stats.TotalSubjects, 
                    Math.Max(stats.TotalGrades, stats.TotalAttendanceRecords)))));

                if (maxValue == 0) maxValue = 1; // Avoid division by zero

                var chartData = new[]
                {
                    new { Label = "Students", Value = stats.TotalStudents, Color = Color.FromArgb(59, 130, 246) },
                    new { Label = "Professors", Value = stats.TotalProfessors, Color = Color.FromArgb(34, 197, 94) },
                    new { Label = "Users", Value = stats.TotalUsers, Color = Color.FromArgb(168, 85, 247) },
                    new { Label = "Subjects", Value = stats.TotalSubjects, Color = Color.FromArgb(236, 72, 153) },
                    new { Label = "Grades", Value = stats.TotalGrades, Color = Color.FromArgb(251, 146, 60) },
                    new { Label = "Attendance", Value = stats.TotalAttendanceRecords, Color = Color.FromArgb(14, 165, 233) }
                };

                // Draw bars
                for (int i = 0; i < chartData.Length; i++)
                {
                    int x = startX + i * (barWidth + spacing);
                    int barHeight = Math.Max(0, (int)((double)chartData[i].Value / maxValue * chartHeight));
                    int barY = baseY - barHeight;

                    // Validate bar dimensions
                    if (barWidth <= 0 || barHeight <= 0 || x < 0 || barY < 0 || x + barWidth > panel.Width)
                    {
                        continue; // Skip invalid bars
                    }

                    // Draw bar with rounded corners
                    using (var brush = new SolidBrush(chartData[i].Color))
                    {
                        // For very small bars, just draw a rectangle
                        if (barHeight < 16)
                        {
                            g.FillRectangle(brush, x, barY, barWidth, barHeight);
                        }
                        else
                        {
                            using (var path = new System.Drawing.Drawing2D.GraphicsPath())
                            {
                                int radius = 8;
                                int diameter = Math.Min(radius * 2, Math.Min(barWidth, barHeight));
                                
                                if (diameter > 0)
                                {
                                    var arc = new Rectangle(x, barY, diameter, diameter);
                                    if (arc.Width > 0 && arc.Height > 0)
                                    {
                                        path.AddArc(arc, 180, 90); // Top left
                                    }
                                    
                                    arc.X = x + barWidth - diameter;
                                    if (arc.X >= 0 && arc.Width > 0 && arc.Height > 0)
                                    {
                                        path.AddArc(arc, 270, 90); // Top right
                                    }
                                    
                                    arc.Y = barY + barHeight - diameter;
                                    if (arc.Y >= 0 && arc.Width > 0 && arc.Height > 0)
                                    {
                                        path.AddArc(arc, 0, 90); // Bottom right
                                    }
                                    
                                    arc.X = x;
                                    if (arc.X >= 0 && arc.Width > 0 && arc.Height > 0)
                                    {
                                        path.AddArc(arc, 90, 90); // Bottom left
                                    }
                                    
                                    path.CloseFigure();
                                    if (path.PointCount > 0)
                                    {
                                        g.FillPath(brush, path);
                                    }
                                }
                                else
                                {
                                    // Fallback to rectangle if diameter is invalid
                                    g.FillRectangle(brush, x, barY, barWidth, barHeight);
                                }
                            }
                        }
                    }

                    // Draw value on top of bar
                    if (chartData[i].Value > 0)
                    {
                        var valueText = chartData[i].Value.ToString("N0");
                        var textSize = g.MeasureString(valueText, new Font("Segoe UI", 9, FontStyle.Bold));
                        g.DrawString(valueText, new Font("Segoe UI", 9, FontStyle.Bold), 
                            new SolidBrush(Color.FromArgb(30, 41, 59)), 
                            x + (barWidth - textSize.Width) / 2, barY - 20);
                    }

                    // Draw label below bar
                    var labelSize = g.MeasureString(chartData[i].Label, new Font("Segoe UI", 8));
                    g.DrawString(chartData[i].Label, new Font("Segoe UI", 8), 
                        new SolidBrush(Color.FromArgb(107, 114, 128)), 
                        x + (barWidth - labelSize.Width) / 2, baseY + 10);
                }

                // Draw grid lines
                if (chartHeight > 0 && chartWidth > 0)
                {
                    using (var pen = new Pen(Color.FromArgb(229, 231, 235), 1))
                    {
                        for (int i = 0; i <= 5; i++)
                        {
                            int y = baseY - (int)(chartHeight * i / 5);
                            if (y >= 0 && y <= panel.Height)
                            {
                                g.DrawLine(pen, padding, y, Math.Min(panel.Width - padding, panel.Width), y);
                            }
                        }
                    }
                }
            };
        }

        private void LoadProfessorSubjectsPanel()
        {
            if (mainContentPanel != null)
            {
                mainContentPanel.Controls.Clear();
                
                // Create a panel to hold the professor subjects interface
                var panel = new Panel
                {
                    Dock = DockStyle.Fill,
                    Padding = new Padding(20)
                };

                // Title
                var lblTitle = new Label
                {
                    Text = "My Subjects & Enrolled Students",
                    Font = new Font("Segoe UI", 16, FontStyle.Bold),
                    ForeColor = Color.FromArgb(51, 65, 85),
                    AutoSize = true,
                    Location = new Point(20, 20)
                };
                panel.Controls.Add(lblTitle);

                // Subjects DataGridView
                var lblSubjects = new Label
                {
                    Text = "My Assigned Subjects:",
                    Font = new Font("Segoe UI", 12, FontStyle.Bold),
                    ForeColor = Color.FromArgb(51, 65, 85),
                    AutoSize = true,
                    Location = new Point(20, 60)
                };
                panel.Controls.Add(lblSubjects);

                var dgvSubjects = new DataGridView
                {
                    Location = new Point(20, 90),
                    Size = new Size(panel.Width - 40, 200),
                    Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                    BackgroundColor = Color.White,
                    BorderStyle = BorderStyle.Fixed3D,
                    AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                    SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                    ReadOnly = true,
                    AllowUserToAddRows = false,
                    AllowUserToDeleteRows = false
                };
                panel.Controls.Add(dgvSubjects);

                // Students DataGridView
                var lblStudents = new Label
                {
                    Text = "Enrolled Students:",
                    Font = new Font("Segoe UI", 12, FontStyle.Bold),
                    ForeColor = Color.FromArgb(51, 65, 85),
                    AutoSize = true,
                    Location = new Point(20, 310)
                };
                panel.Controls.Add(lblStudents);

                var dgvStudents = new DataGridView
                {
                    Location = new Point(20, 340),
                    Size = new Size(panel.Width - 40, 200),
                    Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                    BackgroundColor = Color.White,
                    BorderStyle = BorderStyle.Fixed3D,
                    AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                    SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                    ReadOnly = true,
                    AllowUserToAddRows = false,
                    AllowUserToDeleteRows = false
                };
                panel.Controls.Add(dgvStudents);

                // Load data
                LoadProfessorSubjectsData(dgvSubjects, dgvStudents);

                mainContentPanel.Controls.Add(panel);
            }
        }

        private async void LoadProfessorSubjectsData(DataGridView dgvSubjects, DataGridView dgvStudents)
        {
            try
            {
                // Load professor's subjects
                using var connection = DatabaseHelper.GetConnection();
                await connection.OpenAsync();

                var subjectsQuery = @"
                    SELECT s.Id, s.Code, s.Name, s.GradeLevel, s.Section,
                           COUNT(ss.StudentId) as EnrolledCount
                    FROM Subjects s
                    LEFT JOIN StudentSubjects ss ON s.Id = ss.SubjectId
                    WHERE s.ProfessorId = @professorId AND s.IsActive = 1
                    GROUP BY s.Id, s.Code, s.Name, s.GradeLevel, s.Section
                    ORDER BY s.GradeLevel, s.Name";

                using var subjectsCommand = new SqlCommand(subjectsQuery, connection);
                subjectsCommand.Parameters.AddWithValue("@professorId", currentUser.Id);

                using var subjectsAdapter = new SqlDataAdapter(subjectsCommand);
                var subjectsTable = new DataTable();
                subjectsAdapter.Fill(subjectsTable);

                dgvSubjects.DataSource = subjectsTable;
                if (dgvSubjects.Columns.Count > 0)
                {
                    dgvSubjects.Columns["Id"].Visible = false;
                    dgvSubjects.Columns["Code"].HeaderText = "Code";
                    dgvSubjects.Columns["Name"].HeaderText = "Subject";
                    dgvSubjects.Columns["GradeLevel"].HeaderText = "Grade";
                    dgvSubjects.Columns["Section"].HeaderText = "Section";
                    dgvSubjects.Columns["EnrolledCount"].HeaderText = "Students Enrolled";
                }

                // Show message if no subjects found
                if (subjectsTable.Rows.Count == 0)
                {
                    var lblNoSubjects = new Label
                    {
                        Text = "No subjects assigned to you yet. Contact administrator to assign subjects.",
                        Font = new Font("Segoe UI", 10, FontStyle.Italic),
                        ForeColor = Color.FromArgb(107, 114, 128),
                        AutoSize = true,
                        Location = new Point(20, 300)
                    };
                    dgvSubjects.Parent.Controls.Add(lblNoSubjects);
                }

                // Load enrolled students across all professor's subjects
                var studentsQuery = @"
                    SELECT DISTINCT st.Id, st.StudentId, st.FirstName, st.LastName, st.GradeLevel, st.Section,
                           s.Name as SubjectName, s.Code as SubjectCode
                    FROM Students st
                    INNER JOIN StudentSubjects ss ON st.Id = ss.StudentId
                    INNER JOIN Subjects s ON ss.SubjectId = s.Id
                    WHERE s.ProfessorId = @professorId AND st.IsActive = 1 AND s.IsActive = 1
                    ORDER BY st.GradeLevel, st.LastName, st.FirstName";

                using var studentsCommand = new SqlCommand(studentsQuery, connection);
                studentsCommand.Parameters.AddWithValue("@professorId", currentUser.Id);

                using var studentsAdapter = new SqlDataAdapter(studentsCommand);
                var studentsTable = new DataTable();
                studentsAdapter.Fill(studentsTable);

                dgvStudents.DataSource = studentsTable;
                if (dgvStudents.Columns.Count > 0)
                {
                    dgvStudents.Columns["Id"].Visible = false;
                    dgvStudents.Columns["StudentId"].HeaderText = "Student ID";
                    dgvStudents.Columns["FirstName"].HeaderText = "First Name";
                    dgvStudents.Columns["LastName"].HeaderText = "Last Name";
                    dgvStudents.Columns["GradeLevel"].HeaderText = "Grade";
                    dgvStudents.Columns["Section"].HeaderText = "Section";
                    dgvStudents.Columns["SubjectName"].HeaderText = "Subject";
                    dgvStudents.Columns["SubjectCode"].HeaderText = "Subject Code";
                }

                // Show message if no students found
                if (studentsTable.Rows.Count == 0)
                {
                    var lblNoStudents = new Label
                    {
                        Text = "No students enrolled in your subjects yet.",
                        Font = new Font("Segoe UI", 10, FontStyle.Italic),
                        ForeColor = Color.FromArgb(107, 114, 128),
                        AutoSize = true,
                        Location = new Point(20, 550)
                    };
                    dgvStudents.Parent.Controls.Add(lblNoStudents);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading professor data: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadAttendancePanel()
        {
            if (mainContentPanel != null)
                mainContentPanel.Controls.Clear();
            var attendancePanel = new ProfessorAttendancePanel(currentUser);
            attendancePanel.Dock = DockStyle.Fill;
            if (mainContentPanel != null)
                mainContentPanel.Controls.Add(attendancePanel);
        }

        private void LoadGradesPanel()
        {
            if (mainContentPanel != null)
                mainContentPanel.Controls.Clear();
            var gradesPanel = new ProfessorGradesPanel(currentUser);
            gradesPanel.Dock = DockStyle.Fill;
            if (mainContentPanel != null)
                mainContentPanel.Controls.Add(gradesPanel);
        }

        private void LoadProfessorReportsPanel()
        {
            var label = new Label
            {
                Text = "Professor Reports Panel",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(51, 65, 85),
                AutoSize = true
            };
            if (mainContentPanel != null)
                mainContentPanel.Controls.Add(label);
        }

        private void LoadViewerGradesPanel()
        {
            if (mainContentPanel != null)
                mainContentPanel.Controls.Clear();
            var gradesPanel = new ViewerGradesPanel(currentUser);
            gradesPanel.Dock = DockStyle.Fill;
            if (mainContentPanel != null)
                mainContentPanel.Controls.Add(gradesPanel);
        }

        private void LoadStudentEnrollmentPanel()
        {
            if (mainContentPanel != null)
                mainContentPanel.Controls.Clear();
            var enrollmentPanel = new StudentSubjectEnrollment();
            enrollmentPanel.Dock = DockStyle.Fill;
            if (mainContentPanel != null)
                mainContentPanel.Controls.Add(enrollmentPanel);
        }

        private void LoadViewerAttendancePanel()
        {
            if (mainContentPanel != null)
                mainContentPanel.Controls.Clear();
            var attendancePanel = new ViewerAttendancePanel(currentUser);
            attendancePanel.Dock = DockStyle.Fill;
            if (mainContentPanel != null)
                mainContentPanel.Controls.Add(attendancePanel);
        }

        private void LoadProfilePanel()
        {
            var label = new Label
            {
                Text = "Profile Panel",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(51, 65, 85),
                AutoSize = true
            };
            if (mainContentPanel != null)
                mainContentPanel.Controls.Add(label);
        }

        private void ClearSidebarButtons()
        {
            flpSidebarButtons.Controls.Clear();
        }

        private void UpdateSidebarButtonWidths()
        {
            var targetWidth = Math.Max(120, flpSidebarButtons.ClientSize.Width - flpSidebarButtons.Padding.Horizontal);
            foreach (Control control in flpSidebarButtons.Controls)
            {
                control.Width = targetWidth;
            }
        }

        private void btnLogout_Click(object sender, EventArgs e)
        {
            var confirmation = MessageBox.Show("Are you sure you want to log out?", "Confirm Logout",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirmation != DialogResult.Yes)
            {
                return;
            }

            // Hide the dashboard and show the login form again
            this.Hide();
            using (var loginForm = new LoginForm())
            {
                if (loginForm.ShowDialog() == DialogResult.OK && loginForm.CurrentUser != null)
                {
                    // Re-login successful, show dashboard for new user
                    currentUser = loginForm.CurrentUser;
                    
                    // Clear the sidebar and content panels
                    mainContentPanel?.Controls.Clear();
                    ClearSidebarButtons();

                    // Update user info and reload interface for new user
                    lblUserInfo.Text = FormatUserInfo();
                    lblSidebarTitle.Text = $"{GetRoleDisplayName(currentUser.Role)} Dashboard";
                    LoadUserInterface();
                    this.Show();
                }
                else
                {
                    // If login cancelled or failed, close the dashboard
                    this.Close();
                }
            }
        }

        private string FormatUserInfo()
        {
            var roleDisplay = GetRoleDisplayName(currentUser.Role);
            return $"{currentUser.FirstName} {currentUser.LastName}\n({roleDisplay})";
        }

        private static string GetRoleDisplayName(UserRole role)
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
    }
}

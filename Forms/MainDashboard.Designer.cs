namespace StudentReportInitial.Forms
{
    partial class MainDashboard
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            pnlHeader = new Panel();
            lblAppTitle = new Label();
            lblUserInfo = new Label();
            btnLogout = new Button();
            pnlSidebar = new Panel();
            pnlSidebarHeader = new Panel();
            lblSidebarTitle = new Label();
            flpSidebarButtons = new FlowLayoutPanel();
            pnlHeader.SuspendLayout();
            pnlSidebar.SuspendLayout();
            pnlSidebarHeader.SuspendLayout();
            SuspendLayout();
            // 
            // pnlHeader
            // 
            pnlHeader.Controls.Add(lblAppTitle);
            pnlHeader.Controls.Add(lblUserInfo);
            pnlHeader.Controls.Add(btnLogout);
            pnlHeader.Dock = DockStyle.Top;
            pnlHeader.Location = new Point(0, 0);
            pnlHeader.Name = "pnlHeader";
            pnlHeader.Padding = new Padding(24, 0, 24, 0);
            pnlHeader.Size = new Size(1050, 72);
            pnlHeader.TabIndex = 0;
            // 
            // lblAppTitle
            // 
            lblAppTitle.AutoSize = true;
            lblAppTitle.Location = new Point(24, 24);
            lblAppTitle.Name = "lblAppTitle";
            lblAppTitle.Size = new Size(133, 15);
            lblAppTitle.TabIndex = 2;
            lblAppTitle.Text = "Student Report System";
            // 
            // lblUserInfo
            // 
            lblUserInfo.Dock = DockStyle.Right;
            lblUserInfo.Location = new Point(854, 0);
            lblUserInfo.Name = "lblUserInfo";
            lblUserInfo.Padding = new Padding(0, 12, 12, 12);
            lblUserInfo.Size = new Size(240, 72);
            lblUserInfo.TabIndex = 1;
            lblUserInfo.Text = "User Info";
            lblUserInfo.TextAlign = ContentAlignment.MiddleRight;
            // 
            // btnLogout
            // 
            btnLogout.Dock = DockStyle.Right;
            btnLogout.Location = new Point(974, 0);
            btnLogout.Name = "btnLogout";
            btnLogout.Size = new Size(100, 72);
            btnLogout.TabIndex = 0;
            btnLogout.Text = "Logout";
            btnLogout.UseVisualStyleBackColor = false;
            btnLogout.Click += btnLogout_Click;
            // 
            // pnlSidebar
            // 
            pnlSidebar.Controls.Add(flpSidebarButtons);
            pnlSidebar.Controls.Add(pnlSidebarHeader);
            pnlSidebar.Dock = DockStyle.Left;
            pnlSidebar.Location = new Point(0, 72);
            pnlSidebar.Name = "pnlSidebar";
            pnlSidebar.Padding = new Padding(0, 0, 0, 16);
            pnlSidebar.Size = new Size(240, 528);
            pnlSidebar.TabIndex = 1;
            // 
            // pnlSidebarHeader
            // 
            pnlSidebarHeader.Controls.Add(lblSidebarTitle);
            pnlSidebarHeader.Dock = DockStyle.Top;
            pnlSidebarHeader.Location = new Point(0, 0);
            pnlSidebarHeader.Name = "pnlSidebarHeader";
            pnlSidebarHeader.Padding = new Padding(20, 24, 20, 12);
            pnlSidebarHeader.Size = new Size(240, 92);
            pnlSidebarHeader.TabIndex = 0;
            // 
            // lblSidebarTitle
            // 
            lblSidebarTitle.Dock = DockStyle.Fill;
            lblSidebarTitle.Location = new Point(20, 24);
            lblSidebarTitle.Name = "lblSidebarTitle";
            lblSidebarTitle.Size = new Size(200, 56);
            lblSidebarTitle.TabIndex = 0;
            lblSidebarTitle.Text = "Navigation";
            lblSidebarTitle.TextAlign = ContentAlignment.BottomLeft;
            // 
            // flpSidebarButtons
            // 
            flpSidebarButtons.AutoScroll = true;
            flpSidebarButtons.Dock = DockStyle.Fill;
            flpSidebarButtons.FlowDirection = FlowDirection.TopDown;
            flpSidebarButtons.Location = new Point(0, 92);
            flpSidebarButtons.Name = "flpSidebarButtons";
            flpSidebarButtons.Padding = new Padding(20, 10, 20, 10);
            flpSidebarButtons.Size = new Size(240, 420);
            flpSidebarButtons.TabIndex = 1;
            flpSidebarButtons.WrapContents = false;
            // 
            // MainDashboard
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1050, 600);
            Controls.Add(pnlSidebar);
            Controls.Add(pnlHeader);
            Name = "MainDashboard";
            Text = "MainDashboard";
            pnlHeader.ResumeLayout(false);
            pnlHeader.PerformLayout();
            pnlSidebar.ResumeLayout(false);
            pnlSidebarHeader.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Panel pnlHeader;
        private System.Windows.Forms.Label lblUserInfo;
        private System.Windows.Forms.Button btnLogout;
        private System.Windows.Forms.Panel pnlSidebar;
        private System.Windows.Forms.Label lblAppTitle;
        private System.Windows.Forms.Panel pnlSidebarHeader;
        private System.Windows.Forms.Label lblSidebarTitle;
        private System.Windows.Forms.FlowLayoutPanel flpSidebarButtons;
    }
}

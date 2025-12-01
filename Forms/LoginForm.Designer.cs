namespace StudentReportInitial.Forms
{
    partial class LoginForm
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
            components = new System.ComponentModel.Container();
            lblHeroSubtitle = new Label();
            pnlHero = new Panel();
            lblTitle = new Label();
            pnlLogin = new Panel();
            pnlLoginCard = new Panel();
            btnTogglePassword = new Button();
            lblError = new Label();
            btnLogin = new Button();
            txtPassword = new TextBox();
            lblPassword = new Label();
            txtUsername = new TextBox();
            lblUsername = new Label();
            lblSubtitle = new Label();
            lblLoginHeading = new Label();
            timerError = new System.Windows.Forms.Timer(components);
            pnlHero.SuspendLayout();
            pnlLogin.SuspendLayout();
            pnlLoginCard.SuspendLayout();
            SuspendLayout();
            // 
            // lblHeroSubtitle
            // 
            lblHeroSubtitle.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lblHeroSubtitle.AutoSize = true;
            lblHeroSubtitle.ForeColor = Color.White;
            lblHeroSubtitle.Location = new Point(32, 124);
            lblHeroSubtitle.MaximumSize = new Size(196, 0);
            lblHeroSubtitle.Name = "lblHeroSubtitle";
            lblHeroSubtitle.Size = new Size(196, 45);
            lblHeroSubtitle.TabIndex = 1;
            lblHeroSubtitle.Text = "Streamline class records, grades, and communication in one modern workspace.";
            lblHeroSubtitle.Click += lblHeroSubtitle_Click;
            // 
            // pnlHero
            // 
            pnlHero.BackColor = Color.Transparent;
            pnlHero.Controls.Add(lblHeroSubtitle);
            pnlHero.Controls.Add(lblTitle);
            pnlHero.Dock = DockStyle.Left;
            pnlHero.Location = new Point(0, 0);
            pnlHero.Name = "pnlHero";
            pnlHero.Size = new Size(260, 420);
            pnlHero.TabIndex = 0;
            // 
            // lblTitle
            // 
            lblTitle.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lblTitle.AutoSize = true;
            lblTitle.ForeColor = Color.White;
            lblTitle.Location = new Point(32, 56);
            lblTitle.MaximumSize = new Size(196, 0);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(73, 15);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "STI BALIUAG";
            // 
            // pnlLogin
            // 
            pnlLogin.BackColor = Color.Transparent;
            pnlLogin.Controls.Add(pnlLoginCard);
            pnlLogin.Dock = DockStyle.Fill;
            pnlLogin.Location = new Point(260, 0);
            pnlLogin.Name = "pnlLogin";
            pnlLogin.Padding = new Padding(60, 48, 60, 48);
            pnlLogin.Size = new Size(440, 420);
            pnlLogin.TabIndex = 1;
            // 
            // pnlLoginCard
            // 
            pnlLoginCard.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            pnlLoginCard.BackColor = Color.White;
            pnlLoginCard.Controls.Add(btnTogglePassword);
            pnlLoginCard.Controls.Add(lblError);
            pnlLoginCard.Controls.Add(btnLogin);
            pnlLoginCard.Controls.Add(txtPassword);
            pnlLoginCard.Controls.Add(lblPassword);
            pnlLoginCard.Controls.Add(txtUsername);
            pnlLoginCard.Controls.Add(lblUsername);
            pnlLoginCard.Controls.Add(lblSubtitle);
            pnlLoginCard.Controls.Add(lblLoginHeading);
            pnlLoginCard.Location = new Point(20, 24);
            pnlLoginCard.Name = "pnlLoginCard";
            pnlLoginCard.Padding = new Padding(28, 32, 28, 32);
            pnlLoginCard.Size = new Size(360, 320);
            pnlLoginCard.TabIndex = 0;
            // 
            // btnTogglePassword
            // 
            btnTogglePassword.Location = new Point(274, 178);
            btnTogglePassword.Name = "btnTogglePassword";
            btnTogglePassword.Size = new Size(58, 23);
            btnTogglePassword.TabIndex = 8;
            btnTogglePassword.Text = "Show";
            btnTogglePassword.UseVisualStyleBackColor = true;
            btnTogglePassword.Click += btnTogglePassword_Click;
            // 
            // lblError
            // 
            lblError.AutoSize = true;
            lblError.ForeColor = Color.FromArgb(239, 68, 68);
            lblError.Location = new Point(28, 264);
            lblError.Name = "lblError";
            lblError.Size = new Size(81, 15);
            lblError.TabIndex = 7;
            lblError.Text = "Error message";
            lblError.Visible = false;
            // 
            // btnLogin
            // 
            btnLogin.BackColor = Color.FromArgb(37, 99, 235);
            btnLogin.FlatStyle = FlatStyle.Flat;
            btnLogin.ForeColor = Color.White;
            btnLogin.Location = new Point(28, 216);
            btnLogin.Name = "btnLogin";
            btnLogin.Size = new Size(304, 38);
            btnLogin.TabIndex = 6;
            btnLogin.Text = "Login";
            btnLogin.UseVisualStyleBackColor = false;
            btnLogin.Click += btnLogin_Click;
            // 
            // txtPassword
            // 
            txtPassword.Location = new Point(28, 178);
            txtPassword.Name = "txtPassword";
            txtPassword.PasswordChar = '*';
            txtPassword.Size = new Size(240, 23);
            txtPassword.TabIndex = 5;
            txtPassword.KeyPress += txtPassword_KeyPress;
            // 
            // lblPassword
            // 
            lblPassword.AutoSize = true;
            lblPassword.Location = new Point(28, 156);
            lblPassword.Name = "lblPassword";
            lblPassword.Size = new Size(57, 15);
            lblPassword.TabIndex = 4;
            lblPassword.Text = "Password";
            // 
            // txtUsername
            // 
            txtUsername.Location = new Point(28, 122);
            txtUsername.Name = "txtUsername";
            txtUsername.Size = new Size(304, 23);
            txtUsername.TabIndex = 3;
            // 
            // lblUsername
            // 
            lblUsername.AutoSize = true;
            lblUsername.Location = new Point(28, 100);
            lblUsername.Name = "lblUsername";
            lblUsername.Size = new Size(60, 15);
            lblUsername.TabIndex = 2;
            lblUsername.Text = "Username";
            // 
            // lblSubtitle
            // 
            lblSubtitle.AutoSize = true;
            lblSubtitle.ForeColor = Color.FromArgb(100, 116, 139);
            lblSubtitle.Location = new Point(28, 64);
            lblSubtitle.Name = "lblSubtitle";
            lblSubtitle.Size = new Size(165, 15);
            lblSubtitle.TabIndex = 1;
            lblSubtitle.Text = "Please sign in to your account";
            // 
            // lblLoginHeading
            // 
            lblLoginHeading.AutoSize = true;
            lblLoginHeading.ForeColor = Color.FromArgb(51, 65, 85);
            lblLoginHeading.Location = new Point(28, 32);
            lblLoginHeading.Name = "lblLoginHeading";
            lblLoginHeading.Size = new Size(118, 15);
            lblLoginHeading.TabIndex = 0;
            lblLoginHeading.Text = "Welcome back, login";
            // 
            // timerError
            // 
            timerError.Interval = 5000;
            timerError.Tick += timerError_Tick;
            // 
            // LoginForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(245, 247, 250);
            ClientSize = new Size(700, 420);
            Controls.Add(pnlLogin);
            Controls.Add(pnlHero);
            Name = "LoginForm";
            Text = "Login";
            pnlHero.ResumeLayout(false);
            pnlHero.PerformLayout();
            pnlLogin.ResumeLayout(false);
            pnlLoginCard.ResumeLayout(false);
            pnlLoginCard.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblSubtitle;
        private System.Windows.Forms.Label lblUsername;
        private System.Windows.Forms.TextBox txtUsername;
        private System.Windows.Forms.Label lblPassword;
        private System.Windows.Forms.TextBox txtPassword;
        private System.Windows.Forms.Button btnLogin;
        private System.Windows.Forms.Label lblError;
        private System.Windows.Forms.Timer timerError;
        private System.Windows.Forms.Panel pnlHero;
        private System.Windows.Forms.Panel pnlLogin;
        private System.Windows.Forms.Panel pnlLoginCard;
        private System.Windows.Forms.Label lblLoginHeading;
        private System.Windows.Forms.Label lblHeroSubtitle;
        private System.Windows.Forms.Button btnTogglePassword;
    }
}

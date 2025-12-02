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
            lnkForgotPassword = new LinkLabel();
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
            pnlHero.Anchor = AnchorStyles.None;
            pnlHero.Location = new Point(50, 50);
            pnlHero.Name = "pnlHero";
            pnlHero.Size = new Size(450, 550);
            pnlHero.TabIndex = 0;
            pnlHero.Paint += pnlHero_Paint;
            // 
            // lblTitle
            // 
            lblTitle.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lblTitle.AutoSize = false;
            lblTitle.ForeColor = Color.White;
            lblTitle.Location = new Point(0, 50);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(450, 40);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "STI College Login Portal";
            lblTitle.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // pnlLogin
            // 
            pnlLogin.BackColor = Color.Transparent;
            pnlLogin.Controls.Add(pnlLoginCard);
            pnlLogin.Anchor = AnchorStyles.None;
            pnlLogin.Location = new Point(500, 50);
            pnlLogin.Name = "pnlLogin";
            pnlLogin.Padding = new Padding(0);
            pnlLogin.Size = new Size(550, 550);
            pnlLogin.TabIndex = 1;
            // 
            // pnlLoginCard
            // 
            pnlLoginCard.Anchor = AnchorStyles.None;
            pnlLoginCard.BackColor = Color.White;
            pnlLoginCard.Controls.Add(lnkForgotPassword);
            pnlLoginCard.Controls.Add(btnTogglePassword);
            pnlLoginCard.Controls.Add(lblError);
            pnlLoginCard.Controls.Add(btnLogin);
            pnlLoginCard.Controls.Add(txtPassword);
            pnlLoginCard.Controls.Add(lblPassword);
            pnlLoginCard.Controls.Add(txtUsername);
            pnlLoginCard.Controls.Add(lblUsername);
            pnlLoginCard.Controls.Add(lblSubtitle);
            pnlLoginCard.Controls.Add(lblLoginHeading);
            pnlLoginCard.Location = new Point(0, 0);
            pnlLoginCard.Name = "pnlLoginCard";
            pnlLoginCard.Padding = new Padding(50, 50, 50, 50);
            pnlLoginCard.Size = new Size(550, 550);
            pnlLoginCard.TabIndex = 0;
            pnlLoginCard.Paint += pnlLoginCard_Paint;
            // 
            // btnTogglePassword
            // 
            btnTogglePassword.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnTogglePassword.Location = new Point(440, 245);
            btnTogglePassword.Name = "btnTogglePassword";
            btnTogglePassword.Size = new Size(60, 23);
            btnTogglePassword.TabIndex = 8;
            btnTogglePassword.Text = "Show";
            btnTogglePassword.UseVisualStyleBackColor = true;
            btnTogglePassword.Click += btnTogglePassword_Click;
            // 
            // lblError
            // 
            lblError.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lblError.AutoSize = false;
            lblError.ForeColor = Color.FromArgb(239, 68, 68);
            lblError.Location = new Point(50, 280);
            lblError.Name = "lblError";
            lblError.Size = new Size(450, 20);
            lblError.TabIndex = 7;
            lblError.Text = "Error message";
            lblError.TextAlign = ContentAlignment.MiddleLeft;
            lblError.Visible = false;
            // 
            // btnLogin
            // 
            btnLogin.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            btnLogin.BackColor = Color.FromArgb(37, 99, 235);
            btnLogin.FlatStyle = FlatStyle.Flat;
            btnLogin.ForeColor = Color.White;
            btnLogin.Location = new Point(50, 320);
            btnLogin.Name = "btnLogin";
            btnLogin.Size = new Size(450, 40);
            btnLogin.TabIndex = 6;
            btnLogin.Text = "Login";
            btnLogin.UseVisualStyleBackColor = false;
            btnLogin.Click += btnLogin_Click;
            // 
            // lnkForgotPassword
            // 
            lnkForgotPassword.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lnkForgotPassword.AutoSize = false;
            lnkForgotPassword.LinkColor = Color.FromArgb(37, 99, 235);
            lnkForgotPassword.Location = new Point(50, 370);
            lnkForgotPassword.Name = "lnkForgotPassword";
            lnkForgotPassword.Size = new Size(450, 23);
            lnkForgotPassword.TabIndex = 9;
            lnkForgotPassword.TabStop = true;
            lnkForgotPassword.Text = "Forgot Password?";
            lnkForgotPassword.TextAlign = ContentAlignment.MiddleRight;
            lnkForgotPassword.LinkClicked += lnkForgotPassword_LinkClicked;
            // 
            // txtPassword
            // 
            txtPassword.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtPassword.Location = new Point(50, 245);
            txtPassword.Name = "txtPassword";
            txtPassword.PasswordChar = '*';
            txtPassword.Size = new Size(380, 23);
            txtPassword.TabIndex = 5;
            txtPassword.KeyPress += txtPassword_KeyPress;
            // 
            // lblPassword
            // 
            lblPassword.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lblPassword.AutoSize = false;
            lblPassword.Location = new Point(50, 220);
            lblPassword.Name = "lblPassword";
            lblPassword.Size = new Size(450, 20);
            lblPassword.TabIndex = 4;
            lblPassword.Text = "Password";
            lblPassword.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // txtUsername
            // 
            txtUsername.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtUsername.Location = new Point(50, 175);
            txtUsername.Name = "txtUsername";
            txtUsername.Size = new Size(450, 23);
            txtUsername.TabIndex = 3;
            // 
            // lblUsername
            // 
            lblUsername.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lblUsername.AutoSize = false;
            lblUsername.Location = new Point(50, 150);
            lblUsername.Name = "lblUsername";
            lblUsername.Size = new Size(450, 20);
            lblUsername.TabIndex = 2;
            lblUsername.Text = "Username";
            lblUsername.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblSubtitle
            // 
            lblSubtitle.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lblSubtitle.AutoSize = false;
            lblSubtitle.ForeColor = Color.FromArgb(100, 116, 139);
            lblSubtitle.Location = new Point(50, 90);
            lblSubtitle.Name = "lblSubtitle";
            lblSubtitle.Size = new Size(450, 20);
            lblSubtitle.TabIndex = 1;
            lblSubtitle.Text = "Please sign in to your account";
            lblSubtitle.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblLoginHeading
            // 
            lblLoginHeading.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lblLoginHeading.AutoSize = false;
            lblLoginHeading.ForeColor = Color.FromArgb(51, 65, 85);
            lblLoginHeading.Location = new Point(50, 50);
            lblLoginHeading.Name = "lblLoginHeading";
            lblLoginHeading.Size = new Size(450, 30);
            lblLoginHeading.TabIndex = 0;
            lblLoginHeading.Text = "STI Student Portal Login";
            lblLoginHeading.TextAlign = ContentAlignment.MiddleLeft;
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
            ClientSize = new Size(1100, 650);
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
        private System.Windows.Forms.LinkLabel lnkForgotPassword;
    }
}

using System;
using System.Drawing;
using System.Windows.Forms;
using Guna.UI2.WinForms;
using FAST.SocietiesManagement.UI.Theme;

namespace FAST.SocietiesManagement.UI.Forms
{
    public class LoginForm : Form
    {
        private Guna2TextBox txtUsername;
        private Guna2TextBox txtEmail;
        private Guna2TextBox txtFullName;
        private Guna2TextBox txtRollNumber;
        private Guna2TextBox txtDepartment;
        private Guna2TextBox txtPassword;
        private Guna2TextBox txtConfirmPassword;
        private Guna2Button btnAction;
        private Label lblError;
        private LinkLabel lnkSwitchMode;
        private Label lblTitle;
        private Label lblSubtitle;
        private bool isLoginMode = true;
        private Panel formPanel;

        public LoginForm()
        {
            InitializeComponent();
            SetMode(true);
        }

        private void InitializeComponent()
        {
            Text = "FAST SMS - Secure Access";
            MinimumSize = new Size(1040, 720);
            Size = new Size(1080, 740);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.None;
            BackColor = EnterpriseTheme.AppBackground;

            var leftPanel = new Guna2GradientPanel
            {
                Dock = DockStyle.Left,
                Width = 430,
                FillColor = EnterpriseTheme.PrimaryDark,
                FillColor2 = Color.FromArgb(6, 95, 70),
                GradientMode = System.Drawing.Drawing2D.LinearGradientMode.ForwardDiagonal
            };
            leftPanel.Controls.Add(new Label { Text = "FAST SMS", Font = new Font("Segoe UI", 33, FontStyle.Bold), ForeColor = Color.White, BackColor = Color.Transparent, AutoSize = true, Location = new Point(38, 210) });
            leftPanel.Controls.Add(new Label { Text = "Societies Operations Suite", Font = new Font("Segoe UI Semibold", 14), ForeColor = Color.White, BackColor = Color.Transparent, AutoSize = true, Location = new Point(43, 274) });
            leftPanel.Controls.Add(new Label { Text = "Memberships  |  Events  |  Approvals  |  Reports", Font = new Font("Segoe UI Semibold", 9, FontStyle.Bold), ForeColor = Color.White, BackColor = Color.Transparent, AutoSize = true, Location = new Point(45, 315) });

            formPanel = new Panel { Dock = DockStyle.Fill, BackColor = EnterpriseTheme.Surface };
            var btnClose = new Guna2ControlBox { Anchor = AnchorStyles.Top | AnchorStyles.Right, Location = new Point(594, 18), Size = new Size(42, 34), FillColor = EnterpriseTheme.Danger, IconColor = Color.White, HoverState = { FillColor = EnterpriseTheme.Danger, IconColor = Color.White } };
            var btnMinimize = new Guna2ControlBox { ControlBoxType = Guna.UI2.WinForms.Enums.ControlBoxType.MinimizeBox, Anchor = AnchorStyles.Top | AnchorStyles.Right, Location = new Point(548, 18), Size = new Size(42, 34), FillColor = EnterpriseTheme.Primary, IconColor = Color.White, HoverState = { FillColor = EnterpriseTheme.Primary, IconColor = Color.White } };

            lblTitle = new Label { Font = new Font("Segoe UI", 28, FontStyle.Bold), ForeColor = EnterpriseTheme.Text, Location = new Point(95, 62), AutoSize = true };
            lblSubtitle = new Label { Font = new Font("Segoe UI Semibold", 10, FontStyle.Bold), ForeColor = Color.FromArgb(51, 65, 85), Location = new Point(100, 118), AutoSize = true };

            txtUsername = Input("Username", 100, 170);
            txtEmail = Input("University Email", 100, 225);
            txtFullName = Input("Full Name", 100, 280);
            txtRollNumber = Input("Roll Number", 100, 335);
            txtDepartment = Input("Department", 100, 390);
            txtPassword = Input("Password", 100, 445);
            txtPassword.UseSystemPasswordChar = true;
            txtConfirmPassword = Input("Confirm Password", 100, 500);
            txtConfirmPassword.UseSystemPasswordChar = true;

            lblError = new Label { Text = "", ForeColor = EnterpriseTheme.Danger, Location = new Point(103, 555), Size = new Size(410, 40), Font = new Font("Segoe UI Semibold", 9, FontStyle.Bold) };
            btnAction = new Guna2Button { Location = new Point(100, 600), Size = new Size(380, 46), BorderRadius = 8, FillColor = EnterpriseTheme.Primary, Font = new Font("Segoe UI Semibold", 11, FontStyle.Bold), Cursor = Cursors.Hand };
            btnAction.HoverState.FillColor = EnterpriseTheme.PrimaryHover;
            btnAction.Click += BtnAction_Click;
            lnkSwitchMode = new LinkLabel { Location = new Point(190, 655), AutoSize = true, Font = new Font("Segoe UI Semibold", 9), LinkColor = EnterpriseTheme.Primary, ActiveLinkColor = EnterpriseTheme.PrimaryDark, Cursor = Cursors.Hand };
            lnkSwitchMode.Click += (s, e) => SetMode(!isLoginMode);

            formPanel.Controls.AddRange(new Control[] { btnMinimize, btnClose, lblTitle, lblSubtitle, txtUsername, txtEmail, txtFullName, txtRollNumber, txtDepartment, txtPassword, txtConfirmPassword, lblError, btnAction, lnkSwitchMode });
            formPanel.Resize += (s, e) =>
            {
                btnClose.Location = new Point(formPanel.Width - 58, 18);
                btnMinimize.Location = new Point(formPanel.Width - 106, 18);
                CenterSwitchLink();
            };
            Controls.Add(formPanel);
            Controls.Add(leftPanel);
        }

        private static Guna2TextBox Input(string placeholder, int x, int y)
        {
            return new Guna2TextBox
            {
                Location = new Point(x, y),
                Size = new Size(380, 43),
                PlaceholderText = placeholder,
                BorderRadius = 8,
                BorderColor = EnterpriseTheme.Border,
                FocusedState = { BorderColor = EnterpriseTheme.Primary },
                FillColor = EnterpriseTheme.Surface,
                ForeColor = EnterpriseTheme.Text,
                PlaceholderForeColor = EnterpriseTheme.MutedText,
                Font = new Font("Segoe UI", 10)
            };
        }

        private void SetMode(bool loginMode)
        {
            isLoginMode = loginMode;
            lblError.Text = "";
            txtPassword.Clear();
            txtConfirmPassword.Clear();

            txtEmail.Visible = !isLoginMode;
            txtFullName.Visible = !isLoginMode;
            txtRollNumber.Visible = !isLoginMode;
            txtDepartment.Visible = !isLoginMode;
            txtConfirmPassword.Visible = !isLoginMode;

            txtPassword.Location = isLoginMode ? new Point(100, 225) : new Point(100, 445);
            lblError.Location = isLoginMode ? new Point(103, 285) : new Point(103, 555);
            btnAction.Location = isLoginMode ? new Point(100, 330) : new Point(100, 600);
            CenterSwitchLink();

            lblTitle.Text = isLoginMode ? "Welcome Back" : "Student Registration";
            lblSubtitle.Text = isLoginMode ? "Sign in with your username and password" : "Create a complete student profile";
            btnAction.Text = isLoginMode ? "Login" : "Create Student Account";
            lnkSwitchMode.Text = isLoginMode ? "New student? Create account" : "Already registered? Login";
            CenterSwitchLink();
        }

        private void CenterSwitchLink()
        {
            if (lnkSwitchMode == null || btnAction == null) return;
            lnkSwitchMode.Left = btnAction.Left + (btnAction.Width - lnkSwitchMode.Width) / 2;
            lnkSwitchMode.Top = btnAction.Bottom + 12;
        }

        private void BtnAction_Click(object sender, EventArgs e)
        {
            lblError.ForeColor = EnterpriseTheme.Warning;
            lblError.Text = "Processing...";
            Application.DoEvents();

            if (isLoginMode)
            {
                var (success, message, user) = ServiceLocator.AuthService.Login(txtUsername.Text, txtPassword.Text);
                if (!success)
                {
                    lblError.ForeColor = EnterpriseTheme.Danger;
                    lblError.Text = message;
                    return;
                }

                Hide();
                using var dashboard = new MainDashboard(user);
                dashboard.ShowDialog();

                if (dashboard.LogoutRequested)
                {
                    SetMode(true);
                    txtPassword.Clear();
                    lblError.ForeColor = EnterpriseTheme.MutedText;
                    lblError.Text = "You have been logged out.";
                    Show();
                    return;
                }

                Close();
                return;
            }

            var result = ServiceLocator.AuthService.RegisterStudent(
                txtUsername.Text,
                txtEmail.Text,
                txtPassword.Text,
                txtConfirmPassword.Text,
                txtFullName.Text,
                txtRollNumber.Text,
                txtDepartment.Text);

            lblError.ForeColor = result.Success ? EnterpriseTheme.Success : EnterpriseTheme.Danger;
            lblError.Text = result.Message;
            if (result.Success) SetMode(true);
        }
    }
}

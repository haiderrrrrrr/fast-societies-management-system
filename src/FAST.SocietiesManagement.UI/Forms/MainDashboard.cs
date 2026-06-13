using System;
using System.Drawing;
using System.Windows.Forms;
using Guna.UI2.WinForms;
using FAST.SocietiesManagement.Core.DTOs;
using FAST.SocietiesManagement.Core.Enums;
using FAST.SocietiesManagement.UI.Theme;
using FAST.SocietiesManagement.UI.UserControls;

namespace FAST.SocietiesManagement.UI.Forms
{
    public class MainDashboard : Form
    {
        private UserDto _currentUser;
        private Guna2GradientPanel sidebarPanel;
        private Guna2Panel headerPanel;
        private Panel mainContentPanel;
        private Guna2DragControl dragControl;
        private Label lblPageTitle;
        private Label lblPageSubtitle;
        private Guna2Button activeButton;
        public bool LogoutRequested { get; private set; }
        
        public MainDashboard(UserDto user)
        {
            _currentUser = user;
            InitializeComponent();
            LoadDefaultView();
        }

        private void InitializeComponent()
        {
            Text = "FAST SMS Enterprise Dashboard";
            MinimumSize = new Size(1180, 760);
            Size = new Size(1320, 860);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = EnterpriseTheme.AppBackground;
            this.FormBorderStyle = FormBorderStyle.None;

            sidebarPanel = new Guna2GradientPanel 
            { 
                Dock = DockStyle.Left, Width = 280, 
                FillColor = EnterpriseTheme.SidebarTop,
                FillColor2 = EnterpriseTheme.SidebarBottom,
                GradientMode = System.Drawing.Drawing2D.LinearGradientMode.Vertical
            };
            Label lblLogo = new Label { Text = "FAST SMS", Font = new Font("Segoe UI", 22, FontStyle.Bold), ForeColor = Color.White, BackColor = Color.Transparent, AutoSize = true, Location = new Point(25, 24) };
            Label lblRole = new Label { Text = _currentUser.Role.ToString(), Font = new Font("Segoe UI Semibold", 9), ForeColor = Color.FromArgb(191, 219, 254), BackColor = Color.Transparent, AutoSize = true, Location = new Point(28, 62) };
            sidebarPanel.Controls.Add(lblLogo);
            sidebarPanel.Controls.Add(lblRole);
            AddSidebarButtons();

            headerPanel = new Guna2Panel { Dock = DockStyle.Top, Height = 86, FillColor = EnterpriseTheme.Surface, Padding = new Padding(24, 0, 24, 0) };
            headerPanel.ShadowDecoration.Enabled = true;
            headerPanel.ShadowDecoration.Depth = 7;
            
            dragControl = new Guna2DragControl { TargetControl = headerPanel };

            lblPageTitle = new Label { Text = "Dashboard", Font = new Font("Segoe UI", 18, FontStyle.Bold), ForeColor = EnterpriseTheme.Text, BackColor = Color.Transparent, AutoSize = true, Location = new Point(30, 16) };
            lblPageSubtitle = new Label { Text = $"Signed in as {_currentUser.Username}", Font = new Font("Segoe UI", 9), ForeColor = EnterpriseTheme.MutedText, BackColor = Color.Transparent, AutoSize = true, Location = new Point(32, 51) };
            Guna2ControlBox btnClose = new Guna2ControlBox { Anchor = AnchorStyles.Top | AnchorStyles.Right, Location = new Point(970, 22), Size = new Size(45, 38), FillColor = EnterpriseTheme.Danger, IconColor = Color.White, HoverState = { FillColor = EnterpriseTheme.Danger, IconColor = Color.White } };
            Guna2ControlBox btnMinimize = new Guna2ControlBox { ControlBoxType = Guna.UI2.WinForms.Enums.ControlBoxType.MinimizeBox, Anchor = AnchorStyles.Top | AnchorStyles.Right, Location = new Point(920, 22), Size = new Size(45, 38), FillColor = EnterpriseTheme.Primary, IconColor = Color.White, HoverState = { FillColor = EnterpriseTheme.Primary, IconColor = Color.White } };
            headerPanel.Controls.Add(lblPageTitle);
            headerPanel.Controls.Add(lblPageSubtitle);
            headerPanel.Controls.Add(btnMinimize);
            headerPanel.Controls.Add(btnClose);

            headerPanel.Resize += (s, e) =>
            {
                btnClose.Location = new Point(headerPanel.Width - 66, 22);
                btnMinimize.Location = new Point(headerPanel.Width - 116, 22);
            };

            mainContentPanel = new Panel { Dock = DockStyle.Fill, BackColor = EnterpriseTheme.AppBackground, Padding = new Padding(22) };
            this.Controls.Add(mainContentPanel); this.Controls.Add(headerPanel); this.Controls.Add(sidebarPanel);
        }

        private void AddSidebarButtons()
        {
            int yPos = 100;
            AddNavButton("Dashboard", yPos, null); yPos += 60;

            if (_currentUser.Role == RoleType.Admin)
            {
                AddNavButton("Manage Societies", yPos, null); yPos += 55;
                AddNavButton("Manage Accounts", yPos, null); yPos += 55;
                AddNavButton("Event Approvals", yPos, null); yPos += 55;
                AddNavButton("Announcements", yPos, null); yPos += 55;
                AddNavButton("Attendance", yPos, null); yPos += 55;
                AddNavButton("Reports", yPos, null); yPos += 55;
            }
            else if (_currentUser.Role == RoleType.SocietyHead)
            {
                AddNavButton("Manage Events", yPos, null); yPos += 55;
                AddNavButton("Membership Requests", yPos, null); yPos += 55;
                AddNavButton("Member Lists", yPos, null); yPos += 55;
                AddNavButton("Assign Tasks", yPos, null); yPos += 55;
                AddNavButton("Announcements", yPos, null); yPos += 55;
                AddNavButton("Attendance", yPos, null); yPos += 55;
                AddNavButton("Reports", yPos, null); yPos += 55;
            }
            else // Student
            {
                AddNavButton("Browse Societies", yPos, null); yPos += 55;
                AddNavButton("Event Registrations", yPos, null); yPos += 55;
                AddNavButton("Announcements", yPos, null); yPos += 55;
                AddNavButton("Feedback", yPos, null); yPos += 55;
                AddNavButton("My Profile & Tickets", yPos, null); yPos += 55;
            }

            AddNavButton("Logout", 700, (s, e) => Logout());
        }

        private void Logout()
        {
            LogoutRequested = true;
            Close();
        }

        private void AddNavButton(string text, int yPos, EventHandler clickAction)
        {
            Guna2Button btn = new Guna2Button { Text = text, TextAlign = HorizontalAlignment.Left, TextOffset = new Point(18, 0), Size = new Size(240, 46), Location = new Point(20, yPos), BorderRadius = 8, FillColor = Color.Transparent, ForeColor = Color.FromArgb(203, 213, 225), Font = new Font("Segoe UI Semibold", 10), Cursor = Cursors.Hand, HoverState = { FillColor = Color.FromArgb(37, 99, 235), ForeColor = Color.White }, BackColor = Color.Transparent };
            if (clickAction != null) btn.Click += clickAction; else btn.Click += (s, e) => { SetActiveButton(btn); LoadContent(text); };
            sidebarPanel.Controls.Add(btn);
        }

        private void SetActiveButton(Guna2Button button)
        {
            if (activeButton != null)
            {
                activeButton.FillColor = Color.Transparent;
                activeButton.ForeColor = Color.FromArgb(203, 213, 225);
            }

            activeButton = button;
            activeButton.FillColor = EnterpriseTheme.Primary;
            activeButton.ForeColor = Color.White;
        }

        private void LoadContent(string title)
        {
            mainContentPanel.Controls.Clear();
            lblPageTitle.Text = title == "Dashboard Overview" ? "Dashboard" : title;
            lblPageSubtitle.Text = $"{_currentUser.Role} workspace for FAST society operations";
            if (title == "Dashboard" || title == "Dashboard Overview") { ShowControl(new DashboardOverviewControl(_currentUser)); return; }
            
            // ADMIN
            if (title == "Manage Societies" && _currentUser.Role == RoleType.Admin) { ShowControl(new ManageSocietiesControl(_currentUser)); return; }
            if (title == "Manage Accounts" && _currentUser.Role == RoleType.Admin) { ShowControl(new AdminAccountsControl(_currentUser)); return; }
            if (title == "Event Approvals" && _currentUser.Role == RoleType.Admin) { ShowControl(new AdminEventsControl(_currentUser)); return; }
            if (title == "Announcements" && _currentUser.Role == RoleType.Admin) { ShowControl(new AnnouncementControl(_currentUser)); return; }
            if (title == "Attendance" && _currentUser.Role == RoleType.Admin) { ShowControl(new EventOperationsControl(_currentUser)); return; }
            if (title == "Reports" && _currentUser.Role == RoleType.Admin) { ShowControl(new AdminMonitorControl(_currentUser)); return; }

            // SOCIETY HEAD
            if (title == "Manage Events" && _currentUser.Role == RoleType.SocietyHead) { ShowControl(new SocietyHeadEventCreatorControl(_currentUser)); return; }
            if (title == "Membership Requests" && _currentUser.Role == RoleType.SocietyHead) { ShowControl(new SocietyHeadMembershipApprovalsControl(_currentUser)); return; }
            if (title == "Member Lists" && _currentUser.Role == RoleType.SocietyHead) { ShowControl(new SocietyMembersControl(_currentUser)); return; }
            if (title == "Assign Tasks" && _currentUser.Role == RoleType.SocietyHead) { ShowControl(new SocietyHeadTaskControl(_currentUser)); return; }
            if (title == "Announcements" && _currentUser.Role == RoleType.SocietyHead) { ShowControl(new AnnouncementControl(_currentUser)); return; }
            if (title == "Attendance" && _currentUser.Role == RoleType.SocietyHead) { ShowControl(new EventOperationsControl(_currentUser)); return; }
            if (title == "Reports" && _currentUser.Role == RoleType.SocietyHead) { ShowControl(new AdminMonitorControl(_currentUser)); return; }

            // STUDENT
            if (title == "Browse Societies" && _currentUser.Role == RoleType.Student) { ShowControl(new StudentBrowseSocietiesControl(_currentUser)); return; }
            if (title == "Event Registrations" && _currentUser.Role == RoleType.Student) { ShowControl(new EventBrowserControl(_currentUser)); return; }
            if (title == "Announcements" && _currentUser.Role == RoleType.Student) { ShowControl(new AnnouncementControl(_currentUser)); return; }
            if (title == "Feedback" && _currentUser.Role == RoleType.Student) { ShowControl(new EventOperationsControl(_currentUser)); return; }
            if (title == "My Profile & Tickets" && _currentUser.Role == RoleType.Student) { ShowControl(new StudentProfileControl(_currentUser)); return; }

            Label lblContent = new Label { Text = title + " View", Font = new Font("Segoe UI", 24, FontStyle.Bold), AutoSize = true, Location = new Point(30, 30), ForeColor = Color.Silver };
            mainContentPanel.Controls.Add(lblContent);
        }

        private void ShowControl(UserControl control)
        {
            control.Dock = DockStyle.Fill;
            EnterpriseTheme.ApplyToScreen(control);
            mainContentPanel.Controls.Add(control);
        }

        private void LoadDefaultView()
        {
            LoadContent("Dashboard Overview");
        }
    }
}

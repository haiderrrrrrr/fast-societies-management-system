using System;
using System.Drawing;
using System.Windows.Forms;
using FAST.SocietiesManagement.Core.DTOs;
using FAST.SocietiesManagement.Core.Enums;

namespace FAST.SocietiesManagement.UI.UserControls
{
    public class AdminAccountsControl : UserControl
    {
        private readonly UserDto _currentUser;
        private DataGridView gridUsers;
        private TextBox txtUsername;
        private TextBox txtEmail;
        private TextBox txtPassword;
        private ComboBox cmbRole;
        private Label lblMessage;

        public AdminAccountsControl(UserDto currentUser)
        {
            _currentUser = currentUser;
            InitializeComponent();
            LoadUsers();
        }

        private void InitializeComponent()
        {
            Dock = DockStyle.Fill;
            BackColor = Color.FromArgb(248, 249, 250);
            Controls.Add(new Label { Text = "Manage Accounts", Font = new Font("Segoe UI", 20, FontStyle.Bold), Location = new Point(20, 20), AutoSize = true });

            var form = new Panel { Location = new Point(20, 70), Size = new Size(960, 120), BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
            txtUsername = new TextBox { Location = new Point(20, 45), Size = new Size(160, 25) };
            txtEmail = new TextBox { Location = new Point(195, 45), Size = new Size(210, 25) };
            txtPassword = new TextBox { Location = new Point(420, 45), Size = new Size(150, 25), UseSystemPasswordChar = true };
            cmbRole = new ComboBox { Location = new Point(585, 45), Size = new Size(150, 25), DropDownStyle = ComboBoxStyle.DropDownList, DataSource = new[] { RoleType.SocietyHead, RoleType.Admin } };
            var btnCreate = Button("Create Staff", new Point(760, 38), Color.FromArgb(37, 99, 235));
            btnCreate.Click += (s, e) => CreateStaff();
            lblMessage = new Label { Location = new Point(20, 85), AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold) };

            form.Controls.AddRange(new Control[]
            {
                new Label { Text = "Username", Location = new Point(20, 20), AutoSize = true }, txtUsername,
                new Label { Text = "Email", Location = new Point(195, 20), AutoSize = true }, txtEmail,
                new Label { Text = "Password", Location = new Point(420, 20), AutoSize = true }, txtPassword,
                new Label { Text = "Role", Location = new Point(585, 20), AutoSize = true }, cmbRole,
                btnCreate, lblMessage
            });

            gridUsers = new DataGridView { Location = new Point(20, 220), Size = new Size(960, 370), BackgroundColor = Color.White, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, AllowUserToAddRows = false, ReadOnly = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect };
            var btnSuspend = Button("Suspend", new Point(20, 610), Color.FromArgb(239, 68, 68));
            btnSuspend.Click += (s, e) => SetActive(false);
            var btnActivate = Button("Activate", new Point(130, 610), Color.FromArgb(16, 185, 129));
            btnActivate.Click += (s, e) => SetActive(true);
            var btnDelete = Button("Delete", new Point(240, 610), Color.FromArgb(127, 29, 29));
            btnDelete.Click += (s, e) => DeleteUser();
            Controls.AddRange(new Control[] { form, gridUsers, btnSuspend, btnActivate, btnDelete });
        }

        private static Button Button(string text, Point location, Color color)
        {
            return new Button { Text = text, Location = location, Size = new Size(100, 34), BackColor = color, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9, FontStyle.Bold), Cursor = Cursors.Hand };
        }

        private void LoadUsers()
        {
            gridUsers.DataSource = ServiceLocator.AuthService.GetAllUsers(_currentUser.Role);
        }

        private int? SelectedUserId()
        {
            if (gridUsers.SelectedRows.Count == 0) return null;
            return Convert.ToInt32(gridUsers.SelectedRows[0].Cells["UserID"].Value);
        }

        private void CreateStaff()
        {
            var role = (RoleType)cmbRole.SelectedItem;
            var (success, msg) = ServiceLocator.AuthService.CreateStaffAccount(txtUsername.Text, txtEmail.Text, txtPassword.Text, txtPassword.Text, role, _currentUser.UserID);
            Show(success, msg);
            if (success) { txtUsername.Clear(); txtEmail.Clear(); txtPassword.Clear(); LoadUsers(); }
        }

        private void SetActive(bool active)
        {
            var userId = SelectedUserId();
            if (!userId.HasValue) { Show(false, "Select a user first."); return; }
            var (success, msg) = ServiceLocator.AuthService.SetUserActiveStatus(userId.Value, active, _currentUser.UserID, _currentUser.Role);
            Show(success, msg);
            if (success) LoadUsers();
        }

        private void DeleteUser()
        {
            var userId = SelectedUserId();
            if (!userId.HasValue) { Show(false, "Select a user first."); return; }
            var (success, msg) = ServiceLocator.AuthService.DeleteUser(userId.Value, _currentUser.UserID, _currentUser.Role);
            Show(success, msg);
            if (success) LoadUsers();
        }

        private void Show(bool success, string msg)
        {
            lblMessage.ForeColor = success ? Color.Green : Color.Red;
            lblMessage.Text = msg;
        }
    }
}

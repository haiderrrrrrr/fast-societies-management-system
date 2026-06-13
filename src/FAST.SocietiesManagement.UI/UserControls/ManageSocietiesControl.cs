using System;
using System.Drawing;
using System.Windows.Forms;
using FAST.SocietiesManagement.Core.DTOs;
using FAST.SocietiesManagement.Core.Enums;

namespace FAST.SocietiesManagement.UI.UserControls
{
    public class ManageSocietiesControl : UserControl
    {
        private readonly UserDto _currentUser;
        private TextBox txtName;
        private TextBox txtDescription;
        private ComboBox cmbHead;
        private Label lblMessage;
        private DataGridView gridSocieties;

        public ManageSocietiesControl(UserDto currentUser)
        {
            _currentUser = currentUser;
            InitializeComponent();
            LoadHeads();
            LoadSocieties();
        }

        private void InitializeComponent()
        {
            Dock = DockStyle.Fill;
            BackColor = Color.FromArgb(248, 249, 250);
            Controls.Add(new Label { Text = "Manage Societies", Font = new Font("Segoe UI", 20, FontStyle.Bold), Location = new Point(20, 20), AutoSize = true });

            var formPanel = new Panel { Location = new Point(20, 75), Size = new Size(980, 165), BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
            txtName = new TextBox { Location = new Point(20, 45), Size = new Size(220, 25), Font = new Font("Segoe UI", 10) };
            txtDescription = new TextBox { Location = new Point(260, 45), Size = new Size(300, 70), Multiline = true, Font = new Font("Segoe UI", 10) };
            cmbHead = new ComboBox { Location = new Point(580, 45), Size = new Size(180, 25), DropDownStyle = ComboBoxStyle.DropDownList };
            var btnCreate = Button("Create", new Point(790, 25), Color.FromArgb(37, 99, 235));
            btnCreate.Click += (s, e) => CreateSociety();
            var btnUpdate = Button("Update", new Point(790, 65), Color.FromArgb(14, 116, 144));
            btnUpdate.Click += (s, e) => UpdateSociety();
            var btnAssign = Button("Assign Head", new Point(790, 105), Color.FromArgb(79, 70, 229));
            btnAssign.Click += (s, e) => AssignHead();
            lblMessage = new Label { Location = new Point(20, 125), AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold) };

            formPanel.Controls.AddRange(new Control[]
            {
                new Label { Text = "Society Name", Location = new Point(20, 20), AutoSize = true }, txtName,
                new Label { Text = "Description", Location = new Point(260, 20), AutoSize = true }, txtDescription,
                new Label { Text = "Society Head", Location = new Point(580, 20), AutoSize = true }, cmbHead,
                btnCreate, btnUpdate, btnAssign, lblMessage
            });

            gridSocieties = new DataGridView { Location = new Point(20, 270), Size = new Size(980, 350), BackgroundColor = Color.White, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, AllowUserToAddRows = false, ReadOnly = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect };
            gridSocieties.SelectionChanged += (s, e) => FillSelectedSociety();
            var btnSuspend = Button("Suspend", new Point(20, 635), Color.FromArgb(239, 68, 68));
            btnSuspend.Click += (s, e) => UpdateStatus(SocietyStatusEnum.Suspended);
            var btnActivate = Button("Activate", new Point(180, 635), Color.FromArgb(16, 185, 129));
            btnActivate.Click += (s, e) => UpdateStatus(SocietyStatusEnum.Active);
            var btnDelete = Button("Delete", new Point(340, 635), Color.FromArgb(127, 29, 29));
            btnDelete.Click += (s, e) => DeleteSociety();

            Controls.AddRange(new Control[] { formPanel, gridSocieties, btnSuspend, btnActivate, btnDelete });
        }

        private static Button Button(string text, Point location, Color color)
        {
            return new Button { Text = text, Location = location, Size = new Size(140, 34), BackColor = color, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9, FontStyle.Bold), Cursor = Cursors.Hand };
        }

        private void LoadHeads()
        {
            cmbHead.DataSource = ServiceLocator.AuthService.GetSocietyHeads(_currentUser.Role);
            cmbHead.DisplayMember = "Username";
            cmbHead.ValueMember = "UserID";
        }

        private void LoadSocieties()
        {
            gridSocieties.DataSource = ServiceLocator.SocietyService.GetAllSocieties(_currentUser.Role);
        }

        private int? SelectedSocietyId()
        {
            if (gridSocieties.SelectedRows.Count == 0) return null;
            return Convert.ToInt32(gridSocieties.SelectedRows[0].Cells["SocietyID"].Value);
        }

        private void FillSelectedSociety()
        {
            if (gridSocieties.SelectedRows.Count == 0) return;
            txtName.Text = Convert.ToString(gridSocieties.SelectedRows[0].Cells["Name"].Value);
            txtDescription.Text = Convert.ToString(gridSocieties.SelectedRows[0].Cells["Description"].Value);
        }

        private void CreateSociety()
        {
            var head = cmbHead.SelectedItem as UserDto;
            var (success, msg) = ServiceLocator.SocietyService.CreateSociety(new SocietyDto { Name = txtName.Text, Description = txtDescription.Text, HeadUserID = head?.UserID }, _currentUser.UserID, _currentUser.Role);
            ShowMessage(success, msg);
            if (success) { txtName.Clear(); txtDescription.Clear(); LoadSocieties(); }
        }

        private void UpdateSociety()
        {
            var societyId = SelectedSocietyId();
            if (!societyId.HasValue) { ShowMessage(false, "Select a society first."); return; }
            var head = cmbHead.SelectedItem as UserDto;
            var (success, msg) = ServiceLocator.SocietyService.UpdateSociety(new SocietyDto { SocietyID = societyId.Value, Name = txtName.Text, Description = txtDescription.Text, HeadUserID = head?.UserID }, _currentUser.UserID, _currentUser.Role);
            ShowMessage(success, msg);
            if (success) LoadSocieties();
        }

        private void AssignHead()
        {
            var societyId = SelectedSocietyId();
            if (!societyId.HasValue) { ShowMessage(false, "Select a society first."); return; }
            var head = cmbHead.SelectedItem as UserDto;
            var (success, msg) = ServiceLocator.SocietyService.AssignHead(societyId.Value, head?.UserID, _currentUser.UserID, _currentUser.Role);
            ShowMessage(success, msg);
            if (success) LoadSocieties();
        }

        private void UpdateStatus(SocietyStatusEnum status)
        {
            var societyId = SelectedSocietyId();
            if (!societyId.HasValue) { ShowMessage(false, "Select a society first."); return; }
            var (success, msg) = ServiceLocator.SocietyService.UpdateStatus(societyId.Value, status, _currentUser.UserID, _currentUser.Role);
            ShowMessage(success, msg);
            if (success) LoadSocieties();
        }

        private void DeleteSociety()
        {
            var societyId = SelectedSocietyId();
            if (!societyId.HasValue) { ShowMessage(false, "Select a society first."); return; }
            var (success, msg) = ServiceLocator.SocietyService.DeleteSociety(societyId.Value, _currentUser.UserID, _currentUser.Role);
            ShowMessage(success, msg);
            if (success) LoadSocieties();
        }

        private void ShowMessage(bool success, string message)
        {
            lblMessage.ForeColor = success ? Color.Green : Color.Red;
            lblMessage.Text = message;
        }
    }
}

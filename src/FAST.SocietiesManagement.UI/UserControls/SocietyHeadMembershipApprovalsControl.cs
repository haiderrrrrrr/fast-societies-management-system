using System;
using System.Drawing;
using System.Windows.Forms;
using Guna.UI2.WinForms;
using FAST.SocietiesManagement.Core.DTOs;
using FAST.SocietiesManagement.Core.Enums;

namespace FAST.SocietiesManagement.UI.UserControls
{
    public class SocietyHeadMembershipApprovalsControl : UserControl
    {
        private readonly UserDto _currentUser;
        private ComboBox cmbSociety;
        private Guna2DataGridView gridRequests;
        private Label lblMessage;
        private Label lblEmpty;
        private Guna2Button btnApprove;
        private Guna2Button btnReject;

        public SocietyHeadMembershipApprovalsControl(UserDto currentUser)
        {
            _currentUser = currentUser;
            InitializeComponent();
            LoadSocieties();
        }

        private void InitializeComponent()
        {
            Dock = DockStyle.Fill;
            BackColor = Color.Transparent;
            Controls.Add(new Label { Text = "Membership Requests", Font = new Font("Segoe UI", 24, FontStyle.Bold), ForeColor = Color.FromArgb(17, 24, 39), AutoSize = true, Location = new Point(20, 20) });
            Controls.Add(new Label { Text = "Society", Font = new Font("Segoe UI Semibold", 9, FontStyle.Bold), ForeColor = Color.FromArgb(71, 85, 105), AutoSize = true, Location = new Point(25, 72) });
            cmbSociety = new ComboBox { Location = new Point(25, 96), Size = new Size(320, 28), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbSociety.SelectedIndexChanged += (s, e) => LoadRequests();
            Controls.Add(new Label { Text = "Pending Applications", Font = new Font("Segoe UI Semibold", 11, FontStyle.Bold), AutoSize = true, Location = new Point(25, 146) });
            gridRequests = new Guna2DataGridView { Location = new Point(25, 176), Size = new Size(950, 345), AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, AllowUserToAddRows = false, ReadOnly = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect, MultiSelect = false };
            lblEmpty = new Label { Text = "No pending membership requests for this society.", Location = new Point(40, 200), Size = new Size(500, 32), ForeColor = Color.FromArgb(100, 116, 139), Font = new Font("Segoe UI Semibold", 11), Visible = false };
            btnApprove = new Guna2Button { Text = "Approve Selected", Size = new Size(190, 42), Location = new Point(25, 545), BorderRadius = 8, FillColor = Color.FromArgb(16, 185, 129), Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            btnApprove.Click += (s, e) => ProcessRequest(true);
            btnReject = new Guna2Button { Text = "Reject Selected", Size = new Size(190, 42), Location = new Point(235, 545), BorderRadius = 8, FillColor = Color.FromArgb(239, 68, 68), Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            btnReject.Click += (s, e) => ProcessRequest(false);
            lblMessage = new Label { Text = "", Location = new Point(25, 610), AutoSize = true, Font = new Font("Segoe UI Semibold", 10) };
            Controls.AddRange(new Control[] { cmbSociety, gridRequests, lblEmpty, btnApprove, btnReject, lblMessage });
        }

        private void LoadSocieties()
        {
            cmbSociety.DataSource = _currentUser.Role == RoleType.Admin
                ? ServiceLocator.SocietyService.GetAllSocieties(_currentUser.Role)
                : ServiceLocator.SocietyService.GetManagedSocieties(_currentUser.UserID);
            cmbSociety.DisplayMember = "Name";
            cmbSociety.ValueMember = "SocietyID";
            LoadRequests();
        }

        private void LoadRequests()
        {
            if (cmbSociety.SelectedValue is int societyId)
            {
                var requests = ServiceLocator.MembershipService.GetPendingRequests(societyId, _currentUser.Role);
                gridRequests.DataSource = requests;
                lblEmpty.Visible = requests.Count == 0;
                gridRequests.Visible = requests.Count > 0;
                btnApprove.Enabled = requests.Count > 0;
                btnReject.Enabled = requests.Count > 0;
            }
        }

        private void ProcessRequest(bool approve)
        {
            if (gridRequests.SelectedRows.Count == 0) return;
            int membershipId = Convert.ToInt32(gridRequests.SelectedRows[0].Cells["MembershipID"].Value);
            var (success, msg) = ServiceLocator.MembershipService.DecisionOnMembership(membershipId, approve, _currentUser.UserID, _currentUser.Role);
            lblMessage.ForeColor = success ? Color.Green : Color.Red;
            lblMessage.Text = msg;
            if (success) LoadRequests();
        }
    }
}

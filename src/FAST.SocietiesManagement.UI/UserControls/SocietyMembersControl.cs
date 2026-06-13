using System.Drawing;
using System.Windows.Forms;
using FAST.SocietiesManagement.Core.DTOs;
using FAST.SocietiesManagement.Core.Enums;

namespace FAST.SocietiesManagement.UI.UserControls
{
    public class SocietyMembersControl : UserControl
    {
        private readonly UserDto _currentUser;
        private ComboBox cmbSociety;
        private DataGridView gridMembers;

        public SocietyMembersControl(UserDto currentUser)
        {
            _currentUser = currentUser;
            Dock = DockStyle.Fill;
            BackColor = Color.FromArgb(248, 249, 250);
            Controls.Add(new Label { Text = "Member Lists", Font = new Font("Segoe UI", 22, FontStyle.Bold), Location = new Point(20, 20), AutoSize = true });
            Controls.Add(new Label { Text = "Society", Font = new Font("Segoe UI Semibold", 9, FontStyle.Bold), ForeColor = Color.FromArgb(71, 85, 105), Location = new Point(20, 76), AutoSize = true });
            cmbSociety = new ComboBox { Location = new Point(20, 100), Size = new Size(320, 28), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbSociety.SelectedIndexChanged += (s, e) => LoadMembers();
            Controls.Add(new Label { Text = "Approved Members", Font = new Font("Segoe UI Semibold", 11, FontStyle.Bold), Location = new Point(20, 150), AutoSize = true });
            gridMembers = new DataGridView { Location = new Point(20, 180), Size = new Size(960, 450), BackgroundColor = Color.White, ReadOnly = true, AllowUserToAddRows = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };
            Controls.AddRange(new Control[] { cmbSociety, gridMembers });
            cmbSociety.DataSource = _currentUser.Role == RoleType.Admin ? ServiceLocator.SocietyService.GetAllSocieties(_currentUser.Role) : ServiceLocator.SocietyService.GetManagedSocieties(_currentUser.UserID);
            cmbSociety.DisplayMember = "Name";
            cmbSociety.ValueMember = "SocietyID";
            LoadMembers();
        }

        private void LoadMembers()
        {
            if (cmbSociety.SelectedValue is int societyId)
                gridMembers.DataSource = ServiceLocator.MembershipService.GetApprovedMembers(societyId, _currentUser.Role);
        }
    }
}

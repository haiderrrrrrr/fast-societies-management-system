using System.Drawing;
using System.Windows.Forms;
using FAST.SocietiesManagement.Core.DTOs;
using FAST.SocietiesManagement.Core.Enums;

namespace FAST.SocietiesManagement.UI.UserControls
{
    public class AdminMonitorControl : UserControl
    {
        private readonly UserDto _currentUser;
        private ComboBox cmbSociety;
        private DataGridView gridReport;

        public AdminMonitorControl(UserDto currentUser)
        {
            _currentUser = currentUser;
            InitializeComponent();
            LoadReport();
        }

        private void InitializeComponent()
        {
            Dock = DockStyle.Fill;
            BackColor = Color.FromArgb(248, 249, 250);
            Controls.Add(new Label { Text = _currentUser.Role == RoleType.Admin ? "University Reports" : "Society Reports", Font = new Font("Segoe UI", 22, FontStyle.Bold), Location = new Point(20, 20), AutoSize = true });
            Controls.Add(new Label { Text = "Report Scope", Font = new Font("Segoe UI Semibold", 9, FontStyle.Bold), ForeColor = Color.FromArgb(71, 85, 105), Location = new Point(20, 76), AutoSize = true });
            cmbSociety = new ComboBox { Location = new Point(20, 100), Size = new Size(320, 28), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbSociety.SelectedIndexChanged += (s, e) => LoadReport();
            Controls.Add(new Label { Text = "Report Results", Font = new Font("Segoe UI Semibold", 11, FontStyle.Bold), Location = new Point(20, 150), AutoSize = true });
            gridReport = new DataGridView { Location = new Point(20, 180), Size = new Size(960, 420), BackgroundColor = Color.White, ReadOnly = true, AllowUserToAddRows = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };
            Controls.AddRange(new Control[] { cmbSociety, gridReport });

            if (_currentUser.Role == RoleType.Admin)
            {
                cmbSociety.Visible = false;
            }
            else
            {
                cmbSociety.DataSource = ServiceLocator.SocietyService.GetManagedSocieties(_currentUser.UserID);
                cmbSociety.DisplayMember = "Name";
                cmbSociety.ValueMember = "SocietyID";
            }
        }

        private void LoadReport()
        {
            if (_currentUser.Role == RoleType.Admin)
                gridReport.DataSource = ServiceLocator.EnterpriseService.GetUniversityReport(_currentUser.Role);
            else if (cmbSociety.SelectedValue is int societyId)
                gridReport.DataSource = ServiceLocator.EnterpriseService.GetSocietyReport(societyId, _currentUser.Role);
        }
    }
}

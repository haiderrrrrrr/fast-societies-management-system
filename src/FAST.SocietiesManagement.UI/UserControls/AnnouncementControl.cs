using System;
using System.Drawing;
using System.Windows.Forms;
using FAST.SocietiesManagement.Core.DTOs;
using FAST.SocietiesManagement.Core.Enums;
using FAST.SocietiesManagement.UI.Theme;

namespace FAST.SocietiesManagement.UI.UserControls
{
    public class AnnouncementControl : UserControl
    {
        private readonly UserDto _currentUser;
        private ComboBox cmbSociety;
        private TextBox txtTitle;
        private TextBox txtMessage;
        private DataGridView gridAnnouncements;
        private Label lblMessage;
        private Label lblEmpty;
        private Label lblPublished;

        public AnnouncementControl(UserDto currentUser)
        {
            _currentUser = currentUser;
            InitializeComponent();
            LoadSocieties();
            LoadAnnouncements();
        }

        private void InitializeComponent()
        {
            Dock = DockStyle.Fill;
            BackColor = Color.FromArgb(248, 249, 250);
            Controls.Add(new Label { Text = "Announcements", Font = new Font("Segoe UI", 22, FontStyle.Bold), Location = new Point(20, 20), AutoSize = true });

            var lblSociety = FieldLabel("Society", new Point(20, 76));
            cmbSociety = new ComboBox { Location = new Point(20, 100), Size = new Size(260, 28), DropDownStyle = ComboBoxStyle.DropDownList };
            var lblTitle = FieldLabel("Announcement Title", new Point(300, 76));
            txtTitle = new TextBox { Location = new Point(300, 100), Size = new Size(310, 28) };
            var lblBody = FieldLabel("Message", new Point(20, 144));
            txtMessage = new TextBox { Location = new Point(20, 168), Size = new Size(590, 70), Multiline = true };
            var btnPublish = new Button { Text = "Publish", Location = new Point(635, 168), Size = new Size(120, 34), BackColor = Color.FromArgb(37, 99, 235), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnPublish.Click += (s, e) => Publish();
            lblMessage = new Label { Location = new Point(635, 207), Size = new Size(300, 34), Font = new Font("Segoe UI", 9, FontStyle.Bold) };

            lblPublished = new Label { Text = "Published Announcements", Location = new Point(20, 268), AutoSize = true, Font = new Font("Segoe UI Semibold", 11, FontStyle.Bold) };
            Controls.Add(lblPublished);
            gridAnnouncements = new DataGridView { Location = new Point(20, 300), Size = new Size(960, 350), BackgroundColor = Color.White, ReadOnly = true, AllowUserToAddRows = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, TabStop = false };
            gridAnnouncements.SelectionChanged += (s, e) => ClearAnnouncementSelection();
            gridAnnouncements.CellMouseDown += (s, e) => ClearAnnouncementSelection();
            gridAnnouncements.DataBindingComplete += (s, e) => ClearAnnouncementSelection();
            lblEmpty = new Label { Text = "No announcements yet.", Location = new Point(35, 320), Size = new Size(600, 32), Font = new Font("Segoe UI Semibold", 11), ForeColor = EnterpriseTheme.MutedText, Visible = false };
            Controls.AddRange(new Control[] { lblSociety, cmbSociety, lblTitle, txtTitle, lblBody, txtMessage, btnPublish, lblMessage, gridAnnouncements, lblEmpty });

            bool canPublish = _currentUser.Role == RoleType.Admin || _currentUser.Role == RoleType.SocietyHead;
            cmbSociety.Visible = canPublish;
            lblSociety.Visible = canPublish;
            lblTitle.Visible = canPublish;
            lblBody.Visible = canPublish;
            txtTitle.Visible = canPublish;
            txtMessage.Visible = canPublish;
            btnPublish.Visible = canPublish;

            if (!canPublish)
            {
                lblPublished.Location = new Point(20, 88);
                gridAnnouncements.Location = new Point(20, 120);
                gridAnnouncements.Size = new Size(960, 540);
                lblEmpty.Location = new Point(35, 142);
            }
        }

        private static Label FieldLabel(string text, Point location)
        {
            return new Label { Text = text, Location = location, AutoSize = true, Font = new Font("Segoe UI Semibold", 9, FontStyle.Bold), ForeColor = Color.FromArgb(71, 85, 105) };
        }

        private void LoadSocieties()
        {
            cmbSociety.DataSource = _currentUser.Role == RoleType.Admin
                ? ServiceLocator.SocietyService.GetAllSocieties(_currentUser.Role)
                : ServiceLocator.SocietyService.GetManagedSocieties(_currentUser.UserID);
            cmbSociety.DisplayMember = "Name";
            cmbSociety.ValueMember = "SocietyID";
        }

        private void LoadAnnouncements()
        {
            System.Collections.Generic.List<AnnouncementDto> announcements = new();
            if (_currentUser.Role == RoleType.Student)
                announcements = ServiceLocator.EnterpriseService.GetStudentAnnouncements(_currentUser.StudentID);
            else if (_currentUser.Role == RoleType.Admin)
                announcements = ServiceLocator.EnterpriseService.GetAllAnnouncements(_currentUser.Role);
            else if (cmbSociety.SelectedValue is int id)
                announcements = ServiceLocator.EnterpriseService.GetSocietyAnnouncements(id);

            gridAnnouncements.DataSource = announcements;
            lblEmpty.Visible = announcements.Count == 0;
            gridAnnouncements.Visible = announcements.Count > 0;
            ClearAnnouncementSelection();
            FormatAnnouncementGrid();
        }

        private void ClearAnnouncementSelection()
        {
            if (gridAnnouncements == null)
            {
                return;
            }

            gridAnnouncements.ClearSelection();
            gridAnnouncements.CurrentCell = null;
        }

        private void FormatAnnouncementGrid()
        {
            if (gridAnnouncements.Columns.Count == 0)
            {
                return;
            }

            if (gridAnnouncements.Columns.Contains("Message"))
            {
                gridAnnouncements.Columns["Message"].DefaultCellStyle.WrapMode = DataGridViewTriState.False;
            }

            if (gridAnnouncements.Columns.Contains("PublishedAt"))
            {
                gridAnnouncements.Columns["PublishedAt"].DefaultCellStyle.Format = "g";
            }
        }

        private void Publish()
        {
            if (cmbSociety.SelectedValue is not int societyId) return;
            var (success, msg) = ServiceLocator.EnterpriseService.PublishAnnouncement(new AnnouncementDto
            {
                SocietyID = societyId,
                Title = txtTitle.Text,
                Message = txtMessage.Text,
                CreatedBy = _currentUser.UserID
            }, _currentUser.Role);
            lblMessage.ForeColor = success ? Color.Green : Color.Red;
            lblMessage.Text = msg;
            if (success) { txtTitle.Clear(); txtMessage.Clear(); LoadAnnouncements(); }
        }
    }
}

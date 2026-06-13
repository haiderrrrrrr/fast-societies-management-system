using System;
using System.Drawing;
using System.Windows.Forms;
using FAST.SocietiesManagement.Core.DTOs;
using FAST.SocietiesManagement.Core.Enums;

namespace FAST.SocietiesManagement.UI.UserControls
{
    public class AdminEventsControl : UserControl
    {
        private readonly UserDto _currentUser;
        private DataGridView gridEvents;
        private Label lblMessage;

        public AdminEventsControl(UserDto currentUser)
        {
            _currentUser = currentUser;
            InitializeComponent();
            LoadEvents();
        }

        private void InitializeComponent()
        {
            Dock = DockStyle.Fill;
            BackColor = Color.FromArgb(248, 249, 250);
            Controls.Add(new Label { Text = "Event Approvals", Font = new Font("Segoe UI", 20, FontStyle.Bold), Location = new Point(20, 20), AutoSize = true });
            gridEvents = new DataGridView { Location = new Point(20, 80), Size = new Size(960, 470), BackgroundColor = Color.White, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, AllowUserToAddRows = false, ReadOnly = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect };
            var btnApprove = Button("Approve", new Point(20, 575), Color.FromArgb(16, 185, 129));
            btnApprove.Click += (s, e) => UpdateStatus(EventStatusEnum.Approved);
            var btnCancel = Button("Cancel", new Point(130, 575), Color.FromArgb(239, 68, 68));
            btnCancel.Click += (s, e) => UpdateStatus(EventStatusEnum.Cancelled);
            lblMessage = new Label { Location = new Point(260, 583), AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
            Controls.AddRange(new Control[] { gridEvents, btnApprove, btnCancel, lblMessage });
        }

        private static Button Button(string text, Point location, Color color)
        {
            return new Button { Text = text, Location = location, Size = new Size(95, 34), BackColor = color, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9, FontStyle.Bold), Cursor = Cursors.Hand };
        }

        private void LoadEvents()
        {
            gridEvents.DataSource = ServiceLocator.EventService.GetAllEvents(_currentUser.Role);
        }

        private void UpdateStatus(EventStatusEnum status)
        {
            if (gridEvents.SelectedRows.Count == 0) { Show(false, "Select an event first."); return; }
            int eventId = Convert.ToInt32(gridEvents.SelectedRows[0].Cells["EventID"].Value);
            var (success, msg) = ServiceLocator.EventService.UpdateEventStatus(eventId, status, _currentUser.UserID, _currentUser.Role);
            Show(success, msg);
            if (success) LoadEvents();
        }

        private void Show(bool success, string msg)
        {
            lblMessage.ForeColor = success ? Color.Green : Color.Red;
            lblMessage.Text = msg;
        }
    }
}

using System;
using System.Drawing;
using System.Windows.Forms;
using Guna.UI2.WinForms;
using FAST.SocietiesManagement.Core.DTOs;
using FAST.SocietiesManagement.Core.Enums;

namespace FAST.SocietiesManagement.UI.UserControls
{
    public class SocietyHeadEventCreatorControl : UserControl
    {
        private readonly UserDto _currentUser;
        private ComboBox cmbSociety;
        private ComboBox cmbVenue;
        private Guna2TextBox txtTitle;
        private Guna2TextBox txtDescription;
        private Guna2DateTimePicker dtpDate;
        private Guna2NumericUpDown numCapacity;
        private DataGridView gridEvents;
        private Label lblMessage;

        public SocietyHeadEventCreatorControl(UserDto currentUser)
        {
            _currentUser = currentUser;
            InitializeComponent();
            LoadLookups();
            LoadEvents();
        }

        private void InitializeComponent()
        {
            Dock = DockStyle.Fill;
            BackColor = Color.Transparent;
            Controls.Add(new Label { Text = "Manage Events", Font = new Font("Segoe UI", 22, FontStyle.Bold), ForeColor = Color.FromArgb(17, 24, 39), AutoSize = true, Location = new Point(20, 20) });

            var form = new Guna2Panel { Location = new Point(20, 80), Size = new Size(420, 540), FillColor = Color.FromArgb(219, 234, 254), BorderRadius = 12, ShadowDecoration = { Enabled = true, Depth = 10 } };
            cmbSociety = new ComboBox { Location = new Point(25, 45), Size = new Size(360, 25), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbVenue = new ComboBox { Location = new Point(25, 100), Size = new Size(360, 25), DropDownStyle = ComboBoxStyle.DropDownList };
            txtTitle = new Guna2TextBox { Location = new Point(25, 155), Size = new Size(360, 40), BorderRadius = 8, PlaceholderText = "Event title" };
            txtDescription = new Guna2TextBox { Location = new Point(25, 235), Size = new Size(360, 85), BorderRadius = 8, Multiline = true, PlaceholderText = "Description" };
            dtpDate = new Guna2DateTimePicker { Location = new Point(25, 370), Size = new Size(210, 40), BorderRadius = 8, Value = DateTime.Now.AddDays(7) };
            numCapacity = new Guna2NumericUpDown { Location = new Point(250, 370), Size = new Size(135, 40), BorderRadius = 8, Minimum = 1, Maximum = 10000, Value = 100 };
            var btnCancel = new Guna2Button { Text = "Cancel Selected", Location = new Point(25, 420), Size = new Size(160, 40), BorderRadius = 8, FillColor = Color.FromArgb(239, 68, 68), Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            btnCancel.Click += (s, e) => CancelEvent();
            var btnCreate = new Guna2Button { Text = "Submit Event", Location = new Point(205, 420), Size = new Size(180, 40), BorderRadius = 8, FillColor = Color.FromArgb(37, 99, 235), Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            btnCreate.Click += (s, e) => CreateEvent();
            var btnUpdate = new Guna2Button { Text = "Update Selected", Location = new Point(205, 465), Size = new Size(180, 40), BorderRadius = 8, FillColor = Color.FromArgb(14, 116, 144), Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            btnUpdate.Click += (s, e) => UpdateEvent();
            lblMessage = new Label { Location = new Point(25, 505), Size = new Size(360, 30), Font = new Font("Segoe UI", 9, FontStyle.Bold) };
            form.Controls.AddRange(new Control[]
            {
                new Label { Text = "Society", Location = new Point(25, 20), AutoSize = true }, cmbSociety,
                new Label { Text = "Venue", Location = new Point(25, 75), AutoSize = true }, cmbVenue,
                new Label { Text = "Event Title", Location = new Point(25, 130), AutoSize = true },
                txtTitle, txtDescription,
                new Label { Text = "Description", Location = new Point(25, 210), AutoSize = true },
                new Label { Text = "Event Date", Location = new Point(25, 345), AutoSize = true },
                new Label { Text = "Capacity", Location = new Point(250, 345), AutoSize = true },
                dtpDate, numCapacity, btnCancel, btnCreate, btnUpdate, lblMessage
            });

            gridEvents = new DataGridView { Location = new Point(470, 80), Size = new Size(560, 540), BackgroundColor = Color.White, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, AllowUserToAddRows = false, ReadOnly = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect };
            gridEvents.SelectionChanged += (s, e) => FillSelectedEvent();
            Controls.AddRange(new Control[] { form, gridEvents });
        }

        private void LoadLookups()
        {
            cmbSociety.DataSource = _currentUser.Role == RoleType.Admin ? ServiceLocator.SocietyService.GetAllSocieties(_currentUser.Role) : ServiceLocator.SocietyService.GetManagedSocieties(_currentUser.UserID);
            cmbSociety.DisplayMember = "Name";
            cmbSociety.ValueMember = "SocietyID";
            cmbSociety.SelectedIndexChanged += (s, e) => LoadEvents();
            cmbVenue.DataSource = ServiceLocator.EventService.GetVenues();
            cmbVenue.DisplayMember = "Name";
            cmbVenue.ValueMember = "VenueID";
        }

        private int? SelectedSocietyId() => cmbSociety.SelectedValue is int id ? id : null;

        private void LoadEvents()
        {
            var id = SelectedSocietyId();
            gridEvents.DataSource = id.HasValue ? ServiceLocator.EventService.GetEventsBySociety(id.Value) : null;
        }

        private void FillSelectedEvent()
        {
            if (gridEvents.SelectedRows.Count == 0) return;
            txtTitle.Text = Convert.ToString(gridEvents.SelectedRows[0].Cells["Title"].Value);
            txtDescription.Text = Convert.ToString(gridEvents.SelectedRows[0].Cells["Description"].Value);
            if (DateTime.TryParse(Convert.ToString(gridEvents.SelectedRows[0].Cells["EventDate"].Value), out var eventDate)) dtpDate.Value = eventDate;
            if (int.TryParse(Convert.ToString(gridEvents.SelectedRows[0].Cells["MaxCapacity"].Value), out var cap)) numCapacity.Value = Math.Max(numCapacity.Minimum, Math.Min(numCapacity.Maximum, cap));
        }

        private void CreateEvent()
        {
            var societyId = SelectedSocietyId();
            if (!societyId.HasValue || cmbVenue.SelectedValue is not int venueId) { ShowMessage(false, "Select society and venue."); return; }
            var dto = new EventDto { SocietyID = societyId.Value, VenueID = venueId, Title = txtTitle.Text, Description = txtDescription.Text, EventDate = dtpDate.Value, MaxCapacity = (int)numCapacity.Value };
            var (success, msg) = ServiceLocator.EventService.CreateEvent(dto, _currentUser.UserID, _currentUser.Role);
            ShowMessage(success, msg);
            if (success) { txtTitle.Clear(); txtDescription.Clear(); LoadEvents(); }
        }

        private void UpdateEvent()
        {
            if (gridEvents.SelectedRows.Count == 0) { ShowMessage(false, "Select an event first."); return; }
            var societyId = SelectedSocietyId();
            if (!societyId.HasValue || cmbVenue.SelectedValue is not int venueId) { ShowMessage(false, "Select society and venue."); return; }
            int eventId = Convert.ToInt32(gridEvents.SelectedRows[0].Cells["EventID"].Value);
            var dto = new EventDto { EventID = eventId, SocietyID = societyId.Value, VenueID = venueId, Title = txtTitle.Text, Description = txtDescription.Text, EventDate = dtpDate.Value, MaxCapacity = (int)numCapacity.Value };
            var (success, msg) = ServiceLocator.EventService.UpdateEvent(dto, _currentUser.UserID, _currentUser.Role);
            ShowMessage(success, msg);
            if (success) LoadEvents();
        }

        private void CancelEvent()
        {
            if (gridEvents.SelectedRows.Count == 0) { ShowMessage(false, "Select an event first."); return; }
            int eventId = Convert.ToInt32(gridEvents.SelectedRows[0].Cells["EventID"].Value);
            var (success, msg) = ServiceLocator.EventService.UpdateEventStatus(eventId, EventStatusEnum.Cancelled, _currentUser.UserID, _currentUser.Role);
            ShowMessage(success, msg);
            if (success) LoadEvents();
        }

        private void ShowMessage(bool success, string message)
        {
            lblMessage.ForeColor = success ? Color.Green : Color.Red;
            lblMessage.Text = message;
        }
    }
}

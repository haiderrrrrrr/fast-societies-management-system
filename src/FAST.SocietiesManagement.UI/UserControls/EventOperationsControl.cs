using System;
using System.Drawing;
using System.Windows.Forms;
using FAST.SocietiesManagement.Core.DTOs;
using FAST.SocietiesManagement.Core.Enums;

namespace FAST.SocietiesManagement.UI.UserControls
{
    public class EventOperationsControl : UserControl
    {
        private readonly UserDto _currentUser;
        private ComboBox cmbSociety;
        private ComboBox cmbEvent;
        private ComboBox cmbRegistrant;
        private NumericUpDown numRating;
        private TextBox txtFeedback;
        private DataGridView gridAttendance;
        private DataGridView gridFeedback;
        private Label lblMessage;
        private Label lblSociety;
        private Label lblEvent;
        private Label lblRegistrant;
        private Label lblRating;
        private Label lblFeedback;
        private Label lblAttendanceGrid;
        private Label lblFeedbackGrid;
        private Label lblFeedbackEmpty;
        private Button btnPresent;
        private Button btnAbsent;
        private Button btnFeedback;

        public EventOperationsControl(UserDto currentUser)
        {
            _currentUser = currentUser;
            InitializeComponent();
            LoadSocieties();
        }

        private void InitializeComponent()
        {
            Dock = DockStyle.Fill;
            BackColor = Color.FromArgb(248, 249, 250);
            Resize += (s, e) => LayoutStaffTables();
            Controls.Add(new Label { Text = _currentUser.Role == RoleType.Student ? "Feedback" : "Attendance & Feedback", Font = new Font("Segoe UI", 22, FontStyle.Bold), Location = new Point(20, 20), AutoSize = true });

            lblSociety = FieldLabel("Society", new Point(20, 74));
            cmbSociety = new ComboBox { Location = new Point(20, 98), Size = new Size(240, 28), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbSociety.SelectedIndexChanged += (s, e) => LoadEvents();
            lblEvent = FieldLabel("Event", new Point(280, 74));
            cmbEvent = new ComboBox { Location = new Point(280, 98), Size = new Size(360, 28), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbEvent.SelectedIndexChanged += (s, e) => LoadOperationalData();
            lblRegistrant = FieldLabel("Registered Student", new Point(20, 142));
            cmbRegistrant = new ComboBox { Location = new Point(20, 166), Size = new Size(360, 28), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbRegistrant.Format += (s, e) =>
            {
                if (e.ListItem is EventTicketDto ticket)
                {
                    e.Value = $"{ticket.StudentName} - {ticket.EventTitle}";
                }
            };
            btnPresent = Button("Mark Present", new Point(400, 160), Color.FromArgb(16, 185, 129));
            btnPresent.Click += (s, e) => MarkAttendance(true);
            btnAbsent = Button("Mark Absent", new Point(530, 160), Color.FromArgb(239, 68, 68));
            btnAbsent.Click += (s, e) => MarkAttendance(false);

            lblRating = FieldLabel("Rating", new Point(20, 220));
            numRating = new NumericUpDown { Location = new Point(20, 244), Size = new Size(80, 28), Minimum = 1, Maximum = 5, Value = 5 };
            lblFeedback = FieldLabel("Feedback Comments", new Point(120, 220));
            txtFeedback = new TextBox { Location = new Point(120, 244), Size = new Size(520, 62), Multiline = true };
            btnFeedback = Button(_currentUser.Role == RoleType.Student ? "Submit Feedback" : "Refresh Feedback", new Point(660, 244), Color.FromArgb(37, 99, 235));
            btnFeedback.Click += (s, e) => SubmitOrRefreshFeedback();
            lblMessage = new Label { Location = new Point(660, 286), Size = new Size(320, 36), Font = new Font("Segoe UI", 9, FontStyle.Bold) };

            lblAttendanceGrid = FieldLabel("Attendance Records", new Point(20, 340));
            lblFeedbackGrid = FieldLabel("Feedback Records", new Point(520, 340));
            gridAttendance = new DataGridView { Location = new Point(20, 370), Size = new Size(480, 300), BackgroundColor = Color.White, ReadOnly = true, AllowUserToAddRows = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };
            gridFeedback = new DataGridView { Location = new Point(520, 370), Size = new Size(500, 300), BackgroundColor = Color.White, ReadOnly = true, AllowUserToAddRows = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };
            lblFeedbackEmpty = new Label { Text = "No feedback submitted for the selected event yet.", Location = new Point(535, 420), Size = new Size(440, 36), Font = new Font("Segoe UI Semibold", 10), ForeColor = Color.FromArgb(100, 116, 139), Visible = false };

            Controls.AddRange(new Control[] { lblSociety, cmbSociety, lblEvent, cmbEvent, lblRegistrant, cmbRegistrant, btnPresent, btnAbsent, lblRating, numRating, lblFeedback, txtFeedback, btnFeedback, lblMessage, lblAttendanceGrid, gridAttendance, lblFeedbackGrid, gridFeedback, lblFeedbackEmpty });

            bool staff = _currentUser.Role == RoleType.Admin || _currentUser.Role == RoleType.SocietyHead;
            lblRegistrant.Visible = staff;
            cmbRegistrant.Visible = staff;
            btnPresent.Visible = staff;
            btnAbsent.Visible = staff;
            lblAttendanceGrid.Visible = staff;
            gridAttendance.Visible = staff;
            lblRating.Visible = !staff;
            numRating.Visible = !staff;
            lblFeedback.Visible = !staff;
            txtFeedback.Visible = !staff;
            if (_currentUser.Role == RoleType.Student)
            {
                cmbSociety.Visible = false;
                lblSociety.Visible = false;
                lblEvent.Location = new Point(20, 84);
                cmbEvent.Location = new Point(20, 108);
                cmbEvent.Size = new Size(420, 30);
                lblRating.Location = new Point(20, 162);
                numRating.Location = new Point(20, 186);
                lblFeedback.Location = new Point(120, 162);
                txtFeedback.Location = new Point(120, 186);
                txtFeedback.Size = new Size(520, 90);
                btnFeedback.Location = new Point(660, 186);
                lblMessage.Location = new Point(660, 228);
                lblFeedbackGrid.Visible = false;
                gridFeedback.Visible = false;
                lblFeedbackEmpty.Visible = false;
            }
            else
            {
                btnFeedback.Text = "Refresh Records";
                btnFeedback.Location = new Point(660, 98);
                lblMessage.Location = new Point(660, 142);
                LayoutStaffTables();
            }
        }

        private void LayoutStaffTables()
        {
            if (_currentUser.Role == RoleType.Student || gridAttendance == null || gridFeedback == null)
            {
                return;
            }

            int left = 20;
            int top = 230;
            int width = Math.Max(720, ClientSize.Width - 40);
            int availableHeight = Math.Max(390, ClientSize.Height - top - 35);
            int labelHeight = 25;
            int gap = 34;
            int tableHeight = Math.Max(150, (availableHeight - (labelHeight * 2) - gap) / 2);

            lblAttendanceGrid.Location = new Point(left, top);
            gridAttendance.Location = new Point(left, top + labelHeight);
            gridAttendance.Size = new Size(width, tableHeight);

            int feedbackTop = gridAttendance.Bottom + gap;
            lblFeedbackGrid.Location = new Point(left, feedbackTop);
            gridFeedback.Location = new Point(left, feedbackTop + labelHeight);
            gridFeedback.Size = new Size(width, tableHeight);
            lblFeedbackEmpty.Location = new Point(left + 15, gridFeedback.Top + 52);
            lblFeedbackEmpty.Size = new Size(width - 30, 36);
        }

        private static Label FieldLabel(string text, Point location)
        {
            return new Label { Text = text, Location = location, AutoSize = true, Font = new Font("Segoe UI Semibold", 9, FontStyle.Bold), ForeColor = Color.FromArgb(71, 85, 105) };
        }

        private static Button Button(string text, Point location, Color color)
        {
            return new Button { Text = text, Location = location, Size = new Size(120, 34), BackColor = color, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
        }

        private void LoadSocieties()
        {
            if (_currentUser.Role == RoleType.Student)
            {
                cmbEvent.DataSource = ServiceLocator.EventService.GetUpcomingEvents();
                cmbEvent.DisplayMember = "Title";
                cmbEvent.ValueMember = "EventID";
                var hasEvents = cmbEvent.Items.Count > 0;
                btnFeedback.Enabled = hasEvents;
                Show(hasEvents, hasEvents ? "Select an event, add rating and comments, then submit." : "No upcoming approved events available for feedback.");
                return;
            }

            cmbSociety.DataSource = _currentUser.Role == RoleType.Admin ? ServiceLocator.SocietyService.GetAllSocieties(_currentUser.Role) : ServiceLocator.SocietyService.GetManagedSocieties(_currentUser.UserID);
            cmbSociety.DisplayMember = "Name";
            cmbSociety.ValueMember = "SocietyID";
            LoadEvents();
        }

        private void LoadEvents()
        {
            if (cmbSociety.SelectedValue is int societyId)
            {
                cmbEvent.DataSource = ServiceLocator.EventService.GetEventsBySociety(societyId);
                cmbEvent.DisplayMember = "Title";
                cmbEvent.ValueMember = "EventID";
            }
        }

        private int? EventId()
        {
            if (cmbEvent.SelectedValue is int id)
            {
                return id;
            }

            return cmbEvent.SelectedItem is EventDto selected ? selected.EventID : null;
        }

        private void LoadOperationalData()
        {
            var eventId = EventId();
            if (!eventId.HasValue) return;
            if (_currentUser.Role != RoleType.Student)
            {
                var registrants = ServiceLocator.EnterpriseService.GetEventRegistrants(eventId.Value, _currentUser.Role);
                cmbRegistrant.DataSource = null;
                cmbRegistrant.DataSource = registrants;
                cmbRegistrant.DisplayMember = "StudentName";
                cmbRegistrant.ValueMember = "StudentID";
                cmbRegistrant.Enabled = registrants.Count > 0;
                btnPresent.Enabled = registrants.Count > 0;
                btnAbsent.Enabled = registrants.Count > 0;
                if (registrants.Count == 0)
                {
                    Show(false, "No registered students for this event yet.");
                }
                else
                {
                    lblMessage.Text = "";
                }
                gridAttendance.DataSource = ServiceLocator.EnterpriseService.GetAttendanceByEvent(eventId.Value, _currentUser.Role);
            }

            var feedback = ServiceLocator.EnterpriseService.GetFeedbackByEvent(eventId.Value, _currentUser.Role);
            gridFeedback.DataSource = feedback;
            lblFeedbackGrid.Text = feedback.Count == 1 ? "Feedback Records (1 submitted)" : $"Feedback Records ({feedback.Count} submitted)";
            lblFeedbackEmpty.Visible = _currentUser.Role != RoleType.Student && feedback.Count == 0;
            gridFeedback.Visible = _currentUser.Role != RoleType.Student && feedback.Count > 0;
        }

        private void MarkAttendance(bool present)
        {
            var eventId = EventId();
            if (!eventId.HasValue || cmbRegistrant.SelectedItem is not EventTicketDto ticket) { Show(false, "Select event and registrant."); return; }
            var (success, msg) = ServiceLocator.EnterpriseService.MarkAttendance(eventId.Value, ticket.StudentID, present, _currentUser.UserID, _currentUser.Role);
            Show(success, msg);
            if (success) LoadOperationalData();
        }

        private void SubmitOrRefreshFeedback()
        {
            var eventId = EventId();
            if (!eventId.HasValue) return;
            if (_currentUser.Role == RoleType.Student)
            {
                var (success, msg) = ServiceLocator.EnterpriseService.SubmitFeedback(eventId.Value, _currentUser.StudentID, (int)numRating.Value, txtFeedback.Text);
                Show(success, msg);
                if (success) txtFeedback.Clear();
                return;
            }
            LoadOperationalData();
        }

        private void Show(bool success, string msg)
        {
            lblMessage.ForeColor = success ? Color.Green : Color.Red;
            lblMessage.Text = msg;
        }
    }
}

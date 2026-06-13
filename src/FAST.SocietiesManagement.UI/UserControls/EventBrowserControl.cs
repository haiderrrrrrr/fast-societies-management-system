using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Guna.UI2.WinForms;
using FAST.SocietiesManagement.Core.DTOs;
using FAST.SocietiesManagement.UI.Theme;

namespace FAST.SocietiesManagement.UI.UserControls
{
    public class EventBrowserControl : UserControl
    {
        private readonly UserDto _currentUser;
        private FlowLayoutPanel eventsFlowPanel;

        public EventBrowserControl(UserDto currentUser)
        {
            _currentUser = currentUser;
            InitializeComponent();
            LoadUpcomingEvents();
        }

        private void InitializeComponent()
        {
            Dock = DockStyle.Fill;
            BackColor = Color.Transparent;
            Controls.Add(new Label { Text = "Upcoming Events", Font = new Font("Segoe UI", 22, FontStyle.Bold), ForeColor = Color.FromArgb(17, 24, 39), AutoSize = true, Location = new Point(20, 20) });
            eventsFlowPanel = new FlowLayoutPanel { Location = new Point(20, 80), Size = new Size(1000, 620), AutoScroll = true, BackColor = Color.Transparent };
            Controls.Add(eventsFlowPanel);
        }

        private void LoadUpcomingEvents()
        {
            eventsFlowPanel.Controls.Clear();
            var events = ServiceLocator.EventService.GetUpcomingEvents();
            if (events.Count == 0)
            {
                eventsFlowPanel.Controls.Add(EmptyState("No approved upcoming events yet. Ask an admin to approve society events, then they will appear here."));
                return;
            }

            var registeredEventIds = new HashSet<int>();
            if (_currentUser.StudentID.HasValue)
            {
                foreach (var ticket in ServiceLocator.EventService.GetStudentTickets(_currentUser.StudentID.Value))
                {
                    registeredEventIds.Add(ticket.EventID);
                }
            }

            foreach (var ev in events)
            {
                bool alreadyRegistered = registeredEventIds.Contains(ev.EventID);
                bool eventFull = ev.RegisteredCount >= ev.MaxCapacity;
                var card = new Guna2Panel { Size = new Size(330, 235), BorderRadius = 12, FillColor = Color.FromArgb(219, 234, 254), Margin = new Padding(10), ShadowDecoration = { Enabled = true, Depth = 10 } };
                card.Controls.Add(new Label { Text = ev.Title, Font = new Font("Segoe UI", 14, FontStyle.Bold), ForeColor = Color.FromArgb(31, 41, 55), Location = new Point(15, 15), Size = new Size(300, 28) });
                card.Controls.Add(new Label { Text = $"{ev.SocietyName} | {ev.VenueName}", Font = new Font("Segoe UI", 9), ForeColor = Color.DimGray, Location = new Point(15, 48), Size = new Size(300, 22) });
                card.Controls.Add(new Label { Text = $"Date: {ev.EventDate:dd MMM yyyy hh:mm tt}", Font = new Font("Segoe UI", 9), ForeColor = Color.Gray, Location = new Point(15, 73), Size = new Size(300, 22) });
                card.Controls.Add(new Label { Text = $"Seats: {ev.RegisteredCount}/{ev.MaxCapacity}", Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.FromArgb(37, 99, 235), Location = new Point(15, 98), Size = new Size(300, 22) });
                card.Controls.Add(new Label { Text = ev.Description, Font = new Font("Segoe UI", 9), ForeColor = Color.DarkGray, Location = new Point(15, 125), Size = new Size(300, 45) });

                var btnRegister = new Guna2Button
                {
                    Text = alreadyRegistered ? "Registered" : eventFull ? "Full" : "Register",
                    Size = new Size(120, 35),
                    Location = new Point(15, 185),
                    BorderRadius = 8,
                    FillColor = alreadyRegistered ? Color.FromArgb(37, 99, 235) : eventFull ? Color.FromArgb(100, 116, 139) : Color.FromArgb(16, 185, 129),
                    Font = new Font("Segoe UI", 9, FontStyle.Bold),
                    Cursor = alreadyRegistered || eventFull ? Cursors.Default : Cursors.Hand,
                    Enabled = !alreadyRegistered && !eventFull
                };
                btnRegister.Click += (s, e) =>
                {
                    var (success, msg) = ServiceLocator.EventService.RegisterStudentForEvent(ev.EventID, _currentUser.StudentID, _currentUser.UserID);
                    MessageBox.Show(msg, success ? "Registered" : "Registration Failed", MessageBoxButtons.OK, success ? MessageBoxIcon.Information : MessageBoxIcon.Error);
                    LoadUpcomingEvents();
                };
                card.Controls.Add(btnRegister);
                eventsFlowPanel.Controls.Add(card);
            }
        }

        private static Control EmptyState(string message)
        {
            return new Label
            {
                Text = message,
                Size = new Size(760, 80),
                Margin = new Padding(10),
                Font = new Font("Segoe UI Semibold", 11),
                ForeColor = EnterpriseTheme.MutedText
            };
        }
    }
}

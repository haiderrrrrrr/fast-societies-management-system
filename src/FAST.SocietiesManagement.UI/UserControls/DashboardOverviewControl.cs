using System;
using System.Drawing;
using System.Windows.Forms;
using Guna.UI2.WinForms;
using FAST.SocietiesManagement.Core.DTOs;
using FAST.SocietiesManagement.Core.Enums;
using FAST.SocietiesManagement.UI.Theme;

namespace FAST.SocietiesManagement.UI.UserControls
{
    public class DashboardOverviewControl : UserControl
    {
        public DashboardOverviewControl(UserDto currentUser)
        {
            Dock = DockStyle.Fill;
            BackColor = EnterpriseTheme.AppBackground;

            if (currentUser.Role == RoleType.Admin)
            {
                BuildAdminDashboard(currentUser);
                return;
            }

            BuildStandardDashboard(currentUser);
        }

        private void BuildAdminDashboard(UserDto currentUser)
        {
            var report = ServiceLocator.EnterpriseService.GetUniversityReport(currentUser.Role);
            var row = report.Count > 0 ? report[0] : new ReportRowDto();

            Controls.Add(new Label { Text = "Executive Dashboard", Font = new Font("Segoe UI", 24, FontStyle.Bold), Location = new Point(20, 14), AutoSize = true, ForeColor = EnterpriseTheme.Text });
            Controls.Add(new Label { Text = "University-wide command center for societies, approvals, participation, and engagement.", Font = EnterpriseTheme.SmallFont, ForeColor = EnterpriseTheme.MutedText, Location = new Point(24, 58), AutoSize = true });

            var hero = new Guna2GradientPanel
            {
                Location = new Point(20, 92),
                Size = new Size(990, 112),
                BorderRadius = 8,
                FillColor = Color.FromArgb(30, 64, 175),
                FillColor2 = Color.FromArgb(6, 95, 70),
                GradientMode = System.Drawing.Drawing2D.LinearGradientMode.ForwardDiagonal
            };
            hero.Controls.Add(new Label { Text = "FAST Societies Operations", Font = new Font("Segoe UI Semibold", 15, FontStyle.Bold), ForeColor = Color.White, BackColor = Color.Transparent, Location = new Point(22, 18), Size = new Size(520, 28) });
            hero.Controls.Add(new Label { Text = "Monitor societies, event approvals, registrations, and feedback quality from one place.", Font = new Font("Segoe UI Semibold", 9, FontStyle.Bold), ForeColor = Color.White, BackColor = Color.Transparent, Location = new Point(24, 54), Size = new Size(560, 22) });
            hero.Controls.Add(BuildHeroMetric("Active Societies", row.TotalSocieties.ToString(), new Point(650, 18)));
            hero.Controls.Add(BuildHeroMetric("Pending Approvals", row.PendingEvents.ToString(), new Point(815, 18)));
            Controls.Add(hero);

            AddKpi("Students", row.TotalMembers.ToString(), "Approved society memberships", Color.FromArgb(16, 185, 129), Color.FromArgb(209, 250, 229), new Point(20, 230));
            AddKpi("Events", row.TotalEvents.ToString(), "Events in the system", Color.FromArgb(37, 99, 235), Color.FromArgb(219, 234, 254), new Point(270, 230));
            AddKpi("Approved", row.ApprovedEvents.ToString(), "Ready for registrations", Color.FromArgb(124, 58, 237), Color.FromArgb(237, 233, 254), new Point(520, 230));
            AddKpi("Registrations", row.Registrations.ToString(), "Total event passes issued", Color.FromArgb(234, 88, 12), Color.FromArgb(254, 215, 170), new Point(770, 230));

            AddWorkloadPanel(row, new Point(20, 374));
            AddFeedbackPanel(row, new Point(520, 374));

            Controls.Add(new Label { Text = "University Report", Font = EnterpriseTheme.SectionFont, Location = new Point(20, 560), AutoSize = true, ForeColor = EnterpriseTheme.Text });
            Controls.Add(new Label { Text = "Aggregated data from societies, memberships, events, approvals, registrations, and feedback.", Font = EnterpriseTheme.SmallFont, ForeColor = EnterpriseTheme.MutedText, Location = new Point(22, 586), AutoSize = true });

            var grid = new DataGridView { Location = new Point(20, 620), Size = new Size(990, 125), ReadOnly = true, AllowUserToAddRows = false, DataSource = report };
            Controls.Add(grid);
            StylePremiumReportGrid(grid);
        }

        private void BuildStandardDashboard(UserDto currentUser)
        {
            Controls.Add(new Label { Text = "Dashboard Overview", Font = EnterpriseTheme.TitleFont, Location = new Point(20, 18), AutoSize = true });
            Controls.Add(new Label { Text = "A live operational snapshot for societies, events, memberships, and approvals.", Font = EnterpriseTheme.SmallFont, ForeColor = EnterpriseTheme.MutedText, Location = new Point(24, 58), AutoSize = true });

            var grid = new DataGridView { Location = new Point(20, 255), Size = new Size(990, 330), ReadOnly = true, AllowUserToAddRows = false };
            object reportData;

            if (currentUser.Role == RoleType.SocietyHead)
            {
                var societies = ServiceLocator.SocietyService.GetManagedSocieties(currentUser.UserID);
                var report = societies.Count > 0 ? ServiceLocator.EnterpriseService.GetSocietyReport(societies[0].SocietyID, currentUser.Role) : new System.Collections.Generic.List<ReportRowDto>();
                reportData = report;
                if (report.Count > 0)
                {
                    var row = report[0];
                    AddKpi("Members", row.TotalMembers.ToString(), "Approved society members", Color.FromArgb(16, 185, 129), Color.FromArgb(209, 250, 229), new Point(20, 92));
                    AddKpi("Events", row.TotalEvents.ToString(), "Managed event records", Color.FromArgb(37, 99, 235), Color.FromArgb(219, 234, 254), new Point(270, 92));
                    AddKpi("Approved", row.ApprovedEvents.ToString(), "Approved events", Color.FromArgb(124, 58, 237), Color.FromArgb(237, 233, 254), new Point(520, 92));
                    AddKpi("Registrations", row.Registrations.ToString(), "Event participants", Color.FromArgb(234, 88, 12), Color.FromArgb(254, 215, 170), new Point(770, 92));
                }
            }
            else
            {
                var memberships = ServiceLocator.MembershipService.GetMembershipsByStudent(currentUser.StudentID);
                reportData = memberships;
                AddKpi("Applications", memberships.Count.ToString(), "Society membership records", Color.FromArgb(37, 99, 235), Color.FromArgb(219, 234, 254), new Point(20, 92));
                AddKpi("Tickets", ServiceLocator.EventService.GetStudentTickets(currentUser.StudentID ?? 0).Count.ToString(), "Issued event passes", Color.FromArgb(16, 185, 129), Color.FromArgb(209, 250, 229), new Point(270, 92));
                AddKpi("Profile", currentUser.RollNumber, "Registered roll number", Color.FromArgb(124, 58, 237), Color.FromArgb(237, 233, 254), new Point(520, 92));
                AddKpi("Role", "Student", "Current access level", Color.FromArgb(234, 88, 12), Color.FromArgb(254, 215, 170), new Point(770, 92));
            }

            grid.DataSource = reportData;
            Controls.Add(new Label { Text = "Detailed Records", Font = EnterpriseTheme.SectionFont, Location = new Point(20, 222), AutoSize = true });
            Controls.Add(grid);
            StylePremiumReportGrid(grid);
        }

        private Control BuildHeroMetric(string title, string value, Point location)
        {
            var panel = new Panel { Location = location, Size = new Size(155, 76), BackColor = Color.Transparent };
            panel.Controls.Add(new Label { Text = title, Font = new Font("Segoe UI Semibold", 9, FontStyle.Bold), ForeColor = Color.White, BackColor = Color.Transparent, Location = new Point(0, 5), Size = new Size(150, 18), TextAlign = ContentAlignment.MiddleLeft });
            panel.Controls.Add(new Label { Text = value, Font = new Font("Segoe UI", 26, FontStyle.Bold), ForeColor = Color.White, BackColor = Color.Transparent, Location = new Point(0, 25), Size = new Size(150, 44) });
            return panel;
        }

        private void AddWorkloadPanel(ReportRowDto row, Point location)
        {
            var panel = PanelCard(location, new Size(470, 145), Color.FromArgb(255, 251, 235));
            panel.Controls.Add(new Label { Text = "Approval Workload", Font = EnterpriseTheme.SectionFont, ForeColor = EnterpriseTheme.Text, Location = new Point(18, 14), AutoSize = true });
            panel.Controls.Add(new Label { Text = "Events waiting for administrative decision", Font = EnterpriseTheme.SmallFont, ForeColor = EnterpriseTheme.MutedText, Location = new Point(20, 42), AutoSize = true });

            int totalDecisionItems = Math.Max(row.PendingEvents + row.ApprovedEvents, 1);
            int approvedPercent = Math.Min(100, (int)Math.Round(row.ApprovedEvents * 100.0 / totalDecisionItems));
            var progress = new Guna2ProgressBar
            {
                Location = new Point(20, 78),
                Size = new Size(420, 16),
                BorderRadius = 4,
                FillColor = Color.FromArgb(226, 232, 240),
                ProgressColor = EnterpriseTheme.Success,
                ProgressColor2 = EnterpriseTheme.Success,
                Value = approvedPercent
            };
            panel.Controls.Add(progress);
            panel.Controls.Add(new Label { Text = $"{row.ApprovedEvents} approved / {row.PendingEvents} pending", Font = EnterpriseTheme.SmallFont, ForeColor = EnterpriseTheme.Text, Location = new Point(20, 104), AutoSize = true });
            panel.Controls.Add(new Label { Text = row.PendingEvents > 0 ? "Action needed" : "Clear", Font = new Font("Segoe UI Semibold", 9, FontStyle.Bold), ForeColor = row.PendingEvents > 0 ? EnterpriseTheme.Warning : EnterpriseTheme.Success, Location = new Point(352, 104), AutoSize = true });
            Controls.Add(panel);
        }

        private void AddFeedbackPanel(ReportRowDto row, Point location)
        {
            var panel = PanelCard(location, new Size(490, 145), Color.FromArgb(240, 253, 250));
            panel.Controls.Add(new Label { Text = "Engagement Quality", Font = EnterpriseTheme.SectionFont, ForeColor = EnterpriseTheme.Text, Location = new Point(18, 14), AutoSize = true });
            panel.Controls.Add(new Label { Text = "Average feedback rating and participant reach", Font = EnterpriseTheme.SmallFont, ForeColor = EnterpriseTheme.MutedText, Location = new Point(20, 42), AutoSize = true });
            panel.Controls.Add(new Label { Text = $"{row.AverageFeedback:0.00}", Font = new Font("Segoe UI", 28, FontStyle.Bold), ForeColor = EnterpriseTheme.Primary, Location = new Point(20, 70), Size = new Size(110, 50) });
            panel.Controls.Add(new Label { Text = "avg. rating", Font = EnterpriseTheme.SmallFont, ForeColor = EnterpriseTheme.MutedText, Location = new Point(27, 117), AutoSize = true });
            panel.Controls.Add(new Label { Text = row.Registrations.ToString(), Font = new Font("Segoe UI", 28, FontStyle.Bold), ForeColor = EnterpriseTheme.Success, Location = new Point(190, 70), Size = new Size(110, 50) });
            panel.Controls.Add(new Label { Text = "registrations", Font = EnterpriseTheme.SmallFont, ForeColor = EnterpriseTheme.MutedText, Location = new Point(196, 117), AutoSize = true });
            panel.Controls.Add(new Label { Text = row.TotalSocieties.ToString(), Font = new Font("Segoe UI", 28, FontStyle.Bold), ForeColor = EnterpriseTheme.Warning, Location = new Point(350, 70), Size = new Size(110, 50) });
            panel.Controls.Add(new Label { Text = "societies", Font = EnterpriseTheme.SmallFont, ForeColor = EnterpriseTheme.MutedText, Location = new Point(358, 117), AutoSize = true });
            Controls.Add(panel);
        }

        private Guna2Panel PanelCard(Point location, Size size, Color? fillColor = null)
        {
            var panel = new Guna2Panel
            {
                Location = location,
                Size = size,
                BorderRadius = 8,
                FillColor = fillColor ?? EnterpriseTheme.Surface
            };
            panel.ShadowDecoration.Enabled = true;
            panel.ShadowDecoration.Depth = 8;
            return panel;
        }

        private void AddKpi(string title, string value, string subtitle, Color accent, Color fill, Point location)
        {
            var card = PanelCard(location, new Size(220, 112), fill);
            card.Controls.Add(new Panel { Location = new Point(0, 0), Size = new Size(5, 112), BackColor = accent });
            card.Controls.Add(new Label { Text = title, Font = new Font("Segoe UI Semibold", 9, FontStyle.Bold), ForeColor = Color.FromArgb(51, 65, 85), Location = new Point(18, 14), AutoSize = true });
            card.Controls.Add(new Label { Text = value, Font = new Font("Segoe UI", 20, FontStyle.Bold), ForeColor = EnterpriseTheme.Text, Location = new Point(18, 36), Size = new Size(180, 34), AutoEllipsis = true });
            card.Controls.Add(new Label { Text = subtitle, Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.FromArgb(51, 65, 85), Location = new Point(18, 78), Size = new Size(185, 22), AutoEllipsis = true });
            Controls.Add(card);
        }

        private void StylePremiumReportGrid(DataGridView grid)
        {
            EnterpriseTheme.StyleGrid(grid);
            grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(15, 23, 42);
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            grid.DefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
            grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(219, 234, 254);
            grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(191, 219, 254);
            grid.GridColor = Color.FromArgb(203, 213, 225);
            grid.RowTemplate.Height = 38;
            grid.ColumnHeadersHeight = 42;
        }
    }
}

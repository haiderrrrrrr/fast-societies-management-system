using System;
using System.Drawing;
using System.Windows.Forms;
using Guna.UI2.WinForms;
using FAST.SocietiesManagement.Core.DTOs;
using FAST.SocietiesManagement.Core.Enums;
using FAST.SocietiesManagement.UI.Theme;

namespace FAST.SocietiesManagement.UI.UserControls
{
    public class StudentBrowseSocietiesControl : UserControl
    {
        private readonly UserDto _currentUser;
        private FlowLayoutPanel societiesFlowPanel;

        public StudentBrowseSocietiesControl(UserDto currentUser)
        {
            _currentUser = currentUser;
            InitializeComponent();
            LoadSocieties();
        }

        private void InitializeComponent()
        {
            Dock = DockStyle.Fill;
            BackColor = Color.Transparent;
            Controls.Add(new Label { Text = "Explore Societies", Font = new Font("Segoe UI", 22, FontStyle.Bold), ForeColor = Color.FromArgb(17, 24, 39), AutoSize = true, Location = new Point(20, 20) });
            societiesFlowPanel = new FlowLayoutPanel { Location = new Point(20, 80), Size = new Size(1000, 620), AutoScroll = true, BackColor = Color.Transparent };
            Controls.Add(societiesFlowPanel);
        }

        private void LoadSocieties()
        {
            societiesFlowPanel.Controls.Clear();
            var memberships = ServiceLocator.MembershipService.GetMembershipsByStudent(_currentUser.StudentID);
            var societies = ServiceLocator.SocietyService.GetActiveSocieties();
            if (societies.Count == 0)
            {
                societiesFlowPanel.Controls.Add(new Label
                {
                    Text = "No active societies are available yet.",
                    Size = new Size(700, 60),
                    Margin = new Padding(10),
                    Font = new Font("Segoe UI Semibold", 11),
                    ForeColor = EnterpriseTheme.MutedText
                });
                return;
            }

            foreach (var soc in societies)
            {
                var existing = memberships.Find(m => m.SocietyID == soc.SocietyID);
                var card = new Guna2Panel { Size = new Size(315, 230), BorderRadius = 12, FillColor = Color.FromArgb(209, 250, 229), Margin = new Padding(10), ShadowDecoration = { Enabled = true, Depth = 10 } };
                card.Controls.Add(new Label { Text = soc.Name, Font = new Font("Segoe UI", 15, FontStyle.Bold), ForeColor = Color.FromArgb(37, 99, 235), Location = new Point(15, 15), Size = new Size(280, 30) });
                card.Controls.Add(new Label { Text = $"Head: {(string.IsNullOrWhiteSpace(soc.HeadName) ? "Not assigned" : soc.HeadName)}", Font = new Font("Segoe UI", 9), ForeColor = Color.DimGray, Location = new Point(15, 48), Size = new Size(280, 22) });
                card.Controls.Add(new Label { Text = soc.Description, Font = new Font("Segoe UI", 9), ForeColor = Color.DarkGray, Location = new Point(15, 78), Size = new Size(280, 70) });

                var btnJoin = new Guna2Button { Size = new Size(165, 36), Location = new Point(15, 170), BorderRadius = 8, Font = new Font("Segoe UI", 9, FontStyle.Bold), Cursor = Cursors.Hand };
                if (existing != null)
                {
                    btnJoin.Text = existing.Status.ToString();
                    btnJoin.Enabled = false;
                    btnJoin.FillColor = existing.Status == MembershipStatusEnum.Approved ? Color.FromArgb(16, 185, 129) : Color.Gray;
                }
                else
                {
                    btnJoin.Text = "Request to Join";
                    btnJoin.FillColor = Color.FromArgb(31, 41, 55);
                    btnJoin.Click += (s, e) =>
                    {
                        var (success, message) = ServiceLocator.MembershipService.ApplyForMembership(_currentUser.StudentID, soc.SocietyID, _currentUser.UserID);
                        MessageBox.Show(message, success ? "Request Submitted" : "Request Failed", MessageBoxButtons.OK, success ? MessageBoxIcon.Information : MessageBoxIcon.Error);
                        LoadSocieties();
                    };
                }

                card.Controls.Add(btnJoin);
                societiesFlowPanel.Controls.Add(card);
            }
        }
    }
}

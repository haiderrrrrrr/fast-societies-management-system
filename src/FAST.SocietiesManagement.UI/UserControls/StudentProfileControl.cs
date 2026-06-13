using System.Drawing;
using System.Windows.Forms;
using FAST.SocietiesManagement.Core.DTOs;

namespace FAST.SocietiesManagement.UI.UserControls
{
    public class StudentProfileControl : UserControl
    {
        private readonly UserDto _currentUser;
        private DataGridView gridMemberships;
        private DataGridView gridTickets;
        private DataGridView gridTasks;

        public StudentProfileControl(UserDto currentUser)
        {
            _currentUser = currentUser;
            InitializeComponent();
            LoadData();
        }

        private void InitializeComponent()
        {
            Dock = DockStyle.Fill;
            BackColor = Color.FromArgb(248, 249, 250);
            Controls.Add(new Label { Text = "My Profile", Font = new Font("Segoe UI", 22, FontStyle.Bold), ForeColor = Color.FromArgb(17, 24, 39), AutoSize = true, Location = new Point(20, 20) });
            Controls.Add(new Label { Text = $"{_currentUser.FullName} | {_currentUser.RollNumber} | {_currentUser.Department} | {_currentUser.Email}", Font = new Font("Segoe UI", 10), ForeColor = Color.DimGray, AutoSize = true, Location = new Point(25, 62) });

            gridMemberships = Grid(new Point(20, 115), new Size(980, 150));
            gridTickets = Grid(new Point(20, 315), new Size(980, 150));
            gridTasks = Grid(new Point(20, 515), new Size(980, 150));

            Controls.Add(new Label { Text = "Membership Status", Font = new Font("Segoe UI Semibold", 12), AutoSize = true, Location = new Point(20, 90) });
            Controls.Add(gridMemberships);
            Controls.Add(new Label { Text = "Event Tickets", Font = new Font("Segoe UI Semibold", 12), AutoSize = true, Location = new Point(20, 290) });
            Controls.Add(gridTickets);
            Controls.Add(new Label { Text = "Assigned Tasks", Font = new Font("Segoe UI Semibold", 12), AutoSize = true, Location = new Point(20, 490) });
            Controls.Add(gridTasks);
        }

        private static DataGridView Grid(Point location, Size size)
        {
            return new DataGridView
            {
                Location = location,
                Size = size,
                BackgroundColor = Color.White,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };
        }

        private void LoadData()
        {
            gridMemberships.DataSource = ServiceLocator.MembershipService.GetMembershipsByStudent(_currentUser.StudentID);
            gridTickets.DataSource = _currentUser.StudentID.HasValue ? ServiceLocator.EventService.GetStudentTickets(_currentUser.StudentID.Value) : null;
            gridTasks.DataSource = ServiceLocator.TaskService.GetStudentTasks(_currentUser.StudentID);
        }
    }
}

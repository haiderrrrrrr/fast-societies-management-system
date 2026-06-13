using System;
using System.Drawing;
using System.Windows.Forms;
using Guna.UI2.WinForms;
using FAST.SocietiesManagement.Core.DTOs;
using FAST.SocietiesManagement.Core.Enums;

namespace FAST.SocietiesManagement.UI.UserControls
{
    public class SocietyHeadTaskControl : UserControl
    {
        private readonly UserDto _currentUser;
        private ComboBox cmbSociety;
        private ComboBox cmbMember;
        private Guna2TextBox txtTitle;
        private Guna2TextBox txtDescription;
        private Guna2DateTimePicker dtpDate;
        private DataGridView gridTasks;
        private Label lblMessage;
        private Guna2Button btnAssign;

        public SocietyHeadTaskControl(UserDto currentUser)
        {
            _currentUser = currentUser;
            InitializeComponent();
            LoadSocieties();
        }

        private void InitializeComponent()
        {
            Dock = DockStyle.Fill;
            BackColor = Color.Transparent;
            Controls.Add(new Label { Text = "Assign Tasks", Font = new Font("Segoe UI", 22, FontStyle.Bold), ForeColor = Color.FromArgb(17, 24, 39), AutoSize = true, Location = new Point(20, 20) });

            var form = new Guna2Panel { Location = new Point(20, 80), Size = new Size(400, 500), FillColor = Color.FromArgb(237, 233, 254), BorderRadius = 12, ShadowDecoration = { Enabled = true, Depth = 10 } };
            cmbSociety = new ComboBox { Location = new Point(20, 45), Size = new Size(360, 25), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbSociety.SelectedIndexChanged += (s, e) => { LoadMembers(); LoadTasks(); };
            cmbMember = new ComboBox { Location = new Point(20, 100), Size = new Size(360, 25), DropDownStyle = ComboBoxStyle.DropDownList };
            txtTitle = new Guna2TextBox { Location = new Point(20, 155), Size = new Size(360, 40), BorderRadius = 8, PlaceholderText = "Task title" };
            txtDescription = new Guna2TextBox { Location = new Point(20, 220), Size = new Size(360, 80), BorderRadius = 8, Multiline = true, PlaceholderText = "Task details" };
            dtpDate = new Guna2DateTimePicker { Location = new Point(20, 335), Size = new Size(360, 40), BorderRadius = 8, Value = DateTime.Now.AddDays(3) };
            btnAssign = new Guna2Button { Text = "Assign Task", Location = new Point(20, 410), Size = new Size(360, 40), BorderRadius = 8, FillColor = Color.FromArgb(37, 99, 235), Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            btnAssign.Click += (s, e) => ProcessAssign();
            lblMessage = new Label { Text = "", Location = new Point(20, 460), AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
            form.Controls.AddRange(new Control[]
            {
                new Label { Text = "Society", Location = new Point(20, 20), AutoSize = true }, cmbSociety,
                new Label { Text = "Approved Member", Location = new Point(20, 75), AutoSize = true }, cmbMember,
                txtTitle, txtDescription,
                new Label { Text = "Due Date", Location = new Point(20, 310), AutoSize = true }, dtpDate,
                btnAssign, lblMessage
            });

            gridTasks = new DataGridView { Location = new Point(450, 80), Size = new Size(570, 500), BackgroundColor = Color.White, ReadOnly = true, AllowUserToAddRows = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };
            Controls.AddRange(new Control[] { form, gridTasks });
        }

        private void LoadSocieties()
        {
            cmbSociety.DataSource = _currentUser.Role == RoleType.Admin
                ? ServiceLocator.SocietyService.GetAllSocieties(_currentUser.Role)
                : ServiceLocator.SocietyService.GetManagedSocieties(_currentUser.UserID);
            cmbSociety.DisplayMember = "Name";
            cmbSociety.ValueMember = "SocietyID";
            LoadMembers();
            LoadTasks();
        }

        private int? SocietyId() => cmbSociety.SelectedValue is int id ? id : null;

        private void LoadMembers()
        {
            var societyId = SocietyId();
            var members = societyId.HasValue ? ServiceLocator.MembershipService.GetApprovedMembers(societyId.Value, _currentUser.Role) : new System.Collections.Generic.List<MembershipDto>();
            cmbMember.DataSource = members;
            cmbMember.DisplayMember = "StudentName";
            cmbMember.ValueMember = "StudentID";
            btnAssign.Enabled = members.Count > 0;
            if (members.Count == 0) Show(false, "No approved members yet. Approve a membership request before assigning tasks.");
            else lblMessage.Text = "";
        }

        private void LoadTasks()
        {
            var societyId = SocietyId();
            gridTasks.DataSource = societyId.HasValue ? ServiceLocator.TaskService.GetSocietyTasks(societyId.Value) : null;
        }

        private void ProcessAssign()
        {
            var societyId = SocietyId();
            if (!societyId.HasValue || cmbMember.SelectedValue is not int studentId) { Show(false, "Select a society and approved member."); return; }
            var task = new TaskDto { SocietyID = societyId.Value, AssignedToStudentID = studentId, Title = txtTitle.Text, Description = txtDescription.Text, DueDate = dtpDate.Value };
            var (success, msg) = ServiceLocator.TaskService.AssignTask(task, _currentUser.UserID, _currentUser.Role);
            Show(success, msg);
            if (success) { txtTitle.Clear(); txtDescription.Clear(); LoadTasks(); }
        }

        private void Show(bool success, string msg)
        {
            lblMessage.ForeColor = success ? Color.Green : Color.Red;
            lblMessage.Text = msg;
        }
    }
}

using System;
using FAST.SocietiesManagement.Core.Enums;

namespace FAST.SocietiesManagement.Core.DTOs
{
    public class MembershipDto
    {
        public int MembershipID { get; set; }
        public int StudentID { get; set; }
        public string StudentName { get; set; } // Flattened for UI
        public int SocietyID { get; set; }
        public string SocietyName { get; set; }
        public MembershipStatusEnum Status { get; set; }
        public string Role { get; set; }
        public DateTime RequestDate { get; set; }
    }

    public class TaskDto
    {
        public int TaskID { get; set; }
        public int SocietyID { get; set; }
        public int AssignedToStudentID { get; set; }
        public string AssignedToName { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public TaskStatusEnum Status { get; set; }
        public DateTime DueDate { get; set; }
    }

    public class EventTicketDto
    {
        public int RegistrationID { get; set; }
        public int EventID { get; set; }
        public int StudentID { get; set; }
        public string StudentName { get; set; }
        public string EventTitle { get; set; }
        public string SocietyName { get; set; }
        public string VenueName { get; set; }
        public DateTime EventDate { get; set; }
        public Guid TicketNumber { get; set; }
        public DateTime RegistrationDate { get; set; }
    }
}

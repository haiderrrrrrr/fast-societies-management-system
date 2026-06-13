using System;
using FAST.SocietiesManagement.Core.Enums;

namespace FAST.SocietiesManagement.Core.DTOs
{
    public class SocietyDto
    {
        public int SocietyID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int? HeadUserID { get; set; }
        public string HeadName { get; set; }
        public SocietyStatusEnum Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class EventDto
    {
        public int EventID { get; set; }
        public int SocietyID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int VenueID { get; set; }
        public string VenueName { get; set; }
        public DateTime EventDate { get; set; }
        public int MaxCapacity { get; set; }
        public EventStatusEnum Status { get; set; }
        public string SocietyName { get; set; }
        public int RegisteredCount { get; set; }
    }
    
    public class StudentProfileDto
    {
        public int StudentID { get; set; }
        public int UserID { get; set; }
        public string FullName { get; set; }
        public string RollNumber { get; set; }
        public string Department { get; set; }
        public string Email { get; set; }
    }

    public class VenueDto
    {
        public int VenueID { get; set; }
        public string Name { get; set; }
        public int Capacity { get; set; }
    }

    public class AnnouncementDto
    {
        public int AnnouncementID { get; set; }
        public int SocietyID { get; set; }
        public string SocietyName { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public DateTime PublishedAt { get; set; }
        public int CreatedBy { get; set; }
    }

    public class FeedbackDto
    {
        public int FeedbackID { get; set; }
        public int EventID { get; set; }
        public string EventTitle { get; set; }
        public int StudentID { get; set; }
        public string StudentName { get; set; }
        public int Rating { get; set; }
        public string Comments { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class AttendanceDto
    {
        public int AttendanceID { get; set; }
        public int EventID { get; set; }
        public string EventTitle { get; set; }
        public int StudentID { get; set; }
        public string StudentName { get; set; }
        public bool IsPresent { get; set; }
        public DateTime MarkedAt { get; set; }
    }

    public class ReportRowDto
    {
        public string Category { get; set; }
        public int TotalSocieties { get; set; }
        public int TotalMembers { get; set; }
        public int TotalEvents { get; set; }
        public int ApprovedEvents { get; set; }
        public int PendingEvents { get; set; }
        public int Registrations { get; set; }
        public decimal AverageFeedback { get; set; }
    }
}

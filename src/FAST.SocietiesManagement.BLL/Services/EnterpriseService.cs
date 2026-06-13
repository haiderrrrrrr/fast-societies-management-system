using System;
using System.Collections.Generic;
using FAST.SocietiesManagement.Core.DTOs;
using FAST.SocietiesManagement.Core.Enums;
using FAST.SocietiesManagement.Core.Utilities;
using FAST.SocietiesManagement.DAL.Interfaces;

namespace FAST.SocietiesManagement.BLL.Services
{
    public class EnterpriseService
    {
        private readonly IEnterpriseRepository _repository;
        private readonly IMembershipRepository _membershipRepository;

        public EnterpriseService(IEnterpriseRepository repository, IMembershipRepository membershipRepository)
        {
            _repository = repository;
            _membershipRepository = membershipRepository;
        }

        public (bool Success, string Message) PublishAnnouncement(AnnouncementDto announcement, RoleType role)
        {
            if (role != RoleType.Admin && role != RoleType.SocietyHead)
                return (false, "Only admins and society heads can publish announcements.");
            if (announcement.SocietyID <= 0) return (false, "Select a society.");
            if (string.IsNullOrWhiteSpace(announcement.Title)) return (false, "Announcement title is required.");
            if (string.IsNullOrWhiteSpace(announcement.Message)) return (false, "Announcement message is required.");

            try
            {
                _repository.CreateAnnouncement(announcement);
                return (true, "Announcement published.");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "EnterpriseService.PublishAnnouncement", announcement.CreatedBy);
                return (false, "Failed to publish announcement.");
            }
        }

        public List<AnnouncementDto> GetStudentAnnouncements(int? studentId)
        {
            return studentId.HasValue ? _repository.GetAnnouncementsForStudent(studentId.Value) : new List<AnnouncementDto>();
        }

        public List<AnnouncementDto> GetSocietyAnnouncements(int societyId) => _repository.GetAnnouncementsBySociety(societyId);

        public List<AnnouncementDto> GetAllAnnouncements(RoleType role)
        {
            return role == RoleType.Admin ? _repository.GetAllAnnouncements() : new List<AnnouncementDto>();
        }

        public List<EventTicketDto> GetEventRegistrants(int eventId, RoleType role)
        {
            return role == RoleType.Admin || role == RoleType.SocietyHead ? _repository.GetEventRegistrants(eventId) : new List<EventTicketDto>();
        }

        public (bool Success, string Message) MarkAttendance(int eventId, int studentId, bool isPresent, int markedBy, RoleType role)
        {
            if (role != RoleType.Admin && role != RoleType.SocietyHead) return (false, "Unauthorized to mark attendance.");
            try
            {
                _repository.MarkAttendance(eventId, studentId, isPresent, markedBy);
                return (true, isPresent ? "Attendance marked present." : "Attendance marked absent.");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "EnterpriseService.MarkAttendance", markedBy);
                return (false, "Failed to mark attendance.");
            }
        }

        public List<AttendanceDto> GetAttendanceByEvent(int eventId, RoleType role)
        {
            return role == RoleType.Admin || role == RoleType.SocietyHead ? _repository.GetAttendanceByEvent(eventId) : new List<AttendanceDto>();
        }

        public (bool Success, string Message) SubmitFeedback(int eventId, int? studentId, int rating, string comments)
        {
            if (!studentId.HasValue) return (false, "Student profile is missing.");
            if (rating < 1 || rating > 5) return (false, "Rating must be between 1 and 5.");
            if (string.IsNullOrWhiteSpace(comments)) return (false, "Feedback comments are required.");

            try
            {
                _repository.SubmitFeedback(new FeedbackDto { EventID = eventId, StudentID = studentId.Value, Rating = rating, Comments = comments.Trim() });
                return (true, "Feedback submitted.");
            }
            catch (Microsoft.Data.SqlClient.SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
            {
                return (false, "You already submitted feedback for this event.");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "EnterpriseService.SubmitFeedback");
                return (false, "Failed to submit feedback.");
            }
        }

        public List<FeedbackDto> GetFeedbackByEvent(int eventId, RoleType role)
        {
            return role == RoleType.Admin || role == RoleType.SocietyHead ? _repository.GetFeedbackByEvent(eventId) : new List<FeedbackDto>();
        }

        public List<ReportRowDto> GetUniversityReport(RoleType role)
        {
            return role == RoleType.Admin ? _repository.GetUniversityReport() : new List<ReportRowDto>();
        }

        public List<ReportRowDto> GetSocietyReport(int societyId, RoleType role)
        {
            return role == RoleType.Admin || role == RoleType.SocietyHead ? _repository.GetSocietyReport(societyId) : new List<ReportRowDto>();
        }
    }
}

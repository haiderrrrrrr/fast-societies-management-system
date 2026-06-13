using System.Collections.Generic;
using FAST.SocietiesManagement.Core.DTOs;

namespace FAST.SocietiesManagement.DAL.Interfaces
{
    public interface IMembershipRepository
    {
        void RequestMembership(int studentId, int societyId, int createdBy);
        List<MembershipDto> GetPendingRequests(int societyId);
        List<MembershipDto> GetMembershipsByStudent(int studentId);
        List<MembershipDto> GetApprovedMembers(int societyId);
        bool IsApprovedMember(int studentId, int societyId);
        void UpdateMembershipStatus(int membershipId, int statusId, int updatedBy);
    }

    public interface ITaskRepository
    {
        void CreateTask(TaskDto task, int createdBy);
        List<TaskDto> GetTasksBySociety(int societyId);
        List<TaskDto> GetTasksByStudent(int studentId);
    }

    public interface IEnterpriseRepository
    {
        void CreateAnnouncement(AnnouncementDto announcement);
        List<AnnouncementDto> GetAnnouncementsForStudent(int studentId);
        List<AnnouncementDto> GetAnnouncementsBySociety(int societyId);
        List<AnnouncementDto> GetAllAnnouncements();

        List<EventTicketDto> GetEventRegistrants(int eventId);
        void MarkAttendance(int eventId, int studentId, bool isPresent, int markedBy);
        List<AttendanceDto> GetAttendanceByEvent(int eventId);

        void SubmitFeedback(FeedbackDto feedback);
        List<FeedbackDto> GetFeedbackByEvent(int eventId);

        List<ReportRowDto> GetUniversityReport();
        List<ReportRowDto> GetSocietyReport(int societyId);
    }
}

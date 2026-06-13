using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using FAST.SocietiesManagement.Core.DTOs;
using FAST.SocietiesManagement.DAL.Interfaces;

namespace FAST.SocietiesManagement.DAL.Repositories
{
    public class EnterpriseRepository : IEnterpriseRepository
    {
        public void CreateAnnouncement(AnnouncementDto announcement)
        {
            using var connection = DbConnectionProvider.GetConnection();
            var cmd = new SqlCommand(@"
                INSERT INTO Announcements (SocietyID, Title, Message, CreatedBy)
                VALUES (@SocietyID, @Title, @Message, @CreatedBy)", connection);
            cmd.Parameters.AddWithValue("@SocietyID", announcement.SocietyID);
            cmd.Parameters.AddWithValue("@Title", announcement.Title);
            cmd.Parameters.AddWithValue("@Message", announcement.Message);
            cmd.Parameters.AddWithValue("@CreatedBy", announcement.CreatedBy);
            connection.Open();
            cmd.ExecuteNonQuery();
        }

        public List<AnnouncementDto> GetAnnouncementsForStudent(int studentId)
        {
            return QueryAnnouncements(@"
                INNER JOIN SocietyStatus ss ON ss.StatusID = s.StatusID
                WHERE a.IsDeleted = 0 AND s.IsDeleted = 0 AND ss.StatusName = 'Active'", null);
        }

        public List<AnnouncementDto> GetAnnouncementsBySociety(int societyId)
        {
            return QueryAnnouncements("WHERE a.SocietyID = @SocietyID AND a.IsDeleted = 0", cmd => cmd.Parameters.AddWithValue("@SocietyID", societyId));
        }

        public List<AnnouncementDto> GetAllAnnouncements()
        {
            return QueryAnnouncements("WHERE a.IsDeleted = 0", null);
        }

        public List<EventTicketDto> GetEventRegistrants(int eventId)
        {
            var list = new List<EventTicketDto>();
            using var connection = DbConnectionProvider.GetConnection();
            var cmd = new SqlCommand(@"
                SELECT er.RegistrationID, e.EventID, er.StudentID, sp.FullName, e.Title, so.Name, v.Name, e.EventDate, er.TicketNumber, er.RegistrationDate
                FROM EventRegistrations er
                INNER JOIN Events e ON e.EventID = er.EventID
                INNER JOIN Societies so ON so.SocietyID = e.SocietyID
                INNER JOIN Venues v ON v.VenueID = e.VenueID
                INNER JOIN StudentProfiles sp ON sp.StudentID = er.StudentID
                WHERE er.EventID = @EventID AND er.IsDeleted = 0
                ORDER BY er.RegistrationDate", connection);
            cmd.Parameters.AddWithValue("@EventID", eventId);
            connection.Open();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new EventTicketDto
                {
                    RegistrationID = reader.GetInt32(0),
                    EventID = reader.GetInt32(1),
                    StudentID = reader.GetInt32(2),
                    StudentName = reader.GetString(3),
                    EventTitle = reader.GetString(4),
                    SocietyName = reader.GetString(5),
                    VenueName = reader.GetString(6),
                    EventDate = reader.GetDateTime(7),
                    TicketNumber = reader.GetGuid(8),
                    RegistrationDate = reader.GetDateTime(9)
                });
            }
            return list;
        }

        public void MarkAttendance(int eventId, int studentId, bool isPresent, int markedBy)
        {
            using var connection = DbConnectionProvider.GetConnection();
            var cmd = new SqlCommand(@"
                IF EXISTS (SELECT 1 FROM EventAttendance WHERE EventID = @EventID AND StudentID = @StudentID)
                    UPDATE EventAttendance
                    SET IsPresent = @IsPresent, MarkedAt = GETDATE(), MarkedBy = @MarkedBy
                    WHERE EventID = @EventID AND StudentID = @StudentID
                ELSE
                    INSERT INTO EventAttendance (EventID, StudentID, IsPresent, MarkedBy)
                    VALUES (@EventID, @StudentID, @IsPresent, @MarkedBy)", connection);
            cmd.Parameters.AddWithValue("@EventID", eventId);
            cmd.Parameters.AddWithValue("@StudentID", studentId);
            cmd.Parameters.AddWithValue("@IsPresent", isPresent);
            cmd.Parameters.AddWithValue("@MarkedBy", markedBy);
            connection.Open();
            cmd.ExecuteNonQuery();
        }

        public List<AttendanceDto> GetAttendanceByEvent(int eventId)
        {
            var list = new List<AttendanceDto>();
            using var connection = DbConnectionProvider.GetConnection();
            var cmd = new SqlCommand(@"
                SELECT ea.AttendanceID, ea.EventID, e.Title, ea.StudentID, sp.FullName, ea.IsPresent, ea.MarkedAt
                FROM EventAttendance ea
                INNER JOIN Events e ON e.EventID = ea.EventID
                INNER JOIN StudentProfiles sp ON sp.StudentID = ea.StudentID
                WHERE ea.EventID = @EventID
                ORDER BY sp.FullName", connection);
            cmd.Parameters.AddWithValue("@EventID", eventId);
            connection.Open();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new AttendanceDto
                {
                    AttendanceID = reader.GetInt32(0),
                    EventID = reader.GetInt32(1),
                    EventTitle = reader.GetString(2),
                    StudentID = reader.GetInt32(3),
                    StudentName = reader.GetString(4),
                    IsPresent = reader.GetBoolean(5),
                    MarkedAt = reader.GetDateTime(6)
                });
            }
            return list;
        }

        public void SubmitFeedback(FeedbackDto feedback)
        {
            using var connection = DbConnectionProvider.GetConnection();
            var cmd = new SqlCommand(@"
                INSERT INTO EventFeedback (EventID, StudentID, Rating, Comments)
                VALUES (@EventID, @StudentID, @Rating, @Comments)", connection);
            cmd.Parameters.AddWithValue("@EventID", feedback.EventID);
            cmd.Parameters.AddWithValue("@StudentID", feedback.StudentID);
            cmd.Parameters.AddWithValue("@Rating", feedback.Rating);
            cmd.Parameters.AddWithValue("@Comments", feedback.Comments);
            connection.Open();
            cmd.ExecuteNonQuery();
        }

        public List<FeedbackDto> GetFeedbackByEvent(int eventId)
        {
            var list = new List<FeedbackDto>();
            using var connection = DbConnectionProvider.GetConnection();
            var cmd = new SqlCommand(@"
                SELECT f.FeedbackID, f.EventID, e.Title, f.StudentID, sp.FullName, f.Rating, f.Comments, f.CreatedAt
                FROM EventFeedback f
                INNER JOIN Events e ON e.EventID = f.EventID
                INNER JOIN StudentProfiles sp ON sp.StudentID = f.StudentID
                WHERE f.EventID = @EventID
                ORDER BY f.CreatedAt DESC", connection);
            cmd.Parameters.AddWithValue("@EventID", eventId);
            connection.Open();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new FeedbackDto
                {
                    FeedbackID = reader.GetInt32(0),
                    EventID = reader.GetInt32(1),
                    EventTitle = reader.GetString(2),
                    StudentID = reader.GetInt32(3),
                    StudentName = reader.GetString(4),
                    Rating = reader.GetInt32(5),
                    Comments = reader.GetString(6),
                    CreatedAt = reader.GetDateTime(7)
                });
            }
            return list;
        }

        public List<ReportRowDto> GetUniversityReport()
        {
            return QueryReports(null);
        }

        public List<ReportRowDto> GetSocietyReport(int societyId)
        {
            return QueryReports(societyId);
        }

        private static List<AnnouncementDto> QueryAnnouncements(string clause, Action<SqlCommand> addParameters)
        {
            var list = new List<AnnouncementDto>();
            using var connection = DbConnectionProvider.GetConnection();
            var cmd = new SqlCommand($@"
                SELECT a.AnnouncementID, a.SocietyID, s.Name, a.Title, a.Message, a.PublishedAt, a.CreatedBy
                FROM Announcements a
                INNER JOIN Societies s ON s.SocietyID = a.SocietyID
                {clause}
                ORDER BY a.PublishedAt DESC", connection);
            addParameters?.Invoke(cmd);
            connection.Open();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new AnnouncementDto
                {
                    AnnouncementID = reader.GetInt32(0),
                    SocietyID = reader.GetInt32(1),
                    SocietyName = reader.GetString(2),
                    Title = reader.GetString(3),
                    Message = reader.GetString(4),
                    PublishedAt = reader.GetDateTime(5),
                    CreatedBy = reader.GetInt32(6)
                });
            }
            return list;
        }

        private static List<ReportRowDto> QueryReports(int? societyId)
        {
            var list = new List<ReportRowDto>();
            using var connection = DbConnectionProvider.GetConnection();
            var cmd = new SqlCommand(@"
                SELECT
                    CASE WHEN @SocietyID IS NULL THEN 'University Wide' ELSE s.Name END AS Category,
                    COUNT(DISTINCT s.SocietyID) AS TotalSocieties,
                    COUNT(DISTINCT CASE WHEN m.StatusID = 2 THEN m.MembershipID END) AS TotalMembers,
                    COUNT(DISTINCT e.EventID) AS TotalEvents,
                    COUNT(DISTINCT CASE WHEN e.StatusID = 3 THEN e.EventID END) AS ApprovedEvents,
                    COUNT(DISTINCT CASE WHEN e.StatusID = 2 THEN e.EventID END) AS PendingEvents,
                    COUNT(DISTINCT er.RegistrationID) AS Registrations,
                    CAST(ISNULL(AVG(CAST(f.Rating AS DECIMAL(10,2))), 0) AS DECIMAL(10,2)) AS AverageFeedback
                FROM Societies s
                LEFT JOIN Memberships m ON m.SocietyID = s.SocietyID AND m.IsDeleted = 0
                LEFT JOIN Events e ON e.SocietyID = s.SocietyID AND e.IsDeleted = 0
                LEFT JOIN EventRegistrations er ON er.EventID = e.EventID AND er.IsDeleted = 0
                LEFT JOIN EventFeedback f ON f.EventID = e.EventID
                WHERE s.IsDeleted = 0 AND (@SocietyID IS NULL OR s.SocietyID = @SocietyID)
                GROUP BY CASE WHEN @SocietyID IS NULL THEN 'University Wide' ELSE s.Name END", connection);
            cmd.Parameters.AddWithValue("@SocietyID", societyId.HasValue ? societyId.Value : DBNull.Value);
            connection.Open();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new ReportRowDto
                {
                    Category = reader.GetString(0),
                    TotalSocieties = reader.GetInt32(1),
                    TotalMembers = reader.GetInt32(2),
                    TotalEvents = reader.GetInt32(3),
                    ApprovedEvents = reader.GetInt32(4),
                    PendingEvents = reader.GetInt32(5),
                    Registrations = reader.GetInt32(6),
                    AverageFeedback = reader.GetDecimal(7)
                });
            }
            return list;
        }
    }
}

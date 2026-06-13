using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using FAST.SocietiesManagement.Core.DTOs;
using FAST.SocietiesManagement.Core.Enums;
using FAST.SocietiesManagement.DAL.Interfaces;

namespace FAST.SocietiesManagement.DAL.Repositories
{
    public class EventRepository : IEventRepository
    {
        public List<EventDto> GetUpcomingEvents()
        {
            return QueryEvents("WHERE e.IsDeleted = 0 AND e.EventDate >= GETDATE() AND e.StatusID = @StatusID", cmd =>
                cmd.Parameters.AddWithValue("@StatusID", (int)EventStatusEnum.Approved));
        }

        public List<EventDto> GetAllEvents()
        {
            return QueryEvents("WHERE e.IsDeleted = 0", null);
        }

        public List<EventDto> GetEventsBySociety(int societyId)
        {
            return QueryEvents("WHERE e.IsDeleted = 0 AND e.SocietyID = @SocietyID", cmd =>
                cmd.Parameters.AddWithValue("@SocietyID", societyId));
        }

        public List<VenueDto> GetVenues()
        {
            var venues = new List<VenueDto>();
            using var connection = DbConnectionProvider.GetConnection();
            var cmd = new SqlCommand("SELECT VenueID, Name, Capacity FROM Venues WHERE IsDeleted = 0 ORDER BY Name", connection);
            connection.Open();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                venues.Add(new VenueDto
                {
                    VenueID = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Capacity = reader.GetInt32(2)
                });
            }
            return venues;
        }

        public void CreateEvent(EventDto eventObj, int createdBy)
        {
            using var connection = DbConnectionProvider.GetConnection();
            var cmd = new SqlCommand(@"
                INSERT INTO Events (SocietyID, Title, Description, VenueID, EventDate, MaxCapacity, StatusID, CreatedBy)
                VALUES (@SocietyID, @Title, @Desc, @VenueID, @EventDate, @MaxCapacity, @StatusID, @CreatedBy)", connection);
            AddEventParameters(cmd, eventObj);
            cmd.Parameters.AddWithValue("@StatusID", (int)eventObj.Status);
            cmd.Parameters.AddWithValue("@CreatedBy", createdBy);

            connection.Open();
            cmd.ExecuteNonQuery();
        }

        public void UpdateEvent(EventDto eventObj, int updatedBy)
        {
            using var connection = DbConnectionProvider.GetConnection();
            var cmd = new SqlCommand(@"
                UPDATE Events
                SET Title = @Title,
                    Description = @Desc,
                    VenueID = @VenueID,
                    EventDate = @EventDate,
                    MaxCapacity = @MaxCapacity,
                    UpdatedBy = @UpdatedBy,
                    UpdatedAt = GETDATE()
                WHERE EventID = @EventID AND IsDeleted = 0", connection);
            AddEventParameters(cmd, eventObj);
            cmd.Parameters.AddWithValue("@EventID", eventObj.EventID);
            cmd.Parameters.AddWithValue("@UpdatedBy", updatedBy);

            connection.Open();
            cmd.ExecuteNonQuery();
        }

        public void UpdateEventStatus(int eventId, int statusId, int updatedBy)
        {
            using var connection = DbConnectionProvider.GetConnection();
            var cmd = new SqlCommand(@"
                UPDATE Events
                SET StatusID = @StatusID, UpdatedBy = @UpdatedBy, UpdatedAt = GETDATE()
                WHERE EventID = @EventID AND IsDeleted = 0", connection);
            cmd.Parameters.AddWithValue("@StatusID", statusId);
            cmd.Parameters.AddWithValue("@UpdatedBy", updatedBy);
            cmd.Parameters.AddWithValue("@EventID", eventId);

            connection.Open();
            cmd.ExecuteNonQuery();
        }

        public bool RegisterForEvent(int eventId, int studentId, int createdBy, out string errorMessage)
        {
            errorMessage = string.Empty;
            using var connection = DbConnectionProvider.GetConnection();
            var cmd = new SqlCommand("sp_RegisterForEvent", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("@EventID", eventId);
            cmd.Parameters.AddWithValue("@StudentID", studentId);
            cmd.Parameters.AddWithValue("@CreatedBy", createdBy);

            try
            {
                connection.Open();
                cmd.ExecuteNonQuery();
                return true;
            }
            catch (SqlException ex)
            {
                errorMessage = ex.Number >= 50000 ? ex.Message : "A database error occurred during registration.";
                return false;
            }
        }

        public List<EventTicketDto> GetStudentTickets(int studentId)
        {
            var tickets = new List<EventTicketDto>();
            using var connection = DbConnectionProvider.GetConnection();
            var cmd = new SqlCommand(@"
                SELECT er.RegistrationID, e.EventID, e.Title, s.Name, v.Name, e.EventDate, er.TicketNumber, er.RegistrationDate
                FROM EventRegistrations er
                INNER JOIN Events e ON e.EventID = er.EventID
                INNER JOIN Societies s ON s.SocietyID = e.SocietyID
                INNER JOIN Venues v ON v.VenueID = e.VenueID
                WHERE er.StudentID = @StudentID AND er.IsDeleted = 0
                ORDER BY e.EventDate DESC", connection);
            cmd.Parameters.AddWithValue("@StudentID", studentId);

            connection.Open();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                tickets.Add(new EventTicketDto
                {
                    RegistrationID = reader.GetInt32(0),
                    EventID = reader.GetInt32(1),
                    StudentID = studentId,
                    StudentName = string.Empty,
                    EventTitle = reader.GetString(2),
                    SocietyName = reader.GetString(3),
                    VenueName = reader.GetString(4),
                    EventDate = reader.GetDateTime(5),
                    TicketNumber = reader.GetGuid(6),
                    RegistrationDate = reader.GetDateTime(7)
                });
            }
            return tickets;
        }

        private static void AddEventParameters(SqlCommand cmd, EventDto eventObj)
        {
            cmd.Parameters.AddWithValue("@SocietyID", eventObj.SocietyID);
            cmd.Parameters.AddWithValue("@Title", eventObj.Title);
            cmd.Parameters.AddWithValue("@Desc", eventObj.Description);
            cmd.Parameters.AddWithValue("@VenueID", eventObj.VenueID);
            cmd.Parameters.AddWithValue("@EventDate", eventObj.EventDate);
            cmd.Parameters.AddWithValue("@MaxCapacity", eventObj.MaxCapacity);
        }

        private static List<EventDto> QueryEvents(string whereClause, Action<SqlCommand> addParameters)
        {
            var events = new List<EventDto>();
            using var connection = DbConnectionProvider.GetConnection();
            var cmd = new SqlCommand($@"
                SELECT e.EventID, e.SocietyID, e.Title, e.Description, e.VenueID, v.Name, e.EventDate,
                       e.MaxCapacity, e.StatusID, s.Name,
                       (SELECT COUNT(*) FROM EventRegistrations er WHERE er.EventID = e.EventID AND er.IsDeleted = 0) AS RegisteredCount
                FROM Events e
                INNER JOIN Societies s ON s.SocietyID = e.SocietyID
                INNER JOIN Venues v ON v.VenueID = e.VenueID
                {whereClause}
                ORDER BY e.EventDate DESC", connection);
            addParameters?.Invoke(cmd);

            connection.Open();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                events.Add(new EventDto
                {
                    EventID = reader.GetInt32(0),
                    SocietyID = reader.GetInt32(1),
                    Title = reader.GetString(2),
                    Description = reader.GetString(3),
                    VenueID = reader.GetInt32(4),
                    VenueName = reader.GetString(5),
                    EventDate = reader.GetDateTime(6),
                    MaxCapacity = reader.GetInt32(7),
                    Status = (EventStatusEnum)reader.GetInt32(8),
                    SocietyName = reader.GetString(9),
                    RegisteredCount = reader.GetInt32(10)
                });
            }
            return events;
        }
    }
}

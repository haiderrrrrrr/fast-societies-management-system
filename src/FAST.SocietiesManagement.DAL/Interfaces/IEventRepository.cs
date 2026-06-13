using System.Collections.Generic;
using FAST.SocietiesManagement.Core.DTOs;

namespace FAST.SocietiesManagement.DAL.Interfaces
{
    public interface IEventRepository
    {
        List<EventDto> GetUpcomingEvents();
        List<EventDto> GetAllEvents();
        List<EventDto> GetEventsBySociety(int societyId);
        List<VenueDto> GetVenues();
        void CreateEvent(EventDto eventObj, int createdBy);
        void UpdateEventStatus(int eventId, int statusId, int updatedBy);
        void UpdateEvent(EventDto eventObj, int updatedBy);
        bool RegisterForEvent(int eventId, int studentId, int createdBy, out string errorMessage);
        List<EventTicketDto> GetStudentTickets(int studentId);
    }
}

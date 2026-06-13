using System;
using System.Collections.Generic;
using FAST.SocietiesManagement.Core.DTOs;
using FAST.SocietiesManagement.Core.Enums;
using FAST.SocietiesManagement.DAL.Interfaces;
using FAST.SocietiesManagement.BLL.Validators;
using FAST.SocietiesManagement.Core.Utilities;

namespace FAST.SocietiesManagement.BLL.Services
{
    public class EventService
    {
        private readonly IEventRepository _eventRepository;
        private readonly EventValidator _validator;

        public EventService(IEventRepository eventRepository)
        {
            _eventRepository = eventRepository;
            _validator = new EventValidator();
        }

        public List<EventDto> GetUpcomingEvents() => _eventRepository.GetUpcomingEvents();

        public List<EventDto> GetAllEvents(RoleType requesterRole)
        {
            return requesterRole == RoleType.Admin ? _eventRepository.GetAllEvents() : new List<EventDto>();
        }

        public List<EventDto> GetEventsBySociety(int societyId) => _eventRepository.GetEventsBySociety(societyId);

        public List<VenueDto> GetVenues() => _eventRepository.GetVenues();

        public List<EventTicketDto> GetStudentTickets(int studentId) => _eventRepository.GetStudentTickets(studentId);

        public (bool Success, string Message) CreateEvent(EventDto eventObj, int creatorUserId, RoleType creatorRole)
        {
            if (creatorRole != RoleType.Admin && creatorRole != RoleType.SocietyHead)
                return (false, "Unauthorized: Only Admins and Society Heads can create events.");

            var validation = _validator.ValidateEventCreation(eventObj);
            if (!validation.IsValid) return (false, validation.ErrorMessage);

            eventObj.Status = creatorRole == RoleType.Admin ? EventStatusEnum.Approved : EventStatusEnum.PendingApproval;

            try
            {
                _eventRepository.CreateEvent(eventObj, creatorUserId);
                Logger.LogInfo($"Event '{eventObj.Title}' created.", "EventService.CreateEvent", creatorUserId);
                return (true, creatorRole == RoleType.Admin ? "Event created and approved." : "Event submitted for admin approval.");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "EventService.CreateEvent", creatorUserId);
                return (false, "Could not create event. Check venue, society, and capacity.");
            }
        }

        public (bool Success, string Message) UpdateEvent(EventDto eventObj, int updatedBy, RoleType role)
        {
            if (role != RoleType.Admin && role != RoleType.SocietyHead) return (false, "Unauthorized.");
            var validation = _validator.ValidateEventCreation(eventObj);
            if (!validation.IsValid) return (false, validation.ErrorMessage);
            try
            {
                _eventRepository.UpdateEvent(eventObj, updatedBy);
                return (true, "Event updated.");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "EventService.UpdateEvent", updatedBy);
                return (false, "Failed to update event.");
            }
        }

        public (bool Success, string Message) UpdateEventStatus(int eventId, EventStatusEnum status, int updatedBy, RoleType requesterRole)
        {
            if (requesterRole != RoleType.Admin && requesterRole != RoleType.SocietyHead)
                return (false, "Unauthorized to update event status.");

            if (requesterRole == RoleType.SocietyHead && status == EventStatusEnum.Approved)
                return (false, "Only admins can approve event requests.");

            try
            {
                _eventRepository.UpdateEventStatus(eventId, (int)status, updatedBy);
                return (true, $"Event marked as {status}.");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "EventService.UpdateEventStatus", updatedBy);
                return (false, "Failed to update event status.");
            }
        }

        public (bool Success, string Message) RegisterStudentForEvent(int eventId, int? studentId, int userId)
        {
            if (!studentId.HasValue) return (false, "Your student profile is missing. Complete registration first.");

            bool result = _eventRepository.RegisterForEvent(eventId, studentId.Value, userId, out string dbError);
            if (result)
            {
                Logger.LogInfo($"Student {studentId} registered for Event {eventId}", "EventService.RegisterStudentForEvent", userId);
                return (true, "Successfully registered for the event.");
            }

            Logger.LogInfo($"Student {studentId} failed event {eventId} registration: {dbError}", "EventService.RegisterStudentForEvent", userId);
            return (false, dbError);
        }
    }
}

using System;
using FAST.SocietiesManagement.Core.DTOs;

namespace FAST.SocietiesManagement.BLL.Validators
{
    public class EventValidator
    {
        public (bool IsValid, string ErrorMessage) ValidateEventCreation(EventDto eventObj)
        {
            if (string.IsNullOrWhiteSpace(eventObj.Title)) return (false, "Event title cannot be empty.");
            if (string.IsNullOrWhiteSpace(eventObj.Description)) return (false, "Event description cannot be empty.");
            if (eventObj.MaxCapacity <= 0) return (false, "Maximum capacity must be greater than zero.");
            if (eventObj.EventDate <= DateTime.Now) return (false, "Event date must be in the future.");
            
            return (true, string.Empty);
        }
    }
}

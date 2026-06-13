using System;
using FAST.SocietiesManagement.Core.DTOs;

namespace FAST.SocietiesManagement.BLL.Validators
{
    public class SocietyValidator
    {
        public (bool IsValid, string ErrorMessage) ValidateSocietyCreation(SocietyDto society)
        {
            if (string.IsNullOrWhiteSpace(society.Name)) return (false, "Society Name cannot be empty.");
            if (society.Name.Length < 3) return (false, "Society Name must be at least 3 characters.");
            if (string.IsNullOrWhiteSpace(society.Description)) return (false, "Society Description cannot be empty.");
            if (society.Description.Length < 10) return (false, "Society Description must be more descriptive.");
            
            return (true, string.Empty);
        }
    }
}

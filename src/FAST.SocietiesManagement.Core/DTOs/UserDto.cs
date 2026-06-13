using System;
using FAST.SocietiesManagement.Core.Enums;

namespace FAST.SocietiesManagement.Core.DTOs
{
    public class UserDto
    {
        public int UserID { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string Salt { get; set; }
        public RoleType Role { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public int? StudentID { get; set; }
        public string FullName { get; set; }
        public string RollNumber { get; set; }
        public string Department { get; set; }
    }
}

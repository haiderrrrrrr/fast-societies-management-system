using System.Collections.Generic;
using FAST.SocietiesManagement.Core.DTOs;

namespace FAST.SocietiesManagement.DAL.Interfaces
{
    public interface IUserRepository
    {
        UserDto GetUserByUsername(string username);
        void CreateUser(UserDto user);
        void CreateStudentAccount(UserDto user, StudentProfileDto profile);
        List<UserDto> GetAllUsers();
        List<UserDto> GetUsersByRole(int roleId);
        StudentProfileDto GetStudentProfileByUserId(int userId);
        void UpdateUserActiveStatus(int userId, bool isActive, int updatedBy);
        void SoftDeleteUser(int userId, int updatedBy);
        bool IsUsernameTaken(string username);
        bool IsEmailTaken(string email);
        bool IsRollNumberTaken(string rollNumber);
    }
}

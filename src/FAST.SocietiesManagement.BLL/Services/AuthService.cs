using System;
using System.Collections.Generic;
using FAST.SocietiesManagement.Core.DTOs;
using FAST.SocietiesManagement.Core.Enums;
using FAST.SocietiesManagement.Core.Utilities;
using FAST.SocietiesManagement.DAL.Interfaces;
using FAST.SocietiesManagement.BLL.Validators;

namespace FAST.SocietiesManagement.BLL.Services
{
    public class AuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly UserValidator _userValidator;

        public AuthService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
            _userValidator = new UserValidator(_userRepository);
        }

        public (bool Success, string Message, UserDto User) Login(string username, string password)
        {
            var validation = _userValidator.ValidateLogin(username, password);
            if (!validation.IsValid)
            {
                Logger.LogInfo($"Failed Login Attempt - Validation Failed for: {username}", "AuthService.Login");
                return (false, validation.ErrorMessage, null);
            }

            var user = _userRepository.GetUserByUsername(username.Trim());
            if (user == null)
            {
                Logger.LogInfo($"Failed Login Attempt - User Not Found: {username}", "AuthService.Login");
                return (false, "Invalid username or password.", null);
            }

            if (!user.IsActive)
            {
                Logger.LogInfo($"Failed Login Attempt - Disabled Account: {username}", "AuthService.Login", user.UserID);
                return (false, "Account is disabled. Please contact the administrator.", null);
            }

            if (!PasswordHasher.VerifyPassword(password, user.PasswordHash, user.Salt))
            {
                Logger.LogInfo($"Failed Login Attempt - Wrong Password for: {username}", "AuthService.Login", user.UserID);
                return (false, "Invalid username or password.", null);
            }

            Logger.LogInfo($"Successful Login: {username}", "AuthService.Login", user.UserID);
            return (true, "Login successful.", user);
        }

        public (bool Success, string Message) RegisterStudent(
            string username,
            string email,
            string password,
            string confirmPassword,
            string fullName,
            string rollNumber,
            string department)
        {
            var validation = _userValidator.ValidateStudentRegistration(username, email, password, confirmPassword, fullName, rollNumber, department);
            if (!validation.IsValid) return (false, validation.ErrorMessage);

            var (hash, salt) = PasswordHasher.HashPassword(password);
            var newUser = new UserDto
            {
                Username = username.Trim(),
                Email = email.Trim(),
                PasswordHash = hash,
                Salt = salt,
                Role = RoleType.Student,
                IsActive = true
            };
            var profile = new StudentProfileDto
            {
                FullName = fullName.Trim(),
                RollNumber = rollNumber.Trim(),
                Department = department.Trim(),
                Email = email.Trim()
            };

            try
            {
                _userRepository.CreateStudentAccount(newUser, profile);
                Logger.LogInfo($"New student registration successful: {username}");
                return (true, "Student account and profile created successfully.");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Registration Error");
                return (false, "An error occurred during registration. Please verify the data and try again.");
            }
        }

        public (bool Success, string Message) CreateStaffAccount(string username, string email, string password, string confirmPassword, RoleType role, int createdBy)
        {
            if (role == RoleType.Student) return (false, "Use student registration for student accounts.");

            var validation = _userValidator.ValidateAccount(username, email, password, confirmPassword);
            if (!validation.IsValid) return (false, validation.ErrorMessage);

            var (hash, salt) = PasswordHasher.HashPassword(password);
            try
            {
                _userRepository.CreateUser(new UserDto
                {
                    Username = username.Trim(),
                    Email = email.Trim(),
                    PasswordHash = hash,
                    Salt = salt,
                    Role = role,
                    IsActive = true
                });
                Logger.LogInfo($"Staff account {username} created.", "AuthService.CreateStaffAccount", createdBy);
                return (true, $"{role} account created.");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "AuthService.CreateStaffAccount", createdBy);
                return (false, "Could not create staff account.");
            }
        }

        public List<UserDto> GetAllUsers(RoleType requesterRole)
        {
            return requesterRole == RoleType.Admin ? _userRepository.GetAllUsers() : new List<UserDto>();
        }

        public List<UserDto> GetSocietyHeads(RoleType requesterRole)
        {
            return requesterRole == RoleType.Admin
                ? _userRepository.GetUsersByRole((int)RoleType.SocietyHead)
                : new List<UserDto>();
        }

        public (bool Success, string Message) SetUserActiveStatus(int userId, bool isActive, int updatedBy, RoleType requesterRole)
        {
            if (requesterRole != RoleType.Admin) return (false, "Only admins can update account status.");
            if (userId == updatedBy && !isActive) return (false, "Admins cannot suspend their own active session.");

            try
            {
                _userRepository.UpdateUserActiveStatus(userId, isActive, updatedBy);
                return (true, isActive ? "Account activated." : "Account suspended.");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "AuthService.SetUserActiveStatus", updatedBy);
                return (false, "Failed to update account status.");
            }
        }

        public (bool Success, string Message) DeleteUser(int userId, int updatedBy, RoleType requesterRole)
        {
            if (requesterRole != RoleType.Admin) return (false, "Only admins can delete accounts.");
            if (userId == updatedBy) return (false, "Admins cannot delete their own active session.");

            try
            {
                _userRepository.SoftDeleteUser(userId, updatedBy);
                return (true, "Account deleted using safe soft-delete.");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "AuthService.DeleteUser", updatedBy);
                return (false, "Failed to delete account.");
            }
        }
    }
}

using System;
using System.Collections.Generic;
using Moq;
using Xunit;
using FAST.SocietiesManagement.BLL.Services;
using FAST.SocietiesManagement.Core.DTOs;
using FAST.SocietiesManagement.Core.Enums;
using FAST.SocietiesManagement.Core.Utilities;
using FAST.SocietiesManagement.DAL.Interfaces;

namespace FAST.SocietiesManagement.Tests.Services
{
    public class AuthServiceTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly AuthService _authService;

        public AuthServiceTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _authService = new AuthService(_userRepositoryMock.Object);
        }

        [Fact]
        public void AuthService_Constructor_ValidUserRepository_ReturnsInstance()
        {
            var service = new AuthService(_userRepositoryMock.Object);
            Assert.NotNull(service);
        }

        [Fact]
        public void Login_ValidCredentials_ReturnsSuccess()
        {
            // Arrange
            var username = "valid-username";
            var password = "valid-password";
            var (hash, salt) = PasswordHasher.HashPassword(password);
            
            var user = new UserDto
            {
                UserID = 1,
                Username = username,
                PasswordHash = hash,
                Salt = salt,
                IsActive = true
            };
            _userRepositoryMock.Setup(repo => repo.GetUserByUsername(username)).Returns(user);

            // Act
            var result = _authService.Login(username, password);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Login successful.", result.Message);
            Assert.NotNull(result.User);
        }

        [Theory]
        [InlineData("", "")]
        public void Login_BlankStringValidation_ReturnsFailure(string username, string password)
        {
            // Act
            var result = _authService.Login(username, password);

            // Assert
            Assert.False(result.Success);
            Assert.Null(result.User);
        }

        [Fact]
        public void Login_UserNotFound_ReturnsFailure()
        {
            // Arrange
            var username = "valid-username";
            var password = "valid-password";
            _userRepositoryMock.Setup(repo => repo.GetUserByUsername(username)).Returns((UserDto)null!);

            // Act
            var result = _authService.Login(username, password);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Invalid username or password.", result.Message);
            Assert.Null(result.User);
        }

        [Fact]
        public void RegisterStudent_ValidInputs_ReturnsSuccess()
        {
            // Arrange
            var username = "valid-username";
            var email = "valid@email.com";
            var password = "ValidPassword1";
            var confirmPassword = "ValidPassword1";
            var fullName = "Valid FullName";
            var rollNumber = "123456";
            var department = "CS";

            _userRepositoryMock.Setup(repo => repo.GetUserByUsername(username)).Returns((UserDto)null!);
            _userRepositoryMock.Setup(repo => repo.CreateStudentAccount(It.IsAny<UserDto>(), It.IsAny<StudentProfileDto>()));

            // Act
            var result = _authService.RegisterStudent(username, email, password, confirmPassword, fullName, rollNumber, department);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Student account and profile created successfully.", result.Message);
        }

        [Theory]
        [InlineData("", "", "", "", "", "", "")]
        public void RegisterStudent_BlankStringValidation_ReturnsFailure(string username, string email, string password, string confirmPassword, string fullName, string rollNumber, string department)
        {
            // Act
            var result = _authService.RegisterStudent(username, email, password, confirmPassword, fullName, rollNumber, department);

            // Assert
            Assert.False(result.Success);
        }

        [Fact]
        public void CreateStaffAccount_ValidInputs_ReturnsSuccess()
        {
            // Arrange
            var username = "valid-staff";
            var email = "staff@email.com";
            var password = "ValidPassword1";
            var confirmPassword = "ValidPassword1";
            var role = RoleType.Admin;
            int createdBy = 1;

            _userRepositoryMock.Setup(repo => repo.GetUserByUsername(username)).Returns((UserDto)null!);
            _userRepositoryMock.Setup(repo => repo.CreateUser(It.IsAny<UserDto>()));

            // Act
            var result = _authService.CreateStaffAccount(username, email, password, confirmPassword, role, createdBy);

            // Assert
            Assert.True(result.Success);
        }

        [Fact]
        public void CreateStaffAccount_StudentRole_ReturnsFailure()
        {
            // Act
            var result = _authService.CreateStaffAccount("user", "email", "pass", "pass", RoleType.Student, 1);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Use student registration for student accounts.", result.Message);
        }

        [Fact]
        public void GetAllUsers_AdminRole_ReturnsUsers()
        {
            // Arrange
            _userRepositoryMock.Setup(repo => repo.GetAllUsers()).Returns(new List<UserDto> { new UserDto() });

            // Act
            var result = _authService.GetAllUsers(RoleType.Admin);

            // Assert
            Assert.NotEmpty(result);
        }

        [Fact]
        public void GetAllUsers_StudentRole_ReturnsEmpty()
        {
            // Act
            var result = _authService.GetAllUsers(RoleType.Student);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void GetSocietyHeads_AdminRole_ReturnsHeads()
        {
            // Arrange
            _userRepositoryMock.Setup(repo => repo.GetUsersByRole((int)RoleType.SocietyHead)).Returns(new List<UserDto> { new UserDto() });

            // Act
            var result = _authService.GetSocietyHeads(RoleType.Admin);

            // Assert
            Assert.NotEmpty(result);
        }

        [Fact]
        public void SetUserActiveStatus_AdminRole_ReturnsSuccess()
        {
            // Arrange
            _userRepositoryMock.Setup(repo => repo.UpdateUserActiveStatus(1, true, 2));

            // Act
            var result = _authService.SetUserActiveStatus(1, true, 2, RoleType.Admin);

            // Assert
            Assert.True(result.Success);
        }

        [Fact]
        public void SetUserActiveStatus_StudentRole_ReturnsFailure()
        {
            // Act
            var result = _authService.SetUserActiveStatus(1, true, 2, RoleType.Student);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Only admins can update account status.", result.Message);
        }

        [Fact]
        public void DeleteUser_AdminRole_ReturnsSuccess()
        {
            // Arrange
            _userRepositoryMock.Setup(repo => repo.SoftDeleteUser(1, 2));

            // Act
            var result = _authService.DeleteUser(1, 2, RoleType.Admin);

            // Assert
            Assert.True(result.Success);
        }

        [Fact]
        public void DeleteUser_SelfDelete_ReturnsFailure()
        {
            // Act
            var result = _authService.DeleteUser(1, 1, RoleType.Admin);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Admins cannot delete their own active session.", result.Message);
        }
    }
}

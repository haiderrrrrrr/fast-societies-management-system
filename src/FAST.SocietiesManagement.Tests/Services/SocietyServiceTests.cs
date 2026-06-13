using System;
using System.Collections.Generic;
using Moq;
using Xunit;
using FAST.SocietiesManagement.BLL.Services;
using FAST.SocietiesManagement.Core.DTOs;
using FAST.SocietiesManagement.Core.Enums;
using FAST.SocietiesManagement.DAL.Interfaces;

namespace FAST.SocietiesManagement.Tests.Services
{
    public class SocietyServiceTests
    {
        private readonly Mock<ISocietyRepository> _societyRepositoryMock;
        private readonly SocietyService _societyService;

        public SocietyServiceTests()
        {
            _societyRepositoryMock = new Mock<ISocietyRepository>();
            _societyService = new SocietyService(_societyRepositoryMock.Object);
        }

        [Fact]
        public void GetAllSocieties_AdminRole_ReturnsSocieties()
        {
            // Arrange
            _societyRepositoryMock.Setup(repo => repo.GetAllSocieties()).Returns(new List<SocietyDto> { new SocietyDto() });

            // Act
            var result = _societyService.GetAllSocieties(RoleType.Admin);

            // Assert
            Assert.NotEmpty(result);
        }

        [Fact]
        public void GetAllSocieties_StudentRole_ReturnsEmpty()
        {
            // Arrange
            _societyRepositoryMock.Setup(repo => repo.GetActiveSocieties()).Returns(new List<SocietyDto>());

            // Act
            var result = _societyService.GetAllSocieties(RoleType.Student);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void CreateSociety_ValidInputs_ReturnsSuccess()
        {
            // Arrange
            var society = new SocietyDto
            {
                Name = "Test Society",
                Description = "A valid test society",
                Status = SocietyStatusEnum.Active
            };

            // Act
            var result = _societyService.CreateSociety(society, 1, RoleType.Admin);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Society successfully created.", result.Message);
        }

        [Fact]
        public void CreateSociety_UnauthorizedRole_ReturnsFailure()
        {
            // Act
            var result = _societyService.CreateSociety(new SocietyDto(), 1, RoleType.Student);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Unauthorized: Only Admins can create societies.", result.Message);
        }

        [Fact]
        public void AssignHead_ValidInputs_ReturnsSuccess()
        {
            // Arrange
            _societyRepositoryMock.Setup(repo => repo.UpdateSocietyHead(1, 2, 1));

            // Act
            var result = _societyService.AssignHead(1, 2, 1, RoleType.Admin);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Society head updated.", result.Message);
        }

        [Fact]
        public void AssignHead_UnauthorizedRole_ReturnsFailure()
        {
            // Act
            var result = _societyService.AssignHead(1, 2, 1, RoleType.Student);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Only admins can assign society heads.", result.Message);
        }

        [Fact]
        public void UpdateStatus_ValidInputs_ReturnsSuccess()
        {
            // Arrange
            _societyRepositoryMock.Setup(repo => repo.UpdateSocietyStatus(1, (int)SocietyStatusEnum.Suspended, 1));

            // Act
            var result = _societyService.UpdateStatus(1, SocietyStatusEnum.Suspended, 1, RoleType.Admin);

            // Assert
            Assert.True(result.Success);
        }

        [Fact]
        public void DeleteSociety_ValidInputs_ReturnsSuccess()
        {
            // Arrange
            _societyRepositoryMock.Setup(repo => repo.SoftDeleteSociety(1, 2));

            // Act
            var result = _societyService.DeleteSociety(1, 2, RoleType.Admin);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Society deleted using safe soft-delete.", result.Message);
        }
    }
}

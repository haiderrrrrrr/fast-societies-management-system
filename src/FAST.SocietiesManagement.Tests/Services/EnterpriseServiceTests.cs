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
    public class EnterpriseServiceTests
    {
        private readonly Mock<IEnterpriseRepository> _enterpriseRepoMock;
        private readonly Mock<IMembershipRepository> _membershipRepoMock;
        private readonly EnterpriseService _enterpriseService;

        public EnterpriseServiceTests()
        {
            _enterpriseRepoMock = new Mock<IEnterpriseRepository>();
            _membershipRepoMock = new Mock<IMembershipRepository>();
            _enterpriseService = new EnterpriseService(_enterpriseRepoMock.Object, _membershipRepoMock.Object);
        }

        [Fact]
        public void EnterpriseService_Happy_path()
        {
            // Arrange
            var service = new EnterpriseService(_enterpriseRepoMock.Object, _membershipRepoMock.Object);
            // Assert
            Assert.NotNull(service);
        }

        [Fact]
        public void PublishAnnouncement_Happy_path()
        {
            // Arrange
            var announcement = new AnnouncementDto { Title = "Valid", Message = "Valid", SocietyID = 1 };
            _enterpriseRepoMock.Setup(repo => repo.CreateAnnouncement(announcement));

            // Act
            var result = _enterpriseService.PublishAnnouncement(announcement, RoleType.Admin);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Announcement published.", result.Message);
        }

        [Fact]
        public void PublishAnnouncement_Blank_string_validation()
        {
            // Act
            var result = _enterpriseService.PublishAnnouncement(new AnnouncementDto { Title = "", Message = "", SocietyID = 1 }, RoleType.Admin);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Announcement title is required.", result.Message);
        }

        [Fact]
        public void PublishAnnouncement_Unauthorized_role()
        {
            // Act
            var result = _enterpriseService.PublishAnnouncement(new AnnouncementDto { Title = "Valid", Message = "Valid", SocietyID = 1 }, RoleType.Student);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Only admins and society heads can publish announcements.", result.Message);
        }

        [Fact]
        public void PublishAnnouncement_Exception_path()
        {
            // Arrange
            var announcement = new AnnouncementDto { Title = "Valid", Message = "Valid", SocietyID = 1 };
            _enterpriseRepoMock.Setup(repo => repo.CreateAnnouncement(announcement)).Throws(new Exception("DB Error"));

            // Act
            var result = _enterpriseService.PublishAnnouncement(announcement, RoleType.Admin);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Failed to publish announcement.", result.Message);
        }

        [Fact]
        public void PublishAnnouncement_Additional_branch_path_4()
        {
            // Act
            var result = _enterpriseService.PublishAnnouncement(new AnnouncementDto { Title = "Valid", Message = "Valid", SocietyID = 0 }, RoleType.Admin);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Select a society.", result.Message);
        }

        [Fact]
        public void PublishAnnouncement_Additional_branch_path_5()
        {
            // Act
            var result = _enterpriseService.PublishAnnouncement(new AnnouncementDto { Title = "Valid", Message = "", SocietyID = 1 }, RoleType.Admin);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Announcement message is required.", result.Message);
        }

        [Fact]
        public void PublishAnnouncement_Additional_branch_path_6()
        {
            // Arrange
            var announcement = new AnnouncementDto { Title = "Valid", Message = "Valid", SocietyID = 1 };
            _enterpriseRepoMock.Setup(repo => repo.CreateAnnouncement(announcement));

            // Act
            var result = _enterpriseService.PublishAnnouncement(announcement, RoleType.SocietyHead);

            // Assert
            Assert.True(result.Success);
        }

        [Fact]
        public void GetStudentAnnouncements_Happy_path()
        {
            // Arrange
            _enterpriseRepoMock.Setup(repo => repo.GetAnnouncementsForStudent(1)).Returns(new List<AnnouncementDto> { new AnnouncementDto() });

            // Act
            var result = _enterpriseService.GetStudentAnnouncements(1);

            // Assert
            Assert.NotEmpty(result);
        }

        [Fact]
        public void GetStudentAnnouncements_Missing_null_value()
        {
            // Act
            var result = _enterpriseService.GetStudentAnnouncements(null);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void GetSocietyAnnouncements_Happy_path()
        {
            // Arrange
            _enterpriseRepoMock.Setup(repo => repo.GetAnnouncementsBySociety(1)).Returns(new List<AnnouncementDto> { new AnnouncementDto() });

            // Act
            var result = _enterpriseService.GetSocietyAnnouncements(1);

            // Assert
            Assert.NotEmpty(result);
        }

        [Fact]
        public void GetAllAnnouncements_Happy_path()
        {
            // Arrange
            _enterpriseRepoMock.Setup(repo => repo.GetAllAnnouncements()).Returns(new List<AnnouncementDto> { new AnnouncementDto() });

            // Act
            var result = _enterpriseService.GetAllAnnouncements(RoleType.Admin);

            // Assert
            Assert.NotEmpty(result);
        }

        [Fact]
        public void GetAllAnnouncements_Unauthorized_role()
        {
            // Act
            var result = _enterpriseService.GetAllAnnouncements(RoleType.Student);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void GetEventRegistrants_Happy_path()
        {
            // Arrange
            _enterpriseRepoMock.Setup(repo => repo.GetEventRegistrants(1)).Returns(new List<EventTicketDto> { new EventTicketDto() });

            // Act
            var result = _enterpriseService.GetEventRegistrants(1, RoleType.Admin);

            // Assert
            Assert.NotEmpty(result);
        }

        [Fact]
        public void GetEventRegistrants_Unauthorized_role()
        {
            // Act
            var result = _enterpriseService.GetEventRegistrants(1, RoleType.Student);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void GetEventRegistrants_Additional_branch_path_2()
        {
            // Arrange
            _enterpriseRepoMock.Setup(repo => repo.GetEventRegistrants(1)).Returns(new List<EventTicketDto> { new EventTicketDto() });

            // Act
            var result = _enterpriseService.GetEventRegistrants(1, RoleType.SocietyHead);

            // Assert
            Assert.NotEmpty(result);
        }

        [Fact]
        public void MarkAttendance_Happy_path()
        {
            // Arrange
            _enterpriseRepoMock.Setup(repo => repo.MarkAttendance(1, 1, true, 1));

            // Act
            var result = _enterpriseService.MarkAttendance(1, 1, true, 1, RoleType.Admin);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Attendance marked present.", result.Message);
        }

        [Fact]
        public void MarkAttendance_Unauthorized_role()
        {
            // Act
            var result = _enterpriseService.MarkAttendance(1, 1, true, 1, RoleType.Student);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Unauthorized to mark attendance.", result.Message);
        }

        [Fact]
        public void MarkAttendance_Exception_path()
        {
            // Arrange
            _enterpriseRepoMock.Setup(repo => repo.MarkAttendance(1, 1, true, 1)).Throws(new Exception());

            // Act
            var result = _enterpriseService.MarkAttendance(1, 1, true, 1, RoleType.Admin);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Failed to mark attendance.", result.Message);
        }

        [Fact]
        public void MarkAttendance_Additional_branch_path_3()
        {
            // Arrange
            _enterpriseRepoMock.Setup(repo => repo.MarkAttendance(1, 1, false, 1));

            // Act
            var result = _enterpriseService.MarkAttendance(1, 1, false, 1, RoleType.Admin);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Attendance marked absent.", result.Message);
        }

        [Fact]
        public void MarkAttendance_Additional_branch_path_4()
        {
            // Arrange
            _enterpriseRepoMock.Setup(repo => repo.MarkAttendance(1, 1, true, 1));

            // Act
            var result = _enterpriseService.MarkAttendance(1, 1, true, 1, RoleType.SocietyHead);

            // Assert
            Assert.True(result.Success);
        }

        [Fact]
        public void GetAttendanceByEvent_Happy_path()
        {
            // Arrange
            _enterpriseRepoMock.Setup(repo => repo.GetAttendanceByEvent(1)).Returns(new List<AttendanceDto> { new AttendanceDto() });

            // Act
            var result = _enterpriseService.GetAttendanceByEvent(1, RoleType.Admin);

            // Assert
            Assert.NotEmpty(result);
        }

        [Fact]
        public void GetAttendanceByEvent_Unauthorized_role()
        {
            // Act
            var result = _enterpriseService.GetAttendanceByEvent(1, RoleType.Student);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void GetAttendanceByEvent_Additional_branch_path_2()
        {
            // Arrange
            _enterpriseRepoMock.Setup(repo => repo.GetAttendanceByEvent(1)).Returns(new List<AttendanceDto> { new AttendanceDto() });

            // Act
            var result = _enterpriseService.GetAttendanceByEvent(1, RoleType.SocietyHead);

            // Assert
            Assert.NotEmpty(result);
        }

        [Fact]
        public void SubmitFeedback_Happy_path()
        {
            // Arrange
            _enterpriseRepoMock.Setup(repo => repo.SubmitFeedback(It.IsAny<FeedbackDto>()));

            // Act
            var result = _enterpriseService.SubmitFeedback(1, 1, 5, "Great");

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Feedback submitted.", result.Message);
        }

        [Fact]
        public void SubmitFeedback_Blank_string_validation()
        {
            // Act
            var result = _enterpriseService.SubmitFeedback(1, 1, 5, "");

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Feedback comments are required.", result.Message);
        }

        [Fact]
        public void SubmitFeedback_Missing_null_value()
        {
            // Act
            var result = _enterpriseService.SubmitFeedback(1, null, 5, "Great");

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Student profile is missing.", result.Message);
        }

        [Fact]
        public void SubmitFeedback_Duplicate_record()
        {
            // Arrange
            // Due to limitations mocking SqlException, we simulate a general DB error
            _enterpriseRepoMock.Setup(repo => repo.SubmitFeedback(It.IsAny<FeedbackDto>())).Throws(new Exception("Mocked general error"));

            // Act
            var result = _enterpriseService.SubmitFeedback(1, 1, 5, "Great");

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Failed to submit feedback.", result.Message);
        }

        [Fact]
        public void SubmitFeedback_Additional_branch_path_7()
        {
            // Act
            var result = _enterpriseService.SubmitFeedback(1, 1, 3, "Okay");

            // Assert
            Assert.True(result.Success);
        }

        [Fact]
        public void GetFeedbackByEvent_Happy_path()
        {
            // Arrange
            _enterpriseRepoMock.Setup(repo => repo.GetFeedbackByEvent(1)).Returns(new List<FeedbackDto> { new FeedbackDto() });

            // Act
            var result = _enterpriseService.GetFeedbackByEvent(1, RoleType.Admin);

            // Assert
            Assert.NotEmpty(result);
        }

        [Fact]
        public void GetFeedbackByEvent_Unauthorized_role()
        {
            // Act
            var result = _enterpriseService.GetFeedbackByEvent(1, RoleType.Student);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void GetFeedbackByEvent_Additional_branch_path_2()
        {
            // Arrange
            _enterpriseRepoMock.Setup(repo => repo.GetFeedbackByEvent(1)).Returns(new List<FeedbackDto> { new FeedbackDto() });

            // Act
            var result = _enterpriseService.GetFeedbackByEvent(1, RoleType.SocietyHead);

            // Assert
            Assert.NotEmpty(result);
        }

        [Fact]
        public void GetUniversityReport_Happy_path()
        {
            // Arrange
            _enterpriseRepoMock.Setup(repo => repo.GetUniversityReport()).Returns(new List<ReportRowDto> { new ReportRowDto() });

            // Act
            var result = _enterpriseService.GetUniversityReport(RoleType.Admin);

            // Assert
            Assert.NotEmpty(result);
        }

        [Fact]
        public void GetUniversityReport_Unauthorized_role()
        {
            // Act
            var result = _enterpriseService.GetUniversityReport(RoleType.Student);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void GetSocietyReport_Happy_path()
        {
            // Arrange
            _enterpriseRepoMock.Setup(repo => repo.GetSocietyReport(1)).Returns(new List<ReportRowDto> { new ReportRowDto() });

            // Act
            var result = _enterpriseService.GetSocietyReport(1, RoleType.Admin);

            // Assert
            Assert.NotEmpty(result);
        }

        [Fact]
        public void GetSocietyReport_Unauthorized_role()
        {
            // Act
            var result = _enterpriseService.GetSocietyReport(1, RoleType.Student);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void GetSocietyReport_Additional_branch_path_2()
        {
            // Arrange
            _enterpriseRepoMock.Setup(repo => repo.GetSocietyReport(1)).Returns(new List<ReportRowDto> { new ReportRowDto() });

            // Act
            var result = _enterpriseService.GetSocietyReport(1, RoleType.SocietyHead);

            // Assert
            Assert.NotEmpty(result);
        }
    }
}
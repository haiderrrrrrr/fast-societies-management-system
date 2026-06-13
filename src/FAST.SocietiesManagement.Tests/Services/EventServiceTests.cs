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
    public class EventServiceTests
    {
        private readonly Mock<IEventRepository> _eventRepoMock;
        private readonly EventService _eventService;

        public EventServiceTests()
        {
            _eventRepoMock = new Mock<IEventRepository>();
            _eventService = new EventService(_eventRepoMock.Object);
        }

        [Fact]
        public void EventService_Happy_path()
        {
            var service = new EventService(_eventRepoMock.Object);
            Assert.NotNull(service);
        }

        [Fact]
        public void GetUpcomingEvents_Happy_path()
        {
            // Arrange
            _eventRepoMock.Setup(m => m.GetUpcomingEvents()).Returns(new List<EventDto> { new EventDto() });

            // Act
            var result = _eventService.GetUpcomingEvents();

            // Assert
            Assert.NotEmpty(result);
        }

        [Fact]
        public void GetAllEvents_Happy_path()
        {
            // Arrange
            _eventRepoMock.Setup(m => m.GetAllEvents()).Returns(new List<EventDto> { new EventDto() });

            // Act
            var result = _eventService.GetAllEvents(RoleType.Admin);

            // Assert
            Assert.NotEmpty(result);
        }

        [Fact]
        public void GetAllEvents_Unauthorized_role()
        {
            // Act
            var result = _eventService.GetAllEvents(RoleType.Student);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void GetEventsBySociety_Happy_path()
        {
            // Arrange
            _eventRepoMock.Setup(m => m.GetEventsBySociety(1)).Returns(new List<EventDto> { new EventDto() });

            // Act
            var result = _eventService.GetEventsBySociety(1);

            // Assert
            Assert.NotEmpty(result);
        }

        [Fact]
        public void GetVenues_Happy_path()
        {
            // Arrange
            _eventRepoMock.Setup(m => m.GetVenues()).Returns(new List<VenueDto> { new VenueDto() });

            // Act
            var result = _eventService.GetVenues();

            // Assert
            Assert.NotEmpty(result);
        }

        [Fact]
        public void GetStudentTickets_Happy_path()
        {
            // Arrange
            _eventRepoMock.Setup(m => m.GetStudentTickets(1)).Returns(new List<EventTicketDto> { new EventTicketDto() });

            // Act
            var result = _eventService.GetStudentTickets(1);

            // Assert
            Assert.NotEmpty(result);
        }

        [Fact]
        public void CreateEvent_Happy_path()
        {
            // Arrange
            var eventObj = new EventDto { Title = "Event", Description = "Desc", EventDate = DateTime.Now.AddDays(1), MaxCapacity = 10, VenueID = 1, SocietyID = 1 };
            _eventRepoMock.Setup(m => m.CreateEvent(eventObj, 1));

            // Act
            var result = _eventService.CreateEvent(eventObj, 1, RoleType.Admin);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Event created and approved.", result.Message);
        }

        [Fact]
        public void CreateEvent_Unauthorized_role()
        {
            // Act
            var result = _eventService.CreateEvent(new EventDto(), 1, RoleType.Student);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Unauthorized: Only Admins and Society Heads can create events.", result.Message);
        }

        [Fact]
        public void CreateEvent_Capacity_boundary()
        {
            // Act
            var result = _eventService.CreateEvent(new EventDto { Title = "Ev", Description = "De", EventDate = DateTime.Now.AddDays(1), MaxCapacity = -1, VenueID = 1, SocietyID = 1 }, 1, RoleType.Admin);

            // Assert
            Assert.False(result.Success);
        }

        [Fact]
        public void CreateEvent_Exception_path()
        {
            // Arrange
            var eventObj = new EventDto { Title = "Event", Description = "Desc", EventDate = DateTime.Now.AddDays(1), MaxCapacity = 10, VenueID = 1, SocietyID = 1 };
            _eventRepoMock.Setup(m => m.CreateEvent(eventObj, 1)).Throws(new Exception());

            // Act
            var result = _eventService.CreateEvent(eventObj, 1, RoleType.Admin);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Could not create event. Check venue, society, and capacity.", result.Message);
        }

        [Fact]
        public void CreateEvent_Additional_branch_path_4()
        {
            // Arrange
            var eventObj = new EventDto { Title = "Event", Description = "Desc", EventDate = DateTime.Now.AddDays(1), MaxCapacity = 10, VenueID = 1, SocietyID = 1 };
            
            // Act
            var result = _eventService.CreateEvent(eventObj, 1, RoleType.SocietyHead);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Event submitted for admin approval.", result.Message);
        }

        [Fact]
        public void CreateEvent_Additional_branch_path_5()
        {
            // Act
            var result = _eventService.CreateEvent(new EventDto { Title = "", Description = "Desc", EventDate = DateTime.Now.AddDays(1), MaxCapacity = 10, VenueID = 1, SocietyID = 1 }, 1, RoleType.Admin);

            // Assert
            Assert.False(result.Success);
        }

        [Fact]
        public void CreateEvent_Additional_branch_path_6()
        {
            // Act
            var result = _eventService.CreateEvent(new EventDto { Title = "Ev", Description = "", EventDate = DateTime.Now.AddDays(1), MaxCapacity = 10, VenueID = 1, SocietyID = 1 }, 1, RoleType.Admin);

            // Assert
            Assert.False(result.Success);
        }

        [Fact]
        public void UpdateEvent_Happy_path()
        {
            // Arrange
            var eventObj = new EventDto { Title = "Event", Description = "Desc", EventDate = DateTime.Now.AddDays(1), MaxCapacity = 10, VenueID = 1, SocietyID = 1 };
            _eventRepoMock.Setup(m => m.UpdateEvent(eventObj, 1));

            // Act
            var result = _eventService.UpdateEvent(eventObj, 1, RoleType.Admin);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Event updated.", result.Message);
        }

        [Fact]
        public void UpdateEvent_Unauthorized_role()
        {
            // Act
            var result = _eventService.UpdateEvent(new EventDto(), 1, RoleType.Student);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Unauthorized.", result.Message);
        }

        [Fact]
        public void UpdateEvent_Exception_path()
        {
            // Arrange
            var eventObj = new EventDto { Title = "Event", Description = "Desc", EventDate = DateTime.Now.AddDays(1), MaxCapacity = 10, VenueID = 1, SocietyID = 1 };
            _eventRepoMock.Setup(m => m.UpdateEvent(eventObj, 1)).Throws(new Exception());

            // Act
            var result = _eventService.UpdateEvent(eventObj, 1, RoleType.Admin);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Failed to update event.", result.Message);
        }

        [Fact]
        public void UpdateEvent_Additional_branch_path_3()
        {
            // Arrange
            var eventObj = new EventDto { Title = "Event", Description = "Desc", EventDate = DateTime.Now.AddDays(1), MaxCapacity = 10, VenueID = 1, SocietyID = 1 };
            
            // Act
            var result = _eventService.UpdateEvent(eventObj, 1, RoleType.SocietyHead);

            // Assert
            Assert.True(result.Success);
        }

        [Fact]
        public void UpdateEvent_Additional_branch_path_4()
        {
            // Act
            var result = _eventService.UpdateEvent(new EventDto { Title = "", Description = "Desc", EventDate = DateTime.Now.AddDays(1), MaxCapacity = 10, VenueID = 1, SocietyID = 1 }, 1, RoleType.Admin);

            // Assert
            Assert.False(result.Success);
        }

        [Fact]
        public void UpdateEventStatus_Happy_path()
        {
            // Arrange
            _eventRepoMock.Setup(m => m.UpdateEventStatus(1, (int)EventStatusEnum.Approved, 1));

            // Act
            var result = _eventService.UpdateEventStatus(1, EventStatusEnum.Approved, 1, RoleType.Admin);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Event marked as Approved.", result.Message);
        }

        [Fact]
        public void UpdateEventStatus_Unauthorized_role()
        {
            // Act
            var result = _eventService.UpdateEventStatus(1, EventStatusEnum.Approved, 1, RoleType.Student);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Unauthorized to update event status.", result.Message);
        }

        [Fact]
        public void UpdateEventStatus_Exception_path()
        {
            // Arrange
            _eventRepoMock.Setup(m => m.UpdateEventStatus(1, (int)EventStatusEnum.Approved, 1)).Throws(new Exception());

            // Act
            var result = _eventService.UpdateEventStatus(1, EventStatusEnum.Approved, 1, RoleType.Admin);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Failed to update event status.", result.Message);
        }

        [Fact]
        public void UpdateEventStatus_Additional_branch_path_3()
        {
            // Act
            var result = _eventService.UpdateEventStatus(1, EventStatusEnum.Approved, 1, RoleType.SocietyHead);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Only admins can approve event requests.", result.Message);
        }

        [Fact]
        public void UpdateEventStatus_Additional_branch_path_4()
        {
            // Arrange
            _eventRepoMock.Setup(m => m.UpdateEventStatus(1, (int)EventStatusEnum.Cancelled, 1));

            // Act
            var result = _eventService.UpdateEventStatus(1, EventStatusEnum.Cancelled, 1, RoleType.SocietyHead);

            // Assert
            Assert.True(result.Success);
        }

        [Fact]
        public void UpdateEventStatus_Additional_branch_path_5()
        {
            // Arrange
            _eventRepoMock.Setup(m => m.UpdateEventStatus(1, (int)EventStatusEnum.PendingApproval, 1));

            // Act
            var result = _eventService.UpdateEventStatus(1, EventStatusEnum.PendingApproval, 1, RoleType.Admin);

            // Assert
            Assert.True(result.Success);
        }

        [Fact]
        public void RegisterStudentForEvent_Happy_path()
        {
            // Arrange
            string dbError;
            _eventRepoMock.Setup(m => m.RegisterForEvent(1, 1, 1, out dbError)).Returns(true);

            // Act
            var result = _eventService.RegisterStudentForEvent(1, 1, 1);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Successfully registered for the event.", result.Message);
        }

        [Fact]
        public void RegisterStudentForEvent_Missing_null_value()
        {
            // Act
            var result = _eventService.RegisterStudentForEvent(1, null, 1);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Your student profile is missing. Complete registration first.", result.Message);
        }

        [Fact]
        public void RegisterStudentForEvent_Additional_branch_path_2()
        {
            // Arrange
            string dbError = "No seats left.";
            _eventRepoMock.Setup(m => m.RegisterForEvent(1, 1, 1, out dbError)).Returns(false);

            // Act
            var result = _eventService.RegisterStudentForEvent(1, 1, 1);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("No seats left.", result.Message);
        }
    }
}
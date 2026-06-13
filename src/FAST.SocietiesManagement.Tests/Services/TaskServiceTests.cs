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
    public class TaskServiceTests
    {
        private readonly Mock<ITaskRepository> _taskRepoMock;
        private readonly Mock<IMembershipRepository> _membershipRepoMock;
        private readonly TaskService _taskService;

        public TaskServiceTests()
        {
            _taskRepoMock = new Mock<ITaskRepository>();
            _membershipRepoMock = new Mock<IMembershipRepository>();
            _taskService = new TaskService(_taskRepoMock.Object, _membershipRepoMock.Object);
        }

        [Fact]
        public void TaskService_Happy_path()
        {
            var service = new TaskService(_taskRepoMock.Object, _membershipRepoMock.Object);
            Assert.NotNull(service);
        }

        [Fact]
        public void AssignTask_Happy_path()
        {
            // Arrange
            var task = new TaskDto { Title = "Task", DueDate = DateTime.Now.AddDays(1), AssignedToStudentID = 1, SocietyID = 1 };
            _membershipRepoMock.Setup(m => m.IsApprovedMember(1, 1)).Returns(true);
            _taskRepoMock.Setup(m => m.CreateTask(task, 1));

            // Act
            var result = _taskService.AssignTask(task, 1, RoleType.Admin);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Task assigned to member.", result.Message);
        }

        [Fact]
        public void AssignTask_Blank_string_validation()
        {
            // Act
            var result = _taskService.AssignTask(new TaskDto { Title = "", DueDate = DateTime.Now.AddDays(1) }, 1, RoleType.Admin);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Task title is required.", result.Message);
        }

        [Fact]
        public void AssignTask_Unauthorized_role()
        {
            // Act
            var result = _taskService.AssignTask(new TaskDto { Title = "Task" }, 1, RoleType.Student);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Unauthorized. Only Society Heads or Admins can assign tasks.", result.Message);
        }

        [Fact]
        public void AssignTask_Date_boundary()
        {
            // Act
            var result = _taskService.AssignTask(new TaskDto { Title = "Task", DueDate = DateTime.Now.AddDays(-1) }, 1, RoleType.Admin);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Due date must be in the future.", result.Message);
        }

        [Fact]
        public void AssignTask_Exception_path()
        {
            // Arrange
            var task = new TaskDto { Title = "Task", DueDate = DateTime.Now.AddDays(1), AssignedToStudentID = 1, SocietyID = 1 };
            _membershipRepoMock.Setup(m => m.IsApprovedMember(1, 1)).Returns(true);
            _taskRepoMock.Setup(m => m.CreateTask(task, 1)).Throws(new Exception());

            // Act
            var result = _taskService.AssignTask(task, 1, RoleType.Admin);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Failed to assign task.", result.Message);
        }

        [Fact]
        public void AssignTask_Additional_branch_path_5()
        {
            // Arrange
            var task = new TaskDto { Title = "Task", DueDate = DateTime.Now.AddDays(1), AssignedToStudentID = 1, SocietyID = 1 };
            _membershipRepoMock.Setup(m => m.IsApprovedMember(1, 1)).Returns(false);

            // Act
            var result = _taskService.AssignTask(task, 1, RoleType.Admin);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Tasks can only be assigned to approved society members.", result.Message);
        }

        [Fact]
        public void AssignTask_Additional_branch_path_6()
        {
            // Arrange
            var task = new TaskDto { Title = "Task", DueDate = DateTime.Now.AddDays(1), AssignedToStudentID = 1, SocietyID = 1 };
            _membershipRepoMock.Setup(m => m.IsApprovedMember(1, 1)).Returns(true);
            _taskRepoMock.Setup(m => m.CreateTask(task, 1));

            // Act
            var result = _taskService.AssignTask(task, 1, RoleType.SocietyHead);

            // Assert
            Assert.True(result.Success);
        }

        [Fact]
        public void GetSocietyTasks_Happy_path()
        {
            // Arrange
            _taskRepoMock.Setup(m => m.GetTasksBySociety(1)).Returns(new List<TaskDto> { new TaskDto() });

            // Act
            var result = _taskService.GetSocietyTasks(1);

            // Assert
            Assert.NotEmpty(result);
        }

        [Fact]
        public void GetStudentTasks_Happy_path()
        {
            // Arrange
            _taskRepoMock.Setup(m => m.GetTasksByStudent(1)).Returns(new List<TaskDto> { new TaskDto() });

            // Act
            var result = _taskService.GetStudentTasks(1);

            // Assert
            Assert.NotEmpty(result);
        }

        [Fact]
        public void GetStudentTasks_Missing_null_value()
        {
            // Act
            var result = _taskService.GetStudentTasks(null);

            // Assert
            Assert.Empty(result);
        }
    }
}
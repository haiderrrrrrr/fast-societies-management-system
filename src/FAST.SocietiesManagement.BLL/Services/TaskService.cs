using System;
using System.Collections.Generic;
using FAST.SocietiesManagement.Core.DTOs;
using FAST.SocietiesManagement.Core.Enums;
using FAST.SocietiesManagement.DAL.Interfaces;
using FAST.SocietiesManagement.Core.Utilities;

namespace FAST.SocietiesManagement.BLL.Services
{
    public class TaskService
    {
        private readonly ITaskRepository _taskRepository;
        private readonly IMembershipRepository _membershipRepository;

        public TaskService(ITaskRepository taskRepository, IMembershipRepository membershipRepository)
        {
            _taskRepository = taskRepository;
            _membershipRepository = membershipRepository;
        }

        public (bool Success, string Message) AssignTask(TaskDto task, int assignerUserId, RoleType role)
        {
            if (role != RoleType.SocietyHead && role != RoleType.Admin)
                return (false, "Unauthorized. Only Society Heads or Admins can assign tasks.");

            if (string.IsNullOrWhiteSpace(task.Title)) return (false, "Task title is required.");
            if (task.DueDate <= DateTime.Now) return (false, "Due date must be in the future.");
            if (!_membershipRepository.IsApprovedMember(task.AssignedToStudentID, task.SocietyID))
                return (false, "Tasks can only be assigned to approved society members.");

            try
            {
                _taskRepository.CreateTask(task, assignerUserId);
                Logger.LogInfo($"Task '{task.Title}' assigned to Student {task.AssignedToStudentID}", "TaskService.AssignTask", assignerUserId);
                return (true, "Task assigned to member.");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "TaskService.AssignTask", assignerUserId);
                return (false, "Failed to assign task.");
            }
        }

        public List<TaskDto> GetSocietyTasks(int societyId) => _taskRepository.GetTasksBySociety(societyId);

        public List<TaskDto> GetStudentTasks(int? studentId)
        {
            return studentId.HasValue ? _taskRepository.GetTasksByStudent(studentId.Value) : new List<TaskDto>();
        }
    }
}

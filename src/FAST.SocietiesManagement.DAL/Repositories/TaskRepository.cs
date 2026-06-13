using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using FAST.SocietiesManagement.Core.DTOs;
using FAST.SocietiesManagement.Core.Enums;
using FAST.SocietiesManagement.DAL.Interfaces;

namespace FAST.SocietiesManagement.DAL.Repositories
{
    public class TaskRepository : ITaskRepository
    {
        public void CreateTask(TaskDto task, int createdBy)
        {
            using var connection = DbConnectionProvider.GetConnection();
            var cmd = new SqlCommand(@"
                INSERT INTO Tasks (SocietyID, AssignedToStudentID, Title, Description, DueDate, StatusID, CreatedBy)
                VALUES (@SocietyID, @StudentID, @Title, @Desc, @Date, @StatusID, @CreatedBy)", connection);
            cmd.Parameters.AddWithValue("@SocietyID", task.SocietyID);
            cmd.Parameters.AddWithValue("@StudentID", task.AssignedToStudentID);
            cmd.Parameters.AddWithValue("@Title", task.Title);
            cmd.Parameters.AddWithValue("@Desc", string.IsNullOrWhiteSpace(task.Description) ? DBNull.Value : task.Description);
            cmd.Parameters.AddWithValue("@Date", task.DueDate);
            cmd.Parameters.AddWithValue("@StatusID", (int)TaskStatusEnum.ToDo);
            cmd.Parameters.AddWithValue("@CreatedBy", createdBy);

            connection.Open();
            cmd.ExecuteNonQuery();
        }

        public List<TaskDto> GetTasksBySociety(int societyId)
        {
            return QueryTasks("WHERE t.SocietyID = @SocietyID AND t.IsDeleted = 0", cmd =>
                cmd.Parameters.AddWithValue("@SocietyID", societyId));
        }

        public List<TaskDto> GetTasksByStudent(int studentId)
        {
            return QueryTasks("WHERE t.AssignedToStudentID = @StudentID AND t.IsDeleted = 0", cmd =>
                cmd.Parameters.AddWithValue("@StudentID", studentId));
        }

        private static List<TaskDto> QueryTasks(string whereClause, Action<SqlCommand> addParameters)
        {
            var list = new List<TaskDto>();
            using var connection = DbConnectionProvider.GetConnection();
            var cmd = new SqlCommand($@"
                SELECT t.TaskID, t.SocietyID, t.AssignedToStudentID, sp.FullName, t.Title, COALESCE(t.Description, ''), t.StatusID, t.DueDate
                FROM Tasks t
                INNER JOIN StudentProfiles sp ON t.AssignedToStudentID = sp.StudentID
                {whereClause}
                ORDER BY t.DueDate", connection);
            addParameters?.Invoke(cmd);

            connection.Open();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new TaskDto
                {
                    TaskID = reader.GetInt32(0),
                    SocietyID = reader.GetInt32(1),
                    AssignedToStudentID = reader.GetInt32(2),
                    AssignedToName = reader.GetString(3),
                    Title = reader.GetString(4),
                    Description = reader.GetString(5),
                    Status = (TaskStatusEnum)reader.GetInt32(6),
                    DueDate = reader.GetDateTime(7)
                });
            }
            return list;
        }
    }
}

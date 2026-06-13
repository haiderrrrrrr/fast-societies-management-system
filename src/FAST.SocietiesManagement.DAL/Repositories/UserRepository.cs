using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using FAST.SocietiesManagement.Core.DTOs;
using FAST.SocietiesManagement.Core.Enums;
using FAST.SocietiesManagement.DAL.Interfaces;

namespace FAST.SocietiesManagement.DAL.Repositories
{
    public class UserRepository : IUserRepository
    {
        public UserDto GetUserByUsername(string username)
        {
            using var connection = DbConnectionProvider.GetConnection();
            var command = new SqlCommand(@"
                SELECT u.UserID, u.Username, u.Email, u.PasswordHash, u.Salt, u.RoleID, u.IsActive, u.CreatedAt,
                       sp.StudentID, sp.FullName, sp.RollNumber, sp.Department
                FROM Users u
                LEFT JOIN StudentProfiles sp ON sp.UserID = u.UserID AND sp.IsDeleted = 0
                WHERE u.Username = @Username AND u.IsDeleted = 0", connection);
            command.Parameters.AddWithValue("@Username", username);

            connection.Open();
            using var reader = command.ExecuteReader();
            return reader.Read() ? MapUser(reader) : null;
        }

        public List<UserDto> GetAllUsers()
        {
            var users = new List<UserDto>();
            using var connection = DbConnectionProvider.GetConnection();
            var command = new SqlCommand(@"
                SELECT u.UserID, u.Username, u.Email, u.PasswordHash, u.Salt, u.RoleID, u.IsActive, u.CreatedAt,
                       sp.StudentID, sp.FullName, sp.RollNumber, sp.Department
                FROM Users u
                LEFT JOIN StudentProfiles sp ON sp.UserID = u.UserID AND sp.IsDeleted = 0
                WHERE u.IsDeleted = 0
                ORDER BY u.RoleID, u.Username", connection);

            connection.Open();
            using var reader = command.ExecuteReader();
            while (reader.Read()) users.Add(MapUser(reader));
            return users;
        }

        public List<UserDto> GetUsersByRole(int roleId)
        {
            var users = new List<UserDto>();
            using var connection = DbConnectionProvider.GetConnection();
            var command = new SqlCommand(@"
                SELECT u.UserID, u.Username, u.Email, u.PasswordHash, u.Salt, u.RoleID, u.IsActive, u.CreatedAt,
                       sp.StudentID, sp.FullName, sp.RollNumber, sp.Department
                FROM Users u
                LEFT JOIN StudentProfiles sp ON sp.UserID = u.UserID AND sp.IsDeleted = 0
                WHERE u.IsDeleted = 0 AND u.RoleID = @RoleID
                ORDER BY u.Username", connection);
            command.Parameters.AddWithValue("@RoleID", roleId);

            connection.Open();
            using var reader = command.ExecuteReader();
            while (reader.Read()) users.Add(MapUser(reader));
            return users;
        }

        public StudentProfileDto GetStudentProfileByUserId(int userId)
        {
            using var connection = DbConnectionProvider.GetConnection();
            var command = new SqlCommand(@"
                SELECT StudentID, UserID, FullName, RollNumber, Department, Email
                FROM StudentProfiles
                WHERE UserID = @UserID AND IsDeleted = 0", connection);
            command.Parameters.AddWithValue("@UserID", userId);

            connection.Open();
            using var reader = command.ExecuteReader();
            if (!reader.Read()) return null;

            return new StudentProfileDto
            {
                StudentID = reader.GetInt32(0),
                UserID = reader.GetInt32(1),
                FullName = reader.GetString(2),
                RollNumber = reader.GetString(3),
                Department = reader.GetString(4),
                Email = reader.GetString(5)
            };
        }

        public void CreateUser(UserDto user)
        {
            using var connection = DbConnectionProvider.GetConnection();
            var command = new SqlCommand(@"
                INSERT INTO Users (Username, Email, PasswordHash, Salt, RoleID, IsActive)
                VALUES (@Username, @Email, @Hash, @Salt, @RoleID, @IsActive)", connection);
            command.Parameters.AddWithValue("@Username", user.Username);
            command.Parameters.AddWithValue("@Email", string.IsNullOrWhiteSpace(user.Email) ? DBNull.Value : user.Email);
            command.Parameters.AddWithValue("@Hash", user.PasswordHash);
            command.Parameters.AddWithValue("@Salt", user.Salt);
            command.Parameters.AddWithValue("@RoleID", (int)user.Role);
            command.Parameters.AddWithValue("@IsActive", user.IsActive);

            connection.Open();
            command.ExecuteNonQuery();
        }

        public void CreateStudentAccount(UserDto user, StudentProfileDto profile)
        {
            using var connection = DbConnectionProvider.GetConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                var userCommand = new SqlCommand(@"
                    INSERT INTO Users (Username, Email, PasswordHash, Salt, RoleID, IsActive)
                    OUTPUT INSERTED.UserID
                    VALUES (@Username, @Email, @Hash, @Salt, @RoleID, @IsActive)", connection, transaction);
                userCommand.Parameters.AddWithValue("@Username", user.Username);
                userCommand.Parameters.AddWithValue("@Email", profile.Email);
                userCommand.Parameters.AddWithValue("@Hash", user.PasswordHash);
                userCommand.Parameters.AddWithValue("@Salt", user.Salt);
                userCommand.Parameters.AddWithValue("@RoleID", (int)RoleType.Student);
                userCommand.Parameters.AddWithValue("@IsActive", true);

                int userId = (int)userCommand.ExecuteScalar();

                var profileCommand = new SqlCommand(@"
                    INSERT INTO StudentProfiles (UserID, FullName, RollNumber, Department, Email, CreatedBy)
                    VALUES (@UserID, @FullName, @RollNumber, @Department, @Email, @CreatedBy)", connection, transaction);
                profileCommand.Parameters.AddWithValue("@UserID", userId);
                profileCommand.Parameters.AddWithValue("@FullName", profile.FullName);
                profileCommand.Parameters.AddWithValue("@RollNumber", profile.RollNumber);
                profileCommand.Parameters.AddWithValue("@Department", profile.Department);
                profileCommand.Parameters.AddWithValue("@Email", profile.Email);
                profileCommand.Parameters.AddWithValue("@CreatedBy", userId);
                profileCommand.ExecuteNonQuery();

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public void UpdateUserActiveStatus(int userId, bool isActive, int updatedBy)
        {
            using var connection = DbConnectionProvider.GetConnection();
            var command = new SqlCommand(@"
                UPDATE Users
                SET IsActive = @IsActive, UpdatedAt = GETDATE(), UpdatedBy = @UpdatedBy
                WHERE UserID = @UserID AND IsDeleted = 0", connection);
            command.Parameters.AddWithValue("@IsActive", isActive);
            command.Parameters.AddWithValue("@UpdatedBy", updatedBy);
            command.Parameters.AddWithValue("@UserID", userId);

            connection.Open();
            command.ExecuteNonQuery();
        }

        public void SoftDeleteUser(int userId, int updatedBy)
        {
            using var connection = DbConnectionProvider.GetConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                var profileCommand = new SqlCommand(@"
                    UPDATE StudentProfiles
                    SET IsDeleted = 1, UpdatedAt = GETDATE(), UpdatedBy = @UpdatedBy
                    WHERE UserID = @UserID AND IsDeleted = 0", connection, transaction);
                profileCommand.Parameters.AddWithValue("@UpdatedBy", updatedBy);
                profileCommand.Parameters.AddWithValue("@UserID", userId);
                profileCommand.ExecuteNonQuery();

                var userCommand = new SqlCommand(@"
                    UPDATE Users
                    SET IsDeleted = 1, IsActive = 0, UpdatedAt = GETDATE(), UpdatedBy = @UpdatedBy
                    WHERE UserID = @UserID AND IsDeleted = 0", connection, transaction);
                userCommand.Parameters.AddWithValue("@UpdatedBy", updatedBy);
                userCommand.Parameters.AddWithValue("@UserID", userId);
                userCommand.ExecuteNonQuery();

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public bool IsUsernameTaken(string username)
        {
            return Exists("SELECT 1 FROM Users WHERE Username = @Value AND IsDeleted = 0", username);
        }

        public bool IsEmailTaken(string email)
        {
            return Exists("SELECT 1 FROM Users WHERE Email = @Value AND IsDeleted = 0", email);
        }

        public bool IsRollNumberTaken(string rollNumber)
        {
            return Exists("SELECT 1 FROM StudentProfiles WHERE RollNumber = @Value AND IsDeleted = 0", rollNumber);
        }

        private bool Exists(string sql, string value)
        {
            using var connection = DbConnectionProvider.GetConnection();
            var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@Value", value);
            connection.Open();
            return command.ExecuteScalar() != null;
        }

        private static UserDto MapUser(SqlDataReader reader)
        {
            return new UserDto
            {
                UserID = reader.GetInt32(0),
                Username = reader.GetString(1),
                Email = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                PasswordHash = reader.GetString(3),
                Salt = reader.GetString(4),
                Role = (RoleType)reader.GetInt32(5),
                IsActive = reader.GetBoolean(6),
                CreatedAt = reader.GetDateTime(7),
                StudentID = reader.IsDBNull(8) ? null : reader.GetInt32(8),
                FullName = reader.IsDBNull(9) ? string.Empty : reader.GetString(9),
                RollNumber = reader.IsDBNull(10) ? string.Empty : reader.GetString(10),
                Department = reader.IsDBNull(11) ? string.Empty : reader.GetString(11)
            };
        }
    }
}

using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using FAST.SocietiesManagement.Core.DTOs;
using FAST.SocietiesManagement.Core.Enums;
using FAST.SocietiesManagement.DAL.Interfaces;

namespace FAST.SocietiesManagement.DAL.Repositories
{
    public class SocietyRepository : ISocietyRepository
    {
        public List<SocietyDto> GetAllSocieties()
        {
            return QuerySocieties("WHERE s.IsDeleted = 0", null);
        }

        public List<SocietyDto> GetActiveSocieties()
        {
            return QuerySocieties("WHERE s.IsDeleted = 0 AND s.StatusID = @StatusID", cmd =>
                cmd.Parameters.AddWithValue("@StatusID", (int)SocietyStatusEnum.Active));
        }

        public List<SocietyDto> GetSocietiesByHeadUserId(int headUserId)
        {
            return QuerySocieties("WHERE s.IsDeleted = 0 AND s.HeadUserID = @HeadUserID", cmd =>
                cmd.Parameters.AddWithValue("@HeadUserID", headUserId));
        }

        public SocietyDto GetSocietyById(int societyId)
        {
            var societies = QuerySocieties("WHERE s.IsDeleted = 0 AND s.SocietyID = @SocietyID", cmd =>
                cmd.Parameters.AddWithValue("@SocietyID", societyId));
            return societies.Count == 0 ? null : societies[0];
        }

        public void CreateSociety(SocietyDto society, int createdBy)
        {
            using var connection = DbConnectionProvider.GetConnection();
            var cmd = new SqlCommand(@"
                INSERT INTO Societies (Name, Description, HeadUserID, StatusID, CreatedBy)
                VALUES (@Name, @Desc, @HeadId, @StatusId, @CreatedBy)", connection);
            cmd.Parameters.AddWithValue("@Name", society.Name);
            cmd.Parameters.AddWithValue("@Desc", society.Description);
            cmd.Parameters.AddWithValue("@HeadId", society.HeadUserID.HasValue ? society.HeadUserID.Value : DBNull.Value);
            cmd.Parameters.AddWithValue("@StatusId", (int)society.Status);
            cmd.Parameters.AddWithValue("@CreatedBy", createdBy);

            connection.Open();
            cmd.ExecuteNonQuery();
        }

        public void UpdateSociety(SocietyDto society, int updatedBy)
        {
            using var connection = DbConnectionProvider.GetConnection();
            var cmd = new SqlCommand(@"
                UPDATE Societies
                SET Name = @Name,
                    Description = @Desc,
                    HeadUserID = @HeadId,
                    UpdatedBy = @UpdatedBy,
                    UpdatedAt = GETDATE()
                WHERE SocietyID = @SocietyID AND IsDeleted = 0", connection);
            cmd.Parameters.AddWithValue("@Name", society.Name);
            cmd.Parameters.AddWithValue("@Desc", society.Description);
            cmd.Parameters.AddWithValue("@HeadId", society.HeadUserID.HasValue ? society.HeadUserID.Value : DBNull.Value);
            cmd.Parameters.AddWithValue("@UpdatedBy", updatedBy);
            cmd.Parameters.AddWithValue("@SocietyID", society.SocietyID);

            connection.Open();
            cmd.ExecuteNonQuery();
        }

        public void UpdateSocietyStatus(int societyId, int statusId, int updatedBy)
        {
            using var connection = DbConnectionProvider.GetConnection();
            var cmd = new SqlCommand(@"
                UPDATE Societies
                SET StatusID = @StatusId, UpdatedBy = @UpdatedBy, UpdatedAt = GETDATE()
                WHERE SocietyID = @SocietyId AND IsDeleted = 0", connection);
            cmd.Parameters.AddWithValue("@StatusId", statusId);
            cmd.Parameters.AddWithValue("@UpdatedBy", updatedBy);
            cmd.Parameters.AddWithValue("@SocietyId", societyId);

            connection.Open();
            cmd.ExecuteNonQuery();
        }

        public void UpdateSocietyHead(int societyId, int? headUserId, int updatedBy)
        {
            using var connection = DbConnectionProvider.GetConnection();
            var cmd = new SqlCommand(@"
                UPDATE Societies
                SET HeadUserID = @HeadUserID, UpdatedBy = @UpdatedBy, UpdatedAt = GETDATE()
                WHERE SocietyID = @SocietyId AND IsDeleted = 0", connection);
            cmd.Parameters.AddWithValue("@HeadUserID", headUserId.HasValue ? headUserId.Value : DBNull.Value);
            cmd.Parameters.AddWithValue("@UpdatedBy", updatedBy);
            cmd.Parameters.AddWithValue("@SocietyId", societyId);

            connection.Open();
            cmd.ExecuteNonQuery();
        }

        public void SoftDeleteSociety(int societyId, int updatedBy)
        {
            using var connection = DbConnectionProvider.GetConnection();
            var cmd = new SqlCommand(@"
                UPDATE Societies
                SET IsDeleted = 1,
                    StatusID = @SuspendedStatus,
                    UpdatedBy = @UpdatedBy,
                    UpdatedAt = GETDATE()
                WHERE SocietyID = @SocietyID", connection);
            cmd.Parameters.AddWithValue("@SuspendedStatus", (int)SocietyStatusEnum.Suspended);
            cmd.Parameters.AddWithValue("@UpdatedBy", updatedBy);
            cmd.Parameters.AddWithValue("@SocietyID", societyId);

            connection.Open();
            cmd.ExecuteNonQuery();
        }

        private static List<SocietyDto> QuerySocieties(string whereClause, Action<SqlCommand> addParameters)
        {
            var societies = new List<SocietyDto>();
            using var connection = DbConnectionProvider.GetConnection();
            var cmd = new SqlCommand($@"
                SELECT s.SocietyID, s.Name, s.Description, s.HeadUserID, COALESCE(u.Username, ''), s.StatusID, s.CreatedAt
                FROM Societies s
                LEFT JOIN Users u ON u.UserID = s.HeadUserID
                {whereClause}
                ORDER BY s.Name", connection);
            addParameters?.Invoke(cmd);

            connection.Open();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                societies.Add(new SocietyDto
                {
                    SocietyID = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Description = reader.GetString(2),
                    HeadUserID = reader.IsDBNull(3) ? null : reader.GetInt32(3),
                    HeadName = reader.GetString(4),
                    Status = (SocietyStatusEnum)reader.GetInt32(5),
                    CreatedAt = reader.GetDateTime(6)
                });
            }
            return societies;
        }
    }
}

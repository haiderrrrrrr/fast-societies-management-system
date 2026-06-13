using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using FAST.SocietiesManagement.Core.DTOs;
using FAST.SocietiesManagement.Core.Enums;
using FAST.SocietiesManagement.DAL.Interfaces;

namespace FAST.SocietiesManagement.DAL.Repositories
{
    public class MembershipRepository : IMembershipRepository
    {
        public void RequestMembership(int studentId, int societyId, int createdBy)
        {
            using var connection = DbConnectionProvider.GetConnection();
            var cmd = new SqlCommand(@"
                INSERT INTO Memberships (StudentID, SocietyID, StatusID, Role, CreatedBy)
                VALUES (@StudentID, @SocietyID, @StatusID, 'Member', @CreatedBy)", connection);
            cmd.Parameters.AddWithValue("@StudentID", studentId);
            cmd.Parameters.AddWithValue("@SocietyID", societyId);
            cmd.Parameters.AddWithValue("@StatusID", (int)MembershipStatusEnum.Pending);
            cmd.Parameters.AddWithValue("@CreatedBy", createdBy);

            connection.Open();
            cmd.ExecuteNonQuery();
        }

        public List<MembershipDto> GetPendingRequests(int societyId)
        {
            return QueryMemberships("WHERE m.SocietyID = @SocietyID AND m.StatusID = @StatusID AND m.IsDeleted = 0", cmd =>
            {
                cmd.Parameters.AddWithValue("@SocietyID", societyId);
                cmd.Parameters.AddWithValue("@StatusID", (int)MembershipStatusEnum.Pending);
            });
        }

        public List<MembershipDto> GetMembershipsByStudent(int studentId)
        {
            return QueryMemberships("WHERE m.StudentID = @StudentID AND m.IsDeleted = 0", cmd =>
                cmd.Parameters.AddWithValue("@StudentID", studentId));
        }

        public List<MembershipDto> GetApprovedMembers(int societyId)
        {
            return QueryMemberships("WHERE m.SocietyID = @SocietyID AND m.StatusID = @StatusID AND m.IsDeleted = 0", cmd =>
            {
                cmd.Parameters.AddWithValue("@SocietyID", societyId);
                cmd.Parameters.AddWithValue("@StatusID", (int)MembershipStatusEnum.Approved);
            });
        }

        public bool IsApprovedMember(int studentId, int societyId)
        {
            using var connection = DbConnectionProvider.GetConnection();
            var cmd = new SqlCommand(@"
                SELECT 1
                FROM Memberships
                WHERE StudentID = @StudentID AND SocietyID = @SocietyID AND StatusID = @StatusID AND IsDeleted = 0", connection);
            cmd.Parameters.AddWithValue("@StudentID", studentId);
            cmd.Parameters.AddWithValue("@SocietyID", societyId);
            cmd.Parameters.AddWithValue("@StatusID", (int)MembershipStatusEnum.Approved);

            connection.Open();
            return cmd.ExecuteScalar() != null;
        }

        public void UpdateMembershipStatus(int membershipId, int statusId, int updatedBy)
        {
            using var connection = DbConnectionProvider.GetConnection();
            var cmd = new SqlCommand(@"
                UPDATE Memberships
                SET StatusID = @StatusID,
                    JoinedDate = CASE WHEN @StatusID = @ApprovedStatus THEN GETDATE() ELSE JoinedDate END,
                    UpdatedBy = @UpdatedBy,
                    UpdatedAt = GETDATE()
                WHERE MembershipID = @MembershipID AND IsDeleted = 0", connection);
            cmd.Parameters.AddWithValue("@StatusID", statusId);
            cmd.Parameters.AddWithValue("@ApprovedStatus", (int)MembershipStatusEnum.Approved);
            cmd.Parameters.AddWithValue("@UpdatedBy", updatedBy);
            cmd.Parameters.AddWithValue("@MembershipID", membershipId);

            connection.Open();
            cmd.ExecuteNonQuery();
        }

        private static List<MembershipDto> QueryMemberships(string whereClause, Action<SqlCommand> addParameters)
        {
            var list = new List<MembershipDto>();
            using var connection = DbConnectionProvider.GetConnection();
            var cmd = new SqlCommand($@"
                SELECT m.MembershipID, m.StudentID, sp.FullName, m.SocietyID, so.Name, m.StatusID, m.Role, m.CreatedAt
                FROM Memberships m
                INNER JOIN StudentProfiles sp ON m.StudentID = sp.StudentID
                INNER JOIN Societies so ON so.SocietyID = m.SocietyID
                {whereClause}
                ORDER BY m.CreatedAt DESC", connection);
            addParameters?.Invoke(cmd);

            connection.Open();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new MembershipDto
                {
                    MembershipID = reader.GetInt32(0),
                    StudentID = reader.GetInt32(1),
                    StudentName = reader.GetString(2),
                    SocietyID = reader.GetInt32(3),
                    SocietyName = reader.GetString(4),
                    Status = (MembershipStatusEnum)reader.GetInt32(5),
                    Role = reader.GetString(6),
                    RequestDate = reader.GetDateTime(7)
                });
            }
            return list;
        }
    }
}

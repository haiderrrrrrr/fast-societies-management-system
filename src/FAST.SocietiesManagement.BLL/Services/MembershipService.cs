using System.Collections.Generic;
using FAST.SocietiesManagement.Core.DTOs;
using FAST.SocietiesManagement.Core.Enums;
using FAST.SocietiesManagement.DAL.Interfaces;
using FAST.SocietiesManagement.Core.Utilities;

namespace FAST.SocietiesManagement.BLL.Services
{
    public class MembershipService
    {
        private readonly IMembershipRepository _membershipRepository;

        public MembershipService(IMembershipRepository membershipRepository)
        {
            _membershipRepository = membershipRepository;
        }

        public (bool Success, string Message) ApplyForMembership(int? studentId, int societyId, int userId)
        {
            if (!studentId.HasValue) return (false, "Student profile not found. Register as a student first.");
            try
            {
                _membershipRepository.RequestMembership(studentId.Value, societyId, userId);
                Logger.LogInfo($"Student {studentId} applied for society {societyId}.", "MembershipService.ApplyForMembership", userId);
                return (true, "Membership request submitted successfully.");
            }
            catch (Microsoft.Data.SqlClient.SqlException ex)
            {
                if (ex.Number == 2627 || ex.Number == 2601)
                    return (false, "You have already applied to or joined this society.");

                Logger.LogError(ex, "MembershipService.ApplyForMembership", userId);
                return (false, "Database error occurred while submitting membership.");
            }
        }

        public List<MembershipDto> GetPendingRequests(int societyId, RoleType requestingRole)
        {
            if (requestingRole != RoleType.SocietyHead && requestingRole != RoleType.Admin)
                return new List<MembershipDto>();
            return _membershipRepository.GetPendingRequests(societyId);
        }

        public List<MembershipDto> GetMembershipsByStudent(int? studentId)
        {
            return studentId.HasValue ? _membershipRepository.GetMembershipsByStudent(studentId.Value) : new List<MembershipDto>();
        }

        public List<MembershipDto> GetApprovedMembers(int societyId, RoleType requestingRole)
        {
            if (requestingRole != RoleType.SocietyHead && requestingRole != RoleType.Admin)
                return new List<MembershipDto>();
            return _membershipRepository.GetApprovedMembers(societyId);
        }

        public bool IsApprovedMember(int studentId, int societyId)
        {
            return _membershipRepository.IsApprovedMember(studentId, societyId);
        }

        public (bool Success, string Message) DecisionOnMembership(int membershipId, bool isApprove, int updatedBy, RoleType requestingRole)
        {
            if (requestingRole != RoleType.SocietyHead && requestingRole != RoleType.Admin)
                return (false, "Unauthorized to approve members.");

            int statusId = isApprove ? (int)MembershipStatusEnum.Approved : (int)MembershipStatusEnum.Rejected;

            try
            {
                _membershipRepository.UpdateMembershipStatus(membershipId, statusId, updatedBy);
                Logger.LogInfo($"Membership {membershipId} {(isApprove ? "Approved" : "Rejected")}.", "MembershipService.Decision", updatedBy);
                return (true, $"Membership {(isApprove ? "approved" : "rejected")}.");
            }
            catch (System.Exception ex)
            {
                Logger.LogError(ex, "MembershipService.Decision", updatedBy);
                return (false, "Failed to update membership status.");
            }
        }
    }
}

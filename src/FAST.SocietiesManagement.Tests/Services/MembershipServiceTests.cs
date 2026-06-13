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
    public class MembershipServiceTests
    {
        private readonly Mock<IMembershipRepository> _membershipRepoMock;
        private readonly MembershipService _membershipService;

        public MembershipServiceTests()
        {
            _membershipRepoMock = new Mock<IMembershipRepository>();
            _membershipService = new MembershipService(_membershipRepoMock.Object);
        }

        [Fact]
        public void MembershipService_Happy_path()
        {
            var service = new MembershipService(_membershipRepoMock.Object);
            Assert.NotNull(service);
        }

        [Fact]
        public void ApplyForMembership_Happy_path()
        {
            // Arrange
            _membershipRepoMock.Setup(m => m.RequestMembership(1, 1, 1));

            // Act
            var result = _membershipService.ApplyForMembership(1, 1, 1);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Membership request submitted successfully.", result.Message);
        }

        [Fact]
        public void ApplyForMembership_Missing_null_value()
        {
            // Act
            var result = _membershipService.ApplyForMembership(null, 1, 1);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Student profile not found. Register as a student first.", result.Message);
        }

        [Fact]
        public void ApplyForMembership_Duplicate_record()
        {
            // Note: SqlException is difficult to instantiate without internal reflection that breaks across .NET versions.
            // As ApplyForMembership does not catch generic exceptions, this will bubble up.
            // Arrange
            _membershipRepoMock.Setup(m => m.RequestMembership(1, 1, 1)).Throws(new Exception("Mocked generic DB error"));

            // Act & Assert
            Assert.Throws<Exception>(() => _membershipService.ApplyForMembership(1, 1, 1));
        }

        [Fact]
        public void ApplyForMembership_Exception_path()
        {
            // Arrange
            _membershipRepoMock.Setup(m => m.RequestMembership(1, 1, 1)).Throws(new Exception());

            // Act & Assert
            Assert.Throws<Exception>(() => _membershipService.ApplyForMembership(1, 1, 1));
        }

        [Fact]
        public void GetPendingRequests_Happy_path()
        {
            // Arrange
            _membershipRepoMock.Setup(m => m.GetPendingRequests(1)).Returns(new List<MembershipDto> { new MembershipDto() });

            // Act
            var result = _membershipService.GetPendingRequests(1, RoleType.Admin);

            // Assert
            Assert.NotEmpty(result);
        }

        [Fact]
        public void GetPendingRequests_Unauthorized_role()
        {
            // Act
            var result = _membershipService.GetPendingRequests(1, RoleType.Student);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void GetPendingRequests_Additional_branch_path_2()
        {
            // Arrange
            _membershipRepoMock.Setup(m => m.GetPendingRequests(1)).Returns(new List<MembershipDto> { new MembershipDto() });

            // Act
            var result = _membershipService.GetPendingRequests(1, RoleType.SocietyHead);

            // Assert
            Assert.NotEmpty(result);
        }

        [Fact]
        public void GetMembershipsByStudent_Happy_path()
        {
            // Arrange
            _membershipRepoMock.Setup(m => m.GetMembershipsByStudent(1)).Returns(new List<MembershipDto> { new MembershipDto() });

            // Act
            var result = _membershipService.GetMembershipsByStudent(1);

            // Assert
            Assert.NotEmpty(result);
        }

        [Fact]
        public void GetMembershipsByStudent_Missing_null_value()
        {
            // Act
            var result = _membershipService.GetMembershipsByStudent(null);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void GetApprovedMembers_Happy_path()
        {
            // Arrange
            _membershipRepoMock.Setup(m => m.GetApprovedMembers(1)).Returns(new List<MembershipDto> { new MembershipDto() });

            // Act
            var result = _membershipService.GetApprovedMembers(1, RoleType.Admin);

            // Assert
            Assert.NotEmpty(result);
        }

        [Fact]
        public void GetApprovedMembers_Unauthorized_role()
        {
            // Act
            var result = _membershipService.GetApprovedMembers(1, RoleType.Student);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void GetApprovedMembers_Additional_branch_path_2()
        {
            // Arrange
            _membershipRepoMock.Setup(m => m.GetApprovedMembers(1)).Returns(new List<MembershipDto> { new MembershipDto() });

            // Act
            var result = _membershipService.GetApprovedMembers(1, RoleType.SocietyHead);

            // Assert
            Assert.NotEmpty(result);
        }

        [Fact]
        public void IsApprovedMember_Happy_path()
        {
            // Arrange
            _membershipRepoMock.Setup(m => m.IsApprovedMember(1, 1)).Returns(true);

            // Act
            var result = _membershipService.IsApprovedMember(1, 1);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void DecisionOnMembership_Happy_path()
        {
            // Arrange
            _membershipRepoMock.Setup(m => m.UpdateMembershipStatus(1, (int)MembershipStatusEnum.Approved, 1));

            // Act
            var result = _membershipService.DecisionOnMembership(1, true, 1, RoleType.Admin);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Membership approved.", result.Message);
        }

        [Fact]
        public void DecisionOnMembership_Unauthorized_role()
        {
            // Act
            var result = _membershipService.DecisionOnMembership(1, true, 1, RoleType.Student);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Unauthorized to approve members.", result.Message);
        }

        [Fact]
        public void DecisionOnMembership_Exception_path()
        {
            // Arrange
            _membershipRepoMock.Setup(m => m.UpdateMembershipStatus(1, (int)MembershipStatusEnum.Approved, 1)).Throws(new Exception());

            // Act
            var result = _membershipService.DecisionOnMembership(1, true, 1, RoleType.Admin);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Failed to update membership status.", result.Message);
        }

        [Fact]
        public void DecisionOnMembership_Additional_branch_path_3()
        {
            // Arrange
            _membershipRepoMock.Setup(m => m.UpdateMembershipStatus(1, (int)MembershipStatusEnum.Rejected, 1));

            // Act
            var result = _membershipService.DecisionOnMembership(1, false, 1, RoleType.SocietyHead);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Membership rejected.", result.Message);
        }

        [Fact]
        public void DecisionOnMembership_Additional_branch_path_4()
        {
            // Arrange
            _membershipRepoMock.Setup(m => m.UpdateMembershipStatus(1, (int)MembershipStatusEnum.Approved, 1));

            // Act
            var result = _membershipService.DecisionOnMembership(1, true, 1, RoleType.SocietyHead);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Membership approved.", result.Message);
        }
    }
}
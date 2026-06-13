using System;
using System.Collections.Generic;
using FAST.SocietiesManagement.Core.DTOs;
using FAST.SocietiesManagement.Core.Enums;
using FAST.SocietiesManagement.DAL.Interfaces;
using FAST.SocietiesManagement.BLL.Validators;
using FAST.SocietiesManagement.Core.Utilities;

namespace FAST.SocietiesManagement.BLL.Services
{
    public class SocietyService
    {
        private readonly ISocietyRepository _societyRepository;
        private readonly SocietyValidator _validator;

        public SocietyService(ISocietyRepository societyRepository)
        {
            _societyRepository = societyRepository;
            _validator = new SocietyValidator();
        }

        public List<SocietyDto> GetAllSocieties(RoleType requesterRole)
        {
            return requesterRole == RoleType.Admin ? _societyRepository.GetAllSocieties() : _societyRepository.GetActiveSocieties();
        }

        public List<SocietyDto> GetActiveSocieties()
        {
            return _societyRepository.GetActiveSocieties();
        }

        public List<SocietyDto> GetManagedSocieties(int headUserId)
        {
            return _societyRepository.GetSocietiesByHeadUserId(headUserId);
        }

        public (bool Success, string Message) CreateSociety(SocietyDto society, int creatorUserId, RoleType creatorRole)
        {
            if (creatorRole != RoleType.Admin)
                return (false, "Unauthorized: Only Admins can create societies.");

            var validation = _validator.ValidateSocietyCreation(society);
            if (!validation.IsValid) return (false, validation.ErrorMessage);

            society.Status = SocietyStatusEnum.Active;

            try
            {
                _societyRepository.CreateSociety(society, creatorUserId);
                Logger.LogInfo($"Society '{society.Name}' created.", "SocietyService.CreateSociety", creatorUserId);
                return (true, "Society successfully created.");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "SocietyService.CreateSociety", creatorUserId);
                return (false, "Database Error: a society with this name might already exist.");
            }
        }

        public (bool Success, string Message) AssignHead(int societyId, int? headUserId, int updatedBy, RoleType requesterRole)
        {
            if (requesterRole != RoleType.Admin) return (false, "Only admins can assign society heads.");
            try
            {
                _societyRepository.UpdateSocietyHead(societyId, headUserId, updatedBy);
                return (true, "Society head updated.");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "SocietyService.AssignHead", updatedBy);
                return (false, "Failed to assign society head.");
            }
        }

        public (bool Success, string Message) UpdateSociety(SocietyDto society, int updatedBy, RoleType requesterRole)
        {
            if (requesterRole != RoleType.Admin && requesterRole != RoleType.SocietyHead)
                return (false, "Unauthorized to update society profile.");

            var validation = _validator.ValidateSocietyCreation(society);
            if (!validation.IsValid) return (false, validation.ErrorMessage);

            try
            {
                _societyRepository.UpdateSociety(society, updatedBy);
                return (true, "Society profile updated.");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "SocietyService.UpdateSociety", updatedBy);
                return (false, "Failed to update society profile.");
            }
        }

        public (bool Success, string Message) UpdateStatus(int societyId, SocietyStatusEnum status, int updatedBy, RoleType requesterRole)
        {
            if (requesterRole != RoleType.Admin) return (false, "Only admins can update society status.");
            try
            {
                _societyRepository.UpdateSocietyStatus(societyId, (int)status, updatedBy);
                return (true, $"Society marked as {status}.");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "SocietyService.UpdateStatus", updatedBy);
                return (false, "Failed to update society status.");
            }
        }

        public (bool Success, string Message) DeleteSociety(int societyId, int updatedBy, RoleType requesterRole)
        {
            if (requesterRole != RoleType.Admin) return (false, "Only admins can delete societies.");
            try
            {
                _societyRepository.SoftDeleteSociety(societyId, updatedBy);
                return (true, "Society deleted using safe soft-delete.");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "SocietyService.DeleteSociety", updatedBy);
                return (false, "Failed to delete society.");
            }
        }
    }
}

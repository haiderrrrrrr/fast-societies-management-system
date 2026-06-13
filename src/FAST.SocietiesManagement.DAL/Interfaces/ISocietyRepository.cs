using System.Collections.Generic;
using FAST.SocietiesManagement.Core.DTOs;

namespace FAST.SocietiesManagement.DAL.Interfaces
{
    public interface ISocietyRepository
    {
        List<SocietyDto> GetAllSocieties();
        List<SocietyDto> GetActiveSocieties();
        List<SocietyDto> GetSocietiesByHeadUserId(int headUserId);
        SocietyDto GetSocietyById(int societyId);
        void CreateSociety(SocietyDto society, int createdBy);
        void UpdateSociety(SocietyDto society, int updatedBy);
        void UpdateSocietyStatus(int societyId, int statusId, int updatedBy);
        void UpdateSocietyHead(int societyId, int? headUserId, int updatedBy);
        void SoftDeleteSociety(int societyId, int updatedBy);
    }
}

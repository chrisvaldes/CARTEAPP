using SYSGES_MAGs.Models;
using SYSGES_MAGs.Models.ModelsDto;

namespace SYSGES_MAGs.Services.IServices
{
    public interface IProfileService
    {
        Task<ProfilDto> SaveAsync(ProfilDto profil);
        Task<ProfilDto> UpdateAsync(Profil profil);
        Task<bool> DeleteAsync(Guid id);
        Task<ServiceResult<Profil>> GetByIdAsync(Guid id);
        Task<IEnumerable<ProfilDto>> GetAll();
        public Task<Profil> GetByUserAg(string userAg);
    }
}

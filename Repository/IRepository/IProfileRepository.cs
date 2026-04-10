using SYSGES_MAGs.Models;
using SYSGES_MAGs.Models.ModelsDto;

namespace SYSGES_MAGs.Repository.IRepository
{
    public interface IProfileRepository
    {
        Task<Profil> SaveAsync(Profil profil);
        Task<ProfilDto> UpdateAsync(Profil profil);
        Task<bool> DeleteAsync(Guid id);
        Task<Profil?> GetByIdAsync(Guid id);
        Task<Profil> GetByUserAgAsync(string userAg);
        Task<IEnumerable<Profil>> GetAll();
    }
}

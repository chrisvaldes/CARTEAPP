using SYSGES_MAGs.Models;
using SYSGES_MAGs.Models.ModelsDto;

namespace SYSGES_MAGs.Services.IServices
{
    public interface IBkmvtiService
    {
        public Task<List<Bkmvti>> BkmvtisByMagType(Guid typeMagId);
        public Task<List<BkmvtiSyntheseDto>> GetSyntheseAsync(Guid typeMagId);
    }
}

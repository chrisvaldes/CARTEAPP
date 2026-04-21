using SYSGES_MAGs.Models;
using SYSGES_MAGs.Models.ModelsDto;

namespace SYSGES_MAGs.Services.IServices
{
    public interface IBkmvtiService
    {
        public Task<List<BkmvtiResult>> BkmvtisByMagType(Guid typeMagId);
        public Task<List<BkmvtiSyntheseDto>> GetSyntheseAsync(Guid typeMagId);
    }
}

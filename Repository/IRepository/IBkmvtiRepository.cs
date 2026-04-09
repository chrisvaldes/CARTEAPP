using SYSGES_MAGs.Models;
using SYSGES_MAGs.Models.ModelsDto;

namespace SYSGES_MAGs.Repository.IRepository
{
    public interface IBkmvtiRepository
    {
        public Task<List<Bkmvti>> BkmvtisByMagType(Guid typeMagId);
        Task<List<BkmvtiSyntheseDto>> GetSyntheseByTypeMagAsync(Guid typeMagId);
        public Task<List<Bkmvti>> SaveBkmvtiAsync(List<Bkmvti> bkmvtis);
    }
}

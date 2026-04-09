using SYSGES_MAGs.Models;
using SYSGES_MAGs.Models.ModelsDto;

namespace SYSGES_MAGs.Repository.IRepository
{
    public interface ITypeMagRepository
    {
        public Task<TypeMag> SaveTypeMagAsync(TypeMag typeMag);
        public Task<IEnumerable<TypeMag>> getAllMag();
        public Task<TypeMagWithSyntheseDto> GetTypeMagWithSyntheseAsync(Guid typeMagId);
        public Task<TypeMag?> IsTypeMagExist(DateTime startPeriod);
    }
}

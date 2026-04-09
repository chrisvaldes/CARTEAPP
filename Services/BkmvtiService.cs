using SYSGES_MAGs.Models;
using SYSGES_MAGs.Models.ModelsDto;
using SYSGES_MAGs.Repository.IRepository;
using SYSGES_MAGs.Services.IServices;

namespace SYSGES_MAGs.Services
{
    public class BkmvtiService : IBkmvtiService
    {
        private readonly IBkmvtiRepository _bkmvtiRepository;
        public BkmvtiService(IBkmvtiRepository bkmvtiRepository) {
            _bkmvtiRepository = bkmvtiRepository;
        }
        public async Task<List<Bkmvti>> BkmvtisByMagType(Guid typeMagId)
        {
            return await _bkmvtiRepository.BkmvtisByMagType(typeMagId);
        }
        public async Task<List<BkmvtiSyntheseDto>> GetSyntheseAsync(Guid typeMagId)
        {
            return await _bkmvtiRepository.GetSyntheseByTypeMagAsync(typeMagId);
        }

        //public async Task<TypeMagWithSyntheseDto> GetTypeMagWithSynthese(Guid typeMagId)
        //{
        //    return await _bkmvtiRepository.GetTypeMagWithSyntheseAsync(typeMagId);
        //}
    }
}

using SYSGES_MAGs.Models;
using SYSGES_MAGs.Models.ModelsDto;

namespace SYSGES_MAGs.Repository.IRepository
{
    public interface IBkPrdCliRepository
    {
        public Task<BkPrdCliDto> GetByNcpAsync(string ncp);
        public Task<List<BkPrdCliDto>> GetNbOccurenceByNcpAsync(string ncp);
    }
}

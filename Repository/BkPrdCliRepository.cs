using Microsoft.EntityFrameworkCore;
using SYSGES_MAGs.Data;
using SYSGES_MAGs.Models;
using SYSGES_MAGs.Models.ModelsDto;
using SYSGES_MAGs.Repository.IRepository;

namespace SYSGES_MAGs.Repository
{
    public class BkPrdCliRepository : IBkPrdCliRepository
    {
        private readonly ApplicationDbContext _dbContext;
        public BkPrdCliRepository(ApplicationDbContext context)
        {
            _dbContext = context;
        }

        public async Task<BkPrdCliDto?> GetByNcpAsync(string ncp)
        {
            return await _dbContext.bkprdclis
               .Where(x => x.ncp == ncp)
               .Select(x => new BkPrdCliDto
               {
                   cpro = x.cpro,
                   ncp = x.ncp,
                   ddsou = x.ddsou,
                   dfsou = x.dfsou,
                   ctar = x.ctar,
                   ttar = x.ttar,
                   ncpf = x.ncpf, 
                   eta = x.eta, 
               })
               .FirstOrDefaultAsync(); ;
        }

        public async Task<List<BkPrdCliDto>> GetNbOccurenceByNcpAsync(string ncp)
        {
            return await _dbContext.bkprdclis
                           .Where(x => x.ncp == ncp)
                           .Select(x => new BkPrdCliDto
                           {
                               cpro = x.cpro,
                               ncp = x.ncp,
                               ddsou = x.ddsou,
                               dfsou = x.dfsou,
                               ctar = x.ctar,
                               ttar = x.ttar,
                               ncpf = x.ncpf,
                               eta = x.eta,
                           })
                           .ToListAsync();
        }
    }
}

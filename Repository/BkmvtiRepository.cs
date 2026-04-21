using Microsoft.EntityFrameworkCore;
using SYSGES_MAGs.Data;
using SYSGES_MAGs.Models;
using SYSGES_MAGs.Models.ModelsDto;
using SYSGES_MAGs.Repository.IRepository;

namespace SYSGES_MAGs.Repository
{
    public class BkmvtiRepository : IBkmvtiRepository
    {
        public readonly ApplicationDbContext _dbContext;
        public BkmvtiRepository(ApplicationDbContext context) { 
            _dbContext = context;
        }

        public async Task<List<BkmvtiResult>> BkmvtisByMagType(Guid typeMagId)
        {


            return await _dbContext.Bkmvtis
                .Where(x => x.TypeMag == typeMagId)
                .GroupBy(x => x.NumeroCompte)
                .Select(g => new BkmvtiResult
                {
                    NumeroCompte = g.Key,
                    CodeAgence = g.First().CodeAgence,
                    DatePrelevement = g.First().DatePrelevement,
                    LibelleCarte = g.First().LibelleCarte,
                    Total = g.Sum(x => x.PrixUnitCarte)
                })
                .ToListAsync();
        }

        public async Task<List<Bkmvti>> SaveBkmvtiAsync(List<Bkmvti> bkmvtis)
        {
            await _dbContext.Bkmvtis.AddRangeAsync(bkmvtis);
            await _dbContext.SaveChangesAsync();
            return bkmvtis; 
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="typeMagId"></param>
        /// <returns></returns>
        public async Task<List<BkmvtiSyntheseDto>> GetSyntheseByTypeMagAsync(Guid typeMagId)
        {
            return await _dbContext.Bkmvtis
                .Where(b => b.TypeMag == typeMagId)
                .GroupBy(b => b.CodeCarte)
                .Select(g => new BkmvtiSyntheseDto
                {
                    CodeCarte = g.Key,
                    NombreClients = g.Count(),
                    DesignationCarte = g.First().DesignationCarte,
                    MontantTotal = g.Sum(x => x.PrixUnitCarte),
                })
                .AsNoTracking()
                .OrderByDescending(x => x.MontantTotal)
                .ToListAsync();
        }
    }
}

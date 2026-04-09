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

        public async Task<List<Bkmvti>> BkmvtisByMagType(Guid typeMagId)
        {
            return await _dbContext.Bkmvtis
                .Where(x => x.TypeMag == typeMagId)
                .Select(x => new Bkmvti
                {
                    Id = x.Id,
                    CodeAgence = x.CodeAgence,
                    Sequence = x.Sequence,
                    CodeIN = x.CodeIN,
                    CodeDevise = x.CodeDevise,
                    EstActif = x.EstActif,
                    NumeroCompte = x.NumeroCompte,
                    DesignationCarte = x.DesignationCarte,
                    TypeBeneficiaire = x.TypeBeneficiaire,
                    ReferenceBeneficiaire = x.ReferenceBeneficiaire,
                    CleBeneficiaire = x.CleBeneficiaire,
                    DatePrelevement = x.DatePrelevement,
                    PrixUnitCarte = x.PrixUnitCarte,
                    ReferenceOperation = x.ReferenceOperation,
                    CodeOperation = x.CodeOperation,
                    CodeEmetteur = x.CodeEmetteur,
                    IndicateurDomiciliation = x.IndicateurDomiciliation,
                    TypeMag = x.TypeMag,
                    LibelleCarte = x.LibelleCarte,
                    Carte = x.Carte,
                    DateValiditeCarte = x.DateValiditeCarte,
                    DateCreationCarte = x.DateCreationCarte,
                    CodeTarif = x.CodeTarif,
                    CodeCarte = x.CodeCarte
                })
                .ToListAsync();
        }

        public async Task<List<Bkmvti>> SaveBkmvtiAsync(List<Bkmvti> bkmvtis)
        {
            await _dbContext.Bkmvtis.AddRangeAsync(bkmvtis);
            await _dbContext.SaveChangesAsync();
            return bkmvtis; 
        }

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
                .OrderByDescending(x => x.MontantTotal)
                .ToListAsync();
        }
    }
}

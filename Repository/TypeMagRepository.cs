using Microsoft.EntityFrameworkCore;
using SYSGES_MAGs.Data;
using SYSGES_MAGs.Models;
using SYSGES_MAGs.Models.ModelsDto;
using SYSGES_MAGs.Repository.IRepository;

public class TypeMagRepository : ITypeMagRepository
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<TypeMagRepository> _logger;

    public TypeMagRepository(ApplicationDbContext dbContext, ILogger<TypeMagRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<IEnumerable<TypeMag>> getAllMag()
    {
        return await _dbContext.TypeMags
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }
 
    public async Task<TypeMagWithSyntheseDto> GetTypeMagWithSyntheseAsync(Guid typeMagId)
    {
        var typeMag = await _dbContext.TypeMags
            .FirstOrDefaultAsync(t => t.Id == typeMagId);

        if (typeMag == null)
            return null;

        var synthese = await _dbContext.Bkmvtis
            .Where(b => b.TypeMag == typeMagId)
            .GroupBy(b => b.CodeCarte)
            .Select(g => new BkmvtiSyntheseDto
            {
                CodeCarte = g.Key,
                NombreClients = g.Count(),
                DesignationCarte = g.First().DesignationCarte,
                MontantTotal = g.Sum(x => x.PrixUnitCarte)
            })
            .OrderByDescending(x => x.MontantTotal)
            .ToListAsync();

        return new TypeMagWithSyntheseDto
        {
            Id = typeMag.Id,
            Description = typeMag.Description,
            Email = typeMag.Email,
            PeriodeDebut = typeMag.PeriodeDebut,
            PeriodeFin = typeMag.PeriodeFin,
            CreatedAt = typeMag.CreatedAt,
            UpdatedAt = typeMag.UpdatedAt,
            SyntheseBkmvtis = synthese
        };
    }

    public async Task<TypeMag?> IsTypeMagExist(DateTime startPeriod)
    {
        var startUtc = DateTime.SpecifyKind(startPeriod, DateTimeKind.Utc);

        return await _dbContext.TypeMags
            .FirstOrDefaultAsync(x => x.PeriodeFin > startUtc);
    }

    public async Task<bool> IsDownload(Guid typeMagId)
    {
        var typeMag = await _dbContext.TypeMags
            .FirstOrDefaultAsync(x => x.Id == typeMagId);

        if (typeMag == null)
            return false;

        typeMag.isAlreadyDownload = true;

        await _dbContext.SaveChangesAsync();

        return true;
    }
    public async Task<TypeMag> SaveTypeMagAsync(TypeMag typeMag)
    {
        try
        {
            _dbContext.TypeMags.Add(typeMag);
            await _dbContext.SaveChangesAsync();
            return typeMag;
        }
        catch (Exception ex)
        {
            _logger.LogInformation("message from type mag repo : " + ex.Message);
            return new TypeMag(); // safe fallback
        }
    }
}
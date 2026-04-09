using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SYSGES_MAGs.Data;
using SYSGES_MAGs.Models;
using SYSGES_MAGs.Models.Enum;
using SYSGES_MAGs.Models.ModelsDto;
using SYSGES_MAGs.Repository.IRepository; 

namespace SYSGES_MAGs.Repository
{
    public class ProfileRepository  : IProfileRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ProfileRepository> _logger;

        public ProfileRepository(ApplicationDbContext context, ILogger<ProfileRepository> logger)
        {
            _context = context;
            _logger = logger;
        }
        public async Task<bool> DeleteAsync(Guid id)
        {
            // Récupérer l'entité
            var profil = await _context.Profiles.FindAsync(id);

            if (profil == null)
            {
                return false; // Rien à supprimer
            }

            _context.Profiles.Remove(profil);
            await _context.SaveChangesAsync();
            return true;
        }


        public async Task<IEnumerable<Profil>> GetAll()
        {
            return await _context.Profiles.ToListAsync();
        }

        public async Task<Profil?> GetByIdAsync(Guid id)
        {
            Profil? profil = await _context.Profiles.FindAsync(id);
            _logger.LogInformation("user profile : " + profil!.Username);
            if (profil == null)
            {
                return null; // Retourne null si non trouvé
            }

            return profil; // Retourne l'objet Profil trouvé
        }



        public async Task<Profil> SaveAsync(Profil profil)
        {
            //Enum.TryParse<EnumProfil>(profil.Status, ignoreCase: true,out EnumProfil profilStatus);
            //profil.TypeProfile = profilStatus;
            await _context.AddAsync(profil);
            await _context.SaveChangesAsync();
            return profil;
        }

        public Task<ProfilDto> UpdateAsync(Profil profil)
        {
            throw new NotImplementedException();
        }
         
    }
}

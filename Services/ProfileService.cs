using AutoMapper; 
using SYSGES_MAGs.Models;
using SYSGES_MAGs.Models.ModelsDto;
using SYSGES_MAGs.Repository.IRepository;
using SYSGES_MAGs.Services.IServices;

namespace SYSGES_MAGs.Services
{
    public class ProfileService : IProfileService
    {
        private readonly IMapper _mapper;
        private readonly IProfileRepository _profileRepository;
        private readonly ILogger<ProfileService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ProfileService(IMapper mapper, IProfileRepository profileRepository, ILogger<ProfileService> logger, IHttpContextAccessor httpContextAccessor)
        {
            _mapper = mapper;
            _profileRepository = profileRepository;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }
        public async Task<bool> DeleteAsync(Guid id)
        {
            return await _profileRepository.DeleteAsync(id);
        }


        public async Task<IEnumerable<ProfilDto>> GetAll()
        {

            var userName = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "Anonyme";
            _logger.LogInformation("Traitement lancé par {User}", userName);

            // Récupère les entités depuis le repository
            IEnumerable<Profil> profils = await _profileRepository.GetAll();

            // Mappe vers DTO
            IEnumerable<ProfilDto> profilDtos = _mapper.Map<IEnumerable<ProfilDto>>(profils);

            // Retourne la liste mappée
            return profilDtos;
        }


        public async Task<ServiceResult<Profil>> GetByIdAsync(Guid id)
        {
            Profil? profil = await _profileRepository.GetByIdAsync(id);
            _logger.LogInformation("user profil name : " + profil!.Username);
            if(profil == null)
            {
                return new ServiceResult<Profil>
                {
                    Success = false,
                    Message = "Oups!!! Le profil à modifier n'existe pas",

                };
            }
            return new ServiceResult<Profil>
            {
                Success = true,
                Message = "Profil Mise à jour avec succès!!!",
                Data = profil
            };
        }

        public async Task<ServiceResult<Profil>> GetByUserEmail(string userEmail)
        { 
            Profil profil = await _profileRepository.GetByUserEmailAsync(userEmail);
            if (profil == null)
            {
                return new ServiceResult<Profil>
                {
                    Success = false,
                    Message = "Oups!!! L'utilisateur n'existe pas",

                };
            }
            return new ServiceResult<Profil>
            {
                Success = true, 
                Data = profil
            };
        }

        public async Task<ProfilDto> SaveAsync(ProfilDto profildto)
        {
            // convertir dto pour entite
            Profil profil = _mapper.Map<Profil>(profildto);
            Profil profileSave = await _profileRepository.SaveAsync(profil);
            if(profileSave == null)
            {
                throw new Exception("Oups!!! Une erreur est survenue pendant l'enregistrement" );
            }
            return profildto;
        }

        public Task<ProfilDto> UpdateAsync(Profil profil)
        {
            throw new NotImplementedException();
        }
    }
}

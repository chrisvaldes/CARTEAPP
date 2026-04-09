using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SYSGES_MAGs.Models;
using SYSGES_MAGs.Models.Enum;
using SYSGES_MAGs.Models.ModelsDto;
using SYSGES_MAGs.Services;
using SYSGES_MAGs.Services.IServices;

namespace SYSGES_MAGs.Controllers
{
    public class ProfilController : Controller
    {
        private readonly ILogger<ProfilController> _logger;
        private readonly IProfileService _profileService;
        IEnumerable<ProfilDto> profilDtos;

        public ProfilController(ILogger<ProfilController> logger, IProfileService profileService, IEnumerable<ProfilDto> profilDtos)
        {
            _logger = logger;
            _profileService = profileService;
            this.profilDtos = profilDtos;
        }
        public async Task<IActionResult> Index()
        {
            profilDtos = await _profileService.GetAll();
            return View(profilDtos);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetProfil(Guid id)
        {

            ServiceResult<Profil> serviceResult = await _profileService.GetByIdAsync(id);

        
            if (!serviceResult.Success) {
                return Json(new { success = false, message = serviceResult.Message});
            }
            else
            {
                return Json(new { success = true, 
                    message = serviceResult.Message, 
                    data = new Profil
                    {
                        Id = serviceResult.Data!.Id,
                        Username = serviceResult.Data!.Username,
                        Userag = serviceResult.Data!.Userag,
                        Email = serviceResult.Data!.Email,
                        TypeProfile = serviceResult.Data!.TypeProfile,
                        Status = serviceResult.Data!.Status
                    }
                });
            } 

        }


        [HttpPost]
        [Authorize]
        public async Task<IActionResult> SaveProfil([FromBody] ProfilDto model)
        {

            Enum.TryParse<EnumProfil>(model.TypeProfileString, ignoreCase: true, out EnumProfil profileType);
            model.TypeProfile=profileType; 
            if (ModelState.IsValid)
            {
                // Sauvegarde en base... 
                ProfilDto profil = await _profileService.SaveAsync(model);
                profilDtos = await _profileService.GetAll();
                return Json(new { success = true, message = "Profil enregistré avec succès", type="success" });
            }

            // Sauvegarde en base... 
            return Json(new { success = false, message = "Tout les champs du formulaire doivent être renseigner.", type="warning" });
        }

        [HttpDelete]
        [Authorize]
        public async Task<IActionResult> DeleteProfil(Guid id)
        {
            try
            {
                bool deleted = await _profileService.DeleteAsync(id);

                if (deleted)
                {
                    return Json(new { success = true, message = "Profil supprimé avec succès." });
                }
                else
                {
                    return Json(new { success = false, message = "Profil introuvable." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Erreur serveur : " + ex.Message });
            }
        }


    }
}

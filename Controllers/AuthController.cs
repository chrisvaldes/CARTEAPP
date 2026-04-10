using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SYSGES_MAGs.Models;
using SYSGES_MAGs.Models.Enum;
using SYSGES_MAGs.Models.ModelsDto;
using SYSGES_MAGs.Services;
using SYSGES_MAGs.Services.IServices;

namespace SYSGES_MAGs.Controllers
{
    public class AuthController   : Controller
    {

        private readonly ILogger<AuthController> _logger;
        private readonly IAuthService _authService;
        private readonly IProfileService _profilService;

        public AuthController(ILogger<AuthController> logger, IAuthService authService, IProfileService profilService)
        {
            _logger = logger;
            _authService = authService;
            _profilService = profilService;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login()
        {
            return View();
        }

         
        [HttpPost]  
        [Route("Auth/LoginAsync")] 
        public async Task<IActionResult> LoginAsync([FromBody] LoginDto request)
        {

            ServiceResult<Profil> profil = await _profilService.GetByUserEmail(request.Username);

            // Exemple avec ta classe User
            var passwordHasher = new PasswordHasher<User>();



            // Crée un utilisateur temporaire juste pour le hachage
            var tempUser = new User();

            // Hash du mot de passe
            string hashedPassword = passwordHasher.HashPassword(tempUser, request.Password);

            // Log pour debug
            _logger.LogInformation("Hashed password: {Password}", hashedPassword);

            if (request == null || string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            {
                return Json(new { success = false, message = "Nom d'utilisateur ou mot de passe invalide" });
            }

            ServiceResult<LoginDto> serviceResult = await _authService.LoginAsync(request);

            if ( !serviceResult.Success)
            {
                return Json(new { success = false, message = serviceResult.Message });
            }

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddHours(1)         
            };

            Response.Cookies.Append("jwt", serviceResult.Token!, cookieOptions);

            // Return the expiration to the client (ISO UTC) so client-side can schedule redirect if desired
            var expiresAtIso = cookieOptions.Expires?.UtcDateTime.ToString("o");
            return Json(new { success = true,
                message = serviceResult.Message,
                expiresAt = expiresAtIso,
                redirectUrl = profil.Data!.TypeProfile switch
                {
                    EnumProfil.SUPER_ADMIN => "/Profil/Index",
                    EnumProfil.ADMIN => "/Profil/Index",
                    EnumProfil.MON_MANAGER => "/Manager/Dashboard",
                    EnumProfil.MON_OFFICER => "/ManqueAGagner/Index",
                    EnumProfil.COMPTABLE => "/ManqueAGagner/Index",
                    _ => "/Home/Index"
                }
            });
        }                   


        public IActionResult Dashboard()
        {
            return View();
        }
    }
}

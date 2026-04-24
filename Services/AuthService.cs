using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SYSGES_MAGs.Data;
using SYSGES_MAGs.Models;
using SYSGES_MAGs.Models.ModelsDto;
using SYSGES_MAGs.Services.IServices;
using System.DirectoryServices;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection.PortableExecutable;
using System.Security.Claims; 
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SYSGES_MAGs.Services
{
    public class AuthService : IAuthService
    {
        private SignInManager<User> _signInManager;
        private UserManager<User> _userManager;
        private ApplicationDbContext _context;
        private IConfiguration _config;
        private ILogger<AuthService> _logger;
        private IProfileService _profilService;
        
        public AuthService(ApplicationDbContext context, IConfiguration config, ILogger<AuthService> logger, IProfileService profilService, SignInManager<User> signInManager, UserManager<User> userManager)
        {
            _signInManager = signInManager;
            _context = context;
            _config = config;
            _logger = logger;
            _profilService = profilService;
            _userManager = userManager;
        }


        public async Task SignInUserAsync(User user)
        {
            await _signInManager.SignInAsync(user, isPersistent: false);
        }

        public async Task<ServiceResult<LoginDto>> LoginAsync(LoginDto loginDto)
        {

            var passwordHasher = new PasswordHasher<User>();

            // Recherche de l'utilisateur
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == loginDto.Username);

            if (user == null)
            {
                return new ServiceResult<LoginDto> {
                    Success = false,
                    Message = "Aucun utilisateur trouvé !!!", 
                };
                
            }

            // Vérification du mot de passe
            var passwordResult = passwordHasher.VerifyHashedPassword(user, user.Password, loginDto.Password);

            _logger.LogInformation("Hash stocké : {Hash}", user.Password);
            _logger.LogInformation("Résultat vérification : {Result}", passwordResult);

            if (passwordResult != PasswordVerificationResult.Success)
            {
                return new ServiceResult<LoginDto>
                {
                    Success = false,
                    Message = "Mot de passe incorrecte !!!",
                };
            }

            return new ServiceResult<LoginDto>
            {
                Success = true,
                Message = "Connexion réussie", 
                Token = await GenerateToken(user)
            };
        }


        


        public bool VerifyPassword(User user, string passwordHasher, string enteredPassword)
        {
            var _passwordHasher = new PasswordHasher<User>();

            var result = _passwordHasher.VerifyHashedPassword(user, passwordHasher, enteredPassword);
            return result == PasswordVerificationResult.Failed;
        }

        public async Task<string> GenerateToken(User user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            ServiceResult<Profil> profil = await _profilService.GetByUserEmail(user.Username);

            // Création des claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username), // Nom d'utilisateur
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), // ID utilisateur  
                new Claim(ClaimTypes.Expiration, DateTime.UtcNow.AddHours(1).ToString("o")),
                new Claim(ClaimTypes.Role, profil.Data!.TypeProfile.ToString()),
            };

            // Clé secrète
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["jwt:SecretKey"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Création du token
            var token = new JwtSecurityToken(
                issuer: _config["jwt:Issuer"]!,
                audience: _config["jwt:Audience"]!,
                claims: claims, 
                expires: DateTime.UtcNow.AddHours(1), 
                signingCredentials: creds
            );

            // Retourne le token sous forme de string
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public Task<Profil> GetByUseragAsync(string userag)
        {
            return null;
        }

        public async Task<ServiceResult<LoginDto>> AuthenticateAsync(string username, string password)
        {
            const string ErrorMessage = "Login ou mot de passe incorrect";
            const string DisabledMessage = "Votre compte a été désactivé.";

            try
            {
                var user = await _userManager.FindByNameAsync(username);
                if (user == null)
                {
                    _logger.LogInformation($"L'utilisateur {username} n'existe pas dans la BD.");
                    return new ServiceResult<LoginDto>
                    {
                        Success = false,
                        Message = ErrorMessage,
                    };
                }

                if (!user.Statut)
                {
                    _logger.LogInformation($"L'utilisateur {username} a été désactivé.");
                    return new ServiceResult<LoginDto>
                    {
                        Success = false,
                        Message = DisabledMessage,
                    };
                }

                //var authMode = await _settingsService.GetAuthModeAsync();
                //_logger.LogInformation($"Mode de connexion paramétré : {authMode}");

                //return authMode == Setting.Auth_AD_Key
                //    ? await AuthenticateLdapAsync(user, username, password)
                //    : await AuthenticateIdentityAsync(user, password);
                return await AuthenticateLdapAsync(user, username, password);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Authentication failed for user {Username}", username);
                return new ServiceResult<LoginDto>
                {
                    Success = false,
                    Message = ErrorMessage, 
                };
            }
        }

        private async Task<ServiceResult<LoginDto>> AuthenticateLdapAsync(User user, string username, string password)
        {
            // On récupère les paramètres de connexion à l'annuaire
            var ldapSettings = new LDAPSetting
            {
                LDAPDirectory="",
                LDAPDomain="",
                LDAPEmail="",
                LDAPPassword="",
                LDAPMatricule=""                
            };
            if (ldapSettings == null)
            {
                return new ServiceResult<LoginDto>
                {
                    Success = false,
                    Message = "Login ou mot de passe incorrecte !!",
                };
            }
            _logger.LogInformation($"Paramètre de connexion à l'annuaire : {ldapSettings.LDAPDomain} - {ldapSettings.LDAPMatricule}");

            try
            {
                // Vérification des paramètres de connexion à l'annuaire
                if (await ValidateLdapCredentialsAsync(ldapSettings, username, password))
                {
                    _logger.LogInformation("Connexion à l'annuaire réussie");
                    await SignInUserAsync(user);
                    return new ServiceResult<LoginDto>
                    {
                        Success = true,
                        Message = "Connexion réussi !!",
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Authentification LDAP échoué pour l'utilisateur {Username}", username);
            }

            return new ServiceResult<LoginDto> { Success = false, Message = "Login ou mot de passe incorrect" };
        }




        //private async Task<LDAPSetting> GetLdapSettingsAsync()
        //{
        //    var setting = await _context.Settings
        //        .FirstOrDefaultAsync(s => s.Code == LDAPSetting.LDAPCode);

        //    return setting == null ? null
        //        : Newtonsoft.Json.JsonConvert.DeserializeObject<LDAPSetting>(setting.Value);
        //}



        /// <summary>
        /// Permet de vérifier les paramètres de connexion dans l'annuaire 
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        private async Task<bool> ValidateLdapCredentialsAsync(LDAPSetting settings, string username, string password)
        {
            var ldapPath = settings.LDAPDirectory + settings.LDAPDomain;
            var adminLogin = settings.ElementConnectLDAP == "2"
                ? settings.LDAPMatricule
                : settings.LDAPEmail;

            // First validate admin connection
            using (var userEntry = new System.DirectoryServices.DirectoryEntry(ldapPath, username, password))
            {
                try
                {
                    // Verify admin connection
                    _ = userEntry.NativeObject;

                    // Search for user
                    using (var searcher = new DirectorySearcher(userEntry))
                    {
                        searcher.Filter = $"(sAMAccountName={username})";
                        var result = searcher.FindOne();

                        if (result == null) return false;

                        return true;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, " LDAP connexion ldap ");
                    return false;
                }
            }
        }




        public async Task<ServiceResult<LoginDto>> LoginWithLdapAsync(LoginDto loginDto)
        {
           if(string.IsNullOrEmpty(loginDto.Username) || string.IsNullOrEmpty(loginDto.Password))
            {
                return new ServiceResult<LoginDto>
                {
                    Success = false,
                    Message = "Username ou mot de passe vide !!",
                };
            }
           
            return new ServiceResult<LoginDto>
            {
                Success = false,
                Message = "Aucun utilisateur trouvé !!!",
            };
        }
    }
}

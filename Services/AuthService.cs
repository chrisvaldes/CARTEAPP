using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SYSGES_MAGs.Data;
using SYSGES_MAGs.Models;
using SYSGES_MAGs.Models.ModelsDto;
using SYSGES_MAGs.Services.IServices;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SYSGES_MAGs.Services
{
    public class AuthService (ApplicationDbContext _context, IConfiguration _config, ILogger<AuthService> _logger) : IAuthService
    {


        [HttpPost]
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
                Token = GenerateToken(user)
            };
        }


        public bool VerifyPassword(User user, string passwordHasher, string enteredPassword)
        {
            var _passwordHasher = new PasswordHasher<User>();

            var result = _passwordHasher.VerifyHashedPassword(user, passwordHasher, enteredPassword);
            return result == PasswordVerificationResult.Failed;
        }

        public string GenerateToken(User user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            // Création des claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username), // Nom d'utilisateur
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), // ID utilisateur 
                // expiration claim (ISO UTC)
                new Claim(ClaimTypes.Expiration, DateTime.UtcNow.AddHours(1).ToString("o")),
                new Claim(ClaimTypes.Role, "role"),
            };

            // Clé secrète
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["jwt:SecretKey"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Création du token
            var token = new JwtSecurityToken(
                issuer: _config["jwt:Issuer"]!,
                audience: _config["jwt:Audience"]!,
                claims: claims,
                // token valid for 1 hour
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
    }
}

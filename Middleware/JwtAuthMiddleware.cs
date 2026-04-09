using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Unicode;

namespace SYSGES_MAGs.Middleware
{
    public class JwtAuthMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _config;
        private readonly ILogger<JwtAuthMiddleware> _logger;
        private readonly string[] _publicPrefixes;

        public JwtAuthMiddleware(RequestDelegate next, IConfiguration config, ILogger<JwtAuthMiddleware> logger)
        {
            _next = next;
            _config = config;
            _logger = logger;

            // Charge depuis la config ou utilise les valeurs par défaut
            _publicPrefixes = _config.GetSection("PublicPaths").Get<string[]>() ?? new[]
            {
                "/auth",
                "/login",
                "/css/",
                "/js/",
                "/libs/",
                "/images/"
            };
        }


        public async Task Invoke(HttpContext context)
        {
            var path = context.Request.Path.Value?.ToLower() ?? string.Empty;

            // route public
            if (IsPublicPath(path))
            {
                await _next(context);
                return;
            }


            // recupération du token
            var token = GetTokenFromRequest(context);
            if (!string.IsNullOrEmpty(token))
            {
                var principal = Validatetoken(token);
                if(principal != null)
                {
                    // injection des roles depuis le jwt
                    var roleClaims = GetRoleClaims(token);
                    if (roleClaims.Any())
                    {
                        var identity = (ClaimsIdentity)principal.Identity!;
                        identity.AddClaims(roleClaims);
                    }
                    context.User = principal;
                    await _next(context);
                    return;
                }
            }
            await HandleUnauthorized(context);
        }

        private bool IsPublicPath(string path)
        {
            return _publicPrefixes.Any(prefix => !string.IsNullOrWhiteSpace(prefix) && path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
        }

        private string? GetTokenFromRequest(HttpContext context)
        {

            var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
            if(!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
            {
                return authHeader.Substring("Bearer ".Length).Trim();
            }
            if (context.Request.Cookies.ContainsKey("jwt"))
            {
                return context.Request.Cookies["jwt"];
            }
            return null;
        }

        private ClaimsPrincipal? Validatetoken(string token)
        {
            _logger.LogInformation("token : " + token);

            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_config["Jwt:SecretKey"]!);
                var parameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = _config["Jwt:Issuer"],
                    ValidAudience = _config["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ClockSkew = TimeSpan.Zero
                };


                return tokenHandler.ValidateToken(token, parameters, out _);
            }catch(Exception ex)
            {
                _logger.LogWarning("Jwt invalide {Message}", ex.Message);
                return null;
            }
        }

        private IEnumerable<Claim> GetRoleClaims(string token)
        {
            var handle = new JwtSecurityTokenHandler();
            var jwtToken = handle.ReadJwtToken(token);

            // récupération des role
            var roles = jwtToken.Claims
                .Where(c => c.Type == ClaimTypes.Role || c.Type == "role" || c.Type == "roles")
                .Select(c => new Claim(ClaimTypes.Role, c.Value));

            return roles;
        }
        private async Task HandleUnauthorized(HttpContext context)
        {
            if (context.Response.HasStarted)
            {
                _logger.LogWarning("Impossible de rediriger, la réponse a déjà commencé.");
                return;
            }

            // Détection fiable d'une requête AJAX/API
            bool isApiRequest =
                context.Request.Headers["X-Requested-With"] == "XMLHttpRequest" ||
                (context.Request.Headers["Accept"].Any(h => h!.Contains("application/json", StringComparison.OrdinalIgnoreCase)));

            if (isApiRequest)
            {
                // Réponse JSON pour les appels AJAX ou API
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync("{\"message\":\"Unauthorized\"}");
            }
            else
            {
                // Redirection vers la page de login pour les requêtes normales
                context.Response.Clear();
                context.Response.Redirect("/Auth/Login", false);
            }
        }


    }
}




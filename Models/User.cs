using System.Security.Claims;

namespace SYSGES_MAGs.Models
{
    public class User
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;

        public static implicit operator User?(ClaimsPrincipal? v)
        {
            throw new NotImplementedException();
        }
    }
}

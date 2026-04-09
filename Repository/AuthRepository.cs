using SYSGES_MAGs.Data;
using SYSGES_MAGs.Models;
using SYSGES_MAGs.Repository.IAuhtRepository;

namespace SYSGES_MAGs.Repository
{
    public class AuthRepository : IAuthRepository
    {
        private readonly ApplicationDbContext _context;
        public AuthRepository(ApplicationDbContext context) {
            _context = context;
        }

        public async Task<User> GetByUseragAsync(string username)
        {
            throw new NotImplementedException();
        }
    }
}

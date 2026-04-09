using SYSGES_MAGs.Models;

namespace SYSGES_MAGs.Repository.IAuhtRepository
{
    public interface IAuthRepository
    {
        public Task<User> GetByUseragAsync(string username);
    }
}

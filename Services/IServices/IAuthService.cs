using Microsoft.AspNetCore.Mvc;
using SYSGES_MAGs.Models;
using SYSGES_MAGs.Models.ModelsDto;

namespace SYSGES_MAGs.Services.IServices
{
    public interface IAuthService
    {
        Task<ServiceResult<LoginDto>> LoginAsync(LoginDto loginDto);
        Task<Profil> GetByUseragAsync(string userag);
    }
}

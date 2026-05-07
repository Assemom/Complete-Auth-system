using App.Business.Models;
using App.Domain.Entities;

namespace App.Business.Interfaces;

public interface IJwtService
{
    Task<AccessTokenResult> CreateAccessTokenAsync(ApplicationUser user);
    string GenerateRefreshToken();
}

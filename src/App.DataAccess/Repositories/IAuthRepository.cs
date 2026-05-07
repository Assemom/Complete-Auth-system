using App.Domain.Entities;

namespace App.DataAccess.Repositories;

public interface IAuthRepository
{
    Task<RefreshToken?> GetRefreshTokenAsync(string token);
    Task RevokeAllUserTokensAsync(string userId);
    Task AddRefreshTokenAsync(RefreshToken token);
}

using App.Domain.Entities;

namespace App.DataAccess.Repositories;

public interface IUserRepository
{
    Task<ApplicationUser?> GetByEmailAsync(string email);
    Task<EmailOtp?> GetOtpAsync(string userId, string code);
    Task AddOtpAsync(EmailOtp otp);
    Task InvalidateUserOtpsAsync(string userId);
}

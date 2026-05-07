using App.DataAccess.Context;
using App.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace App.DataAccess.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;

    public UserRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ApplicationUser?> GetByEmailAsync(string email)
    {
        return await _context.Users.FirstOrDefaultAsync(user => user.Email == email);
    }

    public async Task<EmailOtp?> GetOtpAsync(string userId, string code)
    {
        return await _context.EmailOtps.FirstOrDefaultAsync(otp => otp.UserId == userId && otp.Code == code);
    }

    public async Task AddOtpAsync(EmailOtp otp)
    {
        await _context.EmailOtps.AddAsync(otp);
    }

    public async Task InvalidateUserOtpsAsync(string userId)
    {
        var otps = await _context.EmailOtps
            .Where(otp => otp.UserId == userId && !otp.IsUsed)
            .ToListAsync();

        foreach (var otp in otps)
        {
            otp.IsUsed = true;
            otp.UpdatedAt = DateTime.UtcNow;
        }
    }
}

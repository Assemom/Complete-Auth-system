using App.Domain.DTOs;

namespace App.Business.Interfaces;

public interface IAuthService
{
    Task RegisterAsync(RegisterDto dto);
    Task<AuthResponseDto> LoginAsync(LoginDto dto);
    Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenDto dto);
    Task LogoutAsync(string userId);
    Task DeleteAccountAsync(string userId);
    Task UpdateProfileAsync(string userId, UpdateProfileDto dto);
    Task ChangePasswordAsync(string userId, ChangePasswordDto dto);
    Task ForgotPasswordAsync(ForgotPasswordDto dto);
    Task ResetPasswordAsync(ResetPasswordDto dto);
    Task ConfirmEmailAsync(ConfirmEmailDto dto);
    Task ResendConfirmationAsync(ResendConfirmationDto dto);
    Task<AuthResponseDto> GoogleLoginAsync(GoogleLoginDto dto);
}

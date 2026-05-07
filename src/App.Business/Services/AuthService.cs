using System.Security.Cryptography;
using App.Business.Interfaces;
using App.Domain.DTOs;
using App.Domain.Entities;
using App.Domain.Enums;
using App.Shared.Constants;
using App.Shared.Exceptions;
using App.Shared.Settings;
using App.DataAccess.UnitOfWork;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace App.Business.Services;

public class AuthService : IAuthService
{
    private const int OtpLength = 6;
    private const int OtpMaxValue = 1_000_000;
    private const int OtpExpiryMinutes = 10;

    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IJwtService _jwtService;
    private readonly IEmailService _emailService;
    private readonly JwtSettings _jwtSettings;
    private readonly GoogleAuthSettings _googleSettings;
    private readonly FrontendSettings _frontendSettings;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IUnitOfWork unitOfWork,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        SignInManager<ApplicationUser> signInManager,
        IJwtService jwtService,
        IEmailService emailService,
        ILogger<AuthService> logger,
        IOptions<JwtSettings> jwtOptions,
        IOptions<GoogleAuthSettings> googleOptions,
        IOptions<FrontendSettings> frontendOptions)
    {
        _unitOfWork = unitOfWork;
        _userManager = userManager;
        _roleManager = roleManager;
        _signInManager = signInManager;
        _jwtService = jwtService;
        _emailService = emailService;
        _logger = logger;
        _jwtSettings = jwtOptions.Value;
        _googleSettings = googleOptions.Value;
        _frontendSettings = frontendOptions.Value;
    }

    public async Task RegisterAsync(RegisterDto dto)
    {
        _logger.LogInformation("Register attempt for {Email}.", dto.Email);

        var existingUser = await _userManager.FindByEmailAsync(dto.Email);
        if (existingUser is not null)
        {
            _logger.LogWarning("Register failed for {Email}: already exists.", dto.Email);
            throw new ConflictException(AuthErrorMessages.UserAlreadyExists);
        }

        var user = new ApplicationUser
        {
            Email = dto.Email,
            UserName = dto.Email,
            FirstName = dto.FirstName,
            LastName = dto.LastName
        };

        var createResult = await _userManager.CreateAsync(user, dto.Password);
        if (!createResult.Succeeded)
        {
            _logger.LogWarning("Register failed for {Email}.", dto.Email);
            throw new ValidationException(AuthErrorMessages.RegistrationFailed);
        }

        if (!await _roleManager.RoleExistsAsync(nameof(Roles.User)))
        {
            var roleCreationResult = await _roleManager.CreateAsync(new IdentityRole(nameof(Roles.User)));
            if (!roleCreationResult.Succeeded)
            {
                throw new ServerException(AuthErrorMessages.RoleCreationFailed);
            }
        }

        var roleResult = await _userManager.AddToRoleAsync(user, nameof(Roles.User));
        if (!roleResult.Succeeded)
        {
            _logger.LogError("Role assignment failed for {Email}.", dto.Email);
            throw new ServerException(AuthErrorMessages.RoleAssignmentFailed);
        }

        var otp = CreateOtp(user.Id);
        await _unitOfWork.Users.AddOtpAsync(otp);
        await _unitOfWork.SaveChangesAsync();

        if (!string.IsNullOrWhiteSpace(user.Email))
        {
            await _emailService.SendEmailConfirmationAsync(user.Email, otp.Code);
        }
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user is null)
        {
            _logger.LogWarning("Login failed for {Email}: user not found.", dto.Email);
            throw new NotFoundException(AuthErrorMessages.UserNotFound);
        }

        if (!user.EmailConfirmed)
        {
            _logger.LogWarning("Login failed for {Email}: email not confirmed.", dto.Email);
            throw new ForbiddenException(AuthErrorMessages.EmailNotConfirmed);
        }

        var signInResult = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, true);
        if (signInResult.IsLockedOut)
        {
            _logger.LogWarning("Login failed for {Email}: locked out.", dto.Email);
            throw new ForbiddenException(AuthErrorMessages.AccountLocked);
        }

        if (!signInResult.Succeeded)
        {
            _logger.LogWarning("Login failed for {Email}: invalid credentials.", dto.Email);
            throw new UnauthorizedException(AuthErrorMessages.InvalidCredentials);
        }

        _logger.LogInformation("Login succeeded for {Email}.", dto.Email);
        return await CreateAuthResponseAsync(user, true);
    }

    public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenDto dto)
    {
        var refreshToken = await _unitOfWork.Auth.GetRefreshTokenAsync(dto.RefreshToken);
        if (refreshToken is null)
        {
            _logger.LogWarning("Refresh token not found.");
            throw new UnauthorizedException(AuthErrorMessages.RefreshTokenNotFound);
        }

        if (refreshToken.IsRevoked)
        {
            _logger.LogWarning("Refresh token revoked for user {UserId}.", refreshToken.UserId);
            throw new UnauthorizedException(AuthErrorMessages.RefreshTokenRevoked);
        }

        if (refreshToken.ExpiresAt <= DateTime.UtcNow)
        {
            _logger.LogWarning("Refresh token expired for user {UserId}.", refreshToken.UserId);
            throw new UnauthorizedException(AuthErrorMessages.RefreshTokenExpired);
        }

        var user = await _userManager.FindByIdAsync(refreshToken.UserId);
        if (user is null)
        {
            _logger.LogWarning("Refresh token user not found: {UserId}.", refreshToken.UserId);
            throw new NotFoundException(AuthErrorMessages.UserNotFound);
        }

        _logger.LogInformation("Refresh token succeeded for user {UserId}.", user.Id);
        return await CreateAuthResponseAsync(user, true);
    }

    public async Task LogoutAsync(string userId)
    {
        _logger.LogInformation("Logout for user {UserId}.", userId);
        await _unitOfWork.Auth.RevokeAllUserTokensAsync(userId);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteAccountAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            _logger.LogWarning("Delete account failed: user not found {UserId}.", userId);
            throw new NotFoundException(AuthErrorMessages.UserNotFound);
        }

        await _unitOfWork.Auth.RevokeAllUserTokensAsync(userId);
        await _unitOfWork.SaveChangesAsync();

        var lockoutEnabledResult = await _userManager.SetLockoutEnabledAsync(user, true);
        if (!lockoutEnabledResult.Succeeded)
        {
            _logger.LogError("Delete account failed: lockout enable failed for {UserId}.", userId);
            throw new ServerException(AuthErrorMessages.UserDeletionFailed);
        }

        var lockoutEndResult = await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
        if (!lockoutEndResult.Succeeded)
        {
            _logger.LogError("Delete account failed: lockout end failed for {UserId}.", userId);
            throw new ServerException(AuthErrorMessages.UserDeletionFailed);
        }

        _logger.LogInformation("Account deleted for user {UserId}.", userId);
    }

    public async Task UpdateProfileAsync(string userId, UpdateProfileDto dto)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            _logger.LogWarning("Update profile failed: user not found {UserId}.", userId);
            throw new NotFoundException(AuthErrorMessages.UserNotFound);
        }

        var emailChanged = !string.IsNullOrWhiteSpace(dto.Email)
            && !string.Equals(user.Email, dto.Email, StringComparison.OrdinalIgnoreCase);

        if (emailChanged)
        {
            var existingUser = await _userManager.FindByEmailAsync(dto.Email!);
            if (existingUser is not null && existingUser.Id != user.Id)
            {
                _logger.LogWarning("Update profile failed: email already in use {Email}.", dto.Email);
                throw new ConflictException(AuthErrorMessages.UserAlreadyExists);
            }

            var emailResult = await _userManager.SetEmailAsync(user, dto.Email!);
            if (!emailResult.Succeeded)
            {
                _logger.LogWarning("Update profile failed: email update failed for {UserId}.", userId);
                throw new ValidationException(AuthErrorMessages.UserUpdateFailed);
            }

            var userNameResult = await _userManager.SetUserNameAsync(user, dto.Email!);
            if (!userNameResult.Succeeded)
            {
                _logger.LogWarning("Update profile failed: username update failed for {UserId}.", userId);
                throw new ValidationException(AuthErrorMessages.UserUpdateFailed);
            }

            user.EmailConfirmed = false;
        }

        if (dto.FirstName is not null)
        {
            user.FirstName = dto.FirstName;
        }

        if (dto.LastName is not null)
        {
            user.LastName = dto.LastName;
        }

        if (dto.PhoneNumber is not null)
        {
            user.PhoneNumber = dto.PhoneNumber;
        }

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            _logger.LogWarning("Update profile failed for {UserId}.", userId);
            throw new ValidationException(AuthErrorMessages.UserUpdateFailed);
        }

        if (emailChanged)
        {
            await _unitOfWork.Users.InvalidateUserOtpsAsync(user.Id);

            var otp = CreateOtp(user.Id);
            await _unitOfWork.Users.AddOtpAsync(otp);
            await _unitOfWork.SaveChangesAsync();

            if (!string.IsNullOrWhiteSpace(user.Email))
            {
                await _emailService.SendEmailConfirmationAsync(user.Email, otp.Code);
            }
        }

        _logger.LogInformation("Profile updated for user {UserId}.", userId);
    }

    public async Task ChangePasswordAsync(string userId, ChangePasswordDto dto)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            _logger.LogWarning("Change password failed: user not found {UserId}.", userId);
            throw new NotFoundException(AuthErrorMessages.UserNotFound);
        }

        var result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
        if (!result.Succeeded)
        {
            _logger.LogWarning("Change password failed for user {UserId}.", userId);
            throw new ValidationException(AuthErrorMessages.PasswordChangeFailed);
        }

        _logger.LogInformation("Password changed for user {UserId}.", userId);
        await _unitOfWork.Auth.RevokeAllUserTokensAsync(userId);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task ForgotPasswordAsync(ForgotPasswordDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user is null || string.IsNullOrWhiteSpace(user.Email))
        {
            return;
        }

        _logger.LogInformation("Password reset requested for {Email}.", dto.Email);
        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var resetLink = BuildResetPasswordLink(user.Email, token);

        await _emailService.SendPasswordResetAsync(user.Email, resetLink);
    }

    public async Task ResetPasswordAsync(ResetPasswordDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user is null)
        {
            _logger.LogWarning("Password reset failed: user not found {Email}.", dto.Email);
            throw new NotFoundException(AuthErrorMessages.UserNotFound);
        }

        var result = await _userManager.ResetPasswordAsync(user, dto.Token, dto.NewPassword);
        if (!result.Succeeded)
        {
            _logger.LogWarning("Password reset failed for {Email}.", dto.Email);
            throw new ValidationException(AuthErrorMessages.PasswordResetFailed);
        }

        _logger.LogInformation("Password reset completed for {Email}.", dto.Email);
        await _unitOfWork.Auth.RevokeAllUserTokensAsync(user.Id);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task ConfirmEmailAsync(ConfirmEmailDto dto)
    {
        var user = await _userManager.FindByIdAsync(dto.UserId);
        if (user is null)
        {
            _logger.LogWarning("Email confirmation failed: user not found {UserId}.", dto.UserId);
            throw new NotFoundException(AuthErrorMessages.UserNotFound);
        }

        var otp = await _unitOfWork.Users.GetOtpAsync(user.Id, dto.Code);
        if (otp is null)
        {
            _logger.LogWarning("Email confirmation failed: invalid OTP for {UserId}.", dto.UserId);
            throw new ValidationException(AuthErrorMessages.OtpInvalid);
        }

        if (otp.IsUsed)
        {
            _logger.LogWarning("Email confirmation failed: OTP already used for {UserId}.", dto.UserId);
            throw new ValidationException(AuthErrorMessages.OtpUsed);
        }

        if (otp.ExpiresAt <= DateTime.UtcNow)
        {
            _logger.LogWarning("Email confirmation failed: OTP expired for {UserId}.", dto.UserId);
            throw new ValidationException(AuthErrorMessages.OtpExpired);
        }

        otp.IsUsed = true;
        user.EmailConfirmed = true;

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            _logger.LogError("Email confirmation failed: user update failed for {UserId}.", dto.UserId);
            throw new ServerException(AuthErrorMessages.UserUpdateFailed);
        }

        _logger.LogInformation("Email confirmed for user {UserId}.", dto.UserId);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task ResendConfirmationAsync(ResendConfirmationDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user is null)
        {
            _logger.LogWarning("Resend confirmation failed: user not found {Email}.", dto.Email);
            throw new NotFoundException(AuthErrorMessages.UserNotFound);
        }

        if (user.EmailConfirmed)
        {
            _logger.LogWarning("Resend confirmation failed: email already confirmed {Email}.", dto.Email);
            throw new ConflictException(AuthErrorMessages.EmailAlreadyConfirmed);
        }

        await _unitOfWork.Users.InvalidateUserOtpsAsync(user.Id);

        var otp = CreateOtp(user.Id);
        await _unitOfWork.Users.AddOtpAsync(otp);
        await _unitOfWork.SaveChangesAsync();

        if (!string.IsNullOrWhiteSpace(user.Email))
        {
            _logger.LogInformation("Resent confirmation OTP to {Email}.", user.Email);
            await _emailService.SendEmailConfirmationAsync(user.Email, otp.Code);
        }
    }

    public async Task<AuthResponseDto> GoogleLoginAsync(GoogleLoginDto dto)
    {
        GoogleJsonWebSignature.Payload payload;

        try
        {
            payload = await GoogleJsonWebSignature.ValidateAsync(dto.IdToken, new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new[] { _googleSettings.ClientId }
            });
        }
        catch (InvalidJwtException)
        {
            _logger.LogWarning("Google login failed: invalid token.");
            throw new UnauthorizedException(AuthErrorMessages.GoogleTokenInvalid);
        }

        if (string.IsNullOrWhiteSpace(payload.Email))
        {
            _logger.LogWarning("Google login failed: missing email.");
            throw new UnauthorizedException(AuthErrorMessages.GoogleTokenInvalid);
        }

        var user = await _userManager.FindByEmailAsync(payload.Email);
        if (user is null)
        {
            user = new ApplicationUser
            {
                Email = payload.Email,
                UserName = payload.Email,
                EmailConfirmed = true,
                FirstName = payload.GivenName ?? string.Empty,
                LastName = payload.FamilyName ?? string.Empty
            };

            var createResult = await _userManager.CreateAsync(user);
            if (!createResult.Succeeded)
            {
                _logger.LogWarning("Google login failed: registration failed for {Email}.", payload.Email);
                throw new ValidationException(AuthErrorMessages.RegistrationFailed);
            }

            if (!await _roleManager.RoleExistsAsync(nameof(Roles.User)))
            {
                var roleCreationResult = await _roleManager.CreateAsync(new IdentityRole(nameof(Roles.User)));
                if (!roleCreationResult.Succeeded)
                {
                    throw new ServerException(AuthErrorMessages.RoleCreationFailed);
                }
            }

            var roleResult = await _userManager.AddToRoleAsync(user, nameof(Roles.User));
            if (!roleResult.Succeeded)
            {
                _logger.LogError("Google login failed: role assignment failed for {Email}.", payload.Email);
                throw new ServerException(AuthErrorMessages.RoleAssignmentFailed);
            }
        }
        else if (!user.EmailConfirmed)
        {
            user.EmailConfirmed = true;
            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                _logger.LogError("Google login failed: user update failed for {Email}.", payload.Email);
                throw new ServerException(AuthErrorMessages.UserUpdateFailed);
            }
        }

        var canSignIn = await _signInManager.CanSignInAsync(user);
        if (!canSignIn)
        {
            _logger.LogWarning("Google login failed: sign-in not allowed for {Email}.", payload.Email);
            throw new ForbiddenException(AuthErrorMessages.EmailNotConfirmed);
        }

        var loginInfo = new UserLoginInfo(AuthConstants.GoogleProvider, payload.Subject, AuthConstants.GoogleProvider);
        var logins = await _userManager.GetLoginsAsync(user);
        if (logins.All(login => login.LoginProvider != AuthConstants.GoogleProvider))
        {
            var loginResult = await _userManager.AddLoginAsync(user, loginInfo);
            if (!loginResult.Succeeded)
            {
                _logger.LogError("Google login failed: external login failed for {Email}.", payload.Email);
                throw new ServerException(AuthErrorMessages.ExternalLoginFailed);
            }
        }

        _logger.LogInformation("Google login succeeded for {Email}.", payload.Email);
        return await CreateAuthResponseAsync(user, true);
    }

    private EmailOtp CreateOtp(string userId)
    {
        var code = RandomNumberGenerator.GetInt32(OtpMaxValue).ToString($"D{OtpLength}");

        return new EmailOtp
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Code = code,
            ExpiresAt = DateTime.UtcNow.AddMinutes(OtpExpiryMinutes),
            IsUsed = false
        };
    }

    private async Task<AuthResponseDto> CreateAuthResponseAsync(ApplicationUser user, bool revokeExistingTokens)
    {
        var accessTokenResult = await _jwtService.CreateAccessTokenAsync(user);
        var refreshTokenValue = _jwtService.GenerateRefreshToken();

        if (revokeExistingTokens)
        {
            await _unitOfWork.Auth.RevokeAllUserTokensAsync(user.Id);
        }

        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            Token = refreshTokenValue,
            ExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiryDays),
            IsRevoked = false,
            UserId = user.Id
        };

        await _unitOfWork.Auth.AddRefreshTokenAsync(refreshToken);
        await _unitOfWork.SaveChangesAsync();

        return new AuthResponseDto
        {
            AccessToken = accessTokenResult.AccessToken,
            RefreshToken = refreshTokenValue,
            ExpiresAt = accessTokenResult.ExpiresAt,
            Email = user.Email ?? string.Empty,
            Roles = accessTokenResult.Roles
        };
    }

    private string BuildResetPasswordLink(string email, string token)
    {
        if (string.IsNullOrWhiteSpace(_frontendSettings.BaseUrl))
        {
            throw new ServerException(AuthErrorMessages.ResetLinkBaseUrlMissing);
        }

        var encodedEmail = Uri.EscapeDataString(email);
        var encodedToken = Uri.EscapeDataString(token);
        var baseUrl = _frontendSettings.BaseUrl.TrimEnd('/');

        return $"{baseUrl}{AuthConstants.ResetPasswordPath}?email={encodedEmail}&token={encodedToken}";
    }
}

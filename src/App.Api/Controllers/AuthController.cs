using App.Business.Interfaces;
using App.Domain.DTOs;
using App.Shared.Constants;
using App.Shared.Exceptions;
using App.Shared.Responses;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace App.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse<object?>>> Register([FromBody] RegisterDto dto)
    {
        await _authService.RegisterAsync(dto);
        return Ok(ApiResponse<object?>.Ok(null));
    }

    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Login([FromBody] LoginDto dto)
    {
        var response = await _authService.LoginAsync(dto);
        return Ok(ApiResponse<AuthResponseDto>.Ok(response));
    }

    [HttpPost("refresh-token")]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> RefreshToken([FromBody] RefreshTokenDto dto)
    {
        var response = await _authService.RefreshTokenAsync(dto);
        return Ok(ApiResponse<AuthResponseDto>.Ok(response));
    }

    [HttpPost("google-login")]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> GoogleLogin([FromBody] GoogleLoginDto dto)
    {
        var response = await _authService.GoogleLoginAsync(dto);
        return Ok(ApiResponse<AuthResponseDto>.Ok(response));
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<ActionResult<ApiResponse<object?>>> Logout()
    {
        var userId = GetUserId();
        await _authService.LogoutAsync(userId);
        return Ok(ApiResponse<object?>.Ok(null));
    }

    [Authorize]
    [HttpDelete("delete-account")]
    public async Task<ActionResult<ApiResponse<object?>>> DeleteAccount()
    {
        var userId = GetUserId();
        await _authService.DeleteAccountAsync(userId);
        return Ok(ApiResponse<object?>.Ok(null));
    }

    [Authorize]
    [HttpPut("update-profile")]
    public async Task<ActionResult<ApiResponse<object?>>> UpdateProfile([FromBody] UpdateProfileDto dto)
    {
        var userId = GetUserId();
        await _authService.UpdateProfileAsync(userId, dto);
        return Ok(ApiResponse<object?>.Ok(null));
    }

    [HttpPost("confirm-email")]
    public async Task<ActionResult<ApiResponse<object?>>> ConfirmEmail([FromBody] ConfirmEmailDto dto)
    {
        await _authService.ConfirmEmailAsync(dto);
        return Ok(ApiResponse<object?>.Ok(null));
    }

    [HttpPost("resend-confirmation")]
    public async Task<ActionResult<ApiResponse<object?>>> ResendConfirmation([FromBody] ResendConfirmationDto dto)
    {
        await _authService.ResendConfirmationAsync(dto);
        return Ok(ApiResponse<object?>.Ok(null));
    }

    [HttpPost("forgot-password")]
    public async Task<ActionResult<ApiResponse<object?>>> ForgotPassword([FromBody] ForgotPasswordDto dto)
    {
        await _authService.ForgotPasswordAsync(dto);
        return Ok(ApiResponse<object?>.Ok(null));
    }

    [HttpPost("reset-password")]
    public async Task<ActionResult<ApiResponse<object?>>> ResetPassword([FromBody] ResetPasswordDto dto)
    {
        await _authService.ResetPasswordAsync(dto);
        return Ok(ApiResponse<object?>.Ok(null));
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<ActionResult<ApiResponse<object?>>> ChangePassword([FromBody] ChangePasswordDto dto)
    {
        var userId = GetUserId();
        await _authService.ChangePasswordAsync(userId, dto);
        return Ok(ApiResponse<object?>.Ok(null));
    }

    private string GetUserId()
    {
        var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new UnauthorizedException(AuthErrorMessages.UserNotAuthenticated);
        }

        return userId;
    }
}

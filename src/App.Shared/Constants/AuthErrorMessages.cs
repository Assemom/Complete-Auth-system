namespace App.Shared.Constants;

public static class AuthErrorMessages
{
    public const string UserAlreadyExists = "User already exists.";
    public const string UserNotFound = "User not found.";
    public const string InvalidCredentials = "Invalid credentials.";
    public const string EmailNotConfirmed = "Email address is not confirmed.";
    public const string AccountLocked = "Account is locked.";
    public const string RefreshTokenNotFound = "Refresh token not found.";
    public const string RefreshTokenRevoked = "Refresh token has been revoked.";
    public const string RefreshTokenExpired = "Refresh token has expired.";
    public const string OtpInvalid = "OTP is invalid.";
    public const string OtpExpired = "OTP has expired.";
    public const string OtpUsed = "OTP has already been used.";
    public const string RegistrationFailed = "User registration failed.";
    public const string RoleAssignmentFailed = "Role assignment failed.";
    public const string PasswordChangeFailed = "Password change failed.";
    public const string PasswordResetFailed = "Password reset failed.";
    public const string GoogleTokenInvalid = "Google token is invalid.";
    public const string EmailAlreadyConfirmed = "Email is already confirmed.";
    public const string ResetLinkBaseUrlMissing = "Frontend base URL is not configured.";
    public const string RoleCreationFailed = "Role creation failed.";
    public const string UserUpdateFailed = "User update failed.";
    public const string ExternalLoginFailed = "External login failed.";
    public const string UserNotAuthenticated = "User is not authenticated.";
    public const string UserDeletionFailed = "User deletion failed.";
}

namespace App.Shared.Constants;

public static class ValidationConstants
{
    public const int PasswordMinLength = 8;
    public const int OtpLength = 6;
    public const string PasswordUppercaseRegex = "[A-Z]";
    public const string PasswordLowercaseRegex = "[a-z]";
    public const string PasswordDigitRegex = "[0-9]";
    public const string PasswordSpecialRegex = "[^a-zA-Z0-9]";
    public const string OtpRegex = "^\\d{6}$";
}

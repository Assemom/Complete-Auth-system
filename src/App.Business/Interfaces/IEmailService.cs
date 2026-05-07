namespace App.Business.Interfaces;

public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string htmlBody);
    Task SendEmailConfirmationAsync(string to, string otp);
    Task SendPasswordResetAsync(string to, string resetLink);
}

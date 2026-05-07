using System.Net;
using System.Net.Mail;
using App.Business.Interfaces;
using App.Shared.Exceptions;
using App.Shared.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace App.Infrastructure.Email;

public class EmailService : IEmailService
{
    private const int OtpExpiryMinutes = 10;
    private const string ConfirmationSubject = "Email Confirmation";
    private const string ResetSubject = "Password Reset";
    private const string ConfirmationTitle = "Confirm your email";
    private const string ResetTitle = "Reset your password";
    private const string ConfirmationIntro = "Use the code below to confirm your email.";
    private const string ResetIntro = "Click the button below to reset your password.";
    private const string ConfirmationExpiryText = "This code expires in {0} minutes.";
    private const string ResetExpiryText = "This link will expire soon.";
    private const string ButtonText = "Reset Password";
    private const string FooterText = "If you did not request this, you can ignore this email.";

    private readonly EmailSettings _settings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IOptions<EmailSettings> settings, ILogger<EmailService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task SendEmailAsync(string to, string subject, string htmlBody)
    {
        try
        {
            _logger.LogInformation("Sending email to {Email}", to);

            using var message = new MailMessage
            {
                From = new MailAddress(_settings.Email, _settings.DisplayName),
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            };

            message.To.Add(to);

            using var smtpClient = new SmtpClient(_settings.Host, _settings.Port)
            {
                Credentials = new NetworkCredential(_settings.Email, _settings.Password),
                EnableSsl = true
            };

            await smtpClient.SendMailAsync(message);

            _logger.LogInformation("Email sent to {Email}", to);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email}", to);
            throw new ServerException("Failed to send email.");
        }
    }

    public Task SendEmailConfirmationAsync(string to, string otp)
    {
        var htmlBody = BuildEmailTemplate(
            ConfirmationTitle,
            ConfirmationIntro,
            $"<div style=\"font-size: 28px; font-weight: bold; margin: 16px 0;\">{otp}</div>",
            string.Format(ConfirmationExpiryText, OtpExpiryMinutes));

        return SendEmailAsync(to, ConfirmationSubject, htmlBody);
    }

    public Task SendPasswordResetAsync(string to, string resetLink)
    {
        var buttonMarkup = $"<a href=\"{resetLink}\" style=\"display: inline-block; padding: 12px 24px; background-color: #2563eb; color: #ffffff; text-decoration: none; border-radius: 6px; margin: 16px 0;\">{ButtonText}</a>";

        var htmlBody = BuildEmailTemplate(
            ResetTitle,
            ResetIntro,
            buttonMarkup,
            ResetExpiryText);

        return SendEmailAsync(to, ResetSubject, htmlBody);
    }

    private static string BuildEmailTemplate(string title, string intro, string content, string footer)
    {
        return $"""
            <div style=\"font-family: Arial, sans-serif; background-color: #f5f5f5; padding: 24px;\">
                <div style=\"max-width: 600px; margin: 0 auto; background-color: #ffffff; padding: 24px; border-radius: 8px;\">
                    <h2 style=\"margin-top: 0; color: #111827;\">{title}</h2>
                    <p style=\"color: #374151;\">{intro}</p>
                    <div>{content}</div>
                    <p style=\"color: #6b7280;\">{footer}</p>
                    <hr style=\"border: none; border-top: 1px solid #e5e7eb; margin: 24px 0;\" />
                    <p style=\"color: #9ca3af; font-size: 12px;\">{FooterText}</p>
                </div>
            </div>
            """;
    }
}

using System.Net;
using System.Net.Mail;

using Fanda.UserManagement.Api.Models;

namespace Fanda.UserManagement.Api.Services;

public interface IEmailService
{
    public Task SendPasswordResetEmailAsync(string email, string resetToken);
    public Task SendWelcomeEmailAsync(string email, string firstName);
}

public class EmailService : IEmailService
{
    private readonly EmailSettings _emailSettings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(EmailSettings emailSettings, ILogger<EmailService> logger)
    {
        _emailSettings = emailSettings;
        _logger = logger;
    }

    public async Task SendPasswordResetEmailAsync(string email, string resetToken)
    {
        var subject = "Password Reset Request";
        var body = $@"
            <h2>Password Reset Request</h2>
            <p>You have requested to reset your password. Please use the following token to reset your password:</p>
            <p><strong>Reset Token:</strong> {resetToken}</p>
            <p>This token will expire in 24 hours.</p>
            <p>If you did not request this password reset, please ignore this email.</p>
        ";

        await SendEmailAsync(email, subject, body);
    }

    public async Task SendWelcomeEmailAsync(string email, string firstName)
    {
        var subject = "Welcome to Fanda User Management";
        var body = $@"
            <h2>Welcome, {firstName}!</h2>
            <p>Thank you for joining Fanda User Management system.</p>
            <p>Your account has been successfully created and you can now start using our services.</p>
            <p>If you have any questions or need assistance, please don't hesitate to contact our support team.</p>
            <p>Best regards,<br>The Fanda Team</p>
        ";

        await SendEmailAsync(email, subject, body);
    }

    private async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        try
        {
            using var client = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.SmtpPort);
            client.UseDefaultCredentials = false;
            client.Credentials = new NetworkCredential(_emailSettings.SmtpUsername, _emailSettings.SmtpPassword);
            client.EnableSsl = true;

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_emailSettings.FromEmail, _emailSettings.FromName),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            mailMessage.To.Add(toEmail);

            await client.SendMailAsync(mailMessage);

            _logger.LogInformation("Email sent successfully to {Email}", toEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email}", toEmail);
            // Don't throw - email failures shouldn't break the authentication flow
        }
    }
}

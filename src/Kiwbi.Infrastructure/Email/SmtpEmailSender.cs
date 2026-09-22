using System.Net;
using System.Net.Mail;
using Kiwbi.Application.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Kiwbi.Infrastructure.Email;

/// <summary>Sends real transactional emails via SMTP (Brevo in production - Epic 11, Feature 11.5), replacing the
/// dev-only logging adapter. Config keys: Email:Smtp:Host, Email:Smtp:Port, Email:Smtp:Username, Email:Smtp:Password,
/// Email:Smtp:FromEmail, Email:Smtp:FromName (optional, defaults to "Kiwbi").</summary>
public class SmtpEmailSender : IEmailSender
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<SmtpEmailSender> _logger;

    public SmtpEmailSender(IConfiguration configuration, ILogger<SmtpEmailSender> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public Task SendBuyerInvitationEmailAsync(
        string email,
        string token,
        DateTime expiresAtUtc,
        CancellationToken cancellationToken = default)
    {
        var appBaseUrl = _configuration["AppBaseUrl"]?.TrimEnd('/') ?? string.Empty;
        var acceptUrl = $"{appBaseUrl}/Onboarding/Accept?token={Uri.EscapeDataString(token)}";

        var body = $"""
            <p>Has recibido una invitación para acceder a tu vivienda en Kiwbi.</p>
            <p><a href="{acceptUrl}">Aceptar invitación y crear mi cuenta</a></p>
            <p>Este enlace caduca el {expiresAtUtc:d 'de' MMMM 'de' yyyy}.</p>
            """;

        return SendAsync(email, "Te han invitado a Kiwbi", body, cancellationToken);
    }

    public Task SendPasswordResetEmailAsync(string email, string token, CancellationToken cancellationToken = default)
    {
        var appBaseUrl = _configuration["AppBaseUrl"]?.TrimEnd('/') ?? string.Empty;
        var resetUrl = $"{appBaseUrl}/Account/ResetPassword?email={Uri.EscapeDataString(email)}&token={Uri.EscapeDataString(token)}";

        var body = $"""
            <p>Recibimos una solicitud para restablecer tu contraseña de Kiwbi.</p>
            <p><a href="{resetUrl}">Elegir una nueva contraseña</a></p>
            <p>Si no has sido tú, puedes ignorar este correo.</p>
            """;

        return SendAsync(email, "Restablece tu contraseña de Kiwbi", body, cancellationToken);
    }

    private async Task SendAsync(string toEmail, string subject, string htmlBody, CancellationToken cancellationToken)
    {
        var host = RequireConfig("Email:Smtp:Host");
        var port = int.Parse(RequireConfig("Email:Smtp:Port"));
        var username = RequireConfig("Email:Smtp:Username");
        var password = RequireConfig("Email:Smtp:Password");
        var fromEmail = RequireConfig("Email:Smtp:FromEmail");
        var fromName = _configuration["Email:Smtp:FromName"] ?? "Kiwbi";

        using var client = new SmtpClient(host, port)
        {
            Credentials = new NetworkCredential(username, password),
            EnableSsl = true,
            Timeout = 15000, // fail fast instead of SmtpClient's 100s default - a hang almost always means the SMTP port is unreachable, not a slow-but-working send.
        };

        using var message = new MailMessage
        {
            From = new MailAddress(fromEmail, fromName),
            Subject = subject,
            Body = htmlBody,
            IsBodyHtml = true,
        };
        message.To.Add(toEmail);

        _logger.LogInformation("Sending email to {Email} with subject {Subject}", toEmail, subject);

        try
        {
            await client.SendMailAsync(message, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email} via {Host}:{Port}", toEmail, host, port);
            throw;
        }
    }

    private string RequireConfig(string key) =>
        _configuration[key] ?? throw new InvalidOperationException($"Falta la configuración obligatoria '{key}' para el envío de correo SMTP.");
}

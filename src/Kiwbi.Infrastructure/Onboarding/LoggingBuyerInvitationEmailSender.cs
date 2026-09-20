using Kiwbi.Application.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Kiwbi.Infrastructure.Onboarding;

/// <summary>
/// Development-only IEmailSender adapter: logs the Magic Link instead of sending a real email.
/// Replace with an SMTP/transactional provider adapter when needed, without changing Application.
/// </summary>
public class LoggingBuyerInvitationEmailSender : IEmailSender
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<LoggingBuyerInvitationEmailSender> _logger;

    public LoggingBuyerInvitationEmailSender(IConfiguration configuration, ILogger<LoggingBuyerInvitationEmailSender> logger)
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
        var acceptUrl = $"{appBaseUrl}/Onboarding/Accept?token={token}";

        _logger.LogInformation(
            "[DEV EMAIL STUB] Magic Link invitation for {Email}, expires {ExpiresAtUtc:u}: {AcceptUrl}",
            email, expiresAtUtc, acceptUrl);

        return Task.CompletedTask;
    }

    public Task SendPasswordResetEmailAsync(string email, string token, CancellationToken cancellationToken = default)
    {
        var appBaseUrl = _configuration["AppBaseUrl"]?.TrimEnd('/') ?? string.Empty;
        var encodedToken = Uri.EscapeDataString(token);
        var encodedEmail = Uri.EscapeDataString(email);
        var resetUrl = $"{appBaseUrl}/Account/ResetPassword?email={encodedEmail}&token={encodedToken}";

        _logger.LogInformation("[DEV EMAIL STUB] Password reset for {Email}: {ResetUrl}", email, resetUrl);

        return Task.CompletedTask;
    }
}

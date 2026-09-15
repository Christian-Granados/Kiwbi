namespace Kiwbi.Application.Common;

/// <summary>Port for sending transactional emails, isolated from the concrete provider (SMTP, SendGrid, etc.).</summary>
public interface IEmailSender
{
    /// <summary>Sends the Magic Link invitation email for a buyer to join a HousingUnit.</summary>
    Task SendBuyerInvitationEmailAsync(
        string email,
        string token,
        DateTime expiresAtUtc,
        CancellationToken cancellationToken = default);
}

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

    /// <summary>Sends the password reset email (Epic 11, Feature 11.2), shared by both Promotora and Comprador accounts.</summary>
    Task SendPasswordResetEmailAsync(
        string email,
        string token,
        CancellationToken cancellationToken = default);
}

using System.Security.Cryptography;
using Kiwbi.Domain.Common;
using Kiwbi.Domain.Exceptions;

namespace Kiwbi.Domain.Onboarding;

/// <summary>Entity representing a pending Magic Link invitation sent to an email to become a buyer of a HousingUnit.</summary>
public class BuyerInvitation : BaseEntity
{
    private static readonly TimeSpan ValidityPeriod = TimeSpan.FromDays(7);

    public Guid HousingUnitId { get; private set; }
    public string Email { get; private set; } = null!;
    public string Token { get; private set; } = null!;
    public BuyerInvitationStatus Status { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }
    public DateTime ExpiresAtUtc { get; private set; }
    public DateTime? AcceptedAtUtc { get; private set; }
    public string? AcceptedByUserId { get; private set; }

    private BuyerInvitation()
    {
    }

    public static BuyerInvitation Create(Guid housingUnitId, string email)
    {
        if (housingUnitId == Guid.Empty)
        {
            throw new DomainException("La invitación debe pertenecer a una vivienda.");
        }

        var invitation = new BuyerInvitation
        {
            HousingUnitId = housingUnitId,
            Status = BuyerInvitationStatus.Pending,
        };

        invitation.SetEmail(email);
        invitation.CreatedAtUtc = DateTime.UtcNow;
        invitation.Regenerate();

        return invitation;
    }

    public void Resend()
    {
        if (Status != BuyerInvitationStatus.Pending)
        {
            throw new DomainException("Solo se puede reenviar una invitación pendiente.");
        }

        Regenerate();
    }

    public void Cancel()
    {
        if (Status != BuyerInvitationStatus.Pending)
        {
            throw new DomainException("Solo se puede cancelar una invitación pendiente.");
        }

        Status = BuyerInvitationStatus.Cancelled;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void MarkAccepted(string acceptedByUserId, DateTime utcNow)
    {
        if (Status != BuyerInvitationStatus.Pending)
        {
            throw new DomainException("Solo se puede aceptar una invitación pendiente.");
        }

        if (IsExpired(utcNow))
        {
            throw new DomainException("La invitación ha caducado.");
        }

        if (string.IsNullOrWhiteSpace(acceptedByUserId))
        {
            throw new DomainException("El usuario que acepta la invitación es obligatorio.");
        }

        Status = BuyerInvitationStatus.Accepted;
        AcceptedAtUtc = utcNow;
        AcceptedByUserId = acceptedByUserId;
        UpdatedAtUtc = utcNow;
    }

    public bool IsExpired(DateTime utcNow) => Status == BuyerInvitationStatus.Pending && utcNow > ExpiresAtUtc;

    private void SetEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new DomainException("El correo electrónico del comprador es obligatorio.");
        }

        var trimmed = email.Trim();

        if (!trimmed.Contains('@') || trimmed.StartsWith('@') || trimmed.EndsWith('@'))
        {
            throw new DomainException("El correo electrónico del comprador no es válido.");
        }

        Email = trimmed;
    }

    private void Regenerate()
    {
        Token = GenerateToken();
        UpdatedAtUtc = DateTime.UtcNow;
        ExpiresAtUtc = UpdatedAtUtc.Add(ValidityPeriod);
    }

    private static string GenerateToken() =>
        Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');
}

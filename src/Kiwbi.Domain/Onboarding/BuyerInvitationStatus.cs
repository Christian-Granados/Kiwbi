namespace Kiwbi.Domain.Onboarding;

/// <summary>Lifecycle of a BuyerInvitation (Magic Link).</summary>
public enum BuyerInvitationStatus
{
    Pending,
    Accepted,
    Cancelled,
}

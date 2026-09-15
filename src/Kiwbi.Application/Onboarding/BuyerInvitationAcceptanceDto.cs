using Kiwbi.Domain.Onboarding;

namespace Kiwbi.Application.Onboarding;

/// <summary>Presentation-oriented view of a BuyerInvitation resolved by token, for the anonymous acceptance screen.</summary>
public sealed record BuyerInvitationAcceptanceDto(
    Guid HousingUnitId,
    string Email,
    string Floor,
    string Door,
    string HousingPromotionName,
    BuyerInvitationStatus Status,
    bool IsExpired,
    bool AccountAlreadyExists)
{
    public bool CanAccept => Status == BuyerInvitationStatus.Pending && !IsExpired;
}

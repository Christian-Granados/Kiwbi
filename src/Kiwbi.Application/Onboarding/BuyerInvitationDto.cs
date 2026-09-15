using Kiwbi.Domain.Onboarding;

namespace Kiwbi.Application.Onboarding;

public sealed record BuyerInvitationDto(
    Guid Id,
    Guid HousingUnitId,
    string Email,
    BuyerInvitationStatus Status,
    DateTime CreatedAtUtc,
    DateTime ExpiresAtUtc,
    bool IsExpired,
    DateTime? AcceptedAtUtc);

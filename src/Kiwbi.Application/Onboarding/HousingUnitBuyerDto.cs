using Kiwbi.Domain.Onboarding;

namespace Kiwbi.Application.Onboarding;

public sealed record HousingUnitBuyerDto(
    Guid Id,
    Guid HousingUnitId,
    string BuyerUserId,
    string? Email,
    DateTime CreatedAtUtc,
    bool HasConfirmedOrPaidChoices = false);

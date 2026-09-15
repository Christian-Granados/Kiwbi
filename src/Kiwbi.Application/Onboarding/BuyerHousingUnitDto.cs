namespace Kiwbi.Application.Onboarding;

public sealed record BuyerHousingUnitDto(
    Guid HousingUnitId,
    Guid HousingPromotionId,
    string HousingPromotionName,
    string City,
    string Floor,
    string Door,
    string? FloorPlanImagePath);

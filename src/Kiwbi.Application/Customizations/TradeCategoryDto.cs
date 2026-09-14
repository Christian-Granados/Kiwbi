namespace Kiwbi.Application.Customizations;

public sealed record TradeCategoryDto(
    Guid Id,
    Guid HousingPromotionId,
    string Name,
    DateTime SelectionCutOffDateUtc,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);

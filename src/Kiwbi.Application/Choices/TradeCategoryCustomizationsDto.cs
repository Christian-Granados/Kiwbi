namespace Kiwbi.Application.Choices;

public sealed record TradeCategoryCustomizationsDto(
    Guid TradeCategoryId,
    string TradeCategoryName,
    DateTime SelectionCutOffDateUtc,
    bool IsExpired,
    IReadOnlyList<CustomizationForBuyerDto> Customizations);

namespace Kiwbi.Application.Choices;

public sealed record TradeCategoryChoicesDto(
    Guid TradeCategoryId,
    string TradeCategoryName,
    DateTime SelectionCutOffDateUtc,
    bool IsExpired,
    IReadOnlyList<CustomizationChoiceDto> Customizations);

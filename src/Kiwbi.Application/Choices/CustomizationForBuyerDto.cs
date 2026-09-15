namespace Kiwbi.Application.Choices;

public sealed record CustomizationForBuyerDto(
    Guid Id,
    string Name,
    bool CanSelect,
    Guid? SelectedOptionId,
    Guid? EffectiveOptionId,
    IReadOnlyList<CustomizationOptionForBuyerDto> Options);

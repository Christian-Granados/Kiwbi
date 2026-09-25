namespace Kiwbi.Application.Choices;

public sealed record CustomizationOptionForBuyerDto(
    Guid Id,
    string Name,
    decimal SurchargeAmount,
    bool IsDefault,
    bool IsEffectiveSelection,
    string? ThumbnailImagePath);

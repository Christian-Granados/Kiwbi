using Kiwbi.Domain.Choices;

namespace Kiwbi.Application.Choices;

public sealed record CustomizationChoiceDto(
    Guid CustomizationId,
    string CustomizationName,
    HomeCustomizationChoiceStatus Status,
    Guid? EffectiveOptionId,
    string? EffectiveOptionName,
    decimal? EffectiveOptionSurchargeAmount);

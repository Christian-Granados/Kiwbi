namespace Kiwbi.Application.Choices;

public sealed record HousingPromotionChoicesProgressDto(
    IReadOnlyList<HousingUnitChoicesProgressDto> Units);

namespace Kiwbi.Application.RealEstate;

public sealed record HousingPromotionSummaryDto(
    HousingPromotionDto Promotion,
    IReadOnlyList<HousingUnitSummaryItemDto> Units);

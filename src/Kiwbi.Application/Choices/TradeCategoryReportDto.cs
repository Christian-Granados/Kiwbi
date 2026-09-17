namespace Kiwbi.Application.Choices;

public sealed record TradeCategoryReportDto(
    string TradeCategoryName,
    IReadOnlyList<HousingUnitReportDto> HousingUnits);

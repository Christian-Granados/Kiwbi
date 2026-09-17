namespace Kiwbi.Application.Choices;

public sealed record HousingUnitReportDto(
    string Floor,
    string Door,
    IReadOnlyList<CustomizationChoiceDto> Customizations);

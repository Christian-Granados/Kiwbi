using Kiwbi.Domain.RealEstate;

namespace Kiwbi.Application.RealEstate;

public sealed record HousingUnitSummaryItemDto(
    Guid Id,
    string? TypologyName,
    string Floor,
    string Door,
    decimal BuiltAreaSqm,
    decimal? UsableAreaSqm,
    HousingUnitStatus Status);

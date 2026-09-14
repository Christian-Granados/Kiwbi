using Kiwbi.Domain.RealEstate;

namespace Kiwbi.Application.RealEstate;

public sealed record HousingUnitDto(
    Guid Id,
    Guid HousingPromotionId,
    Guid? HousingTypologyId,
    string Floor,
    string Door,
    decimal BuiltAreaSqm,
    decimal? UsableAreaSqm,
    string? FloorPlanImagePath,
    HousingUnitStatus Status,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);

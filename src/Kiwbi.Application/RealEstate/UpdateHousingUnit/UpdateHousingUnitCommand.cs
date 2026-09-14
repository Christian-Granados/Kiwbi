namespace Kiwbi.Application.RealEstate.UpdateHousingUnit;

public sealed record UpdateHousingUnitCommand(
    Guid Id,
    Guid? HousingTypologyId,
    string Floor,
    string Door,
    decimal BuiltAreaSqm,
    decimal? UsableAreaSqm);

namespace Kiwbi.Application.RealEstate.CreateHousingUnit;

public sealed record CreateHousingUnitCommand(
    Guid HousingPromotionId,
    Guid? HousingTypologyId,
    string Floor,
    string Door,
    decimal BuiltAreaSqm,
    decimal? UsableAreaSqm);

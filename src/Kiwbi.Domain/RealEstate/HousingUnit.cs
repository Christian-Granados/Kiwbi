using Kiwbi.Domain.Common;
using Kiwbi.Domain.Exceptions;

namespace Kiwbi.Domain.RealEstate;

/// <summary>Entity representing a concrete housing unit (Vivienda) within a HousingPromotion, optionally grouped by a HousingTypology.</summary>
public class HousingUnit : BaseEntity
{
    public Guid HousingPromotionId { get; private set; }
    public Guid? HousingTypologyId { get; private set; }
    public string Floor { get; private set; } = null!;
    public string Door { get; private set; } = null!;
    public decimal BuiltAreaSqm { get; private set; }
    public decimal? UsableAreaSqm { get; private set; }
    public string? FloorPlanImagePath { get; private set; }
    public HousingUnitStatus Status { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    private HousingUnit()
    {
    }

    public static HousingUnit Create(
        Guid housingPromotionId,
        string floor,
        string door,
        decimal builtAreaSqm,
        decimal? usableAreaSqm,
        Guid? housingTypologyId = null)
    {
        if (housingPromotionId == Guid.Empty)
        {
            throw new DomainException("La vivienda debe pertenecer a una promoción.");
        }

        var unit = new HousingUnit
        {
            HousingPromotionId = housingPromotionId,
            Status = HousingUnitStatus.Available,
        };

        unit.AssignTypology(housingTypologyId);
        unit.UpdateDetails(floor, door, builtAreaSqm, usableAreaSqm);
        unit.CreatedAtUtc = unit.UpdatedAtUtc;

        return unit;
    }

    public void UpdateDetails(string floor, string door, decimal builtAreaSqm, decimal? usableAreaSqm)
    {
        if (string.IsNullOrWhiteSpace(floor))
        {
            throw new DomainException("La planta de la vivienda es obligatoria.");
        }

        if (string.IsNullOrWhiteSpace(door))
        {
            throw new DomainException("La puerta de la vivienda es obligatoria.");
        }

        if (builtAreaSqm <= 0)
        {
            throw new DomainException("La superficie construida debe ser mayor que cero.");
        }

        if (usableAreaSqm is { } usable && usable > builtAreaSqm)
        {
            throw new DomainException("La superficie útil no puede superar la superficie construida.");
        }

        Floor = floor.Trim();
        Door = door.Trim();
        BuiltAreaSqm = builtAreaSqm;
        UsableAreaSqm = usableAreaSqm;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void AssignTypology(Guid? housingTypologyId)
    {
        HousingTypologyId = housingTypologyId;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void UpdateFloorPlanImage(string? floorPlanImagePath)
    {
        FloorPlanImagePath = string.IsNullOrWhiteSpace(floorPlanImagePath) ? null : floorPlanImagePath.Trim();
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void ChangeStatus(HousingUnitStatus status)
    {
        Status = status;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}

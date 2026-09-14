namespace Kiwbi.Domain.RealEstate;

/// <summary>Commercial status of a HousingUnit, administered manually by the developer company. Independent from buyer onboarding (Epic 4) and customization choices (Epic 3/6).</summary>
public enum HousingUnitStatus
{
    Available,
    Reserved,
    Sold,
}

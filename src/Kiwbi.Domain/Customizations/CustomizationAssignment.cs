using Kiwbi.Domain.Common;

namespace Kiwbi.Domain.Customizations;

/// <summary>Child entity of the Customization aggregate: what the Customization applies to (whole promotion, a typology or a unit).</summary>
public class CustomizationAssignment : BaseEntity
{
    public CustomizationScope Scope { get; private set; }
    public Guid? HousingTypologyId { get; private set; }
    public Guid? HousingUnitId { get; private set; }

    private CustomizationAssignment()
    {
    }

    private CustomizationAssignment(CustomizationScope scope, Guid? housingTypologyId, Guid? housingUnitId)
    {
        Scope = scope;
        HousingTypologyId = housingTypologyId;
        HousingUnitId = housingUnitId;
    }

    internal static CustomizationAssignment ForWholePromotion() => new(CustomizationScope.WholePromotion, null, null);

    internal static CustomizationAssignment ForTypology(Guid housingTypologyId) => new(CustomizationScope.Typology, housingTypologyId, null);

    internal static CustomizationAssignment ForUnit(Guid housingUnitId) => new(CustomizationScope.Unit, null, housingUnitId);
}

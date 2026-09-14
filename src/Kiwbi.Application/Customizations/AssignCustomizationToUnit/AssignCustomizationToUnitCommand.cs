namespace Kiwbi.Application.Customizations.AssignCustomizationToUnit;

public sealed record AssignCustomizationToUnitCommand(Guid CustomizationId, Guid HousingUnitId);

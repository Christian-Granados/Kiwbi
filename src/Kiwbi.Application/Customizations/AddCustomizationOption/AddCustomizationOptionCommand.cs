namespace Kiwbi.Application.Customizations.AddCustomizationOption;

public sealed record AddCustomizationOptionCommand(Guid CustomizationId, string Name, decimal SurchargeAmount);

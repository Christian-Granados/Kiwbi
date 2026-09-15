namespace Kiwbi.Application.Customizations.UpdateCustomizationOption;

public sealed record UpdateCustomizationOptionCommand(Guid CustomizationId, Guid OptionId, string Name, decimal SurchargeAmount);

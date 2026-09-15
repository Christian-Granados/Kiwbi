namespace Kiwbi.Application.Choices.SelectCustomizationOption;

public sealed record SelectCustomizationOptionCommand(Guid HousingUnitId, Guid CustomizationId, Guid CustomizationOptionId);

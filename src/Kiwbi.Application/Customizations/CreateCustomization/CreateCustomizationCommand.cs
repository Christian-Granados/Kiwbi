using Kiwbi.Domain.Customizations;

namespace Kiwbi.Application.Customizations.CreateCustomization;

public sealed record CreateCustomizationCommand(
    Guid TradeCategoryId,
    string Name,
    string DefaultOptionName,
    decimal DefaultOptionSurchargeAmount,
    CustomizationScope Scope,
    IReadOnlyList<Guid> HousingTypologyIds,
    IReadOnlyList<Guid> HousingUnitIds);

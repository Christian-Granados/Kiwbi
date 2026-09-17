using Kiwbi.Domain.Choices;
using Kiwbi.Domain.Customizations;

namespace Kiwbi.Application.Choices;

/// <summary>Builds the promotora-facing state (status + effective option) of a Customization for a HousingUnit, shared by the progress panel and the detail view (Feature 6.1).</summary>
internal static class HomeCustomizationChoiceProgressMapper
{
    public static CustomizationChoiceDto Build(Customization customization, HomeCustomizationChoice? choice, bool isExpired)
    {
        var status = choice?.Status ?? HomeCustomizationChoiceStatus.Pending;
        var defaultOption = customization.Options.First(o => o.IsDefault);
        var effectiveOptionId = choice?.SelectedOptionId ?? (isExpired ? defaultOption.Id : null);
        var effectiveOption = effectiveOptionId is { } optionId
            ? customization.Options.FirstOrDefault(o => o.Id == optionId)
            : null;

        return new CustomizationChoiceDto(
            customization.Id,
            customization.Name,
            status,
            effectiveOptionId,
            effectiveOption?.Name,
            effectiveOption?.SurchargeAmount);
    }
}

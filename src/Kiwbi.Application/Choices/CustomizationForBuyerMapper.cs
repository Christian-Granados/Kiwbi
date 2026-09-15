using Kiwbi.Domain.Choices;
using Kiwbi.Domain.Customizations;

namespace Kiwbi.Application.Choices;

/// <summary>Builds a CustomizationForBuyerDto from domain state, shared by the visualizer and selection use cases.</summary>
internal static class CustomizationForBuyerMapper
{
    public static CustomizationForBuyerDto Build(Customization customization, HomeCustomizationChoice? choice, bool isExpired)
    {
        var canSelect = !isExpired;
        var selectedOptionId = choice?.SelectedOptionId;
        var defaultOptionId = customization.Options.First(o => o.IsDefault).Id;
        var effectiveOptionId = selectedOptionId ?? (isExpired ? defaultOptionId : null);

        var options = customization.Options
            .Select(option => new CustomizationOptionForBuyerDto(
                option.Id,
                option.Name,
                option.SurchargeAmount,
                option.IsDefault,
                option.Id == effectiveOptionId))
            .ToList();

        return new CustomizationForBuyerDto(customization.Id, customization.Name, canSelect, selectedOptionId, effectiveOptionId, options);
    }
}

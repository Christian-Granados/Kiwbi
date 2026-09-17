using Kiwbi.Application.Choices;
using Kiwbi.Domain.Choices;

namespace Kiwbi.Web.Models.HousingPromotionChoices;

public class CustomizationChoiceViewModel
{
    public Guid CustomizationId { get; set; }
    public string CustomizationName { get; set; } = string.Empty;
    public HomeCustomizationChoiceStatus Status { get; set; }
    public string? EffectiveOptionName { get; set; }
    public decimal? EffectiveOptionSurchargeAmount { get; set; }

    public static CustomizationChoiceViewModel FromDto(CustomizationChoiceDto dto) => new()
    {
        CustomizationId = dto.CustomizationId,
        CustomizationName = dto.CustomizationName,
        Status = dto.Status,
        EffectiveOptionName = dto.EffectiveOptionName,
        EffectiveOptionSurchargeAmount = dto.EffectiveOptionSurchargeAmount,
    };
}

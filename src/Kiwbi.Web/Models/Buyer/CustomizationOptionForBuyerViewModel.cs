using Kiwbi.Application.Choices;

namespace Kiwbi.Web.Models.Buyer;

public class CustomizationOptionForBuyerViewModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal SurchargeAmount { get; set; }
    public bool IsDefault { get; set; }
    public bool IsEffectiveSelection { get; set; }

    public static CustomizationOptionForBuyerViewModel FromDto(CustomizationOptionForBuyerDto dto) => new()
    {
        Id = dto.Id,
        Name = dto.Name,
        SurchargeAmount = dto.SurchargeAmount,
        IsDefault = dto.IsDefault,
        IsEffectiveSelection = dto.IsEffectiveSelection,
    };
}

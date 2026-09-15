using Kiwbi.Application.Choices;

namespace Kiwbi.Web.Models.Buyer;

public class TradeCategoryCustomizationsViewModel
{
    public Guid TradeCategoryId { get; set; }
    public string TradeCategoryName { get; set; } = string.Empty;
    public DateTime SelectionCutOffDateUtc { get; set; }
    public bool IsExpired { get; set; }
    public List<CustomizationForBuyerViewModel> Customizations { get; set; } = [];

    public static TradeCategoryCustomizationsViewModel FromDto(TradeCategoryCustomizationsDto dto) => new()
    {
        TradeCategoryId = dto.TradeCategoryId,
        TradeCategoryName = dto.TradeCategoryName,
        SelectionCutOffDateUtc = dto.SelectionCutOffDateUtc,
        IsExpired = dto.IsExpired,
        Customizations = dto.Customizations.Select(CustomizationForBuyerViewModel.FromDto).ToList(),
    };
}

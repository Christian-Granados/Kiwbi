using Kiwbi.Application.Choices;

namespace Kiwbi.Web.Models.HousingPromotionChoices;

public class TradeCategoryChoicesViewModel
{
    public Guid TradeCategoryId { get; set; }
    public string TradeCategoryName { get; set; } = string.Empty;
    public DateTime SelectionCutOffDateUtc { get; set; }
    public bool IsExpired { get; set; }
    public List<CustomizationChoiceViewModel> Customizations { get; set; } = [];

    public static TradeCategoryChoicesViewModel FromDto(TradeCategoryChoicesDto dto) => new()
    {
        TradeCategoryId = dto.TradeCategoryId,
        TradeCategoryName = dto.TradeCategoryName,
        SelectionCutOffDateUtc = dto.SelectionCutOffDateUtc,
        IsExpired = dto.IsExpired,
        Customizations = dto.Customizations.Select(CustomizationChoiceViewModel.FromDto).ToList(),
    };
}

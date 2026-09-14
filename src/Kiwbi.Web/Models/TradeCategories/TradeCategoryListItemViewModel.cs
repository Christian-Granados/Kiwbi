using Kiwbi.Application.Customizations;

namespace Kiwbi.Web.Models.TradeCategories;

public class TradeCategoryListItemViewModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime SelectionCutOffDateUtc { get; set; }

    public static TradeCategoryListItemViewModel FromDto(TradeCategoryDto dto) => new()
    {
        Id = dto.Id,
        Name = dto.Name,
        SelectionCutOffDateUtc = dto.SelectionCutOffDateUtc,
    };
}

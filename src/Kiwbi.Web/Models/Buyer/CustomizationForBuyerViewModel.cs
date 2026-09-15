using Kiwbi.Application.Choices;

namespace Kiwbi.Web.Models.Buyer;

public class CustomizationForBuyerViewModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool CanSelect { get; set; }
    public Guid? SelectedOptionId { get; set; }
    public Guid? EffectiveOptionId { get; set; }
    public List<CustomizationOptionForBuyerViewModel> Options { get; set; } = [];

    public static CustomizationForBuyerViewModel FromDto(CustomizationForBuyerDto dto) => new()
    {
        Id = dto.Id,
        Name = dto.Name,
        CanSelect = dto.CanSelect,
        SelectedOptionId = dto.SelectedOptionId,
        EffectiveOptionId = dto.EffectiveOptionId,
        Options = dto.Options.Select(CustomizationOptionForBuyerViewModel.FromDto).ToList(),
    };
}

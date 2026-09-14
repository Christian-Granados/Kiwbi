using Kiwbi.Application.RealEstate;

namespace Kiwbi.Web.Models.HousingPromotions;

public class HousingPromotionListItemViewModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;

    public static HousingPromotionListItemViewModel FromDto(HousingPromotionDto dto) => new()
    {
        Id = dto.Id,
        Name = dto.Name,
        City = dto.City,
        Address = dto.Address,
    };
}

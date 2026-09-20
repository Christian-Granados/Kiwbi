using Kiwbi.Application.RealEstate;

namespace Kiwbi.Web.Models.HousingTypologies;

public class HousingTypologyListItemViewModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int UnitCount { get; set; }

    public static HousingTypologyListItemViewModel FromDto(HousingTypologyDto dto) => new()
    {
        Id = dto.Id,
        Name = dto.Name,
        UnitCount = dto.UnitCount,
    };
}

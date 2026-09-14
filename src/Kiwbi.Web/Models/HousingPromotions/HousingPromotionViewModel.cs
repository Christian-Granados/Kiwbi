using Kiwbi.Application.RealEstate;

namespace Kiwbi.Web.Models.HousingPromotions;

public class HousingPromotionViewModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string? MasterPlanImagePath { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }

    public static HousingPromotionViewModel FromDto(HousingPromotionDto dto) => new()
    {
        Id = dto.Id,
        Name = dto.Name,
        City = dto.City,
        Address = dto.Address,
        MasterPlanImagePath = dto.MasterPlanImagePath,
        CreatedAtUtc = dto.CreatedAtUtc,
        UpdatedAtUtc = dto.UpdatedAtUtc,
    };
}

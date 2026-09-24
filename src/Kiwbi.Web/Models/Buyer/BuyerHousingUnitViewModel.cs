using Kiwbi.Application.Onboarding;

namespace Kiwbi.Web.Models.Buyer;

public class BuyerHousingUnitViewModel
{
    public Guid HousingUnitId { get; set; }
    public string HousingPromotionName { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Floor { get; set; } = string.Empty;
    public string Door { get; set; } = string.Empty;
    public string? FloorPlanImagePath { get; set; }
    public string? MasterPlanImagePath { get; set; }

    public static BuyerHousingUnitViewModel FromDto(BuyerHousingUnitDto dto) => new()
    {
        HousingUnitId = dto.HousingUnitId,
        HousingPromotionName = dto.HousingPromotionName,
        City = dto.City,
        Floor = dto.Floor,
        Door = dto.Door,
        FloorPlanImagePath = dto.FloorPlanImagePath,
        MasterPlanImagePath = dto.MasterPlanImagePath,
    };
}

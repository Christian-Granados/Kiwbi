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
    public List<HousingPromotionUnitSummaryItemViewModel> Units { get; set; } = [];

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

    public static HousingPromotionViewModel FromSummaryDto(HousingPromotionSummaryDto summary)
    {
        var model = FromDto(summary.Promotion);
        model.Units = summary.Units.Select(HousingPromotionUnitSummaryItemViewModel.FromDto).ToList();
        return model;
    }
}

public class HousingPromotionUnitSummaryItemViewModel
{
    public Guid Id { get; set; }
    public string? TypologyName { get; set; }
    public string Floor { get; set; } = string.Empty;
    public string Door { get; set; } = string.Empty;
    public decimal BuiltAreaSqm { get; set; }
    public decimal? UsableAreaSqm { get; set; }
    public string Status { get; set; } = string.Empty;

    public static HousingPromotionUnitSummaryItemViewModel FromDto(HousingUnitSummaryItemDto dto) => new()
    {
        Id = dto.Id,
        TypologyName = dto.TypologyName,
        Floor = dto.Floor,
        Door = dto.Door,
        BuiltAreaSqm = dto.BuiltAreaSqm,
        UsableAreaSqm = dto.UsableAreaSqm,
        Status = dto.Status.ToString(),
    };
}

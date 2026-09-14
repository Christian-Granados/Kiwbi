using Kiwbi.Application.RealEstate;

namespace Kiwbi.Web.Models.HousingUnits;

public class HousingUnitListItemViewModel
{
    public Guid Id { get; set; }
    public string Floor { get; set; } = string.Empty;
    public string Door { get; set; } = string.Empty;
    public string? TypologyName { get; set; }
    public decimal BuiltAreaSqm { get; set; }
    public decimal? UsableAreaSqm { get; set; }
    public string Status { get; set; } = string.Empty;

    public static HousingUnitListItemViewModel FromDto(HousingUnitDto dto, string? typologyName) => new()
    {
        Id = dto.Id,
        Floor = dto.Floor,
        Door = dto.Door,
        TypologyName = typologyName,
        BuiltAreaSqm = dto.BuiltAreaSqm,
        UsableAreaSqm = dto.UsableAreaSqm,
        Status = dto.Status.ToString(),
    };
}

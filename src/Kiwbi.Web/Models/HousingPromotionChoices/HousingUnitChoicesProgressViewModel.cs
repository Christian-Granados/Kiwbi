using Kiwbi.Application.Choices;

namespace Kiwbi.Web.Models.HousingPromotionChoices;

public class HousingUnitChoicesProgressViewModel
{
    public Guid HousingUnitId { get; set; }
    public string? TypologyName { get; set; }
    public string Floor { get; set; } = string.Empty;
    public string Door { get; set; } = string.Empty;
    public int TotalCount { get; set; }
    public int PendingCount { get; set; }
    public int SelectedCount { get; set; }
    public int ConfirmedCount { get; set; }
    public int PaidCount { get; set; }

    public static HousingUnitChoicesProgressViewModel FromDto(HousingUnitChoicesProgressDto dto) => new()
    {
        HousingUnitId = dto.HousingUnitId,
        TypologyName = dto.TypologyName,
        Floor = dto.Floor,
        Door = dto.Door,
        TotalCount = dto.TotalCount,
        PendingCount = dto.PendingCount,
        SelectedCount = dto.SelectedCount,
        ConfirmedCount = dto.ConfirmedCount,
        PaidCount = dto.PaidCount,
    };
}

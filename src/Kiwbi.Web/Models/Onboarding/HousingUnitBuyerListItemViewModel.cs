using Kiwbi.Application.Onboarding;

namespace Kiwbi.Web.Models.Onboarding;

public class HousingUnitBuyerListItemViewModel
{
    public Guid Id { get; set; }
    public string? Email { get; set; }
    public DateTime CreatedAtUtc { get; set; }

    public static HousingUnitBuyerListItemViewModel FromDto(HousingUnitBuyerDto dto) => new()
    {
        Id = dto.Id,
        Email = dto.Email,
        CreatedAtUtc = dto.CreatedAtUtc,
    };
}

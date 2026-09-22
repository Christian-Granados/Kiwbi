using Kiwbi.Application.Developers.GetCurrentDeveloperProfile;
using Kiwbi.Application.Developers.GetDeveloperCompanyOverview;

namespace Kiwbi.Web.Models.DeveloperProfile;

public class DeveloperProfileViewModel
{
    public string Name { get; set; } = string.Empty;
    public string? LogoPath { get; set; }
    public string PrimaryColor { get; set; } = string.Empty;
    public string? SecondaryColor { get; set; }
    public int PromotionsCount { get; set; }
    public int HousingUnitsCount { get; set; }
    public int LinkedBuyersCount { get; set; }
    public int PendingInvitationsCount { get; set; }

    public static DeveloperProfileViewModel FromDto(DeveloperProfileDto dto, DeveloperCompanyOverviewDto overview) => new()
    {
        Name = dto.Name,
        LogoPath = dto.LogoPath,
        PrimaryColor = dto.PrimaryColor,
        SecondaryColor = dto.SecondaryColor,
        PromotionsCount = overview.PromotionsCount,
        HousingUnitsCount = overview.HousingUnitsCount,
        LinkedBuyersCount = overview.LinkedBuyersCount,
        PendingInvitationsCount = overview.PendingInvitationsCount,
    };
}

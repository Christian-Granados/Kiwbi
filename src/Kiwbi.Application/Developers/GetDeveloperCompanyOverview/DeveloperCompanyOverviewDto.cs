namespace Kiwbi.Application.Developers.GetDeveloperCompanyOverview;

public sealed record DeveloperCompanyOverviewDto(
    int PromotionsCount,
    int HousingUnitsCount,
    int LinkedBuyersCount,
    int PendingInvitationsCount);

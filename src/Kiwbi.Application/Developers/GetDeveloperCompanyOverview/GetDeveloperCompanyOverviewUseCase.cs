using Kiwbi.Application.Common;
using Kiwbi.Domain.Onboarding;
using Kiwbi.Domain.RealEstate;

namespace Kiwbi.Application.Developers.GetDeveloperCompanyOverview;

/// <summary>Aggregate counters for the "Mi promotora" screen (Feature 11.10-ish enrichment): total promotions/units
/// across the whole tenant, plus how many buyers are already linked and how many invitations are still pending.
/// Deliberately loops promotions/units in memory (N+1 across promotions) rather than a single cross-aggregate SQL
/// query - same "admin panel scale" tradeoff already accepted elsewhere (e.g. Epic 7's CustomizationsController).</summary>
public class GetDeveloperCompanyOverviewUseCase
{
    private readonly ICurrentUser _currentUser;
    private readonly IHousingPromotionRepository _housingPromotionRepository;
    private readonly IHousingUnitRepository _housingUnitRepository;
    private readonly IHousingUnitBuyerRepository _housingUnitBuyerRepository;
    private readonly IBuyerInvitationRepository _buyerInvitationRepository;

    public GetDeveloperCompanyOverviewUseCase(
        ICurrentUser currentUser,
        IHousingPromotionRepository housingPromotionRepository,
        IHousingUnitRepository housingUnitRepository,
        IHousingUnitBuyerRepository housingUnitBuyerRepository,
        IBuyerInvitationRepository buyerInvitationRepository)
    {
        _currentUser = currentUser;
        _housingPromotionRepository = housingPromotionRepository;
        _housingUnitRepository = housingUnitRepository;
        _housingUnitBuyerRepository = housingUnitBuyerRepository;
        _buyerInvitationRepository = buyerInvitationRepository;
    }

    public async Task<Result<DeveloperCompanyOverviewDto>> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        if (_currentUser.DeveloperCompanyId is not { } developerCompanyId)
        {
            return Result.Failure<DeveloperCompanyOverviewDto>("El usuario actual no está vinculado a ninguna promotora.");
        }

        var promotions = await _housingPromotionRepository.GetByDeveloperCompanyIdAsync(developerCompanyId, cancellationToken);

        var unitIds = new List<Guid>();

        foreach (var promotion in promotions)
        {
            var units = await _housingUnitRepository.GetByHousingPromotionIdAsync(promotion.Id, cancellationToken);
            unitIds.AddRange(units.Select(u => u.Id));
        }

        var linkedBuyersCount = 0;
        var pendingInvitationsCount = 0;

        if (unitIds.Count > 0)
        {
            var housingUnitBuyers = await _housingUnitBuyerRepository.GetByHousingUnitIdsAsync(unitIds, cancellationToken);
            linkedBuyersCount = housingUnitBuyers.Select(b => b.BuyerUserId).Distinct().Count();

            var invitations = await _buyerInvitationRepository.GetByHousingUnitIdsAsync(unitIds, cancellationToken);
            var utcNow = DateTime.UtcNow;
            pendingInvitationsCount = invitations.Count(i => i.Status == BuyerInvitationStatus.Pending && !i.IsExpired(utcNow));
        }

        return Result.Success(new DeveloperCompanyOverviewDto(promotions.Count, unitIds.Count, linkedBuyersCount, pendingInvitationsCount));
    }
}

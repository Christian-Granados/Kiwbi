using FluentAssertions;
using Kiwbi.Application.Common;
using Kiwbi.Application.Developers.GetDeveloperCompanyOverview;
using Kiwbi.Domain.Onboarding;
using Kiwbi.Domain.RealEstate;
using NSubstitute;

namespace Kiwbi.Application.Tests.Developers.GetDeveloperCompanyOverview;

public class GetDeveloperCompanyOverviewUseCaseTests
{
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();
    private readonly IHousingPromotionRepository _housingPromotionRepository = Substitute.For<IHousingPromotionRepository>();
    private readonly IHousingUnitRepository _housingUnitRepository = Substitute.For<IHousingUnitRepository>();
    private readonly IHousingUnitBuyerRepository _housingUnitBuyerRepository = Substitute.For<IHousingUnitBuyerRepository>();
    private readonly IBuyerInvitationRepository _buyerInvitationRepository = Substitute.For<IBuyerInvitationRepository>();
    private readonly GetDeveloperCompanyOverviewUseCase _useCase;

    public GetDeveloperCompanyOverviewUseCaseTests()
    {
        _useCase = new GetDeveloperCompanyOverviewUseCase(
            _currentUser,
            _housingPromotionRepository,
            _housingUnitRepository,
            _housingUnitBuyerRepository,
            _buyerInvitationRepository);
    }

    [Fact]
    public async Task ExecuteAsync_WhenUserHasNoTenant_ShouldReturnFailure()
    {
        _currentUser.DeveloperCompanyId.Returns((Guid?)null);

        var result = await _useCase.ExecuteAsync();

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteAsync_WhenNoPromotionsExist_ShouldReturnAllZeros()
    {
        var developerCompanyId = Guid.NewGuid();
        _currentUser.DeveloperCompanyId.Returns(developerCompanyId);
        _housingPromotionRepository.GetByDeveloperCompanyIdAsync(developerCompanyId, Arg.Any<CancellationToken>())
            .Returns(new List<HousingPromotion>());

        var result = await _useCase.ExecuteAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(new DeveloperCompanyOverviewDto(0, 0, 0, 0));
        await _housingUnitBuyerRepository.DidNotReceive().GetByHousingUnitIdsAsync(Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenPromotionsAndUnitsExist_ShouldAggregateAcrossAllPromotions()
    {
        var developerCompanyId = Guid.NewGuid();
        var promotionA = HousingPromotion.Create(developerCompanyId, "Residencial A", "Madrid", "Calle A, 1");
        var promotionB = HousingPromotion.Create(developerCompanyId, "Residencial B", "Madrid", "Calle B, 2");
        var unitA1 = HousingUnit.Create(promotionA.Id, "1", "A", 80m, null);
        var unitA2 = HousingUnit.Create(promotionA.Id, "1", "B", 80m, null);
        var unitB1 = HousingUnit.Create(promotionB.Id, "1", "A", 80m, null);

        _currentUser.DeveloperCompanyId.Returns(developerCompanyId);
        _housingPromotionRepository.GetByDeveloperCompanyIdAsync(developerCompanyId, Arg.Any<CancellationToken>())
            .Returns(new List<HousingPromotion> { promotionA, promotionB });
        _housingUnitRepository.GetByHousingPromotionIdAsync(promotionA.Id, Arg.Any<CancellationToken>())
            .Returns(new List<HousingUnit> { unitA1, unitA2 });
        _housingUnitRepository.GetByHousingPromotionIdAsync(promotionB.Id, Arg.Any<CancellationToken>())
            .Returns(new List<HousingUnit> { unitB1 });

        // Two units linked to the SAME buyer (should count as 1 distinct linked buyer) + one unit with no buyer.
        var sharedBuyerId = "buyer-1";
        var buyerLinkA1 = HousingUnitBuyer.Create(unitA1.Id, sharedBuyerId);
        var buyerLinkA2 = HousingUnitBuyer.Create(unitA2.Id, sharedBuyerId);

        // One truly pending invitation, one expired-pending ("Caducada", must NOT count), one cancelled.
        var pendingInvitation = BuyerInvitation.Create(unitB1.Id, "pending@example.com");
        var expiredInvitation = BuyerInvitation.Create(unitA1.Id, "expired@example.com");
        typeof(BuyerInvitation).GetProperty(nameof(BuyerInvitation.ExpiresAtUtc))!
            .SetValue(expiredInvitation, DateTime.UtcNow.AddDays(-1));
        var cancelledInvitation = BuyerInvitation.Create(unitA2.Id, "cancelled@example.com");
        cancelledInvitation.Cancel();
        _housingUnitBuyerRepository
            .GetByHousingUnitIdsAsync(Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(new List<HousingUnitBuyer> { buyerLinkA1, buyerLinkA2 });
        _buyerInvitationRepository
            .GetByHousingUnitIdsAsync(Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(new List<BuyerInvitation> { pendingInvitation, expiredInvitation, cancelledInvitation });

        var result = await _useCase.ExecuteAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(new DeveloperCompanyOverviewDto(
            PromotionsCount: 2,
            HousingUnitsCount: 3,
            LinkedBuyersCount: 1,
            PendingInvitationsCount: 1));
    }
}

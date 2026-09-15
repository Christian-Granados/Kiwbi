using FluentAssertions;
using Kiwbi.Application.Common;
using Kiwbi.Application.Onboarding.GetBuyerInvitationsForHousingUnit;
using Kiwbi.Domain.Onboarding;
using Kiwbi.Domain.RealEstate;
using NSubstitute;

namespace Kiwbi.Application.Tests.Onboarding.GetBuyerInvitationsForHousingUnit;

public class GetBuyerInvitationsForHousingUnitUseCaseTests
{
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();
    private readonly IHousingPromotionRepository _promotionRepository = Substitute.For<IHousingPromotionRepository>();
    private readonly IHousingUnitRepository _unitRepository = Substitute.For<IHousingUnitRepository>();
    private readonly IBuyerInvitationRepository _invitationRepository = Substitute.For<IBuyerInvitationRepository>();
    private readonly GetBuyerInvitationsForHousingUnitUseCase _useCase;

    public GetBuyerInvitationsForHousingUnitUseCaseTests()
    {
        _useCase = new GetBuyerInvitationsForHousingUnitUseCase(_currentUser, _promotionRepository, _unitRepository, _invitationRepository);
    }

    [Fact]
    public async Task ExecuteAsync_WithOwnedUnit_ShouldReturnInvitations()
    {
        var developerCompanyId = Guid.NewGuid();
        var promotion = HousingPromotion.Create(developerCompanyId, "Residencial Acacias", "Madrid", "Calle Mayor 1");
        var unit = HousingUnit.Create(promotion.Id, "1", "A", 90m, 80m);
        var invitation = BuyerInvitation.Create(unit.Id, "buyer@example.com");
        _currentUser.DeveloperCompanyId.Returns(developerCompanyId);
        _unitRepository.GetByIdAsync(unit.Id, Arg.Any<CancellationToken>()).Returns(unit);
        _promotionRepository.GetByIdAsync(promotion.Id, Arg.Any<CancellationToken>()).Returns(promotion);
        _invitationRepository.GetByHousingUnitIdAsync(unit.Id, Arg.Any<CancellationToken>()).Returns([invitation]);

        var result = await _useCase.ExecuteAsync(unit.Id);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().ContainSingle(i => i.Id == invitation.Id && i.Email == "buyer@example.com");
    }

    [Fact]
    public async Task ExecuteAsync_WhenUnitBelongsToAnotherTenant_ShouldReturnFailure()
    {
        var promotion = HousingPromotion.Create(Guid.NewGuid(), "Residencial Acacias", "Madrid", "Calle Mayor 1");
        var unit = HousingUnit.Create(promotion.Id, "1", "A", 90m, 80m);
        _currentUser.DeveloperCompanyId.Returns(Guid.NewGuid());
        _unitRepository.GetByIdAsync(unit.Id, Arg.Any<CancellationToken>()).Returns(unit);
        _promotionRepository.GetByIdAsync(promotion.Id, Arg.Any<CancellationToken>()).Returns(promotion);

        var result = await _useCase.ExecuteAsync(unit.Id);

        result.IsFailure.Should().BeTrue();
    }
}

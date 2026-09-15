using FluentAssertions;
using Kiwbi.Application.Common;
using Kiwbi.Application.Onboarding.CancelBuyerInvitation;
using Kiwbi.Domain.Onboarding;
using Kiwbi.Domain.RealEstate;
using NSubstitute;

namespace Kiwbi.Application.Tests.Onboarding.CancelBuyerInvitation;

public class CancelBuyerInvitationUseCaseTests
{
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();
    private readonly IHousingPromotionRepository _promotionRepository = Substitute.For<IHousingPromotionRepository>();
    private readonly IHousingUnitRepository _unitRepository = Substitute.For<IHousingUnitRepository>();
    private readonly IBuyerInvitationRepository _invitationRepository = Substitute.For<IBuyerInvitationRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly CancelBuyerInvitationUseCase _useCase;

    public CancelBuyerInvitationUseCaseTests()
    {
        _useCase = new CancelBuyerInvitationUseCase(_currentUser, _promotionRepository, _unitRepository, _invitationRepository, _unitOfWork);
    }

    [Fact]
    public async Task ExecuteAsync_WithOwnedPendingInvitation_ShouldCancelIt()
    {
        var developerCompanyId = Guid.NewGuid();
        var promotion = HousingPromotion.Create(developerCompanyId, "Residencial Acacias", "Madrid", "Calle Mayor 1");
        var unit = HousingUnit.Create(promotion.Id, "1", "A", 90m, 80m);
        var invitation = BuyerInvitation.Create(unit.Id, "buyer@example.com");
        _currentUser.DeveloperCompanyId.Returns(developerCompanyId);
        _invitationRepository.GetByIdAsync(invitation.Id, Arg.Any<CancellationToken>()).Returns(invitation);
        _unitRepository.GetByIdAsync(unit.Id, Arg.Any<CancellationToken>()).Returns(unit);
        _promotionRepository.GetByIdAsync(promotion.Id, Arg.Any<CancellationToken>()).Returns(promotion);

        var result = await _useCase.ExecuteAsync(invitation.Id);

        result.IsSuccess.Should().BeTrue();
        invitation.Status.Should().Be(BuyerInvitationStatus.Cancelled);
        _invitationRepository.Received(1).Update(invitation);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenInvitationBelongsToAnotherTenant_ShouldReturnFailure()
    {
        var promotion = HousingPromotion.Create(Guid.NewGuid(), "Residencial Acacias", "Madrid", "Calle Mayor 1");
        var unit = HousingUnit.Create(promotion.Id, "1", "A", 90m, 80m);
        var invitation = BuyerInvitation.Create(unit.Id, "buyer@example.com");
        _currentUser.DeveloperCompanyId.Returns(Guid.NewGuid());
        _invitationRepository.GetByIdAsync(invitation.Id, Arg.Any<CancellationToken>()).Returns(invitation);
        _unitRepository.GetByIdAsync(unit.Id, Arg.Any<CancellationToken>()).Returns(unit);
        _promotionRepository.GetByIdAsync(promotion.Id, Arg.Any<CancellationToken>()).Returns(promotion);

        var result = await _useCase.ExecuteAsync(invitation.Id);

        result.IsFailure.Should().BeTrue();
        _invitationRepository.DidNotReceive().Update(Arg.Any<BuyerInvitation>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenInvitationAlreadyAccepted_ShouldReturnFailure()
    {
        var developerCompanyId = Guid.NewGuid();
        var promotion = HousingPromotion.Create(developerCompanyId, "Residencial Acacias", "Madrid", "Calle Mayor 1");
        var unit = HousingUnit.Create(promotion.Id, "1", "A", 90m, 80m);
        var invitation = BuyerInvitation.Create(unit.Id, "buyer@example.com");
        invitation.MarkAccepted("user-1", DateTime.UtcNow);
        _currentUser.DeveloperCompanyId.Returns(developerCompanyId);
        _invitationRepository.GetByIdAsync(invitation.Id, Arg.Any<CancellationToken>()).Returns(invitation);
        _unitRepository.GetByIdAsync(unit.Id, Arg.Any<CancellationToken>()).Returns(unit);
        _promotionRepository.GetByIdAsync(promotion.Id, Arg.Any<CancellationToken>()).Returns(promotion);

        var result = await _useCase.ExecuteAsync(invitation.Id);

        result.IsFailure.Should().BeTrue();
        _invitationRepository.DidNotReceive().Update(Arg.Any<BuyerInvitation>());
    }
}

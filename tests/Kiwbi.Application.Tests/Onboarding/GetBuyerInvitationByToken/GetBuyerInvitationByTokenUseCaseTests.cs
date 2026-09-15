using FluentAssertions;
using Kiwbi.Application.Onboarding;
using Kiwbi.Application.Onboarding.GetBuyerInvitationByToken;
using Kiwbi.Domain.Onboarding;
using Kiwbi.Domain.RealEstate;
using NSubstitute;

namespace Kiwbi.Application.Tests.Onboarding.GetBuyerInvitationByToken;

public class GetBuyerInvitationByTokenUseCaseTests
{
    private readonly IBuyerInvitationRepository _invitationRepository = Substitute.For<IBuyerInvitationRepository>();
    private readonly IHousingUnitRepository _unitRepository = Substitute.For<IHousingUnitRepository>();
    private readonly IHousingPromotionRepository _promotionRepository = Substitute.For<IHousingPromotionRepository>();
    private readonly IBuyerAccountProvisioningService _buyerAccountProvisioningService = Substitute.For<IBuyerAccountProvisioningService>();
    private readonly GetBuyerInvitationByTokenUseCase _useCase;

    public GetBuyerInvitationByTokenUseCaseTests()
    {
        _useCase = new GetBuyerInvitationByTokenUseCase(_invitationRepository, _unitRepository, _promotionRepository, _buyerAccountProvisioningService);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidPendingToken_ShouldReturnAcceptableDto()
    {
        var promotion = HousingPromotion.Create(Guid.NewGuid(), "Residencial Acacias", "Madrid", "Calle Mayor 1");
        var unit = HousingUnit.Create(promotion.Id, "1", "A", 90m, 80m);
        var invitation = BuyerInvitation.Create(unit.Id, "buyer@example.com");
        _invitationRepository.GetByTokenAsync(invitation.Token, Arg.Any<CancellationToken>()).Returns(invitation);
        _unitRepository.GetByIdAsync(unit.Id, Arg.Any<CancellationToken>()).Returns(unit);
        _promotionRepository.GetByIdAsync(promotion.Id, Arg.Any<CancellationToken>()).Returns(promotion);
        _buyerAccountProvisioningService.ExistsByEmailAsync("buyer@example.com", Arg.Any<CancellationToken>()).Returns(false);

        var result = await _useCase.ExecuteAsync(invitation.Token);

        result.IsSuccess.Should().BeTrue();
        result.Value!.CanAccept.Should().BeTrue();
        result.Value.Email.Should().Be("buyer@example.com");
        result.Value.AccountAlreadyExists.Should().BeFalse();
        result.Value.HousingPromotionName.Should().Be("Residencial Acacias");
    }

    [Fact]
    public async Task ExecuteAsync_WithUnknownToken_ShouldReturnFailure()
    {
        _invitationRepository.GetByTokenAsync("missing-token", Arg.Any<CancellationToken>()).Returns((BuyerInvitation?)null);

        var result = await _useCase.ExecuteAsync("missing-token");

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteAsync_WhenAccountAlreadyExists_ShouldReturnFlagSet()
    {
        var promotion = HousingPromotion.Create(Guid.NewGuid(), "Residencial Acacias", "Madrid", "Calle Mayor 1");
        var unit = HousingUnit.Create(promotion.Id, "1", "A", 90m, 80m);
        var invitation = BuyerInvitation.Create(unit.Id, "buyer@example.com");
        _invitationRepository.GetByTokenAsync(invitation.Token, Arg.Any<CancellationToken>()).Returns(invitation);
        _unitRepository.GetByIdAsync(unit.Id, Arg.Any<CancellationToken>()).Returns(unit);
        _promotionRepository.GetByIdAsync(promotion.Id, Arg.Any<CancellationToken>()).Returns(promotion);
        _buyerAccountProvisioningService.ExistsByEmailAsync("buyer@example.com", Arg.Any<CancellationToken>()).Returns(true);

        var result = await _useCase.ExecuteAsync(invitation.Token);

        result.IsSuccess.Should().BeTrue();
        result.Value!.AccountAlreadyExists.Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteAsync_WithCancelledInvitation_ShouldReturnDtoWithCanAcceptFalse()
    {
        var promotion = HousingPromotion.Create(Guid.NewGuid(), "Residencial Acacias", "Madrid", "Calle Mayor 1");
        var unit = HousingUnit.Create(promotion.Id, "1", "A", 90m, 80m);
        var invitation = BuyerInvitation.Create(unit.Id, "buyer@example.com");
        invitation.Cancel();
        _invitationRepository.GetByTokenAsync(invitation.Token, Arg.Any<CancellationToken>()).Returns(invitation);
        _unitRepository.GetByIdAsync(unit.Id, Arg.Any<CancellationToken>()).Returns(unit);
        _promotionRepository.GetByIdAsync(promotion.Id, Arg.Any<CancellationToken>()).Returns(promotion);

        var result = await _useCase.ExecuteAsync(invitation.Token);

        result.IsSuccess.Should().BeTrue();
        result.Value!.CanAccept.Should().BeFalse();
        result.Value.Status.Should().Be(BuyerInvitationStatus.Cancelled);
    }
}

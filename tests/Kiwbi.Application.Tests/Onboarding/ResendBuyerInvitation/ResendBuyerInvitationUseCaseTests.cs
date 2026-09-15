using FluentAssertions;
using Kiwbi.Application.Common;
using Kiwbi.Application.Onboarding.ResendBuyerInvitation;
using Kiwbi.Domain.Onboarding;
using Kiwbi.Domain.RealEstate;
using NSubstitute;

namespace Kiwbi.Application.Tests.Onboarding.ResendBuyerInvitation;

public class ResendBuyerInvitationUseCaseTests
{
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();
    private readonly IHousingPromotionRepository _promotionRepository = Substitute.For<IHousingPromotionRepository>();
    private readonly IHousingUnitRepository _unitRepository = Substitute.For<IHousingUnitRepository>();
    private readonly IBuyerInvitationRepository _invitationRepository = Substitute.For<IBuyerInvitationRepository>();
    private readonly IEmailSender _emailSender = Substitute.For<IEmailSender>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ResendBuyerInvitationUseCase _useCase;

    public ResendBuyerInvitationUseCaseTests()
    {
        _useCase = new ResendBuyerInvitationUseCase(
            _currentUser, _promotionRepository, _unitRepository, _invitationRepository, _emailSender, _unitOfWork);
    }

    [Fact]
    public async Task ExecuteAsync_WithOwnedPendingInvitation_ShouldRegenerateAndResendEmail()
    {
        var developerCompanyId = Guid.NewGuid();
        var promotion = HousingPromotion.Create(developerCompanyId, "Residencial Acacias", "Madrid", "Calle Mayor 1");
        var unit = HousingUnit.Create(promotion.Id, "1", "A", 90m, 80m);
        var invitation = BuyerInvitation.Create(unit.Id, "buyer@example.com");
        var originalToken = invitation.Token;
        _currentUser.DeveloperCompanyId.Returns(developerCompanyId);
        _invitationRepository.GetByIdAsync(invitation.Id, Arg.Any<CancellationToken>()).Returns(invitation);
        _unitRepository.GetByIdAsync(unit.Id, Arg.Any<CancellationToken>()).Returns(unit);
        _promotionRepository.GetByIdAsync(promotion.Id, Arg.Any<CancellationToken>()).Returns(promotion);

        var result = await _useCase.ExecuteAsync(invitation.Id);

        result.IsSuccess.Should().BeTrue();
        invitation.Token.Should().NotBe(originalToken);
        _invitationRepository.Received(1).Update(invitation);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        await _emailSender.Received(1).SendBuyerInvitationEmailAsync("buyer@example.com", invitation.Token, invitation.ExpiresAtUtc, Arg.Any<CancellationToken>());
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
    public async Task ExecuteAsync_WhenInvitationAlreadyCancelled_ShouldReturnFailure()
    {
        var developerCompanyId = Guid.NewGuid();
        var promotion = HousingPromotion.Create(developerCompanyId, "Residencial Acacias", "Madrid", "Calle Mayor 1");
        var unit = HousingUnit.Create(promotion.Id, "1", "A", 90m, 80m);
        var invitation = BuyerInvitation.Create(unit.Id, "buyer@example.com");
        invitation.Cancel();
        _currentUser.DeveloperCompanyId.Returns(developerCompanyId);
        _invitationRepository.GetByIdAsync(invitation.Id, Arg.Any<CancellationToken>()).Returns(invitation);
        _unitRepository.GetByIdAsync(unit.Id, Arg.Any<CancellationToken>()).Returns(unit);
        _promotionRepository.GetByIdAsync(promotion.Id, Arg.Any<CancellationToken>()).Returns(promotion);

        var result = await _useCase.ExecuteAsync(invitation.Id);

        result.IsFailure.Should().BeTrue();
        _invitationRepository.DidNotReceive().Update(Arg.Any<BuyerInvitation>());
    }
}

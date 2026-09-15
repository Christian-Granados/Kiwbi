using FluentAssertions;
using Kiwbi.Application.Common;
using Kiwbi.Application.Onboarding;
using Kiwbi.Application.Onboarding.InviteBuyerToHousingUnit;
using Kiwbi.Domain.Onboarding;
using Kiwbi.Domain.RealEstate;
using NSubstitute;

namespace Kiwbi.Application.Tests.Onboarding.InviteBuyerToHousingUnit;

public class InviteBuyerToHousingUnitUseCaseTests
{
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();
    private readonly IHousingPromotionRepository _promotionRepository = Substitute.For<IHousingPromotionRepository>();
    private readonly IHousingUnitRepository _unitRepository = Substitute.For<IHousingUnitRepository>();
    private readonly IBuyerInvitationRepository _invitationRepository = Substitute.For<IBuyerInvitationRepository>();
    private readonly IEmailSender _emailSender = Substitute.For<IEmailSender>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly InviteBuyerToHousingUnitUseCase _useCase;

    public InviteBuyerToHousingUnitUseCaseTests()
    {
        _useCase = new InviteBuyerToHousingUnitUseCase(
            _currentUser, _promotionRepository, _unitRepository, _invitationRepository, _emailSender, _unitOfWork);
    }

    [Fact]
    public async Task ExecuteAsync_WithOwnedUnitAndNoDuplicate_ShouldCreateInvitationAndSendEmail()
    {
        var developerCompanyId = Guid.NewGuid();
        var promotion = HousingPromotion.Create(developerCompanyId, "Residencial Acacias", "Madrid", "Calle Mayor 1");
        var unit = HousingUnit.Create(promotion.Id, "1", "A", 90m, 80m);
        _currentUser.DeveloperCompanyId.Returns(developerCompanyId);
        _unitRepository.GetByIdAsync(unit.Id, Arg.Any<CancellationToken>()).Returns(unit);
        _promotionRepository.GetByIdAsync(promotion.Id, Arg.Any<CancellationToken>()).Returns(promotion);

        var result = await _useCase.ExecuteAsync(new InviteBuyerToHousingUnitCommand(unit.Id, "buyer@example.com"));

        result.IsSuccess.Should().BeTrue();
        await _invitationRepository.Received(1).AddAsync(Arg.Any<BuyerInvitation>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        await _emailSender.Received(1).SendBuyerInvitationEmailAsync("buyer@example.com", Arg.Any<string>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenUnitBelongsToAnotherTenant_ShouldReturnFailure()
    {
        var promotion = HousingPromotion.Create(Guid.NewGuid(), "Residencial Acacias", "Madrid", "Calle Mayor 1");
        var unit = HousingUnit.Create(promotion.Id, "1", "A", 90m, 80m);
        _currentUser.DeveloperCompanyId.Returns(Guid.NewGuid());
        _unitRepository.GetByIdAsync(unit.Id, Arg.Any<CancellationToken>()).Returns(unit);
        _promotionRepository.GetByIdAsync(promotion.Id, Arg.Any<CancellationToken>()).Returns(promotion);

        var result = await _useCase.ExecuteAsync(new InviteBuyerToHousingUnitCommand(unit.Id, "buyer@example.com"));

        result.IsFailure.Should().BeTrue();
        await _invitationRepository.DidNotReceive().AddAsync(Arg.Any<BuyerInvitation>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WithExistingPendingInvitationForSameEmail_ShouldReturnFailure()
    {
        var developerCompanyId = Guid.NewGuid();
        var promotion = HousingPromotion.Create(developerCompanyId, "Residencial Acacias", "Madrid", "Calle Mayor 1");
        var unit = HousingUnit.Create(promotion.Id, "1", "A", 90m, 80m);
        _currentUser.DeveloperCompanyId.Returns(developerCompanyId);
        _unitRepository.GetByIdAsync(unit.Id, Arg.Any<CancellationToken>()).Returns(unit);
        _promotionRepository.GetByIdAsync(promotion.Id, Arg.Any<CancellationToken>()).Returns(promotion);
        _invitationRepository.ExistsPendingByHousingUnitIdAndEmailAsync(unit.Id, "buyer@example.com", Arg.Any<CancellationToken>()).Returns(true);

        var result = await _useCase.ExecuteAsync(new InviteBuyerToHousingUnitCommand(unit.Id, "buyer@example.com"));

        result.IsFailure.Should().BeTrue();
        await _invitationRepository.DidNotReceive().AddAsync(Arg.Any<BuyerInvitation>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WithInvalidEmail_ShouldReturnFailure()
    {
        var developerCompanyId = Guid.NewGuid();
        var promotion = HousingPromotion.Create(developerCompanyId, "Residencial Acacias", "Madrid", "Calle Mayor 1");
        var unit = HousingUnit.Create(promotion.Id, "1", "A", 90m, 80m);
        _currentUser.DeveloperCompanyId.Returns(developerCompanyId);
        _unitRepository.GetByIdAsync(unit.Id, Arg.Any<CancellationToken>()).Returns(unit);
        _promotionRepository.GetByIdAsync(promotion.Id, Arg.Any<CancellationToken>()).Returns(promotion);

        var result = await _useCase.ExecuteAsync(new InviteBuyerToHousingUnitCommand(unit.Id, "not-an-email"));

        result.IsFailure.Should().BeTrue();
        await _invitationRepository.DidNotReceive().AddAsync(Arg.Any<BuyerInvitation>(), Arg.Any<CancellationToken>());
    }
}

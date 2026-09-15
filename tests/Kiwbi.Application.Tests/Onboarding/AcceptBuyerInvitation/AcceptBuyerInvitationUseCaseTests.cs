using FluentAssertions;
using Kiwbi.Application.Common;
using Kiwbi.Application.Developers;
using Kiwbi.Application.Onboarding;
using Kiwbi.Application.Onboarding.AcceptBuyerInvitation;
using Kiwbi.Domain.Onboarding;
using NSubstitute;

namespace Kiwbi.Application.Tests.Onboarding.AcceptBuyerInvitation;

public class AcceptBuyerInvitationUseCaseTests
{
    private readonly IBuyerInvitationRepository _invitationRepository = Substitute.For<IBuyerInvitationRepository>();
    private readonly IHousingUnitBuyerRepository _housingUnitBuyerRepository = Substitute.For<IHousingUnitBuyerRepository>();
    private readonly IBuyerAccountProvisioningService _buyerAccountProvisioningService = Substitute.For<IBuyerAccountProvisioningService>();
    private readonly IAuthenticationService _authenticationService = Substitute.For<IAuthenticationService>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly AcceptBuyerInvitationUseCase _useCase;

    public AcceptBuyerInvitationUseCaseTests()
    {
        _unitOfWork
            .ExecuteInTransactionAsync(Arg.Any<Func<CancellationToken, Task<Result>>>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => callInfo.Arg<Func<CancellationToken, Task<Result>>>().Invoke(CancellationToken.None));

        _useCase = new AcceptBuyerInvitationUseCase(
            _invitationRepository, _housingUnitBuyerRepository, _buyerAccountProvisioningService, _authenticationService, _unitOfWork);
    }

    [Fact]
    public async Task ExecuteAsync_WithNewAccount_ShouldCreateAccountLinkAndSignIn()
    {
        var invitation = BuyerInvitation.Create(Guid.NewGuid(), "buyer@example.com");
        _invitationRepository.GetByTokenAsync(invitation.Token, Arg.Any<CancellationToken>()).Returns(invitation);
        _buyerAccountProvisioningService.FindUserIdByEmailAsync("buyer@example.com", Arg.Any<CancellationToken>()).Returns((string?)null);
        _buyerAccountProvisioningService.CreateBuyerAccountAsync("buyer@example.com", "P@ssw0rd!", Arg.Any<CancellationToken>())
            .Returns(Result.Success("user-1"));
        _authenticationService.SignInAsync("buyer@example.com", "P@ssw0rd!", false, Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var result = await _useCase.ExecuteAsync(new AcceptBuyerInvitationCommand(invitation.Token, "P@ssw0rd!"));

        result.IsSuccess.Should().BeTrue();
        invitation.Status.Should().Be(BuyerInvitationStatus.Accepted);
        await _housingUnitBuyerRepository.Received(1).AddAsync(
            Arg.Is<HousingUnitBuyer>(l => l.HousingUnitId == invitation.HousingUnitId && l.BuyerUserId == "user-1"),
            Arg.Any<CancellationToken>());
        await _authenticationService.Received(1).SignInAsync("buyer@example.com", "P@ssw0rd!", false, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WithExistingAccountAndCorrectPassword_ShouldLinkWithoutCreatingAccount()
    {
        var invitation = BuyerInvitation.Create(Guid.NewGuid(), "buyer@example.com");
        _invitationRepository.GetByTokenAsync(invitation.Token, Arg.Any<CancellationToken>()).Returns(invitation);
        _buyerAccountProvisioningService.FindUserIdByEmailAsync("buyer@example.com", Arg.Any<CancellationToken>()).Returns("user-1");
        _authenticationService.SignInAsync("buyer@example.com", "P@ssw0rd!", false, Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var result = await _useCase.ExecuteAsync(new AcceptBuyerInvitationCommand(invitation.Token, "P@ssw0rd!"));

        result.IsSuccess.Should().BeTrue();
        await _buyerAccountProvisioningService.DidNotReceive().CreateBuyerAccountAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _housingUnitBuyerRepository.Received(1).AddAsync(
            Arg.Is<HousingUnitBuyer>(l => l.BuyerUserId == "user-1"), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WithExistingAccountAndWrongPassword_ShouldReturnFailureWithoutLinking()
    {
        var invitation = BuyerInvitation.Create(Guid.NewGuid(), "buyer@example.com");
        _invitationRepository.GetByTokenAsync(invitation.Token, Arg.Any<CancellationToken>()).Returns(invitation);
        _buyerAccountProvisioningService.FindUserIdByEmailAsync("buyer@example.com", Arg.Any<CancellationToken>()).Returns("user-1");
        _authenticationService.SignInAsync("buyer@example.com", "wrong", false, Arg.Any<CancellationToken>())
            .Returns(Result.Failure("Correo electrónico o contraseña incorrectos."));

        var result = await _useCase.ExecuteAsync(new AcceptBuyerInvitationCommand(invitation.Token, "wrong"));

        result.IsFailure.Should().BeTrue();
        invitation.Status.Should().Be(BuyerInvitationStatus.Pending);
        await _housingUnitBuyerRepository.DidNotReceive().AddAsync(Arg.Any<HousingUnitBuyer>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WithAlreadyLinkedBuyer_ShouldNotDuplicateLink()
    {
        var invitation = BuyerInvitation.Create(Guid.NewGuid(), "buyer@example.com");
        _invitationRepository.GetByTokenAsync(invitation.Token, Arg.Any<CancellationToken>()).Returns(invitation);
        _buyerAccountProvisioningService.FindUserIdByEmailAsync("buyer@example.com", Arg.Any<CancellationToken>()).Returns("user-1");
        _housingUnitBuyerRepository.ExistsByHousingUnitIdAndBuyerUserIdAsync(invitation.HousingUnitId, "user-1", Arg.Any<CancellationToken>()).Returns(true);
        _authenticationService.SignInAsync("buyer@example.com", "P@ssw0rd!", false, Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var result = await _useCase.ExecuteAsync(new AcceptBuyerInvitationCommand(invitation.Token, "P@ssw0rd!"));

        result.IsSuccess.Should().BeTrue();
        await _housingUnitBuyerRepository.DidNotReceive().AddAsync(Arg.Any<HousingUnitBuyer>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WithUnknownToken_ShouldReturnFailure()
    {
        _invitationRepository.GetByTokenAsync("missing", Arg.Any<CancellationToken>()).Returns((BuyerInvitation?)null);

        var result = await _useCase.ExecuteAsync(new AcceptBuyerInvitationCommand("missing", "P@ssw0rd!"));

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteAsync_WithCancelledInvitation_ShouldReturnFailure()
    {
        var invitation = BuyerInvitation.Create(Guid.NewGuid(), "buyer@example.com");
        invitation.Cancel();
        _invitationRepository.GetByTokenAsync(invitation.Token, Arg.Any<CancellationToken>()).Returns(invitation);

        var result = await _useCase.ExecuteAsync(new AcceptBuyerInvitationCommand(invitation.Token, "P@ssw0rd!"));

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteAsync_WithAlreadyAcceptedInvitation_ShouldReturnFailure()
    {
        var invitation = BuyerInvitation.Create(Guid.NewGuid(), "buyer@example.com");
        invitation.MarkAccepted("user-1", DateTime.UtcNow);
        _invitationRepository.GetByTokenAsync(invitation.Token, Arg.Any<CancellationToken>()).Returns(invitation);

        var result = await _useCase.ExecuteAsync(new AcceptBuyerInvitationCommand(invitation.Token, "P@ssw0rd!"));

        result.IsFailure.Should().BeTrue();
    }
}

using FluentAssertions;
using Kiwbi.Application.Common;
using Kiwbi.Application.Onboarding.UnlinkHousingUnitBuyer;
using Kiwbi.Domain.Onboarding;
using Kiwbi.Domain.RealEstate;
using NSubstitute;

namespace Kiwbi.Application.Tests.Onboarding.UnlinkHousingUnitBuyer;

public class UnlinkHousingUnitBuyerUseCaseTests
{
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();
    private readonly IHousingPromotionRepository _promotionRepository = Substitute.For<IHousingPromotionRepository>();
    private readonly IHousingUnitRepository _unitRepository = Substitute.For<IHousingUnitRepository>();
    private readonly IHousingUnitBuyerRepository _housingUnitBuyerRepository = Substitute.For<IHousingUnitBuyerRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly UnlinkHousingUnitBuyerUseCase _useCase;

    public UnlinkHousingUnitBuyerUseCaseTests()
    {
        _useCase = new UnlinkHousingUnitBuyerUseCase(
            _currentUser, _promotionRepository, _unitRepository, _housingUnitBuyerRepository, _unitOfWork);
    }

    [Fact]
    public async Task ExecuteAsync_WithOwnedLink_ShouldRemoveIt()
    {
        var developerCompanyId = Guid.NewGuid();
        var promotion = HousingPromotion.Create(developerCompanyId, "Residencial Acacias", "Madrid", "Calle Mayor 1");
        var unit = HousingUnit.Create(promotion.Id, "1", "A", 90m, 80m);
        var link = HousingUnitBuyer.Create(unit.Id, "user-1");
        _currentUser.DeveloperCompanyId.Returns(developerCompanyId);
        _housingUnitBuyerRepository.GetByIdAsync(link.Id, Arg.Any<CancellationToken>()).Returns(link);
        _unitRepository.GetByIdAsync(unit.Id, Arg.Any<CancellationToken>()).Returns(unit);
        _promotionRepository.GetByIdAsync(promotion.Id, Arg.Any<CancellationToken>()).Returns(promotion);

        var result = await _useCase.ExecuteAsync(link.Id);

        result.IsSuccess.Should().BeTrue();
        _housingUnitBuyerRepository.Received(1).Remove(link);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenLinkBelongsToAnotherTenant_ShouldReturnFailureAndNotRemove()
    {
        var promotion = HousingPromotion.Create(Guid.NewGuid(), "Residencial Acacias", "Madrid", "Calle Mayor 1");
        var unit = HousingUnit.Create(promotion.Id, "1", "A", 90m, 80m);
        var link = HousingUnitBuyer.Create(unit.Id, "user-1");
        _currentUser.DeveloperCompanyId.Returns(Guid.NewGuid());
        _housingUnitBuyerRepository.GetByIdAsync(link.Id, Arg.Any<CancellationToken>()).Returns(link);
        _unitRepository.GetByIdAsync(unit.Id, Arg.Any<CancellationToken>()).Returns(unit);
        _promotionRepository.GetByIdAsync(promotion.Id, Arg.Any<CancellationToken>()).Returns(promotion);

        var result = await _useCase.ExecuteAsync(link.Id);

        result.IsFailure.Should().BeTrue();
        _housingUnitBuyerRepository.DidNotReceive().Remove(Arg.Any<HousingUnitBuyer>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenLinkDoesNotExist_ShouldReturnFailure()
    {
        _currentUser.DeveloperCompanyId.Returns(Guid.NewGuid());
        _housingUnitBuyerRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((HousingUnitBuyer?)null);

        var result = await _useCase.ExecuteAsync(Guid.NewGuid());

        result.IsFailure.Should().BeTrue();
    }
}

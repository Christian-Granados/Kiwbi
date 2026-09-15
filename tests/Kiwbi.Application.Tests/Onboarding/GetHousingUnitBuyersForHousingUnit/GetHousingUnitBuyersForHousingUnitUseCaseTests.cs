using FluentAssertions;
using Kiwbi.Application.Common;
using Kiwbi.Application.Onboarding;
using Kiwbi.Application.Onboarding.GetHousingUnitBuyersForHousingUnit;
using Kiwbi.Domain.Onboarding;
using Kiwbi.Domain.RealEstate;
using NSubstitute;

namespace Kiwbi.Application.Tests.Onboarding.GetHousingUnitBuyersForHousingUnit;

public class GetHousingUnitBuyersForHousingUnitUseCaseTests
{
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();
    private readonly IHousingPromotionRepository _promotionRepository = Substitute.For<IHousingPromotionRepository>();
    private readonly IHousingUnitRepository _unitRepository = Substitute.For<IHousingUnitRepository>();
    private readonly IHousingUnitBuyerRepository _housingUnitBuyerRepository = Substitute.For<IHousingUnitBuyerRepository>();
    private readonly IBuyerAccountProvisioningService _buyerAccountProvisioningService = Substitute.For<IBuyerAccountProvisioningService>();
    private readonly GetHousingUnitBuyersForHousingUnitUseCase _useCase;

    public GetHousingUnitBuyersForHousingUnitUseCaseTests()
    {
        _useCase = new GetHousingUnitBuyersForHousingUnitUseCase(
            _currentUser, _promotionRepository, _unitRepository, _housingUnitBuyerRepository, _buyerAccountProvisioningService);
    }

    [Fact]
    public async Task ExecuteAsync_WithOwnedUnit_ShouldReturnBuyersWithResolvedEmail()
    {
        var developerCompanyId = Guid.NewGuid();
        var promotion = HousingPromotion.Create(developerCompanyId, "Residencial Acacias", "Madrid", "Calle Mayor 1");
        var unit = HousingUnit.Create(promotion.Id, "1", "A", 90m, 80m);
        var link = HousingUnitBuyer.Create(unit.Id, "user-1");
        _currentUser.DeveloperCompanyId.Returns(developerCompanyId);
        _unitRepository.GetByIdAsync(unit.Id, Arg.Any<CancellationToken>()).Returns(unit);
        _promotionRepository.GetByIdAsync(promotion.Id, Arg.Any<CancellationToken>()).Returns(promotion);
        _housingUnitBuyerRepository.GetByHousingUnitIdAsync(unit.Id, Arg.Any<CancellationToken>()).Returns([link]);
        _buyerAccountProvisioningService.GetEmailByUserIdAsync("user-1", Arg.Any<CancellationToken>()).Returns("buyer@example.com");

        var result = await _useCase.ExecuteAsync(unit.Id);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().ContainSingle(b => b.BuyerUserId == "user-1" && b.Email == "buyer@example.com");
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

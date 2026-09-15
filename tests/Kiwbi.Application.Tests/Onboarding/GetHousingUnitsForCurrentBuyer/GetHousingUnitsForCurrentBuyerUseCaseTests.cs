using FluentAssertions;
using Kiwbi.Application.Common;
using Kiwbi.Application.Onboarding;
using Kiwbi.Application.Onboarding.GetHousingUnitsForCurrentBuyer;
using Kiwbi.Domain.Onboarding;
using Kiwbi.Domain.RealEstate;
using NSubstitute;

namespace Kiwbi.Application.Tests.Onboarding.GetHousingUnitsForCurrentBuyer;

public class GetHousingUnitsForCurrentBuyerUseCaseTests
{
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();
    private readonly IHousingUnitBuyerRepository _housingUnitBuyerRepository = Substitute.For<IHousingUnitBuyerRepository>();
    private readonly IHousingUnitRepository _housingUnitRepository = Substitute.For<IHousingUnitRepository>();
    private readonly IHousingPromotionRepository _housingPromotionRepository = Substitute.For<IHousingPromotionRepository>();
    private readonly GetHousingUnitsForCurrentBuyerUseCase _useCase;

    public GetHousingUnitsForCurrentBuyerUseCaseTests()
    {
        _useCase = new GetHousingUnitsForCurrentBuyerUseCase(
            _currentUser, _housingUnitBuyerRepository, _housingUnitRepository, _housingPromotionRepository);
    }

    [Fact]
    public async Task ExecuteAsync_WhenUserNotAuthenticated_ShouldReturnFailure()
    {
        _currentUser.UserId.Returns((string?)null);

        var result = await _useCase.ExecuteAsync();

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteAsync_WithLinkedUnits_ShouldReturnThemWithPromotionData()
    {
        const string buyerUserId = "buyer-1";
        var promotion = HousingPromotion.Create(Guid.NewGuid(), "Residencial Acacias", "Madrid", "Calle Mayor 1");
        var unit = HousingUnit.Create(promotion.Id, "1", "A", 90m, 80m);
        var link = HousingUnitBuyer.Create(unit.Id, buyerUserId);
        _currentUser.UserId.Returns(buyerUserId);
        _housingUnitBuyerRepository.GetByBuyerUserIdAsync(buyerUserId, Arg.Any<CancellationToken>()).Returns([link]);
        _housingUnitRepository.GetByIdAsync(unit.Id, Arg.Any<CancellationToken>()).Returns(unit);
        _housingPromotionRepository.GetByIdAsync(promotion.Id, Arg.Any<CancellationToken>()).Returns(promotion);

        var result = await _useCase.ExecuteAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().ContainSingle(dto =>
            dto.HousingUnitId == unit.Id &&
            dto.HousingPromotionName == "Residencial Acacias" &&
            dto.Floor == "1" &&
            dto.Door == "A");
    }

    [Fact]
    public async Task ExecuteAsync_WhenLinkedUnitNoLongerExists_ShouldSkipIt()
    {
        const string buyerUserId = "buyer-1";
        var link = HousingUnitBuyer.Create(Guid.NewGuid(), buyerUserId);
        _currentUser.UserId.Returns(buyerUserId);
        _housingUnitBuyerRepository.GetByBuyerUserIdAsync(buyerUserId, Arg.Any<CancellationToken>()).Returns([link]);
        _housingUnitRepository.GetByIdAsync(link.HousingUnitId, Arg.Any<CancellationToken>()).Returns((HousingUnit?)null);

        var result = await _useCase.ExecuteAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task ExecuteAsync_WithNoLinkedUnits_ShouldReturnEmptyList()
    {
        const string buyerUserId = "buyer-1";
        _currentUser.UserId.Returns(buyerUserId);
        _housingUnitBuyerRepository.GetByBuyerUserIdAsync(buyerUserId, Arg.Any<CancellationToken>()).Returns([]);

        var result = await _useCase.ExecuteAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }
}

using FluentAssertions;
using Kiwbi.Application.Common;
using Kiwbi.Application.RealEstate.UpdateHousingUnit;
using Kiwbi.Domain.RealEstate;
using NSubstitute;

namespace Kiwbi.Application.Tests.RealEstate.UpdateHousingUnit;

public class UpdateHousingUnitUseCaseTests
{
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();
    private readonly IHousingPromotionRepository _promotionRepository = Substitute.For<IHousingPromotionRepository>();
    private readonly IHousingTypologyRepository _typologyRepository = Substitute.For<IHousingTypologyRepository>();
    private readonly IHousingUnitRepository _unitRepository = Substitute.For<IHousingUnitRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly UpdateHousingUnitUseCase _useCase;

    public UpdateHousingUnitUseCaseTests()
    {
        _useCase = new UpdateHousingUnitUseCase(_currentUser, _promotionRepository, _typologyRepository, _unitRepository, _unitOfWork);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidCommand_ShouldUpdateAndPersistUnit()
    {
        var developerCompanyId = Guid.NewGuid();
        var promotion = HousingPromotion.Create(developerCompanyId, "Residencial Acacias", "Madrid", "Calle Mayor 1");
        var unit = HousingUnit.Create(promotion.Id, "1", "A", 90m, 80m);
        _currentUser.DeveloperCompanyId.Returns(developerCompanyId);
        _unitRepository.GetByIdAsync(unit.Id, Arg.Any<CancellationToken>()).Returns(unit);
        _promotionRepository.GetByIdAsync(promotion.Id, Arg.Any<CancellationToken>()).Returns(promotion);
        _unitRepository.ExistsWithFloorAndDoorAsync(promotion.Id, "2", "B", unit.Id, Arg.Any<CancellationToken>()).Returns(false);

        var command = new UpdateHousingUnitCommand(unit.Id, null, "2", "B", 100m, 95m);

        var result = await _useCase.ExecuteAsync(command);

        result.IsSuccess.Should().BeTrue();
        unit.Floor.Should().Be("2");
        unit.Door.Should().Be("B");
        _unitRepository.Received(1).Update(unit);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WithDuplicateFloorAndDoor_ShouldReturnFailureWithoutPersisting()
    {
        var developerCompanyId = Guid.NewGuid();
        var promotion = HousingPromotion.Create(developerCompanyId, "Residencial Acacias", "Madrid", "Calle Mayor 1");
        var unit = HousingUnit.Create(promotion.Id, "1", "A", 90m, 80m);
        _currentUser.DeveloperCompanyId.Returns(developerCompanyId);
        _unitRepository.GetByIdAsync(unit.Id, Arg.Any<CancellationToken>()).Returns(unit);
        _promotionRepository.GetByIdAsync(promotion.Id, Arg.Any<CancellationToken>()).Returns(promotion);
        _unitRepository.ExistsWithFloorAndDoorAsync(promotion.Id, "2", "B", unit.Id, Arg.Any<CancellationToken>()).Returns(true);

        var command = new UpdateHousingUnitCommand(unit.Id, null, "2", "B", 100m, 95m);

        var result = await _useCase.ExecuteAsync(command);

        result.IsFailure.Should().BeTrue();
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenUnitBelongsToAnotherTenant_ShouldReturnFailure()
    {
        var promotion = HousingPromotion.Create(Guid.NewGuid(), "Residencial Acacias", "Madrid", "Calle Mayor 1");
        var unit = HousingUnit.Create(promotion.Id, "1", "A", 90m, 80m);
        _currentUser.DeveloperCompanyId.Returns(Guid.NewGuid());
        _unitRepository.GetByIdAsync(unit.Id, Arg.Any<CancellationToken>()).Returns(unit);
        _promotionRepository.GetByIdAsync(promotion.Id, Arg.Any<CancellationToken>()).Returns(promotion);

        var command = new UpdateHousingUnitCommand(unit.Id, null, "2", "B", 100m, 95m);

        var result = await _useCase.ExecuteAsync(command);

        result.IsFailure.Should().BeTrue();
    }
}

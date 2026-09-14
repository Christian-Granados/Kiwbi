using FluentAssertions;
using Kiwbi.Application.Common;
using Kiwbi.Application.RealEstate.ChangeHousingUnitStatus;
using Kiwbi.Domain.RealEstate;
using NSubstitute;

namespace Kiwbi.Application.Tests.RealEstate.ChangeHousingUnitStatus;

public class ChangeHousingUnitStatusUseCaseTests
{
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();
    private readonly IHousingPromotionRepository _promotionRepository = Substitute.For<IHousingPromotionRepository>();
    private readonly IHousingUnitRepository _unitRepository = Substitute.For<IHousingUnitRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ChangeHousingUnitStatusUseCase _useCase;

    public ChangeHousingUnitStatusUseCaseTests()
    {
        _useCase = new ChangeHousingUnitStatusUseCase(_currentUser, _promotionRepository, _unitRepository, _unitOfWork);
    }

    [Fact]
    public async Task ExecuteAsync_WithOwnedUnit_ShouldChangeStatusAndPersist()
    {
        var developerCompanyId = Guid.NewGuid();
        var promotion = HousingPromotion.Create(developerCompanyId, "Residencial Acacias", "Madrid", "Calle Mayor 1");
        var unit = HousingUnit.Create(promotion.Id, "1", "A", 90m, 80m);
        _currentUser.DeveloperCompanyId.Returns(developerCompanyId);
        _unitRepository.GetByIdAsync(unit.Id, Arg.Any<CancellationToken>()).Returns(unit);
        _promotionRepository.GetByIdAsync(promotion.Id, Arg.Any<CancellationToken>()).Returns(promotion);

        var result = await _useCase.ExecuteAsync(new ChangeHousingUnitStatusCommand(unit.Id, HousingUnitStatus.Reserved));

        result.IsSuccess.Should().BeTrue();
        unit.Status.Should().Be(HousingUnitStatus.Reserved);
        _unitRepository.Received(1).Update(unit);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenUnitBelongsToAnotherTenant_ShouldReturnFailure()
    {
        var promotion = HousingPromotion.Create(Guid.NewGuid(), "Residencial Acacias", "Madrid", "Calle Mayor 1");
        var unit = HousingUnit.Create(promotion.Id, "1", "A", 90m, 80m);
        _currentUser.DeveloperCompanyId.Returns(Guid.NewGuid());
        _unitRepository.GetByIdAsync(unit.Id, Arg.Any<CancellationToken>()).Returns(unit);
        _promotionRepository.GetByIdAsync(promotion.Id, Arg.Any<CancellationToken>()).Returns(promotion);

        var result = await _useCase.ExecuteAsync(new ChangeHousingUnitStatusCommand(unit.Id, HousingUnitStatus.Sold));

        result.IsFailure.Should().BeTrue();
    }
}

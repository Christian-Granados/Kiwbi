using FluentAssertions;
using Kiwbi.Application.Common;
using Kiwbi.Application.RealEstate.DeleteHousingUnit;
using Kiwbi.Domain.Customizations;
using Kiwbi.Domain.RealEstate;
using NSubstitute;

namespace Kiwbi.Application.Tests.RealEstate.DeleteHousingUnit;

public class DeleteHousingUnitUseCaseTests
{
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();
    private readonly IHousingPromotionRepository _promotionRepository = Substitute.For<IHousingPromotionRepository>();
    private readonly IHousingUnitRepository _unitRepository = Substitute.For<IHousingUnitRepository>();
    private readonly ICustomizationRepository _customizationRepository = Substitute.For<ICustomizationRepository>();
    private readonly IFileStorageService _fileStorageService = Substitute.For<IFileStorageService>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly DeleteHousingUnitUseCase _useCase;

    public DeleteHousingUnitUseCaseTests()
    {
        _useCase = new DeleteHousingUnitUseCase(_currentUser, _promotionRepository, _unitRepository, _customizationRepository, _fileStorageService, _unitOfWork);
    }

    [Fact]
    public async Task ExecuteAsync_WithOwnedUnit_ShouldRemoveItAndItsImage()
    {
        var developerCompanyId = Guid.NewGuid();
        var promotion = HousingPromotion.Create(developerCompanyId, "Residencial Acacias", "Madrid", "Calle Mayor 1");
        var unit = HousingUnit.Create(promotion.Id, "1", "A", 90m, 80m);
        unit.UpdateFloorPlanImage("uploads/units/plan.png");
        _currentUser.DeveloperCompanyId.Returns(developerCompanyId);
        _unitRepository.GetByIdAsync(unit.Id, Arg.Any<CancellationToken>()).Returns(unit);
        _promotionRepository.GetByIdAsync(promotion.Id, Arg.Any<CancellationToken>()).Returns(promotion);

        var result = await _useCase.ExecuteAsync(unit.Id);

        result.IsSuccess.Should().BeTrue();
        _unitRepository.Received(1).Remove(unit);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        _fileStorageService.Received(1).Delete("uploads/units/plan.png");
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
        _unitRepository.DidNotReceive().Remove(Arg.Any<HousingUnit>());
    }

    [Fact]
    public async Task ExecuteAsync_WithCustomizationAssignments_ShouldReturnFailureWithoutRemoving()
    {
        var developerCompanyId = Guid.NewGuid();
        var promotion = HousingPromotion.Create(developerCompanyId, "Residencial Acacias", "Madrid", "Calle Mayor 1");
        var unit = HousingUnit.Create(promotion.Id, "1", "A", 90m, 80m);
        _currentUser.DeveloperCompanyId.Returns(developerCompanyId);
        _unitRepository.GetByIdAsync(unit.Id, Arg.Any<CancellationToken>()).Returns(unit);
        _promotionRepository.GetByIdAsync(promotion.Id, Arg.Any<CancellationToken>()).Returns(promotion);
        _customizationRepository.ExistsByHousingUnitIdAsync(unit.Id, Arg.Any<CancellationToken>()).Returns(true);

        var result = await _useCase.ExecuteAsync(unit.Id);

        result.IsFailure.Should().BeTrue();
        _unitRepository.DidNotReceive().Remove(Arg.Any<HousingUnit>());
    }
}

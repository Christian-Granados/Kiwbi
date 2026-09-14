using FluentAssertions;
using Kiwbi.Application.Common;
using Kiwbi.Application.RealEstate.UpdateHousingUnitFloorPlan;
using Kiwbi.Domain.RealEstate;
using NSubstitute;

namespace Kiwbi.Application.Tests.RealEstate.UpdateHousingUnitFloorPlan;

public class UpdateHousingUnitFloorPlanUseCaseTests
{
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();
    private readonly IHousingPromotionRepository _promotionRepository = Substitute.For<IHousingPromotionRepository>();
    private readonly IHousingUnitRepository _unitRepository = Substitute.For<IHousingUnitRepository>();
    private readonly IFileStorageService _fileStorageService = Substitute.For<IFileStorageService>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly UpdateHousingUnitFloorPlanUseCase _useCase;

    public UpdateHousingUnitFloorPlanUseCaseTests()
    {
        _useCase = new UpdateHousingUnitFloorPlanUseCase(_currentUser, _promotionRepository, _unitRepository, _fileStorageService, _unitOfWork);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidFile_ShouldStoreAndSetPath()
    {
        var developerCompanyId = Guid.NewGuid();
        var promotion = HousingPromotion.Create(developerCompanyId, "Residencial Acacias", "Madrid", "Calle Mayor 1");
        var unit = HousingUnit.Create(promotion.Id, "1", "A", 90m, 80m);
        _currentUser.DeveloperCompanyId.Returns(developerCompanyId);
        _unitRepository.GetByIdAsync(unit.Id, Arg.Any<CancellationToken>()).Returns(unit);
        _promotionRepository.GetByIdAsync(promotion.Id, Arg.Any<CancellationToken>()).Returns(promotion);
        _fileStorageService
            .SaveAsync(Arg.Any<Stream>(), "plan.png", "units", Arg.Any<CancellationToken>())
            .Returns(Result.Success("uploads/units/abc.png"));

        using var content = new MemoryStream();
        var command = new UpdateHousingUnitFloorPlanCommand(unit.Id, content, "plan.png");

        var result = await _useCase.ExecuteAsync(command);

        result.IsSuccess.Should().BeTrue();
        unit.FloorPlanImagePath.Should().Be("uploads/units/abc.png");
        _unitRepository.Received(1).Update(unit);
    }

    [Fact]
    public async Task ExecuteAsync_WhenUnitBelongsToAnotherTenant_ShouldReturnFailure()
    {
        var promotion = HousingPromotion.Create(Guid.NewGuid(), "Residencial Acacias", "Madrid", "Calle Mayor 1");
        var unit = HousingUnit.Create(promotion.Id, "1", "A", 90m, 80m);
        _currentUser.DeveloperCompanyId.Returns(Guid.NewGuid());
        _unitRepository.GetByIdAsync(unit.Id, Arg.Any<CancellationToken>()).Returns(unit);
        _promotionRepository.GetByIdAsync(promotion.Id, Arg.Any<CancellationToken>()).Returns(promotion);

        using var content = new MemoryStream();
        var command = new UpdateHousingUnitFloorPlanCommand(unit.Id, content, "plan.png");

        var result = await _useCase.ExecuteAsync(command);

        result.IsFailure.Should().BeTrue();
    }
}

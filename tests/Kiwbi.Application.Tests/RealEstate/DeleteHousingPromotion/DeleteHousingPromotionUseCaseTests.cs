using FluentAssertions;
using Kiwbi.Application.Common;
using Kiwbi.Application.RealEstate.DeleteHousingPromotion;
using Kiwbi.Domain.Customizations;
using Kiwbi.Domain.RealEstate;
using NSubstitute;

namespace Kiwbi.Application.Tests.RealEstate.DeleteHousingPromotion;

public class DeleteHousingPromotionUseCaseTests
{
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();
    private readonly IHousingPromotionRepository _repository = Substitute.For<IHousingPromotionRepository>();
    private readonly IHousingTypologyRepository _typologyRepository = Substitute.For<IHousingTypologyRepository>();
    private readonly IHousingUnitRepository _unitRepository = Substitute.For<IHousingUnitRepository>();
    private readonly ITradeCategoryRepository _tradeCategoryRepository = Substitute.For<ITradeCategoryRepository>();
    private readonly IFileStorageService _fileStorageService = Substitute.For<IFileStorageService>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly DeleteHousingPromotionUseCase _useCase;

    public DeleteHousingPromotionUseCaseTests()
    {
        _useCase = new DeleteHousingPromotionUseCase(_currentUser, _repository, _typologyRepository, _unitRepository, _tradeCategoryRepository, _fileStorageService, _unitOfWork);
    }

    [Fact]
    public async Task ExecuteAsync_WithOwnedPromotion_ShouldRemoveItAndItsImage()
    {
        var developerCompanyId = Guid.NewGuid();
        var promotion = HousingPromotion.Create(developerCompanyId, "Residencial Acacias", "Madrid", "Calle Mayor 1");
        promotion.UpdateMasterPlanImage("uploads/promotions/plan.png");
        _currentUser.DeveloperCompanyId.Returns(developerCompanyId);
        _repository.GetByIdAsync(promotion.Id, Arg.Any<CancellationToken>()).Returns(promotion);

        var result = await _useCase.ExecuteAsync(promotion.Id);

        result.IsSuccess.Should().BeTrue();
        _repository.Received(1).Remove(promotion);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        _fileStorageService.Received(1).Delete("uploads/promotions/plan.png");
    }

    [Fact]
    public async Task ExecuteAsync_WhenPromotionHasTypologies_ShouldReturnFailureWithoutRemoving()
    {
        var developerCompanyId = Guid.NewGuid();
        var promotion = HousingPromotion.Create(developerCompanyId, "Residencial Acacias", "Madrid", "Calle Mayor 1");
        _currentUser.DeveloperCompanyId.Returns(developerCompanyId);
        _repository.GetByIdAsync(promotion.Id, Arg.Any<CancellationToken>()).Returns(promotion);
        _typologyRepository.ExistsByHousingPromotionIdAsync(promotion.Id, Arg.Any<CancellationToken>()).Returns(true);

        var result = await _useCase.ExecuteAsync(promotion.Id);

        result.IsFailure.Should().BeTrue();
        _repository.DidNotReceive().Remove(Arg.Any<HousingPromotion>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenPromotionHasUnits_ShouldReturnFailureWithoutRemoving()
    {
        var developerCompanyId = Guid.NewGuid();
        var promotion = HousingPromotion.Create(developerCompanyId, "Residencial Acacias", "Madrid", "Calle Mayor 1");
        _currentUser.DeveloperCompanyId.Returns(developerCompanyId);
        _repository.GetByIdAsync(promotion.Id, Arg.Any<CancellationToken>()).Returns(promotion);
        _unitRepository.ExistsByHousingPromotionIdAsync(promotion.Id, Arg.Any<CancellationToken>()).Returns(true);

        var result = await _useCase.ExecuteAsync(promotion.Id);

        result.IsFailure.Should().BeTrue();
        _repository.DidNotReceive().Remove(Arg.Any<HousingPromotion>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenPromotionHasTradeCategories_ShouldReturnFailureWithoutRemoving()
    {
        var developerCompanyId = Guid.NewGuid();
        var promotion = HousingPromotion.Create(developerCompanyId, "Residencial Acacias", "Madrid", "Calle Mayor 1");
        _currentUser.DeveloperCompanyId.Returns(developerCompanyId);
        _repository.GetByIdAsync(promotion.Id, Arg.Any<CancellationToken>()).Returns(promotion);
        _tradeCategoryRepository.ExistsByHousingPromotionIdAsync(promotion.Id, Arg.Any<CancellationToken>()).Returns(true);

        var result = await _useCase.ExecuteAsync(promotion.Id);

        result.IsFailure.Should().BeTrue();
        _repository.DidNotReceive().Remove(Arg.Any<HousingPromotion>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenPromotionBelongsToAnotherTenant_ShouldReturnFailure()
    {
        var promotion = HousingPromotion.Create(Guid.NewGuid(), "Residencial Acacias", "Madrid", "Calle Mayor 1");
        _currentUser.DeveloperCompanyId.Returns(Guid.NewGuid());
        _repository.GetByIdAsync(promotion.Id, Arg.Any<CancellationToken>()).Returns(promotion);

        var result = await _useCase.ExecuteAsync(promotion.Id);

        result.IsFailure.Should().BeTrue();
        _repository.DidNotReceive().Remove(Arg.Any<HousingPromotion>());
    }
}

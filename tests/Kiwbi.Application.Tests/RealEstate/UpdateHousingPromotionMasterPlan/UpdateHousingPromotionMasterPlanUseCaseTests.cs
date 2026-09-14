using FluentAssertions;
using Kiwbi.Application.Common;
using Kiwbi.Application.RealEstate.UpdateHousingPromotionMasterPlan;
using Kiwbi.Domain.RealEstate;
using NSubstitute;

namespace Kiwbi.Application.Tests.RealEstate.UpdateHousingPromotionMasterPlan;

public class UpdateHousingPromotionMasterPlanUseCaseTests
{
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();
    private readonly IHousingPromotionRepository _repository = Substitute.For<IHousingPromotionRepository>();
    private readonly IFileStorageService _fileStorageService = Substitute.For<IFileStorageService>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly UpdateHousingPromotionMasterPlanUseCase _useCase;

    public UpdateHousingPromotionMasterPlanUseCaseTests()
    {
        _useCase = new UpdateHousingPromotionMasterPlanUseCase(_currentUser, _repository, _fileStorageService, _unitOfWork);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidFile_ShouldStoreAndSetPath()
    {
        var developerCompanyId = Guid.NewGuid();
        var promotion = HousingPromotion.Create(developerCompanyId, "Residencial Acacias", "Madrid", "Calle Mayor 1");
        _currentUser.DeveloperCompanyId.Returns(developerCompanyId);
        _repository.GetByIdAsync(promotion.Id, Arg.Any<CancellationToken>()).Returns(promotion);
        _fileStorageService
            .SaveAsync(Arg.Any<Stream>(), "plan.png", "promotions", Arg.Any<CancellationToken>())
            .Returns(Result.Success("uploads/promotions/abc.png"));

        using var content = new MemoryStream();
        var command = new UpdateHousingPromotionMasterPlanCommand(promotion.Id, content, "plan.png");

        var result = await _useCase.ExecuteAsync(command);

        result.IsSuccess.Should().BeTrue();
        promotion.MasterPlanImagePath.Should().Be("uploads/promotions/abc.png");
        _repository.Received(1).Update(promotion);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenStorageFails_ShouldReturnFailureWithoutPersisting()
    {
        var developerCompanyId = Guid.NewGuid();
        var promotion = HousingPromotion.Create(developerCompanyId, "Residencial Acacias", "Madrid", "Calle Mayor 1");
        _currentUser.DeveloperCompanyId.Returns(developerCompanyId);
        _repository.GetByIdAsync(promotion.Id, Arg.Any<CancellationToken>()).Returns(promotion);
        _fileStorageService
            .SaveAsync(Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<string>("Formato de archivo no permitido."));

        using var content = new MemoryStream();
        var command = new UpdateHousingPromotionMasterPlanCommand(promotion.Id, content, "plan.exe");

        var result = await _useCase.ExecuteAsync(command);

        result.IsFailure.Should().BeTrue();
        promotion.MasterPlanImagePath.Should().BeNull();
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenReplacingExistingImage_ShouldDeletePreviousFile()
    {
        var developerCompanyId = Guid.NewGuid();
        var promotion = HousingPromotion.Create(developerCompanyId, "Residencial Acacias", "Madrid", "Calle Mayor 1");
        promotion.UpdateMasterPlanImage("uploads/promotions/old.png");
        _currentUser.DeveloperCompanyId.Returns(developerCompanyId);
        _repository.GetByIdAsync(promotion.Id, Arg.Any<CancellationToken>()).Returns(promotion);
        _fileStorageService
            .SaveAsync(Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success("uploads/promotions/new.png"));

        using var content = new MemoryStream();
        var command = new UpdateHousingPromotionMasterPlanCommand(promotion.Id, content, "plan.png");

        await _useCase.ExecuteAsync(command);

        _fileStorageService.Received(1).Delete("uploads/promotions/old.png");
    }

    [Fact]
    public async Task ExecuteAsync_WhenPromotionBelongsToAnotherTenant_ShouldReturnFailure()
    {
        var promotion = HousingPromotion.Create(Guid.NewGuid(), "Residencial Acacias", "Madrid", "Calle Mayor 1");
        _currentUser.DeveloperCompanyId.Returns(Guid.NewGuid());
        _repository.GetByIdAsync(promotion.Id, Arg.Any<CancellationToken>()).Returns(promotion);

        using var content = new MemoryStream();
        var command = new UpdateHousingPromotionMasterPlanCommand(promotion.Id, content, "plan.png");

        var result = await _useCase.ExecuteAsync(command);

        result.IsFailure.Should().BeTrue();
    }
}

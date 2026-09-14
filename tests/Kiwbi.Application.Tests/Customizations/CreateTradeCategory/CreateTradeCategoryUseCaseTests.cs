using FluentAssertions;
using Kiwbi.Application.Common;
using Kiwbi.Application.Customizations.CreateTradeCategory;
using Kiwbi.Domain.Customizations;
using Kiwbi.Domain.RealEstate;
using NSubstitute;

namespace Kiwbi.Application.Tests.Customizations.CreateTradeCategory;

public class CreateTradeCategoryUseCaseTests
{
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();
    private readonly IHousingPromotionRepository _promotionRepository = Substitute.For<IHousingPromotionRepository>();
    private readonly ITradeCategoryRepository _tradeCategoryRepository = Substitute.For<ITradeCategoryRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly CreateTradeCategoryUseCase _useCase;

    public CreateTradeCategoryUseCaseTests()
    {
        _useCase = new CreateTradeCategoryUseCase(_currentUser, _promotionRepository, _tradeCategoryRepository, _unitOfWork);
    }

    [Fact]
    public async Task ExecuteAsync_WithOwnedPromotion_ShouldCreateAndPersistTradeCategory()
    {
        var developerCompanyId = Guid.NewGuid();
        var promotion = HousingPromotion.Create(developerCompanyId, "Residencial Acacias", "Madrid", "Calle Mayor 1");
        _currentUser.DeveloperCompanyId.Returns(developerCompanyId);
        _promotionRepository.GetByIdAsync(promotion.Id, Arg.Any<CancellationToken>()).Returns(promotion);

        var cutOffDate = DateTime.UtcNow.AddMonths(1);
        var command = new CreateTradeCategoryCommand(promotion.Id, "Carpintería", cutOffDate);

        var result = await _useCase.ExecuteAsync(command);

        result.IsSuccess.Should().BeTrue();
        await _tradeCategoryRepository.Received(1).AddAsync(
            Arg.Is<TradeCategory>(t => t.Name == "Carpintería" && t.HousingPromotionId == promotion.Id),
            Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenPromotionBelongsToAnotherTenant_ShouldReturnFailure()
    {
        var promotion = HousingPromotion.Create(Guid.NewGuid(), "Residencial Acacias", "Madrid", "Calle Mayor 1");
        _currentUser.DeveloperCompanyId.Returns(Guid.NewGuid());
        _promotionRepository.GetByIdAsync(promotion.Id, Arg.Any<CancellationToken>()).Returns(promotion);

        var command = new CreateTradeCategoryCommand(promotion.Id, "Carpintería", DateTime.UtcNow.AddMonths(1));

        var result = await _useCase.ExecuteAsync(command);

        result.IsFailure.Should().BeTrue();
        await _tradeCategoryRepository.DidNotReceive().AddAsync(Arg.Any<TradeCategory>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WithInvalidName_ShouldReturnFailureWithoutPersisting()
    {
        var developerCompanyId = Guid.NewGuid();
        var promotion = HousingPromotion.Create(developerCompanyId, "Residencial Acacias", "Madrid", "Calle Mayor 1");
        _currentUser.DeveloperCompanyId.Returns(developerCompanyId);
        _promotionRepository.GetByIdAsync(promotion.Id, Arg.Any<CancellationToken>()).Returns(promotion);

        var command = new CreateTradeCategoryCommand(promotion.Id, "   ", DateTime.UtcNow.AddMonths(1));

        var result = await _useCase.ExecuteAsync(command);

        result.IsFailure.Should().BeTrue();
        await _tradeCategoryRepository.DidNotReceive().AddAsync(Arg.Any<TradeCategory>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WithDefaultCutOffDate_ShouldReturnFailureWithoutPersisting()
    {
        var developerCompanyId = Guid.NewGuid();
        var promotion = HousingPromotion.Create(developerCompanyId, "Residencial Acacias", "Madrid", "Calle Mayor 1");
        _currentUser.DeveloperCompanyId.Returns(developerCompanyId);
        _promotionRepository.GetByIdAsync(promotion.Id, Arg.Any<CancellationToken>()).Returns(promotion);

        var command = new CreateTradeCategoryCommand(promotion.Id, "Carpintería", default);

        var result = await _useCase.ExecuteAsync(command);

        result.IsFailure.Should().BeTrue();
        await _tradeCategoryRepository.DidNotReceive().AddAsync(Arg.Any<TradeCategory>(), Arg.Any<CancellationToken>());
    }
}

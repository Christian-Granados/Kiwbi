using FluentAssertions;
using Kiwbi.Application.Common;
using Kiwbi.Application.Customizations.UpdateTradeCategory;
using Kiwbi.Domain.Customizations;
using Kiwbi.Domain.RealEstate;
using NSubstitute;

namespace Kiwbi.Application.Tests.Customizations.UpdateTradeCategory;

public class UpdateTradeCategoryUseCaseTests
{
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();
    private readonly IHousingPromotionRepository _promotionRepository = Substitute.For<IHousingPromotionRepository>();
    private readonly ITradeCategoryRepository _tradeCategoryRepository = Substitute.For<ITradeCategoryRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly UpdateTradeCategoryUseCase _useCase;

    public UpdateTradeCategoryUseCaseTests()
    {
        _useCase = new UpdateTradeCategoryUseCase(_currentUser, _promotionRepository, _tradeCategoryRepository, _unitOfWork);
    }

    [Fact]
    public async Task ExecuteAsync_WithOwnedTradeCategory_ShouldRenameRescheduleAndPersist()
    {
        var developerCompanyId = Guid.NewGuid();
        var promotion = HousingPromotion.Create(developerCompanyId, "Residencial Acacias", "Madrid", "Calle Mayor 1");
        var tradeCategory = TradeCategory.Create(promotion.Id, "Carpintería", DateTime.UtcNow.AddMonths(1));
        _currentUser.DeveloperCompanyId.Returns(developerCompanyId);
        _tradeCategoryRepository.GetByIdAsync(tradeCategory.Id, Arg.Any<CancellationToken>()).Returns(tradeCategory);
        _promotionRepository.GetByIdAsync(promotion.Id, Arg.Any<CancellationToken>()).Returns(promotion);

        var newCutOffDate = DateTime.UtcNow.AddMonths(2);
        var command = new UpdateTradeCategoryCommand(tradeCategory.Id, "Fontanería", newCutOffDate);

        var result = await _useCase.ExecuteAsync(command);

        result.IsSuccess.Should().BeTrue();
        tradeCategory.Name.Should().Be("Fontanería");
        tradeCategory.SelectionCutOffDateUtc.Should().Be(newCutOffDate);
        _tradeCategoryRepository.Received(1).Update(tradeCategory);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenTradeCategoryBelongsToAnotherTenant_ShouldReturnFailure()
    {
        var promotion = HousingPromotion.Create(Guid.NewGuid(), "Residencial Acacias", "Madrid", "Calle Mayor 1");
        var tradeCategory = TradeCategory.Create(promotion.Id, "Carpintería", DateTime.UtcNow.AddMonths(1));
        _currentUser.DeveloperCompanyId.Returns(Guid.NewGuid());
        _tradeCategoryRepository.GetByIdAsync(tradeCategory.Id, Arg.Any<CancellationToken>()).Returns(tradeCategory);
        _promotionRepository.GetByIdAsync(promotion.Id, Arg.Any<CancellationToken>()).Returns(promotion);

        var command = new UpdateTradeCategoryCommand(tradeCategory.Id, "Fontanería", DateTime.UtcNow.AddMonths(2));

        var result = await _useCase.ExecuteAsync(command);

        result.IsFailure.Should().BeTrue();
        _tradeCategoryRepository.DidNotReceive().Update(Arg.Any<TradeCategory>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenTradeCategoryDoesNotExist_ShouldReturnFailure()
    {
        _currentUser.DeveloperCompanyId.Returns(Guid.NewGuid());
        _tradeCategoryRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((TradeCategory?)null);

        var command = new UpdateTradeCategoryCommand(Guid.NewGuid(), "Fontanería", DateTime.UtcNow.AddMonths(2));

        var result = await _useCase.ExecuteAsync(command);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteAsync_WithInvalidName_ShouldReturnFailureAndKeepPreviousValue()
    {
        var developerCompanyId = Guid.NewGuid();
        var promotion = HousingPromotion.Create(developerCompanyId, "Residencial Acacias", "Madrid", "Calle Mayor 1");
        var tradeCategory = TradeCategory.Create(promotion.Id, "Carpintería", DateTime.UtcNow.AddMonths(1));
        _currentUser.DeveloperCompanyId.Returns(developerCompanyId);
        _tradeCategoryRepository.GetByIdAsync(tradeCategory.Id, Arg.Any<CancellationToken>()).Returns(tradeCategory);
        _promotionRepository.GetByIdAsync(promotion.Id, Arg.Any<CancellationToken>()).Returns(promotion);

        var command = new UpdateTradeCategoryCommand(tradeCategory.Id, "   ", DateTime.UtcNow.AddMonths(2));

        var result = await _useCase.ExecuteAsync(command);

        result.IsFailure.Should().BeTrue();
        tradeCategory.Name.Should().Be("Carpintería");
        _tradeCategoryRepository.DidNotReceive().Update(Arg.Any<TradeCategory>());
    }
}

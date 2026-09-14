using FluentAssertions;
using Kiwbi.Application.Common;
using Kiwbi.Application.Customizations.DeleteTradeCategory;
using Kiwbi.Domain.Customizations;
using Kiwbi.Domain.RealEstate;
using NSubstitute;

namespace Kiwbi.Application.Tests.Customizations.DeleteTradeCategory;

public class DeleteTradeCategoryUseCaseTests
{
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();
    private readonly IHousingPromotionRepository _promotionRepository = Substitute.For<IHousingPromotionRepository>();
    private readonly ITradeCategoryRepository _tradeCategoryRepository = Substitute.For<ITradeCategoryRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly DeleteTradeCategoryUseCase _useCase;

    public DeleteTradeCategoryUseCaseTests()
    {
        _useCase = new DeleteTradeCategoryUseCase(_currentUser, _promotionRepository, _tradeCategoryRepository, _unitOfWork);
    }

    [Fact]
    public async Task ExecuteAsync_WithOwnedTradeCategory_ShouldRemoveIt()
    {
        var developerCompanyId = Guid.NewGuid();
        var promotion = HousingPromotion.Create(developerCompanyId, "Residencial Acacias", "Madrid", "Calle Mayor 1");
        var tradeCategory = TradeCategory.Create(promotion.Id, "Carpintería", DateTime.UtcNow.AddMonths(1));
        _currentUser.DeveloperCompanyId.Returns(developerCompanyId);
        _tradeCategoryRepository.GetByIdAsync(tradeCategory.Id, Arg.Any<CancellationToken>()).Returns(tradeCategory);
        _promotionRepository.GetByIdAsync(promotion.Id, Arg.Any<CancellationToken>()).Returns(promotion);

        var result = await _useCase.ExecuteAsync(tradeCategory.Id);

        result.IsSuccess.Should().BeTrue();
        _tradeCategoryRepository.Received(1).Remove(tradeCategory);
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

        var result = await _useCase.ExecuteAsync(tradeCategory.Id);

        result.IsFailure.Should().BeTrue();
        _tradeCategoryRepository.DidNotReceive().Remove(Arg.Any<TradeCategory>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenTradeCategoryDoesNotExist_ShouldReturnFailure()
    {
        _currentUser.DeveloperCompanyId.Returns(Guid.NewGuid());
        _tradeCategoryRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((TradeCategory?)null);

        var result = await _useCase.ExecuteAsync(Guid.NewGuid());

        result.IsFailure.Should().BeTrue();
    }
}

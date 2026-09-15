using FluentAssertions;
using Kiwbi.Application.Common;
using Kiwbi.Application.Customizations.AddCustomizationOption;
using Kiwbi.Domain.Customizations;
using Kiwbi.Domain.RealEstate;
using NSubstitute;

namespace Kiwbi.Application.Tests.Customizations.AddCustomizationOption;

public class AddCustomizationOptionUseCaseTests
{
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();
    private readonly IHousingPromotionRepository _promotionRepository = Substitute.For<IHousingPromotionRepository>();
    private readonly ITradeCategoryRepository _tradeCategoryRepository = Substitute.For<ITradeCategoryRepository>();
    private readonly ICustomizationRepository _customizationRepository = Substitute.For<ICustomizationRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly AddCustomizationOptionUseCase _useCase;

    public AddCustomizationOptionUseCaseTests()
    {
        _useCase = new AddCustomizationOptionUseCase(_currentUser, _promotionRepository, _tradeCategoryRepository, _customizationRepository, _unitOfWork);
    }

    private (Customization Customization, TradeCategory TradeCategory, HousingPromotion Promotion) SetUpOwnedCustomization(Guid developerCompanyId)
    {
        var promotion = HousingPromotion.Create(developerCompanyId, "Residencial Acacias", "Madrid", "Calle Mayor 1");
        var tradeCategory = TradeCategory.Create(promotion.Id, "Carpintería", DateTime.UtcNow.AddMonths(1));
        var customization = Customization.CreateForWholePromotion(tradeCategory.Id, "Suelo", "Parquet Roble", 0m);
        _customizationRepository.GetByIdAsync(customization.Id, Arg.Any<CancellationToken>()).Returns(customization);
        _tradeCategoryRepository.GetByIdAsync(tradeCategory.Id, Arg.Any<CancellationToken>()).Returns(tradeCategory);
        _promotionRepository.GetByIdAsync(promotion.Id, Arg.Any<CancellationToken>()).Returns(promotion);

        return (customization, tradeCategory, promotion);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidData_ShouldAddOptionAndPersist()
    {
        var developerCompanyId = Guid.NewGuid();
        var (customization, _, _) = SetUpOwnedCustomization(developerCompanyId);
        _currentUser.DeveloperCompanyId.Returns(developerCompanyId);

        var result = await _useCase.ExecuteAsync(new AddCustomizationOptionCommand(customization.Id, "Porcelánico Premium", 500m));

        result.IsSuccess.Should().BeTrue();
        customization.Options.Should().ContainSingle(o => o.Name == "Porcelánico Premium" && o.SurchargeAmount == 500m);
        _customizationRepository.Received(1).Update(customization);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WithNegativeSurcharge_ShouldReturnFailureWithoutPersisting()
    {
        var developerCompanyId = Guid.NewGuid();
        var (customization, _, _) = SetUpOwnedCustomization(developerCompanyId);
        _currentUser.DeveloperCompanyId.Returns(developerCompanyId);

        var result = await _useCase.ExecuteAsync(new AddCustomizationOptionCommand(customization.Id, "Porcelánico Premium", -1m));

        result.IsFailure.Should().BeTrue();
        _customizationRepository.DidNotReceive().Update(Arg.Any<Customization>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenCustomizationBelongsToAnotherTenant_ShouldReturnFailure()
    {
        var (customization, _, _) = SetUpOwnedCustomization(Guid.NewGuid());
        _currentUser.DeveloperCompanyId.Returns(Guid.NewGuid());

        var result = await _useCase.ExecuteAsync(new AddCustomizationOptionCommand(customization.Id, "Porcelánico Premium", 500m));

        result.IsFailure.Should().BeTrue();
        _customizationRepository.DidNotReceive().Update(Arg.Any<Customization>());
    }
}

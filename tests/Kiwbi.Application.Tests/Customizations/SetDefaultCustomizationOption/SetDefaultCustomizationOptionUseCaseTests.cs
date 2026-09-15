using FluentAssertions;
using Kiwbi.Application.Common;
using Kiwbi.Application.Customizations.SetDefaultCustomizationOption;
using Kiwbi.Domain.Customizations;
using Kiwbi.Domain.RealEstate;
using NSubstitute;

namespace Kiwbi.Application.Tests.Customizations.SetDefaultCustomizationOption;

public class SetDefaultCustomizationOptionUseCaseTests
{
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();
    private readonly IHousingPromotionRepository _promotionRepository = Substitute.For<IHousingPromotionRepository>();
    private readonly ITradeCategoryRepository _tradeCategoryRepository = Substitute.For<ITradeCategoryRepository>();
    private readonly ICustomizationRepository _customizationRepository = Substitute.For<ICustomizationRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly SetDefaultCustomizationOptionUseCase _useCase;

    public SetDefaultCustomizationOptionUseCaseTests()
    {
        _useCase = new SetDefaultCustomizationOptionUseCase(_currentUser, _promotionRepository, _tradeCategoryRepository, _customizationRepository, _unitOfWork);
    }

    private (Customization Customization, Guid NewDefaultOptionId) SetUpOwnedCustomizationWithTwoOptions(Guid developerCompanyId)
    {
        var promotion = HousingPromotion.Create(developerCompanyId, "Residencial Acacias", "Madrid", "Calle Mayor 1");
        var tradeCategory = TradeCategory.Create(promotion.Id, "Carpintería", DateTime.UtcNow.AddMonths(1));
        var customization = Customization.CreateForWholePromotion(tradeCategory.Id, "Suelo", "Parquet Roble", 0m);
        customization.AddOption("Porcelánico Premium", 500m);
        var newDefaultOptionId = customization.Options.Single(o => o.Name == "Porcelánico Premium").Id;
        _customizationRepository.GetByIdAsync(customization.Id, Arg.Any<CancellationToken>()).Returns(customization);
        _tradeCategoryRepository.GetByIdAsync(tradeCategory.Id, Arg.Any<CancellationToken>()).Returns(tradeCategory);
        _promotionRepository.GetByIdAsync(promotion.Id, Arg.Any<CancellationToken>()).Returns(promotion);

        return (customization, newDefaultOptionId);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidOption_ShouldChangeDefaultAndPersist()
    {
        var developerCompanyId = Guid.NewGuid();
        var (customization, newDefaultOptionId) = SetUpOwnedCustomizationWithTwoOptions(developerCompanyId);
        _currentUser.DeveloperCompanyId.Returns(developerCompanyId);

        var result = await _useCase.ExecuteAsync(customization.Id, newDefaultOptionId);

        result.IsSuccess.Should().BeTrue();
        customization.Options.Should().ContainSingle(o => o.IsDefault && o.Id == newDefaultOptionId);
        _customizationRepository.Received(1).Update(customization);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenCustomizationBelongsToAnotherTenant_ShouldReturnFailure()
    {
        var (customization, newDefaultOptionId) = SetUpOwnedCustomizationWithTwoOptions(Guid.NewGuid());
        _currentUser.DeveloperCompanyId.Returns(Guid.NewGuid());

        var result = await _useCase.ExecuteAsync(customization.Id, newDefaultOptionId);

        result.IsFailure.Should().BeTrue();
        _customizationRepository.DidNotReceive().Update(Arg.Any<Customization>());
    }
}

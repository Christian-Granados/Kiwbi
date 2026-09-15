using FluentAssertions;
using Kiwbi.Application.Common;
using Kiwbi.Application.Customizations.RemoveCustomizationOption;
using Kiwbi.Domain.Customizations;
using Kiwbi.Domain.RealEstate;
using NSubstitute;

namespace Kiwbi.Application.Tests.Customizations.RemoveCustomizationOption;

public class RemoveCustomizationOptionUseCaseTests
{
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();
    private readonly IHousingPromotionRepository _promotionRepository = Substitute.For<IHousingPromotionRepository>();
    private readonly ITradeCategoryRepository _tradeCategoryRepository = Substitute.For<ITradeCategoryRepository>();
    private readonly ICustomizationRepository _customizationRepository = Substitute.For<ICustomizationRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly RemoveCustomizationOptionUseCase _useCase;

    public RemoveCustomizationOptionUseCaseTests()
    {
        _useCase = new RemoveCustomizationOptionUseCase(_currentUser, _promotionRepository, _tradeCategoryRepository, _customizationRepository, _unitOfWork);
    }

    private (Customization Customization, Guid NonDefaultOptionId) SetUpOwnedCustomizationWithTwoOptions(Guid developerCompanyId)
    {
        var promotion = HousingPromotion.Create(developerCompanyId, "Residencial Acacias", "Madrid", "Calle Mayor 1");
        var tradeCategory = TradeCategory.Create(promotion.Id, "Carpintería", DateTime.UtcNow.AddMonths(1));
        var customization = Customization.CreateForWholePromotion(tradeCategory.Id, "Suelo", "Parquet Roble", 0m);
        customization.AddOption("Porcelánico Premium", 500m);
        var nonDefaultOptionId = customization.Options.Single(o => !o.IsDefault).Id;
        _customizationRepository.GetByIdAsync(customization.Id, Arg.Any<CancellationToken>()).Returns(customization);
        _tradeCategoryRepository.GetByIdAsync(tradeCategory.Id, Arg.Any<CancellationToken>()).Returns(tradeCategory);
        _promotionRepository.GetByIdAsync(promotion.Id, Arg.Any<CancellationToken>()).Returns(promotion);

        return (customization, nonDefaultOptionId);
    }

    [Fact]
    public async Task ExecuteAsync_WithNonDefaultOptionAndMoreThanOneExists_ShouldRemoveAndPersist()
    {
        var developerCompanyId = Guid.NewGuid();
        var (customization, nonDefaultOptionId) = SetUpOwnedCustomizationWithTwoOptions(developerCompanyId);
        _currentUser.DeveloperCompanyId.Returns(developerCompanyId);

        var result = await _useCase.ExecuteAsync(customization.Id, nonDefaultOptionId);

        result.IsSuccess.Should().BeTrue();
        customization.Options.Should().ContainSingle();
        _customizationRepository.Received(1).Update(customization);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenOptionIsDefault_ShouldReturnFailureWithoutPersisting()
    {
        var developerCompanyId = Guid.NewGuid();
        var (customization, _) = SetUpOwnedCustomizationWithTwoOptions(developerCompanyId);
        _currentUser.DeveloperCompanyId.Returns(developerCompanyId);
        var defaultOptionId = customization.Options.Single(o => o.IsDefault).Id;

        var result = await _useCase.ExecuteAsync(customization.Id, defaultOptionId);

        result.IsFailure.Should().BeTrue();
        _customizationRepository.DidNotReceive().Update(Arg.Any<Customization>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenCustomizationBelongsToAnotherTenant_ShouldReturnFailure()
    {
        var (customization, nonDefaultOptionId) = SetUpOwnedCustomizationWithTwoOptions(Guid.NewGuid());
        _currentUser.DeveloperCompanyId.Returns(Guid.NewGuid());

        var result = await _useCase.ExecuteAsync(customization.Id, nonDefaultOptionId);

        result.IsFailure.Should().BeTrue();
        _customizationRepository.DidNotReceive().Update(Arg.Any<Customization>());
    }
}

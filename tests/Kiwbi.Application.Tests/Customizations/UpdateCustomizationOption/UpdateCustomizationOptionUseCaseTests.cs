using FluentAssertions;
using Kiwbi.Application.Common;
using Kiwbi.Application.Customizations.UpdateCustomizationOption;
using Kiwbi.Domain.Customizations;
using Kiwbi.Domain.RealEstate;
using NSubstitute;

namespace Kiwbi.Application.Tests.Customizations.UpdateCustomizationOption;

public class UpdateCustomizationOptionUseCaseTests
{
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();
    private readonly IHousingPromotionRepository _promotionRepository = Substitute.For<IHousingPromotionRepository>();
    private readonly ITradeCategoryRepository _tradeCategoryRepository = Substitute.For<ITradeCategoryRepository>();
    private readonly ICustomizationRepository _customizationRepository = Substitute.For<ICustomizationRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly UpdateCustomizationOptionUseCase _useCase;

    public UpdateCustomizationOptionUseCaseTests()
    {
        _useCase = new UpdateCustomizationOptionUseCase(_currentUser, _promotionRepository, _tradeCategoryRepository, _customizationRepository, _unitOfWork);
    }

    private (Customization Customization, Guid OptionId) SetUpOwnedCustomization(Guid developerCompanyId)
    {
        var promotion = HousingPromotion.Create(developerCompanyId, "Residencial Acacias", "Madrid", "Calle Mayor 1");
        var tradeCategory = TradeCategory.Create(promotion.Id, "Carpintería", DateTime.UtcNow.AddMonths(1));
        var customization = Customization.CreateForWholePromotion(tradeCategory.Id, "Suelo", "Parquet Roble", 0m);
        _customizationRepository.GetByIdAsync(customization.Id, Arg.Any<CancellationToken>()).Returns(customization);
        _tradeCategoryRepository.GetByIdAsync(tradeCategory.Id, Arg.Any<CancellationToken>()).Returns(tradeCategory);
        _promotionRepository.GetByIdAsync(promotion.Id, Arg.Any<CancellationToken>()).Returns(promotion);

        return (customization, customization.Options.Single().Id);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidData_ShouldUpdateOptionAndPersist()
    {
        var developerCompanyId = Guid.NewGuid();
        var (customization, optionId) = SetUpOwnedCustomization(developerCompanyId);
        _currentUser.DeveloperCompanyId.Returns(developerCompanyId);

        var result = await _useCase.ExecuteAsync(new UpdateCustomizationOptionCommand(customization.Id, optionId, "Parquet Roble Natural", 50m));

        result.IsSuccess.Should().BeTrue();
        var option = customization.Options.Single();
        option.Name.Should().Be("Parquet Roble Natural");
        option.SurchargeAmount.Should().Be(50m);
        _customizationRepository.Received(1).Update(customization);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenCustomizationBelongsToAnotherTenant_ShouldReturnFailure()
    {
        var (customization, optionId) = SetUpOwnedCustomization(Guid.NewGuid());
        _currentUser.DeveloperCompanyId.Returns(Guid.NewGuid());

        var result = await _useCase.ExecuteAsync(new UpdateCustomizationOptionCommand(customization.Id, optionId, "Parquet Roble Natural", 50m));

        result.IsFailure.Should().BeTrue();
        _customizationRepository.DidNotReceive().Update(Arg.Any<Customization>());
    }
}

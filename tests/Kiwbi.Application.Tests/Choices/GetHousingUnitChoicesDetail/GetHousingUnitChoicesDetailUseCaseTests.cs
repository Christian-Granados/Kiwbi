using FluentAssertions;
using Kiwbi.Application.Choices.GetHousingUnitChoicesDetail;
using Kiwbi.Application.Common;
using Kiwbi.Domain.Choices;
using Kiwbi.Domain.Customizations;
using Kiwbi.Domain.RealEstate;
using NSubstitute;

namespace Kiwbi.Application.Tests.Choices.GetHousingUnitChoicesDetail;

public class GetHousingUnitChoicesDetailUseCaseTests
{
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();
    private readonly IHousingPromotionRepository _housingPromotionRepository = Substitute.For<IHousingPromotionRepository>();
    private readonly IHousingUnitRepository _housingUnitRepository = Substitute.For<IHousingUnitRepository>();
    private readonly ITradeCategoryRepository _tradeCategoryRepository = Substitute.For<ITradeCategoryRepository>();
    private readonly ICustomizationRepository _customizationRepository = Substitute.For<ICustomizationRepository>();
    private readonly IHomeCustomizationChoiceRepository _homeCustomizationChoiceRepository = Substitute.For<IHomeCustomizationChoiceRepository>();
    private readonly GetHousingUnitChoicesDetailUseCase _useCase;

    public GetHousingUnitChoicesDetailUseCaseTests()
    {
        _useCase = new GetHousingUnitChoicesDetailUseCase(
            _currentUser,
            _housingPromotionRepository,
            _housingUnitRepository,
            _tradeCategoryRepository,
            _customizationRepository,
            _homeCustomizationChoiceRepository);
    }

    [Fact]
    public async Task ExecuteAsync_WhenUnitBelongsToAnotherTenant_ShouldReturnFailure()
    {
        var promotion = HousingPromotion.Create(Guid.NewGuid(), "Residencial Acacias", "Madrid", "Calle Mayor 1");
        var unit = HousingUnit.Create(promotion.Id, "1", "A", 90m, 80m);
        _currentUser.DeveloperCompanyId.Returns(Guid.NewGuid());
        _housingUnitRepository.GetByIdAsync(unit.Id, Arg.Any<CancellationToken>()).Returns(unit);
        _housingPromotionRepository.GetByIdAsync(promotion.Id, Arg.Any<CancellationToken>()).Returns(promotion);

        var result = await _useCase.ExecuteAsync(unit.Id);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteAsync_WhenUnitDoesNotExist_ShouldReturnFailure()
    {
        _currentUser.DeveloperCompanyId.Returns(Guid.NewGuid());
        _housingUnitRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((HousingUnit?)null);

        var result = await _useCase.ExecuteAsync(Guid.NewGuid());

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteAsync_WithNoApplicableCustomizations_ShouldOmitTradeCategory()
    {
        var developerCompanyId = Guid.NewGuid();
        var promotion = HousingPromotion.Create(developerCompanyId, "Residencial Acacias", "Madrid", "Calle Mayor 1");
        var unit = HousingUnit.Create(promotion.Id, "1", "A", 90m, 80m);
        var tradeCategory = TradeCategory.Create(promotion.Id, "Carpintería", DateTime.UtcNow.AddMonths(1));
        var otherUnitId = Guid.NewGuid();
        var customization = Customization.CreateForUnits(tradeCategory.Id, "Suelo", "Parquet", 0m, [otherUnitId]);
        SetupOwnedUnit(promotion, developerCompanyId, unit, [tradeCategory], [customization]);

        var result = await _useCase.ExecuteAsync(unit.Id);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task ExecuteAsync_WithSelectedChoice_ShouldExposeStatusAndEffectiveOption()
    {
        var developerCompanyId = Guid.NewGuid();
        var promotion = HousingPromotion.Create(developerCompanyId, "Residencial Acacias", "Madrid", "Calle Mayor 1");
        var unit = HousingUnit.Create(promotion.Id, "1", "A", 90m, 80m);
        var tradeCategory = TradeCategory.Create(promotion.Id, "Carpintería", DateTime.UtcNow.AddMonths(1));
        var customization = Customization.CreateForWholePromotion(tradeCategory.Id, "Suelo", "Parquet", 0m);
        customization.AddOption("Porcelánico Premium", 500m);
        var selectedOption = customization.Options.Single(o => !o.IsDefault);
        var choice = HomeCustomizationChoice.Create(unit.Id, customization.Id);
        choice.SelectOption(selectedOption.Id, DateTime.UtcNow);
        SetupOwnedUnit(promotion, developerCompanyId, unit, [tradeCategory], [customization]);
        _homeCustomizationChoiceRepository.GetByHousingUnitIdAsync(unit.Id, Arg.Any<CancellationToken>()).Returns([choice]);

        var result = await _useCase.ExecuteAsync(unit.Id);

        var dto = result.Value!.Single().Customizations.Single();
        dto.Status.Should().Be(HomeCustomizationChoiceStatus.Selected);
        dto.EffectiveOptionId.Should().Be(selectedOption.Id);
        dto.EffectiveOptionSurchargeAmount.Should().Be(500m);
    }

    [Fact]
    public async Task ExecuteAsync_WhenTradeCategoryExpiredWithoutChoice_ShouldReturnPendingWithDefaultOptionAsEffective()
    {
        var developerCompanyId = Guid.NewGuid();
        var promotion = HousingPromotion.Create(developerCompanyId, "Residencial Acacias", "Madrid", "Calle Mayor 1");
        var unit = HousingUnit.Create(promotion.Id, "1", "A", 90m, 80m);
        var tradeCategory = TradeCategory.Create(promotion.Id, "Carpintería", DateTime.UtcNow.AddMonths(1));
        tradeCategory.Reschedule(DateTime.UtcNow.AddDays(-1));
        var customization = Customization.CreateForWholePromotion(tradeCategory.Id, "Suelo", "Parquet", 0m);
        var defaultOptionId = customization.Options.Single().Id;
        SetupOwnedUnit(promotion, developerCompanyId, unit, [tradeCategory], [customization]);
        _homeCustomizationChoiceRepository.GetByHousingUnitIdAsync(unit.Id, Arg.Any<CancellationToken>()).Returns([]);

        var result = await _useCase.ExecuteAsync(unit.Id);

        var dto = result.Value!.Single().Customizations.Single();
        dto.Status.Should().Be(HomeCustomizationChoiceStatus.Pending);
        dto.EffectiveOptionId.Should().Be(defaultOptionId);
    }

    private void SetupOwnedUnit(
        HousingPromotion promotion,
        Guid developerCompanyId,
        HousingUnit unit,
        IReadOnlyList<TradeCategory> tradeCategories,
        IReadOnlyList<Customization> customizations)
    {
        _currentUser.DeveloperCompanyId.Returns(developerCompanyId);
        _housingUnitRepository.GetByIdAsync(unit.Id, Arg.Any<CancellationToken>()).Returns(unit);
        _housingPromotionRepository.GetByIdAsync(promotion.Id, Arg.Any<CancellationToken>()).Returns(promotion);
        _tradeCategoryRepository.GetByHousingPromotionIdAsync(promotion.Id, Arg.Any<CancellationToken>()).Returns(tradeCategories);

        foreach (var tradeCategory in tradeCategories)
        {
            _customizationRepository.GetByTradeCategoryIdAsync(tradeCategory.Id, Arg.Any<CancellationToken>()).Returns(customizations);
        }

        _homeCustomizationChoiceRepository.GetByHousingUnitIdAsync(unit.Id, Arg.Any<CancellationToken>()).Returns([]);
    }
}

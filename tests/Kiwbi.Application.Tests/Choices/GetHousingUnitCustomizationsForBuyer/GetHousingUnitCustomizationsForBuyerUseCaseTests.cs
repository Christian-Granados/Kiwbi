using FluentAssertions;
using Kiwbi.Application.Choices.GetHousingUnitCustomizationsForBuyer;
using Kiwbi.Application.Common;
using Kiwbi.Domain.Choices;
using Kiwbi.Domain.Customizations;
using Kiwbi.Domain.Onboarding;
using Kiwbi.Domain.RealEstate;
using NSubstitute;

namespace Kiwbi.Application.Tests.Choices.GetHousingUnitCustomizationsForBuyer;

public class GetHousingUnitCustomizationsForBuyerUseCaseTests
{
    private const string BuyerUserId = "buyer-1";

    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();
    private readonly IHousingUnitBuyerRepository _housingUnitBuyerRepository = Substitute.For<IHousingUnitBuyerRepository>();
    private readonly IHousingUnitRepository _housingUnitRepository = Substitute.For<IHousingUnitRepository>();
    private readonly ITradeCategoryRepository _tradeCategoryRepository = Substitute.For<ITradeCategoryRepository>();
    private readonly ICustomizationRepository _customizationRepository = Substitute.For<ICustomizationRepository>();
    private readonly IHomeCustomizationChoiceRepository _homeCustomizationChoiceRepository = Substitute.For<IHomeCustomizationChoiceRepository>();
    private readonly GetHousingUnitCustomizationsForBuyerUseCase _useCase;

    public GetHousingUnitCustomizationsForBuyerUseCaseTests()
    {
        _currentUser.UserId.Returns(BuyerUserId);
        _useCase = new GetHousingUnitCustomizationsForBuyerUseCase(
            _currentUser, _housingUnitBuyerRepository, _housingUnitRepository, _tradeCategoryRepository, _customizationRepository, _homeCustomizationChoiceRepository);
    }

    [Fact]
    public async Task ExecuteAsync_WhenUserNotAuthenticated_ShouldReturnFailure()
    {
        _currentUser.UserId.Returns((string?)null);

        var result = await _useCase.ExecuteAsync(Guid.NewGuid());

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteAsync_WhenBuyerNotLinkedToUnit_ShouldReturnFailure()
    {
        var unitId = Guid.NewGuid();
        _housingUnitBuyerRepository.ExistsByHousingUnitIdAndBuyerUserIdAsync(unitId, BuyerUserId, Arg.Any<CancellationToken>()).Returns(false);

        var result = await _useCase.ExecuteAsync(unitId);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteAsync_WithWholePromotionCustomization_ShouldIncludeItRegardlessOfTypology()
    {
        var promotion = HousingPromotion.Create(Guid.NewGuid(), "Residencial Acacias", "Madrid", "Calle Mayor 1");
        var unit = HousingUnit.Create(promotion.Id, "1", "A", 90m, 80m);
        var tradeCategory = TradeCategory.Create(promotion.Id, "Carpintería", DateTime.UtcNow.AddMonths(1));
        var customization = Customization.CreateForWholePromotion(tradeCategory.Id, "Suelo", "Parquet Roble", 0m);
        SetupOwnedUnit(unit, tradeCategory, [customization]);

        var result = await _useCase.ExecuteAsync(unit.Id);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().ContainSingle(tc => tc.Customizations.Any(c => c.Id == customization.Id));
    }

    [Fact]
    public async Task ExecuteAsync_WithTypologyScopedCustomization_ShouldOnlyApplyWhenUnitHasThatTypology()
    {
        var promotion = HousingPromotion.Create(Guid.NewGuid(), "Residencial Acacias", "Madrid", "Calle Mayor 1");
        var typologyId = Guid.NewGuid();
        var unit = HousingUnit.Create(promotion.Id, "1", "A", 90m, 80m, typologyId);
        var tradeCategory = TradeCategory.Create(promotion.Id, "Carpintería", DateTime.UtcNow.AddMonths(1));
        var customization = Customization.CreateForTypologies(tradeCategory.Id, "Suelo", "Parquet Roble", 0m, [typologyId]);
        SetupOwnedUnit(unit, tradeCategory, [customization]);

        var result = await _useCase.ExecuteAsync(unit.Id);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().ContainSingle(tc => tc.Customizations.Any(c => c.Id == customization.Id));
    }

    [Fact]
    public async Task ExecuteAsync_WithTypologyScopedCustomization_ShouldExcludeItWhenUnitHasDifferentTypology()
    {
        var promotion = HousingPromotion.Create(Guid.NewGuid(), "Residencial Acacias", "Madrid", "Calle Mayor 1");
        var unit = HousingUnit.Create(promotion.Id, "1", "A", 90m, 80m, Guid.NewGuid());
        var tradeCategory = TradeCategory.Create(promotion.Id, "Carpintería", DateTime.UtcNow.AddMonths(1));
        var customization = Customization.CreateForTypologies(tradeCategory.Id, "Suelo", "Parquet Roble", 0m, [Guid.NewGuid()]);
        SetupOwnedUnit(unit, tradeCategory, [customization]);

        var result = await _useCase.ExecuteAsync(unit.Id);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task ExecuteAsync_WithUnitScopedCustomization_ShouldOnlyApplyToThatUnit()
    {
        var promotion = HousingPromotion.Create(Guid.NewGuid(), "Residencial Acacias", "Madrid", "Calle Mayor 1");
        var unit = HousingUnit.Create(promotion.Id, "1", "A", 90m, 80m);
        var otherUnitId = Guid.NewGuid();
        var tradeCategory = TradeCategory.Create(promotion.Id, "Carpintería", DateTime.UtcNow.AddMonths(1));
        var customization = Customization.CreateForUnits(tradeCategory.Id, "Suelo", "Parquet Roble", 0m, [otherUnitId]);
        SetupOwnedUnit(unit, tradeCategory, [customization]);

        var result = await _useCase.ExecuteAsync(unit.Id);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task ExecuteAsync_WhenCutOffExpiredWithoutChoice_ShouldMarkDefaultOptionAsEffectiveAndReadOnly()
    {
        var promotion = HousingPromotion.Create(Guid.NewGuid(), "Residencial Acacias", "Madrid", "Calle Mayor 1");
        var unit = HousingUnit.Create(promotion.Id, "1", "A", 90m, 80m);
        var tradeCategory = TradeCategory.Create(promotion.Id, "Carpintería", DateTime.UtcNow.AddMonths(1));
        tradeCategory.Reschedule(DateTime.UtcNow.AddDays(-1));
        var customization = Customization.CreateForWholePromotion(tradeCategory.Id, "Suelo", "Parquet Roble", 0m);
        var defaultOptionId = customization.Options.Single().Id;
        SetupOwnedUnit(unit, tradeCategory, [customization]);
        _homeCustomizationChoiceRepository.GetByHousingUnitIdAndCustomizationIdAsync(unit.Id, customization.Id, Arg.Any<CancellationToken>())
            .Returns((HomeCustomizationChoice?)null);

        var result = await _useCase.ExecuteAsync(unit.Id);

        var dto = result.Value!.Single().Customizations.Single();
        dto.CanSelect.Should().BeFalse();
        dto.EffectiveOptionId.Should().Be(defaultOptionId);
        dto.Options.Single(o => o.Id == defaultOptionId).IsEffectiveSelection.Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteAsync_WhenChoiceExists_ShouldUseSelectedOptionAsEffective()
    {
        var promotion = HousingPromotion.Create(Guid.NewGuid(), "Residencial Acacias", "Madrid", "Calle Mayor 1");
        var unit = HousingUnit.Create(promotion.Id, "1", "A", 90m, 80m);
        var tradeCategory = TradeCategory.Create(promotion.Id, "Carpintería", DateTime.UtcNow.AddMonths(1));
        var customization = Customization.CreateForWholePromotion(tradeCategory.Id, "Suelo", "Parquet Roble", 0m);
        customization.AddOption("Porcelánico Premium", 500m);
        var selectedOption = customization.Options.Single(o => !o.IsDefault);
        var choice = HomeCustomizationChoice.Create(unit.Id, customization.Id);
        choice.SelectOption(selectedOption.Id, DateTime.UtcNow);
        SetupOwnedUnit(unit, tradeCategory, [customization]);
        _homeCustomizationChoiceRepository.GetByHousingUnitIdAndCustomizationIdAsync(unit.Id, customization.Id, Arg.Any<CancellationToken>())
            .Returns(choice);

        var result = await _useCase.ExecuteAsync(unit.Id);

        var dto = result.Value!.Single().Customizations.Single();
        dto.CanSelect.Should().BeTrue();
        dto.SelectedOptionId.Should().Be(selectedOption.Id);
        dto.EffectiveOptionId.Should().Be(selectedOption.Id);
        dto.Options.Single(o => o.Id == selectedOption.Id).IsEffectiveSelection.Should().BeTrue();
    }

    private void SetupOwnedUnit(HousingUnit unit, TradeCategory tradeCategory, IReadOnlyList<Customization> customizations)
    {
        _housingUnitBuyerRepository.ExistsByHousingUnitIdAndBuyerUserIdAsync(unit.Id, BuyerUserId, Arg.Any<CancellationToken>()).Returns(true);
        _housingUnitRepository.GetByIdAsync(unit.Id, Arg.Any<CancellationToken>()).Returns(unit);
        _tradeCategoryRepository.GetByHousingPromotionIdAsync(unit.HousingPromotionId, Arg.Any<CancellationToken>()).Returns([tradeCategory]);
        _customizationRepository.GetByTradeCategoryIdAsync(tradeCategory.Id, Arg.Any<CancellationToken>()).Returns(customizations);
        _homeCustomizationChoiceRepository.GetByHousingUnitIdAndCustomizationIdAsync(unit.Id, Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((HomeCustomizationChoice?)null);
    }
}

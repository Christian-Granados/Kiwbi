using FluentAssertions;
using Kiwbi.Application.Choices.GetHousingPromotionChoicesProgress;
using Kiwbi.Application.Common;
using Kiwbi.Domain.Choices;
using Kiwbi.Domain.Customizations;
using Kiwbi.Domain.RealEstate;
using NSubstitute;

namespace Kiwbi.Application.Tests.Choices.GetHousingPromotionChoicesProgress;

public class GetHousingPromotionChoicesProgressUseCaseTests
{
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();
    private readonly IHousingPromotionRepository _housingPromotionRepository = Substitute.For<IHousingPromotionRepository>();
    private readonly IHousingTypologyRepository _housingTypologyRepository = Substitute.For<IHousingTypologyRepository>();
    private readonly IHousingUnitRepository _housingUnitRepository = Substitute.For<IHousingUnitRepository>();
    private readonly ITradeCategoryRepository _tradeCategoryRepository = Substitute.For<ITradeCategoryRepository>();
    private readonly ICustomizationRepository _customizationRepository = Substitute.For<ICustomizationRepository>();
    private readonly IHomeCustomizationChoiceRepository _homeCustomizationChoiceRepository = Substitute.For<IHomeCustomizationChoiceRepository>();
    private readonly GetHousingPromotionChoicesProgressUseCase _useCase;

    public GetHousingPromotionChoicesProgressUseCaseTests()
    {
        _useCase = new GetHousingPromotionChoicesProgressUseCase(
            _currentUser,
            _housingPromotionRepository,
            _housingTypologyRepository,
            _housingUnitRepository,
            _tradeCategoryRepository,
            _customizationRepository,
            _homeCustomizationChoiceRepository);
    }

    [Fact]
    public async Task ExecuteAsync_WhenPromotionBelongsToAnotherTenant_ShouldReturnFailure()
    {
        var promotion = HousingPromotion.Create(Guid.NewGuid(), "Residencial Acacias", "Madrid", "Calle Mayor 1");
        _currentUser.DeveloperCompanyId.Returns(Guid.NewGuid());
        _housingPromotionRepository.GetByIdAsync(promotion.Id, Arg.Any<CancellationToken>()).Returns(promotion);

        var result = await _useCase.ExecuteAsync(promotion.Id);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteAsync_WithNoUnitsOrCustomizations_ShouldReturnZeroedCounts()
    {
        var developerCompanyId = Guid.NewGuid();
        var promotion = HousingPromotion.Create(developerCompanyId, "Residencial Acacias", "Madrid", "Calle Mayor 1");
        SetupPromotion(promotion, developerCompanyId, [], [], []);

        var result = await _useCase.ExecuteAsync(promotion.Id);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Units.Should().BeEmpty();
    }

    [Fact]
    public async Task ExecuteAsync_WithMixedChoiceStatuses_ShouldCountEachStatusCorrectly()
    {
        var developerCompanyId = Guid.NewGuid();
        var promotion = HousingPromotion.Create(developerCompanyId, "Residencial Acacias", "Madrid", "Calle Mayor 1");
        var unit = HousingUnit.Create(promotion.Id, "1", "A", 90m, 80m);
        var tradeCategory = TradeCategory.Create(promotion.Id, "Carpintería", DateTime.UtcNow.AddMonths(1));

        var pendingCustomization = Customization.CreateForWholePromotion(tradeCategory.Id, "Suelo", "Parquet", 0m);
        var selectedCustomization = Customization.CreateForWholePromotion(tradeCategory.Id, "Puertas", "Blanca", 0m);
        var selectedChoice = HomeCustomizationChoice.Create(unit.Id, selectedCustomization.Id);
        selectedChoice.SelectOption(selectedCustomization.Options.Single().Id, DateTime.UtcNow);

        SetupPromotion(promotion, developerCompanyId, [unit], [tradeCategory], [pendingCustomization, selectedCustomization]);
        _homeCustomizationChoiceRepository.GetByHousingUnitIdsAsync(Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>())
            .Returns([selectedChoice]);

        var result = await _useCase.ExecuteAsync(promotion.Id);

        var unitDto = result.Value!.Units.Single();
        unitDto.TotalCount.Should().Be(2);
        unitDto.PendingCount.Should().Be(1);
        unitDto.SelectedCount.Should().Be(1);
        unitDto.ConfirmedCount.Should().Be(0);
        unitDto.PaidCount.Should().Be(0);
    }

    [Fact]
    public async Task ExecuteAsync_WhenTradeCategoryExpiredWithoutChoiceRow_ShouldStillCountAsPending()
    {
        var developerCompanyId = Guid.NewGuid();
        var promotion = HousingPromotion.Create(developerCompanyId, "Residencial Acacias", "Madrid", "Calle Mayor 1");
        var unit = HousingUnit.Create(promotion.Id, "1", "A", 90m, 80m);
        var tradeCategory = TradeCategory.Create(promotion.Id, "Carpintería", DateTime.UtcNow.AddMonths(1));
        tradeCategory.Reschedule(DateTime.UtcNow.AddDays(-1));
        var customization = Customization.CreateForWholePromotion(tradeCategory.Id, "Suelo", "Parquet", 0m);

        SetupPromotion(promotion, developerCompanyId, [unit], [tradeCategory], [customization]);
        _homeCustomizationChoiceRepository.GetByHousingUnitIdsAsync(Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>())
            .Returns([]);

        var result = await _useCase.ExecuteAsync(promotion.Id);

        var unitDto = result.Value!.Units.Single();
        unitDto.PendingCount.Should().Be(1);
        unitDto.SelectedCount.Should().Be(0);
        unitDto.ConfirmedCount.Should().Be(0);
        unitDto.PaidCount.Should().Be(0);
    }

    private void SetupPromotion(
        HousingPromotion promotion,
        Guid developerCompanyId,
        IReadOnlyList<HousingUnit> units,
        IReadOnlyList<TradeCategory> tradeCategories,
        IReadOnlyList<Customization> customizations)
    {
        _currentUser.DeveloperCompanyId.Returns(developerCompanyId);
        _housingPromotionRepository.GetByIdAsync(promotion.Id, Arg.Any<CancellationToken>()).Returns(promotion);
        _housingTypologyRepository.GetByHousingPromotionIdAsync(promotion.Id, Arg.Any<CancellationToken>()).Returns(new List<HousingTypology>());
        _housingUnitRepository.GetByHousingPromotionIdAsync(promotion.Id, Arg.Any<CancellationToken>()).Returns(units);
        _tradeCategoryRepository.GetByHousingPromotionIdAsync(promotion.Id, Arg.Any<CancellationToken>()).Returns(tradeCategories);

        foreach (var tradeCategory in tradeCategories)
        {
            _customizationRepository.GetByTradeCategoryIdAsync(tradeCategory.Id, Arg.Any<CancellationToken>()).Returns(customizations);
        }

        _homeCustomizationChoiceRepository.GetByHousingUnitIdsAsync(Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>())
            .Returns([]);
    }
}

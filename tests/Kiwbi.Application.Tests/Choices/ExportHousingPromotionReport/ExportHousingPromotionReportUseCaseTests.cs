using FluentAssertions;
using Kiwbi.Application.Choices;
using Kiwbi.Application.Choices.ExportHousingPromotionReport;
using Kiwbi.Application.Common;
using Kiwbi.Domain.Choices;
using Kiwbi.Domain.Customizations;
using Kiwbi.Domain.RealEstate;
using NSubstitute;

namespace Kiwbi.Application.Tests.Choices.ExportHousingPromotionReport;

public class ExportHousingPromotionReportUseCaseTests
{
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();
    private readonly IHousingPromotionRepository _housingPromotionRepository = Substitute.For<IHousingPromotionRepository>();
    private readonly IHousingUnitRepository _housingUnitRepository = Substitute.For<IHousingUnitRepository>();
    private readonly ITradeCategoryRepository _tradeCategoryRepository = Substitute.For<ITradeCategoryRepository>();
    private readonly ICustomizationRepository _customizationRepository = Substitute.For<ICustomizationRepository>();
    private readonly IHomeCustomizationChoiceRepository _homeCustomizationChoiceRepository = Substitute.For<IHomeCustomizationChoiceRepository>();
    private readonly IHousingPromotionReportGenerator _reportGenerator = Substitute.For<IHousingPromotionReportGenerator>();
    private readonly ExportHousingPromotionReportUseCase _useCase;

    public ExportHousingPromotionReportUseCaseTests()
    {
        _useCase = new ExportHousingPromotionReportUseCase(
            _currentUser,
            _housingPromotionRepository,
            _housingUnitRepository,
            _tradeCategoryRepository,
            _customizationRepository,
            _homeCustomizationChoiceRepository,
            _reportGenerator);
    }

    [Fact]
    public async Task ExecuteAsync_WhenPromotionBelongsToAnotherTenant_ShouldReturnFailure()
    {
        var promotion = HousingPromotion.Create(Guid.NewGuid(), "Residencial Acacias", "Madrid", "Calle Mayor 1");
        _currentUser.DeveloperCompanyId.Returns(Guid.NewGuid());
        _housingPromotionRepository.GetByIdAsync(promotion.Id, Arg.Any<CancellationToken>()).Returns(promotion);

        var result = await _useCase.ExecuteAsync(new(promotion.Id, HousingPromotionReportFormat.Excel));

        result.IsFailure.Should().BeTrue();
        _reportGenerator.DidNotReceive().GenerateExcel(Arg.Any<HousingPromotionReportDto>());
    }

    [Fact]
    public async Task ExecuteAsync_WithCustomizationNotApplicableToUnit_ShouldExcludeItFromThatUnit()
    {
        var developerCompanyId = Guid.NewGuid();
        var promotion = HousingPromotion.Create(developerCompanyId, "Residencial Acacias", "Madrid", "Calle Mayor 1");
        var unit = HousingUnit.Create(promotion.Id, "1", "A", 90m, 80m);
        var otherUnitId = Guid.NewGuid();
        var tradeCategory = TradeCategory.Create(promotion.Id, "Carpintería", DateTime.UtcNow.AddMonths(1));
        var customization = Customization.CreateForUnits(tradeCategory.Id, "Suelo", "Parquet", 0m, [otherUnitId]);
        SetupPromotion(promotion, developerCompanyId, [unit], [tradeCategory], [customization]);
        _reportGenerator.GenerateExcel(Arg.Any<HousingPromotionReportDto>()).Returns([1, 2, 3]);

        var result = await _useCase.ExecuteAsync(new(promotion.Id, HousingPromotionReportFormat.Excel));

        result.IsSuccess.Should().BeTrue();
        _reportGenerator.Received(1).GenerateExcel(Arg.Is<HousingPromotionReportDto>(r => r.TradeCategories.Count == 0));
    }

    [Fact]
    public async Task ExecuteAsync_WithApplicableCustomization_ShouldGroupByTradeCategoryAndUnit()
    {
        var developerCompanyId = Guid.NewGuid();
        var promotion = HousingPromotion.Create(developerCompanyId, "Residencial Acacias", "Madrid", "Calle Mayor 1");
        var unit = HousingUnit.Create(promotion.Id, "1", "A", 90m, 80m);
        var tradeCategory = TradeCategory.Create(promotion.Id, "Carpintería", DateTime.UtcNow.AddMonths(1));
        var customization = Customization.CreateForWholePromotion(tradeCategory.Id, "Suelo", "Parquet", 0m);
        SetupPromotion(promotion, developerCompanyId, [unit], [tradeCategory], [customization]);
        _reportGenerator.GenerateExcel(Arg.Any<HousingPromotionReportDto>()).Returns([1, 2, 3]);

        var result = await _useCase.ExecuteAsync(new(promotion.Id, HousingPromotionReportFormat.Excel));

        result.IsSuccess.Should().BeTrue();
        result.Value!.Content.Should().Equal(1, 2, 3);
        result.Value.ContentType.Should().Be("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        _reportGenerator.Received(1).GenerateExcel(Arg.Is<HousingPromotionReportDto>(r =>
            r.PromotionName == "Residencial Acacias" &&
            r.TradeCategories.Count == 1 &&
            r.TradeCategories[0].TradeCategoryName == "Carpintería" &&
            r.TradeCategories[0].HousingUnits.Count == 1 &&
            r.TradeCategories[0].HousingUnits[0].Floor == "1" &&
            r.TradeCategories[0].HousingUnits[0].Door == "A" &&
            r.TradeCategories[0].HousingUnits[0].Customizations.Single().CustomizationName == "Suelo"));
    }

    [Fact]
    public async Task ExecuteAsync_WithPdfFormat_ShouldDelegateToGeneratePdf()
    {
        var developerCompanyId = Guid.NewGuid();
        var promotion = HousingPromotion.Create(developerCompanyId, "Residencial Acacias", "Madrid", "Calle Mayor 1");
        var unit = HousingUnit.Create(promotion.Id, "1", "A", 90m, 80m);
        var tradeCategory = TradeCategory.Create(promotion.Id, "Carpintería", DateTime.UtcNow.AddMonths(1));
        var customization = Customization.CreateForWholePromotion(tradeCategory.Id, "Suelo", "Parquet", 0m);
        SetupPromotion(promotion, developerCompanyId, [unit], [tradeCategory], [customization]);
        _reportGenerator.GeneratePdf(Arg.Any<HousingPromotionReportDto>()).Returns([9, 9]);

        var result = await _useCase.ExecuteAsync(new(promotion.Id, HousingPromotionReportFormat.Pdf));

        result.IsSuccess.Should().BeTrue();
        result.Value!.ContentType.Should().Be("application/pdf");
        result.Value.FileName.Should().EndWith(".pdf");
        _reportGenerator.Received(1).GeneratePdf(Arg.Any<HousingPromotionReportDto>());
        _reportGenerator.DidNotReceive().GenerateExcel(Arg.Any<HousingPromotionReportDto>());
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

using FluentAssertions;
using Kiwbi.Application.Choices.ConfirmHomeCustomizationChoice;
using Kiwbi.Application.Common;
using Kiwbi.Domain.Choices;
using Kiwbi.Domain.Customizations;
using Kiwbi.Domain.RealEstate;
using NSubstitute;

namespace Kiwbi.Application.Tests.Choices.ConfirmHomeCustomizationChoice;

public class ConfirmHomeCustomizationChoiceUseCaseTests
{
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();
    private readonly IHousingPromotionRepository _housingPromotionRepository = Substitute.For<IHousingPromotionRepository>();
    private readonly IHousingUnitRepository _housingUnitRepository = Substitute.For<IHousingUnitRepository>();
    private readonly ICustomizationRepository _customizationRepository = Substitute.For<ICustomizationRepository>();
    private readonly ITradeCategoryRepository _tradeCategoryRepository = Substitute.For<ITradeCategoryRepository>();
    private readonly IHomeCustomizationChoiceRepository _homeCustomizationChoiceRepository = Substitute.For<IHomeCustomizationChoiceRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ConfirmHomeCustomizationChoiceUseCase _useCase;

    public ConfirmHomeCustomizationChoiceUseCaseTests()
    {
        _useCase = new ConfirmHomeCustomizationChoiceUseCase(
            _currentUser,
            _housingPromotionRepository,
            _housingUnitRepository,
            _customizationRepository,
            _tradeCategoryRepository,
            _homeCustomizationChoiceRepository,
            _unitOfWork);
    }

    [Fact]
    public async Task ExecuteAsync_WhenUnitBelongsToAnotherTenant_ShouldReturnFailure()
    {
        var promotion = HousingPromotion.Create(Guid.NewGuid(), "Residencial Acacias", "Madrid", "Calle Mayor 1");
        var unit = HousingUnit.Create(promotion.Id, "1", "A", 90m, 80m);
        _currentUser.DeveloperCompanyId.Returns(Guid.NewGuid());
        _housingUnitRepository.GetByIdAsync(unit.Id, Arg.Any<CancellationToken>()).Returns(unit);
        _housingPromotionRepository.GetByIdAsync(promotion.Id, Arg.Any<CancellationToken>()).Returns(promotion);

        var result = await _useCase.ExecuteAsync(new(unit.Id, Guid.NewGuid()));

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteAsync_WhenTradeCategoryNotExpired_ShouldReturnFailure()
    {
        var developerCompanyId = Guid.NewGuid();
        var promotion = HousingPromotion.Create(developerCompanyId, "Residencial Acacias", "Madrid", "Calle Mayor 1");
        var unit = HousingUnit.Create(promotion.Id, "1", "A", 90m, 80m);
        var tradeCategory = TradeCategory.Create(promotion.Id, "Carpintería", DateTime.UtcNow.AddMonths(1));
        var customization = Customization.CreateForWholePromotion(tradeCategory.Id, "Suelo", "Parquet", 0m);
        SetupOwnedUnit(promotion, developerCompanyId, unit, customization, tradeCategory);

        var result = await _useCase.ExecuteAsync(new(unit.Id, customization.Id));

        result.IsFailure.Should().BeTrue();
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenExpiredWithoutExistingChoice_ShouldMaterializeDefaultOptionAndConfirm()
    {
        var developerCompanyId = Guid.NewGuid();
        var promotion = HousingPromotion.Create(developerCompanyId, "Residencial Acacias", "Madrid", "Calle Mayor 1");
        var unit = HousingUnit.Create(promotion.Id, "1", "A", 90m, 80m);
        var tradeCategory = TradeCategory.Create(promotion.Id, "Carpintería", DateTime.UtcNow.AddMonths(1));
        tradeCategory.Reschedule(DateTime.UtcNow.AddDays(-1));
        var customization = Customization.CreateForWholePromotion(tradeCategory.Id, "Suelo", "Parquet", 0m);
        var defaultOptionId = customization.Options.Single().Id;
        SetupOwnedUnit(promotion, developerCompanyId, unit, customization, tradeCategory);
        _homeCustomizationChoiceRepository.GetByHousingUnitIdAndCustomizationIdAsync(unit.Id, customization.Id, Arg.Any<CancellationToken>())
            .Returns((HomeCustomizationChoice?)null);

        var result = await _useCase.ExecuteAsync(new(unit.Id, customization.Id));

        result.IsSuccess.Should().BeTrue();
        await _homeCustomizationChoiceRepository.Received(1).AddAsync(
            Arg.Is<HomeCustomizationChoice>(c => c.SelectedOptionId == defaultOptionId && c.Status == HomeCustomizationChoiceStatus.Confirmed),
            Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenChoiceAlreadySelectedByBuyer_ShouldConfirmKeepingSelectedOption()
    {
        var developerCompanyId = Guid.NewGuid();
        var promotion = HousingPromotion.Create(developerCompanyId, "Residencial Acacias", "Madrid", "Calle Mayor 1");
        var unit = HousingUnit.Create(promotion.Id, "1", "A", 90m, 80m);
        var tradeCategory = TradeCategory.Create(promotion.Id, "Carpintería", DateTime.UtcNow.AddMonths(1));
        tradeCategory.Reschedule(DateTime.UtcNow.AddDays(-1));
        var customization = Customization.CreateForWholePromotion(tradeCategory.Id, "Suelo", "Parquet", 0m);
        customization.AddOption("Porcelánico Premium", 500m);
        var selectedOption = customization.Options.Single(o => !o.IsDefault);
        var choice = HomeCustomizationChoice.Create(unit.Id, customization.Id);
        choice.SelectOption(selectedOption.Id, DateTime.UtcNow);
        SetupOwnedUnit(promotion, developerCompanyId, unit, customization, tradeCategory);
        _homeCustomizationChoiceRepository.GetByHousingUnitIdAndCustomizationIdAsync(unit.Id, customization.Id, Arg.Any<CancellationToken>())
            .Returns(choice);

        var result = await _useCase.ExecuteAsync(new(unit.Id, customization.Id));

        result.IsSuccess.Should().BeTrue();
        choice.SelectedOptionId.Should().Be(selectedOption.Id);
        choice.Status.Should().Be(HomeCustomizationChoiceStatus.Confirmed);
        _homeCustomizationChoiceRepository.Received(1).Update(choice);
    }

    [Fact]
    public async Task ExecuteAsync_WhenAlreadyConfirmed_ShouldReturnFailure()
    {
        var developerCompanyId = Guid.NewGuid();
        var promotion = HousingPromotion.Create(developerCompanyId, "Residencial Acacias", "Madrid", "Calle Mayor 1");
        var unit = HousingUnit.Create(promotion.Id, "1", "A", 90m, 80m);
        var tradeCategory = TradeCategory.Create(promotion.Id, "Carpintería", DateTime.UtcNow.AddMonths(1));
        tradeCategory.Reschedule(DateTime.UtcNow.AddDays(-1));
        var customization = Customization.CreateForWholePromotion(tradeCategory.Id, "Suelo", "Parquet", 0m);
        var choice = HomeCustomizationChoice.Create(unit.Id, customization.Id);
        choice.SelectOption(customization.Options.Single().Id, DateTime.UtcNow);
        choice.Confirm(DateTime.UtcNow);
        SetupOwnedUnit(promotion, developerCompanyId, unit, customization, tradeCategory);
        _homeCustomizationChoiceRepository.GetByHousingUnitIdAndCustomizationIdAsync(unit.Id, customization.Id, Arg.Any<CancellationToken>())
            .Returns(choice);

        var result = await _useCase.ExecuteAsync(new(unit.Id, customization.Id));

        result.IsFailure.Should().BeTrue();
    }

    private void SetupOwnedUnit(
        HousingPromotion promotion,
        Guid developerCompanyId,
        HousingUnit unit,
        Customization customization,
        TradeCategory tradeCategory)
    {
        _currentUser.DeveloperCompanyId.Returns(developerCompanyId);
        _housingUnitRepository.GetByIdAsync(unit.Id, Arg.Any<CancellationToken>()).Returns(unit);
        _housingPromotionRepository.GetByIdAsync(promotion.Id, Arg.Any<CancellationToken>()).Returns(promotion);
        _customizationRepository.GetByIdAsync(customization.Id, Arg.Any<CancellationToken>()).Returns(customization);
        _tradeCategoryRepository.GetByIdAsync(tradeCategory.Id, Arg.Any<CancellationToken>()).Returns(tradeCategory);
    }
}

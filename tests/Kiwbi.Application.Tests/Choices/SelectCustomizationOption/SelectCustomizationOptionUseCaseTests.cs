using FluentAssertions;
using Kiwbi.Application.Choices.SelectCustomizationOption;
using Kiwbi.Application.Common;
using Kiwbi.Domain.Choices;
using Kiwbi.Domain.Customizations;
using Kiwbi.Domain.Onboarding;
using Kiwbi.Domain.RealEstate;
using NSubstitute;

namespace Kiwbi.Application.Tests.Choices.SelectCustomizationOption;

public class SelectCustomizationOptionUseCaseTests
{
    private const string BuyerUserId = "buyer-1";

    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();
    private readonly IHousingUnitBuyerRepository _housingUnitBuyerRepository = Substitute.For<IHousingUnitBuyerRepository>();
    private readonly IHousingUnitRepository _housingUnitRepository = Substitute.For<IHousingUnitRepository>();
    private readonly ICustomizationRepository _customizationRepository = Substitute.For<ICustomizationRepository>();
    private readonly ITradeCategoryRepository _tradeCategoryRepository = Substitute.For<ITradeCategoryRepository>();
    private readonly IHomeCustomizationChoiceRepository _homeCustomizationChoiceRepository = Substitute.For<IHomeCustomizationChoiceRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly SelectCustomizationOptionUseCase _useCase;

    public SelectCustomizationOptionUseCaseTests()
    {
        _currentUser.UserId.Returns(BuyerUserId);
        _useCase = new SelectCustomizationOptionUseCase(
            _currentUser, _housingUnitBuyerRepository, _housingUnitRepository, _customizationRepository, _tradeCategoryRepository, _homeCustomizationChoiceRepository, _unitOfWork);
    }

    [Fact]
    public async Task ExecuteAsync_WhenUserNotAuthenticated_ShouldReturnFailure()
    {
        _currentUser.UserId.Returns((string?)null);

        var result = await _useCase.ExecuteAsync(new SelectCustomizationOptionCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()));

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteAsync_WhenBuyerNotLinkedToUnit_ShouldReturnFailure()
    {
        var unitId = Guid.NewGuid();
        _housingUnitBuyerRepository.ExistsByHousingUnitIdAndBuyerUserIdAsync(unitId, BuyerUserId, Arg.Any<CancellationToken>()).Returns(false);

        var result = await _useCase.ExecuteAsync(new SelectCustomizationOptionCommand(unitId, Guid.NewGuid(), Guid.NewGuid()));

        result.IsFailure.Should().BeTrue();
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenCustomizationDoesNotApplyToUnit_ShouldReturnFailure()
    {
        var promotion = HousingPromotion.Create(Guid.NewGuid(), "Residencial Acacias", "Madrid", "Calle Mayor 1");
        var unit = HousingUnit.Create(promotion.Id, "1", "A", 90m, 80m);
        var tradeCategory = TradeCategory.Create(promotion.Id, "Carpintería", DateTime.UtcNow.AddMonths(1));
        var customization = Customization.CreateForUnits(tradeCategory.Id, "Suelo", "Parquet Roble", 0m, [Guid.NewGuid()]);
        SetupUnitAndCustomization(unit, customization);

        var result = await _useCase.ExecuteAsync(new SelectCustomizationOptionCommand(unit.Id, customization.Id, customization.Options.Single().Id));

        result.IsFailure.Should().BeTrue();
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenOptionDoesNotBelongToCustomization_ShouldReturnFailure()
    {
        var promotion = HousingPromotion.Create(Guid.NewGuid(), "Residencial Acacias", "Madrid", "Calle Mayor 1");
        var unit = HousingUnit.Create(promotion.Id, "1", "A", 90m, 80m);
        var tradeCategory = TradeCategory.Create(promotion.Id, "Carpintería", DateTime.UtcNow.AddMonths(1));
        var customization = Customization.CreateForWholePromotion(tradeCategory.Id, "Suelo", "Parquet Roble", 0m);
        SetupUnitAndCustomization(unit, customization);
        _tradeCategoryRepository.GetByIdAsync(tradeCategory.Id, Arg.Any<CancellationToken>()).Returns(tradeCategory);

        var result = await _useCase.ExecuteAsync(new SelectCustomizationOptionCommand(unit.Id, customization.Id, Guid.NewGuid()));

        result.IsFailure.Should().BeTrue();
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenTradeCategoryExpired_ShouldReturnFailure()
    {
        var promotion = HousingPromotion.Create(Guid.NewGuid(), "Residencial Acacias", "Madrid", "Calle Mayor 1");
        var unit = HousingUnit.Create(promotion.Id, "1", "A", 90m, 80m);
        var tradeCategory = TradeCategory.Create(promotion.Id, "Carpintería", DateTime.UtcNow.AddMonths(1));
        tradeCategory.Reschedule(DateTime.UtcNow.AddDays(-1));
        var customization = Customization.CreateForWholePromotion(tradeCategory.Id, "Suelo", "Parquet Roble", 0m);
        SetupUnitAndCustomization(unit, customization);
        _tradeCategoryRepository.GetByIdAsync(tradeCategory.Id, Arg.Any<CancellationToken>()).Returns(tradeCategory);

        var result = await _useCase.ExecuteAsync(new SelectCustomizationOptionCommand(unit.Id, customization.Id, customization.Options.Single().Id));

        result.IsFailure.Should().BeTrue();
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WithValidSelectionAndNoExistingChoice_ShouldCreateIt()
    {
        var promotion = HousingPromotion.Create(Guid.NewGuid(), "Residencial Acacias", "Madrid", "Calle Mayor 1");
        var unit = HousingUnit.Create(promotion.Id, "1", "A", 90m, 80m);
        var tradeCategory = TradeCategory.Create(promotion.Id, "Carpintería", DateTime.UtcNow.AddMonths(1));
        var customization = Customization.CreateForWholePromotion(tradeCategory.Id, "Suelo", "Parquet Roble", 0m);
        var optionId = customization.Options.Single().Id;
        SetupUnitAndCustomization(unit, customization);
        _tradeCategoryRepository.GetByIdAsync(tradeCategory.Id, Arg.Any<CancellationToken>()).Returns(tradeCategory);
        _homeCustomizationChoiceRepository.GetByHousingUnitIdAndCustomizationIdAsync(unit.Id, customization.Id, Arg.Any<CancellationToken>())
            .Returns((HomeCustomizationChoice?)null);

        var result = await _useCase.ExecuteAsync(new SelectCustomizationOptionCommand(unit.Id, customization.Id, optionId));

        result.IsSuccess.Should().BeTrue();
        result.Value!.SelectedOptionId.Should().Be(optionId);
        result.Value!.EffectiveOptionId.Should().Be(optionId);
        await _homeCustomizationChoiceRepository.Received(1).AddAsync(
            Arg.Is<HomeCustomizationChoice>(c => c.HousingUnitId == unit.Id && c.CustomizationId == customization.Id && c.SelectedOptionId == optionId),
            Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WithValidSelectionAndExistingChoice_ShouldUpdateIt()
    {
        var promotion = HousingPromotion.Create(Guid.NewGuid(), "Residencial Acacias", "Madrid", "Calle Mayor 1");
        var unit = HousingUnit.Create(promotion.Id, "1", "A", 90m, 80m);
        var tradeCategory = TradeCategory.Create(promotion.Id, "Carpintería", DateTime.UtcNow.AddMonths(1));
        var customization = Customization.CreateForWholePromotion(tradeCategory.Id, "Suelo", "Parquet Roble", 0m);
        customization.AddOption("Porcelánico Premium", 500m);
        var defaultOptionId = customization.Options.Single(o => o.IsDefault).Id;
        var newOptionId = customization.Options.Single(o => !o.IsDefault).Id;
        var existingChoice = HomeCustomizationChoice.Create(unit.Id, customization.Id);
        existingChoice.SelectOption(defaultOptionId, DateTime.UtcNow);
        SetupUnitAndCustomization(unit, customization);
        _tradeCategoryRepository.GetByIdAsync(tradeCategory.Id, Arg.Any<CancellationToken>()).Returns(tradeCategory);
        _homeCustomizationChoiceRepository.GetByHousingUnitIdAndCustomizationIdAsync(unit.Id, customization.Id, Arg.Any<CancellationToken>())
            .Returns(existingChoice);

        var result = await _useCase.ExecuteAsync(new SelectCustomizationOptionCommand(unit.Id, customization.Id, newOptionId));

        result.IsSuccess.Should().BeTrue();
        result.Value!.SelectedOptionId.Should().Be(newOptionId);
        await _homeCustomizationChoiceRepository.DidNotReceive().AddAsync(Arg.Any<HomeCustomizationChoice>(), Arg.Any<CancellationToken>());
        _homeCustomizationChoiceRepository.Received(1).Update(existingChoice);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    private void SetupUnitAndCustomization(HousingUnit unit, Customization customization)
    {
        _housingUnitBuyerRepository.ExistsByHousingUnitIdAndBuyerUserIdAsync(unit.Id, BuyerUserId, Arg.Any<CancellationToken>()).Returns(true);
        _housingUnitRepository.GetByIdAsync(unit.Id, Arg.Any<CancellationToken>()).Returns(unit);
        _customizationRepository.GetByIdAsync(customization.Id, Arg.Any<CancellationToken>()).Returns(customization);
    }
}

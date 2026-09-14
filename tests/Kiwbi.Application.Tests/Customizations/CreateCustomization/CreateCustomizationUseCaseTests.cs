using FluentAssertions;
using Kiwbi.Application.Common;
using Kiwbi.Application.Customizations.CreateCustomization;
using Kiwbi.Domain.Customizations;
using Kiwbi.Domain.RealEstate;
using NSubstitute;

namespace Kiwbi.Application.Tests.Customizations.CreateCustomization;

public class CreateCustomizationUseCaseTests
{
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();
    private readonly IHousingPromotionRepository _promotionRepository = Substitute.For<IHousingPromotionRepository>();
    private readonly IHousingTypologyRepository _typologyRepository = Substitute.For<IHousingTypologyRepository>();
    private readonly IHousingUnitRepository _unitRepository = Substitute.For<IHousingUnitRepository>();
    private readonly ITradeCategoryRepository _tradeCategoryRepository = Substitute.For<ITradeCategoryRepository>();
    private readonly ICustomizationRepository _customizationRepository = Substitute.For<ICustomizationRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly CreateCustomizationUseCase _useCase;

    public CreateCustomizationUseCaseTests()
    {
        _useCase = new CreateCustomizationUseCase(
            _currentUser, _promotionRepository, _typologyRepository, _unitRepository, _tradeCategoryRepository, _customizationRepository, _unitOfWork);
    }

    private (Guid DeveloperCompanyId, HousingPromotion Promotion, TradeCategory TradeCategory) SetUpOwnedTradeCategory()
    {
        var developerCompanyId = Guid.NewGuid();
        var promotion = HousingPromotion.Create(developerCompanyId, "Residencial Acacias", "Madrid", "Calle Mayor 1");
        var tradeCategory = TradeCategory.Create(promotion.Id, "Carpintería", DateTime.UtcNow.AddMonths(1));
        _currentUser.DeveloperCompanyId.Returns(developerCompanyId);
        _tradeCategoryRepository.GetByIdAsync(tradeCategory.Id, Arg.Any<CancellationToken>()).Returns(tradeCategory);
        _promotionRepository.GetByIdAsync(promotion.Id, Arg.Any<CancellationToken>()).Returns(promotion);

        return (developerCompanyId, promotion, tradeCategory);
    }

    [Fact]
    public async Task ExecuteAsync_ForWholePromotion_ShouldCreateAndPersistCustomization()
    {
        var (_, _, tradeCategory) = SetUpOwnedTradeCategory();

        var command = new CreateCustomizationCommand(
            tradeCategory.Id, "Suelo", "Parquet Roble", 0m, CustomizationScope.WholePromotion, Array.Empty<Guid>(), Array.Empty<Guid>());

        var result = await _useCase.ExecuteAsync(command);

        result.IsSuccess.Should().BeTrue();
        await _customizationRepository.Received(1).AddAsync(
            Arg.Is<Customization>(c => c.Name == "Suelo" && c.Assignments.Single().Scope == CustomizationScope.WholePromotion),
            Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_ForTypologiesOwnedByPromotion_ShouldCreateCustomization()
    {
        var (_, promotion, tradeCategory) = SetUpOwnedTradeCategory();
        var typology = HousingTypology.Create(promotion.Id, "Ático Tipo A");
        _typologyRepository.GetByHousingPromotionIdAsync(promotion.Id, Arg.Any<CancellationToken>()).Returns(new List<HousingTypology> { typology });

        var command = new CreateCustomizationCommand(
            tradeCategory.Id, "Suelo", "Parquet Roble", 0m, CustomizationScope.Typology, new[] { typology.Id }, Array.Empty<Guid>());

        var result = await _useCase.ExecuteAsync(command);

        result.IsSuccess.Should().BeTrue();
        await _customizationRepository.Received(1).AddAsync(Arg.Any<Customization>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_ForTypologyNotOwnedByPromotion_ShouldReturnFailure()
    {
        var (_, promotion, tradeCategory) = SetUpOwnedTradeCategory();
        _typologyRepository.GetByHousingPromotionIdAsync(promotion.Id, Arg.Any<CancellationToken>()).Returns(new List<HousingTypology>());

        var command = new CreateCustomizationCommand(
            tradeCategory.Id, "Suelo", "Parquet Roble", 0m, CustomizationScope.Typology, new[] { Guid.NewGuid() }, Array.Empty<Guid>());

        var result = await _useCase.ExecuteAsync(command);

        result.IsFailure.Should().BeTrue();
        await _customizationRepository.DidNotReceive().AddAsync(Arg.Any<Customization>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_ForUnitsOwnedByPromotion_ShouldCreateCustomization()
    {
        var (_, promotion, tradeCategory) = SetUpOwnedTradeCategory();
        var unit = HousingUnit.Create(promotion.Id, "1", "A", 90m, 80m);
        _unitRepository.GetByHousingPromotionIdAsync(promotion.Id, Arg.Any<CancellationToken>()).Returns(new List<HousingUnit> { unit });

        var command = new CreateCustomizationCommand(
            tradeCategory.Id, "Suelo", "Parquet Roble", 0m, CustomizationScope.Unit, Array.Empty<Guid>(), new[] { unit.Id });

        var result = await _useCase.ExecuteAsync(command);

        result.IsSuccess.Should().BeTrue();
        await _customizationRepository.Received(1).AddAsync(Arg.Any<Customization>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_ForUnitNotOwnedByPromotion_ShouldReturnFailure()
    {
        var (_, promotion, tradeCategory) = SetUpOwnedTradeCategory();
        _unitRepository.GetByHousingPromotionIdAsync(promotion.Id, Arg.Any<CancellationToken>()).Returns(new List<HousingUnit>());

        var command = new CreateCustomizationCommand(
            tradeCategory.Id, "Suelo", "Parquet Roble", 0m, CustomizationScope.Unit, Array.Empty<Guid>(), new[] { Guid.NewGuid() });

        var result = await _useCase.ExecuteAsync(command);

        result.IsFailure.Should().BeTrue();
        await _customizationRepository.DidNotReceive().AddAsync(Arg.Any<Customization>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenTradeCategoryBelongsToAnotherTenant_ShouldReturnFailure()
    {
        var promotion = HousingPromotion.Create(Guid.NewGuid(), "Residencial Acacias", "Madrid", "Calle Mayor 1");
        var tradeCategory = TradeCategory.Create(promotion.Id, "Carpintería", DateTime.UtcNow.AddMonths(1));
        _currentUser.DeveloperCompanyId.Returns(Guid.NewGuid());
        _tradeCategoryRepository.GetByIdAsync(tradeCategory.Id, Arg.Any<CancellationToken>()).Returns(tradeCategory);
        _promotionRepository.GetByIdAsync(promotion.Id, Arg.Any<CancellationToken>()).Returns(promotion);

        var command = new CreateCustomizationCommand(
            tradeCategory.Id, "Suelo", "Parquet Roble", 0m, CustomizationScope.WholePromotion, Array.Empty<Guid>(), Array.Empty<Guid>());

        var result = await _useCase.ExecuteAsync(command);

        result.IsFailure.Should().BeTrue();
        await _customizationRepository.DidNotReceive().AddAsync(Arg.Any<Customization>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WithNegativeSurcharge_ShouldReturnFailureWithoutPersisting()
    {
        var (_, _, tradeCategory) = SetUpOwnedTradeCategory();

        var command = new CreateCustomizationCommand(
            tradeCategory.Id, "Suelo", "Parquet Roble", -1m, CustomizationScope.WholePromotion, Array.Empty<Guid>(), Array.Empty<Guid>());

        var result = await _useCase.ExecuteAsync(command);

        result.IsFailure.Should().BeTrue();
        await _customizationRepository.DidNotReceive().AddAsync(Arg.Any<Customization>(), Arg.Any<CancellationToken>());
    }
}

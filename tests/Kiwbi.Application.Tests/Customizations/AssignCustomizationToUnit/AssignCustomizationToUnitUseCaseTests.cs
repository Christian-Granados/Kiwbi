using FluentAssertions;
using Kiwbi.Application.Common;
using Kiwbi.Application.Customizations.AssignCustomizationToUnit;
using Kiwbi.Domain.Customizations;
using Kiwbi.Domain.RealEstate;
using NSubstitute;

namespace Kiwbi.Application.Tests.Customizations.AssignCustomizationToUnit;

public class AssignCustomizationToUnitUseCaseTests
{
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();
    private readonly IHousingPromotionRepository _promotionRepository = Substitute.For<IHousingPromotionRepository>();
    private readonly IHousingUnitRepository _unitRepository = Substitute.For<IHousingUnitRepository>();
    private readonly ITradeCategoryRepository _tradeCategoryRepository = Substitute.For<ITradeCategoryRepository>();
    private readonly ICustomizationRepository _customizationRepository = Substitute.For<ICustomizationRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly AssignCustomizationToUnitUseCase _useCase;

    public AssignCustomizationToUnitUseCaseTests()
    {
        _useCase = new AssignCustomizationToUnitUseCase(
            _currentUser, _promotionRepository, _unitRepository, _tradeCategoryRepository, _customizationRepository, _unitOfWork);
    }

    [Fact]
    public async Task ExecuteAsync_WithUnitOwnedByPromotion_ShouldAddAssignment()
    {
        var developerCompanyId = Guid.NewGuid();
        var promotion = HousingPromotion.Create(developerCompanyId, "Residencial Acacias", "Madrid", "Calle Mayor 1");
        var tradeCategory = TradeCategory.Create(promotion.Id, "Carpintería", DateTime.UtcNow.AddMonths(1));
        var customization = Customization.CreateForUnits(tradeCategory.Id, "Suelo", "Parquet Roble", 0m, new[] { Guid.NewGuid() });
        var unit = HousingUnit.Create(promotion.Id, "1", "A", 90m, 80m);
        _currentUser.DeveloperCompanyId.Returns(developerCompanyId);
        _customizationRepository.GetByIdAsync(customization.Id, Arg.Any<CancellationToken>()).Returns(customization);
        _tradeCategoryRepository.GetByIdAsync(tradeCategory.Id, Arg.Any<CancellationToken>()).Returns(tradeCategory);
        _promotionRepository.GetByIdAsync(promotion.Id, Arg.Any<CancellationToken>()).Returns(promotion);
        _unitRepository.GetByIdAsync(unit.Id, Arg.Any<CancellationToken>()).Returns(unit);

        var result = await _useCase.ExecuteAsync(new AssignCustomizationToUnitCommand(customization.Id, unit.Id));

        result.IsSuccess.Should().BeTrue();
        customization.Assignments.Should().HaveCount(2);
        _customizationRepository.Received(1).Update(customization);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WithUnitFromAnotherPromotion_ShouldReturnFailure()
    {
        var developerCompanyId = Guid.NewGuid();
        var promotion = HousingPromotion.Create(developerCompanyId, "Residencial Acacias", "Madrid", "Calle Mayor 1");
        var otherPromotion = HousingPromotion.Create(developerCompanyId, "Residencial Naranjos", "Madrid", "Calle Sur 2");
        var tradeCategory = TradeCategory.Create(promotion.Id, "Carpintería", DateTime.UtcNow.AddMonths(1));
        var customization = Customization.CreateForUnits(tradeCategory.Id, "Suelo", "Parquet Roble", 0m, new[] { Guid.NewGuid() });
        var unit = HousingUnit.Create(otherPromotion.Id, "1", "A", 90m, 80m);
        _currentUser.DeveloperCompanyId.Returns(developerCompanyId);
        _customizationRepository.GetByIdAsync(customization.Id, Arg.Any<CancellationToken>()).Returns(customization);
        _tradeCategoryRepository.GetByIdAsync(tradeCategory.Id, Arg.Any<CancellationToken>()).Returns(tradeCategory);
        _promotionRepository.GetByIdAsync(promotion.Id, Arg.Any<CancellationToken>()).Returns(promotion);
        _unitRepository.GetByIdAsync(unit.Id, Arg.Any<CancellationToken>()).Returns(unit);

        var result = await _useCase.ExecuteAsync(new AssignCustomizationToUnitCommand(customization.Id, unit.Id));

        result.IsFailure.Should().BeTrue();
        _customizationRepository.DidNotReceive().Update(Arg.Any<Customization>());
    }
}

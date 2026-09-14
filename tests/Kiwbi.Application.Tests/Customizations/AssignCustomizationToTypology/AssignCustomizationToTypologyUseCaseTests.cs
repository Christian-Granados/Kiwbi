using FluentAssertions;
using Kiwbi.Application.Common;
using Kiwbi.Application.Customizations.AssignCustomizationToTypology;
using Kiwbi.Domain.Customizations;
using Kiwbi.Domain.RealEstate;
using NSubstitute;

namespace Kiwbi.Application.Tests.Customizations.AssignCustomizationToTypology;

public class AssignCustomizationToTypologyUseCaseTests
{
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();
    private readonly IHousingPromotionRepository _promotionRepository = Substitute.For<IHousingPromotionRepository>();
    private readonly IHousingTypologyRepository _typologyRepository = Substitute.For<IHousingTypologyRepository>();
    private readonly ITradeCategoryRepository _tradeCategoryRepository = Substitute.For<ITradeCategoryRepository>();
    private readonly ICustomizationRepository _customizationRepository = Substitute.For<ICustomizationRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly AssignCustomizationToTypologyUseCase _useCase;

    public AssignCustomizationToTypologyUseCaseTests()
    {
        _useCase = new AssignCustomizationToTypologyUseCase(
            _currentUser, _promotionRepository, _typologyRepository, _tradeCategoryRepository, _customizationRepository, _unitOfWork);
    }

    [Fact]
    public async Task ExecuteAsync_WithTypologyOwnedByPromotion_ShouldAddAssignment()
    {
        var developerCompanyId = Guid.NewGuid();
        var promotion = HousingPromotion.Create(developerCompanyId, "Residencial Acacias", "Madrid", "Calle Mayor 1");
        var tradeCategory = TradeCategory.Create(promotion.Id, "Carpintería", DateTime.UtcNow.AddMonths(1));
        var customization = Customization.CreateForTypologies(tradeCategory.Id, "Suelo", "Parquet Roble", 0m, new[] { Guid.NewGuid() });
        var typology = HousingTypology.Create(promotion.Id, "Ático Tipo A");
        _currentUser.DeveloperCompanyId.Returns(developerCompanyId);
        _customizationRepository.GetByIdAsync(customization.Id, Arg.Any<CancellationToken>()).Returns(customization);
        _tradeCategoryRepository.GetByIdAsync(tradeCategory.Id, Arg.Any<CancellationToken>()).Returns(tradeCategory);
        _promotionRepository.GetByIdAsync(promotion.Id, Arg.Any<CancellationToken>()).Returns(promotion);
        _typologyRepository.GetByIdAsync(typology.Id, Arg.Any<CancellationToken>()).Returns(typology);

        var result = await _useCase.ExecuteAsync(new AssignCustomizationToTypologyCommand(customization.Id, typology.Id));

        result.IsSuccess.Should().BeTrue();
        customization.Assignments.Should().HaveCount(2);
        _customizationRepository.Received(1).Update(customization);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WithTypologyFromAnotherPromotion_ShouldReturnFailure()
    {
        var developerCompanyId = Guid.NewGuid();
        var promotion = HousingPromotion.Create(developerCompanyId, "Residencial Acacias", "Madrid", "Calle Mayor 1");
        var otherPromotion = HousingPromotion.Create(developerCompanyId, "Residencial Naranjos", "Madrid", "Calle Sur 2");
        var tradeCategory = TradeCategory.Create(promotion.Id, "Carpintería", DateTime.UtcNow.AddMonths(1));
        var customization = Customization.CreateForTypologies(tradeCategory.Id, "Suelo", "Parquet Roble", 0m, new[] { Guid.NewGuid() });
        var typology = HousingTypology.Create(otherPromotion.Id, "Ático Tipo A");
        _currentUser.DeveloperCompanyId.Returns(developerCompanyId);
        _customizationRepository.GetByIdAsync(customization.Id, Arg.Any<CancellationToken>()).Returns(customization);
        _tradeCategoryRepository.GetByIdAsync(tradeCategory.Id, Arg.Any<CancellationToken>()).Returns(tradeCategory);
        _promotionRepository.GetByIdAsync(promotion.Id, Arg.Any<CancellationToken>()).Returns(promotion);
        _typologyRepository.GetByIdAsync(typology.Id, Arg.Any<CancellationToken>()).Returns(typology);

        var result = await _useCase.ExecuteAsync(new AssignCustomizationToTypologyCommand(customization.Id, typology.Id));

        result.IsFailure.Should().BeTrue();
        _customizationRepository.DidNotReceive().Update(Arg.Any<Customization>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenCustomizationIsAssignedToWholePromotion_ShouldReturnFailure()
    {
        var developerCompanyId = Guid.NewGuid();
        var promotion = HousingPromotion.Create(developerCompanyId, "Residencial Acacias", "Madrid", "Calle Mayor 1");
        var tradeCategory = TradeCategory.Create(promotion.Id, "Carpintería", DateTime.UtcNow.AddMonths(1));
        var customization = Customization.CreateForWholePromotion(tradeCategory.Id, "Suelo", "Parquet Roble", 0m);
        var typology = HousingTypology.Create(promotion.Id, "Ático Tipo A");
        _currentUser.DeveloperCompanyId.Returns(developerCompanyId);
        _customizationRepository.GetByIdAsync(customization.Id, Arg.Any<CancellationToken>()).Returns(customization);
        _tradeCategoryRepository.GetByIdAsync(tradeCategory.Id, Arg.Any<CancellationToken>()).Returns(tradeCategory);
        _promotionRepository.GetByIdAsync(promotion.Id, Arg.Any<CancellationToken>()).Returns(promotion);
        _typologyRepository.GetByIdAsync(typology.Id, Arg.Any<CancellationToken>()).Returns(typology);

        var result = await _useCase.ExecuteAsync(new AssignCustomizationToTypologyCommand(customization.Id, typology.Id));

        result.IsFailure.Should().BeTrue();
        _customizationRepository.DidNotReceive().Update(Arg.Any<Customization>());
    }
}

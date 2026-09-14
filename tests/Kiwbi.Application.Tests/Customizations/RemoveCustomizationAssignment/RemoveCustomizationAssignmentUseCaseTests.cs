using FluentAssertions;
using Kiwbi.Application.Common;
using Kiwbi.Application.Customizations.RemoveCustomizationAssignment;
using Kiwbi.Domain.Customizations;
using Kiwbi.Domain.RealEstate;
using NSubstitute;

namespace Kiwbi.Application.Tests.Customizations.RemoveCustomizationAssignment;

public class RemoveCustomizationAssignmentUseCaseTests
{
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();
    private readonly IHousingPromotionRepository _promotionRepository = Substitute.For<IHousingPromotionRepository>();
    private readonly ITradeCategoryRepository _tradeCategoryRepository = Substitute.For<ITradeCategoryRepository>();
    private readonly ICustomizationRepository _customizationRepository = Substitute.For<ICustomizationRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly RemoveCustomizationAssignmentUseCase _useCase;

    public RemoveCustomizationAssignmentUseCaseTests()
    {
        _useCase = new RemoveCustomizationAssignmentUseCase(_currentUser, _promotionRepository, _tradeCategoryRepository, _customizationRepository, _unitOfWork);
    }

    [Fact]
    public async Task ExecuteAsync_WithMultipleAssignments_ShouldRemoveTheSpecifiedOne()
    {
        var developerCompanyId = Guid.NewGuid();
        var promotion = HousingPromotion.Create(developerCompanyId, "Residencial Acacias", "Madrid", "Calle Mayor 1");
        var tradeCategory = TradeCategory.Create(promotion.Id, "Carpintería", DateTime.UtcNow.AddMonths(1));
        var customization = Customization.CreateForTypologies(tradeCategory.Id, "Suelo", "Parquet Roble", 0m, new[] { Guid.NewGuid(), Guid.NewGuid() });
        var assignmentToRemove = customization.Assignments.First();
        _currentUser.DeveloperCompanyId.Returns(developerCompanyId);
        _customizationRepository.GetByIdAsync(customization.Id, Arg.Any<CancellationToken>()).Returns(customization);
        _tradeCategoryRepository.GetByIdAsync(tradeCategory.Id, Arg.Any<CancellationToken>()).Returns(tradeCategory);
        _promotionRepository.GetByIdAsync(promotion.Id, Arg.Any<CancellationToken>()).Returns(promotion);

        var result = await _useCase.ExecuteAsync(customization.Id, assignmentToRemove.Id);

        result.IsSuccess.Should().BeTrue();
        customization.Assignments.Should().NotContain(a => a.Id == assignmentToRemove.Id);
        _customizationRepository.Received(1).Update(customization);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenOnlyAssignmentRemains_ShouldReturnFailure()
    {
        var developerCompanyId = Guid.NewGuid();
        var promotion = HousingPromotion.Create(developerCompanyId, "Residencial Acacias", "Madrid", "Calle Mayor 1");
        var tradeCategory = TradeCategory.Create(promotion.Id, "Carpintería", DateTime.UtcNow.AddMonths(1));
        var customization = Customization.CreateForWholePromotion(tradeCategory.Id, "Suelo", "Parquet Roble", 0m);
        var assignment = customization.Assignments.Single();
        _currentUser.DeveloperCompanyId.Returns(developerCompanyId);
        _customizationRepository.GetByIdAsync(customization.Id, Arg.Any<CancellationToken>()).Returns(customization);
        _tradeCategoryRepository.GetByIdAsync(tradeCategory.Id, Arg.Any<CancellationToken>()).Returns(tradeCategory);
        _promotionRepository.GetByIdAsync(promotion.Id, Arg.Any<CancellationToken>()).Returns(promotion);

        var result = await _useCase.ExecuteAsync(customization.Id, assignment.Id);

        result.IsFailure.Should().BeTrue();
        _customizationRepository.DidNotReceive().Update(Arg.Any<Customization>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenCustomizationBelongsToAnotherTenant_ShouldReturnFailure()
    {
        var promotion = HousingPromotion.Create(Guid.NewGuid(), "Residencial Acacias", "Madrid", "Calle Mayor 1");
        var tradeCategory = TradeCategory.Create(promotion.Id, "Carpintería", DateTime.UtcNow.AddMonths(1));
        var customization = Customization.CreateForTypologies(tradeCategory.Id, "Suelo", "Parquet Roble", 0m, new[] { Guid.NewGuid(), Guid.NewGuid() });
        var assignmentToRemove = customization.Assignments.First();
        _currentUser.DeveloperCompanyId.Returns(Guid.NewGuid());
        _customizationRepository.GetByIdAsync(customization.Id, Arg.Any<CancellationToken>()).Returns(customization);
        _tradeCategoryRepository.GetByIdAsync(tradeCategory.Id, Arg.Any<CancellationToken>()).Returns(tradeCategory);
        _promotionRepository.GetByIdAsync(promotion.Id, Arg.Any<CancellationToken>()).Returns(promotion);

        var result = await _useCase.ExecuteAsync(customization.Id, assignmentToRemove.Id);

        result.IsFailure.Should().BeTrue();
        _customizationRepository.DidNotReceive().Update(Arg.Any<Customization>());
    }
}

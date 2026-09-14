using FluentAssertions;
using Kiwbi.Application.Common;
using Kiwbi.Application.RealEstate.DeleteHousingTypology;
using Kiwbi.Domain.Customizations;
using Kiwbi.Domain.RealEstate;
using NSubstitute;

namespace Kiwbi.Application.Tests.RealEstate.DeleteHousingTypology;

public class DeleteHousingTypologyUseCaseTests
{
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();
    private readonly IHousingPromotionRepository _promotionRepository = Substitute.For<IHousingPromotionRepository>();
    private readonly IHousingTypologyRepository _typologyRepository = Substitute.For<IHousingTypologyRepository>();
    private readonly IHousingUnitRepository _unitRepository = Substitute.For<IHousingUnitRepository>();
    private readonly ICustomizationRepository _customizationRepository = Substitute.For<ICustomizationRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly DeleteHousingTypologyUseCase _useCase;

    public DeleteHousingTypologyUseCaseTests()
    {
        _useCase = new DeleteHousingTypologyUseCase(_currentUser, _promotionRepository, _typologyRepository, _unitRepository, _customizationRepository, _unitOfWork);
    }

    [Fact]
    public async Task ExecuteAsync_WithoutUnits_ShouldRemoveTypology()
    {
        var developerCompanyId = Guid.NewGuid();
        var promotion = HousingPromotion.Create(developerCompanyId, "Residencial Acacias", "Madrid", "Calle Mayor 1");
        var typology = HousingTypology.Create(promotion.Id, "Ático Tipo A");
        _currentUser.DeveloperCompanyId.Returns(developerCompanyId);
        _typologyRepository.GetByIdAsync(typology.Id, Arg.Any<CancellationToken>()).Returns(typology);
        _promotionRepository.GetByIdAsync(promotion.Id, Arg.Any<CancellationToken>()).Returns(promotion);
        _unitRepository.ExistsByHousingTypologyIdAsync(typology.Id, Arg.Any<CancellationToken>()).Returns(false);

        var result = await _useCase.ExecuteAsync(typology.Id);

        result.IsSuccess.Should().BeTrue();
        _typologyRepository.Received(1).Remove(typology);
    }

    [Fact]
    public async Task ExecuteAsync_WithUnitsAssigned_ShouldReturnFailureWithoutRemoving()
    {
        var developerCompanyId = Guid.NewGuid();
        var promotion = HousingPromotion.Create(developerCompanyId, "Residencial Acacias", "Madrid", "Calle Mayor 1");
        var typology = HousingTypology.Create(promotion.Id, "Ático Tipo A");
        _currentUser.DeveloperCompanyId.Returns(developerCompanyId);
        _typologyRepository.GetByIdAsync(typology.Id, Arg.Any<CancellationToken>()).Returns(typology);
        _promotionRepository.GetByIdAsync(promotion.Id, Arg.Any<CancellationToken>()).Returns(promotion);
        _unitRepository.ExistsByHousingTypologyIdAsync(typology.Id, Arg.Any<CancellationToken>()).Returns(true);

        var result = await _useCase.ExecuteAsync(typology.Id);

        result.IsFailure.Should().BeTrue();
        _typologyRepository.DidNotReceive().Remove(Arg.Any<HousingTypology>());
    }

    [Fact]
    public async Task ExecuteAsync_WithCustomizationAssignments_ShouldReturnFailureWithoutRemoving()
    {
        var developerCompanyId = Guid.NewGuid();
        var promotion = HousingPromotion.Create(developerCompanyId, "Residencial Acacias", "Madrid", "Calle Mayor 1");
        var typology = HousingTypology.Create(promotion.Id, "Ático Tipo A");
        _currentUser.DeveloperCompanyId.Returns(developerCompanyId);
        _typologyRepository.GetByIdAsync(typology.Id, Arg.Any<CancellationToken>()).Returns(typology);
        _promotionRepository.GetByIdAsync(promotion.Id, Arg.Any<CancellationToken>()).Returns(promotion);
        _unitRepository.ExistsByHousingTypologyIdAsync(typology.Id, Arg.Any<CancellationToken>()).Returns(false);
        _customizationRepository.ExistsByHousingTypologyIdAsync(typology.Id, Arg.Any<CancellationToken>()).Returns(true);

        var result = await _useCase.ExecuteAsync(typology.Id);

        result.IsFailure.Should().BeTrue();
        _typologyRepository.DidNotReceive().Remove(Arg.Any<HousingTypology>());
    }
}

using FluentAssertions;
using Kiwbi.Application.Common;
using Kiwbi.Application.RealEstate.GetHousingPromotionSummary;
using Kiwbi.Domain.RealEstate;
using NSubstitute;

namespace Kiwbi.Application.Tests.RealEstate.GetHousingPromotionSummary;

public class GetHousingPromotionSummaryUseCaseTests
{
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();
    private readonly IHousingPromotionRepository _promotionRepository = Substitute.For<IHousingPromotionRepository>();
    private readonly IHousingTypologyRepository _typologyRepository = Substitute.For<IHousingTypologyRepository>();
    private readonly IHousingUnitRepository _unitRepository = Substitute.For<IHousingUnitRepository>();
    private readonly GetHousingPromotionSummaryUseCase _useCase;

    public GetHousingPromotionSummaryUseCaseTests()
    {
        _useCase = new GetHousingPromotionSummaryUseCase(_currentUser, _promotionRepository, _typologyRepository, _unitRepository);
    }

    [Fact]
    public async Task ExecuteAsync_WithOwnedPromotion_ShouldReturnPromotionAndUnitsWithTypologyNames()
    {
        var developerCompanyId = Guid.NewGuid();
        var promotion = HousingPromotion.Create(developerCompanyId, "Residencial Acacias", "Madrid", "Calle Mayor 1");
        var typology = HousingTypology.Create(promotion.Id, "Ático Tipo A");
        var unitWithTypology = HousingUnit.Create(promotion.Id, "1", "A", 90m, 80m, typology.Id);
        var unitWithoutTypology = HousingUnit.Create(promotion.Id, "2", "B", 70m, null);
        unitWithoutTypology.ChangeStatus(HousingUnitStatus.Reserved);

        _currentUser.DeveloperCompanyId.Returns(developerCompanyId);
        _promotionRepository.GetByIdAsync(promotion.Id, Arg.Any<CancellationToken>()).Returns(promotion);
        _typologyRepository.GetByHousingPromotionIdAsync(promotion.Id, Arg.Any<CancellationToken>())
            .Returns(new List<HousingTypology> { typology });
        _unitRepository.GetByHousingPromotionIdAsync(promotion.Id, Arg.Any<CancellationToken>())
            .Returns(new List<HousingUnit> { unitWithTypology, unitWithoutTypology });

        var result = await _useCase.ExecuteAsync(promotion.Id);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Promotion.Name.Should().Be("Residencial Acacias");
        result.Value.Units.Should().HaveCount(2);
        result.Value.Units.Should().Contain(u => u.Id == unitWithTypology.Id && u.TypologyName == "Ático Tipo A");
        result.Value.Units.Should().Contain(u => u.Id == unitWithoutTypology.Id && u.TypologyName == null && u.Status == HousingUnitStatus.Reserved);
    }

    [Fact]
    public async Task ExecuteAsync_WhenPromotionBelongsToAnotherTenant_ShouldReturnFailure()
    {
        var promotion = HousingPromotion.Create(Guid.NewGuid(), "Residencial Acacias", "Madrid", "Calle Mayor 1");
        _currentUser.DeveloperCompanyId.Returns(Guid.NewGuid());
        _promotionRepository.GetByIdAsync(promotion.Id, Arg.Any<CancellationToken>()).Returns(promotion);

        var result = await _useCase.ExecuteAsync(promotion.Id);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteAsync_WhenPromotionDoesNotExist_ShouldReturnFailure()
    {
        _currentUser.DeveloperCompanyId.Returns(Guid.NewGuid());
        _promotionRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((HousingPromotion?)null);

        var result = await _useCase.ExecuteAsync(Guid.NewGuid());

        result.IsFailure.Should().BeTrue();
    }
}

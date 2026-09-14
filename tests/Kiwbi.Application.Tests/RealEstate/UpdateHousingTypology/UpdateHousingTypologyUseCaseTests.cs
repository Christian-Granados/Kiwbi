using FluentAssertions;
using Kiwbi.Application.Common;
using Kiwbi.Application.RealEstate.UpdateHousingTypology;
using Kiwbi.Domain.RealEstate;
using NSubstitute;

namespace Kiwbi.Application.Tests.RealEstate.UpdateHousingTypology;

public class UpdateHousingTypologyUseCaseTests
{
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();
    private readonly IHousingPromotionRepository _promotionRepository = Substitute.For<IHousingPromotionRepository>();
    private readonly IHousingTypologyRepository _typologyRepository = Substitute.For<IHousingTypologyRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly UpdateHousingTypologyUseCase _useCase;

    public UpdateHousingTypologyUseCaseTests()
    {
        _useCase = new UpdateHousingTypologyUseCase(_currentUser, _promotionRepository, _typologyRepository, _unitOfWork);
    }

    [Fact]
    public async Task ExecuteAsync_WithOwnedTypology_ShouldRenameAndPersist()
    {
        var developerCompanyId = Guid.NewGuid();
        var promotion = HousingPromotion.Create(developerCompanyId, "Residencial Acacias", "Madrid", "Calle Mayor 1");
        var typology = HousingTypology.Create(promotion.Id, "Ático Tipo A");
        _currentUser.DeveloperCompanyId.Returns(developerCompanyId);
        _typologyRepository.GetByIdAsync(typology.Id, Arg.Any<CancellationToken>()).Returns(typology);
        _promotionRepository.GetByIdAsync(promotion.Id, Arg.Any<CancellationToken>()).Returns(promotion);

        var result = await _useCase.ExecuteAsync(new UpdateHousingTypologyCommand(typology.Id, "Bajo Tipo B"));

        result.IsSuccess.Should().BeTrue();
        typology.Name.Should().Be("Bajo Tipo B");
        _typologyRepository.Received(1).Update(typology);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenTypologyBelongsToAnotherTenant_ShouldReturnFailure()
    {
        var promotion = HousingPromotion.Create(Guid.NewGuid(), "Residencial Acacias", "Madrid", "Calle Mayor 1");
        var typology = HousingTypology.Create(promotion.Id, "Ático Tipo A");
        _currentUser.DeveloperCompanyId.Returns(Guid.NewGuid());
        _typologyRepository.GetByIdAsync(typology.Id, Arg.Any<CancellationToken>()).Returns(typology);
        _promotionRepository.GetByIdAsync(promotion.Id, Arg.Any<CancellationToken>()).Returns(promotion);

        var result = await _useCase.ExecuteAsync(new UpdateHousingTypologyCommand(typology.Id, "Bajo Tipo B"));

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteAsync_WhenTypologyDoesNotExist_ShouldReturnFailure()
    {
        _currentUser.DeveloperCompanyId.Returns(Guid.NewGuid());
        _typologyRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((HousingTypology?)null);

        var result = await _useCase.ExecuteAsync(new UpdateHousingTypologyCommand(Guid.NewGuid(), "Bajo Tipo B"));

        result.IsFailure.Should().BeTrue();
    }
}

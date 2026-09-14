using FluentAssertions;
using Kiwbi.Application.Common;
using Kiwbi.Application.RealEstate.CreateHousingTypology;
using Kiwbi.Domain.RealEstate;
using NSubstitute;

namespace Kiwbi.Application.Tests.RealEstate.CreateHousingTypology;

public class CreateHousingTypologyUseCaseTests
{
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();
    private readonly IHousingPromotionRepository _promotionRepository = Substitute.For<IHousingPromotionRepository>();
    private readonly IHousingTypologyRepository _typologyRepository = Substitute.For<IHousingTypologyRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly CreateHousingTypologyUseCase _useCase;

    public CreateHousingTypologyUseCaseTests()
    {
        _useCase = new CreateHousingTypologyUseCase(_currentUser, _promotionRepository, _typologyRepository, _unitOfWork);
    }

    [Fact]
    public async Task ExecuteAsync_WithOwnedPromotion_ShouldCreateAndPersistTypology()
    {
        var developerCompanyId = Guid.NewGuid();
        var promotion = HousingPromotion.Create(developerCompanyId, "Residencial Acacias", "Madrid", "Calle Mayor 1");
        _currentUser.DeveloperCompanyId.Returns(developerCompanyId);
        _promotionRepository.GetByIdAsync(promotion.Id, Arg.Any<CancellationToken>()).Returns(promotion);

        var command = new CreateHousingTypologyCommand(promotion.Id, "Ático Tipo A");

        var result = await _useCase.ExecuteAsync(command);

        result.IsSuccess.Should().BeTrue();
        await _typologyRepository.Received(1).AddAsync(
            Arg.Is<HousingTypology>(t => t.Name == "Ático Tipo A" && t.HousingPromotionId == promotion.Id),
            Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenPromotionBelongsToAnotherTenant_ShouldReturnFailure()
    {
        var promotion = HousingPromotion.Create(Guid.NewGuid(), "Residencial Acacias", "Madrid", "Calle Mayor 1");
        _currentUser.DeveloperCompanyId.Returns(Guid.NewGuid());
        _promotionRepository.GetByIdAsync(promotion.Id, Arg.Any<CancellationToken>()).Returns(promotion);

        var command = new CreateHousingTypologyCommand(promotion.Id, "Ático Tipo A");

        var result = await _useCase.ExecuteAsync(command);

        result.IsFailure.Should().BeTrue();
        await _typologyRepository.DidNotReceive().AddAsync(Arg.Any<HousingTypology>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WithInvalidName_ShouldReturnFailureWithoutPersisting()
    {
        var developerCompanyId = Guid.NewGuid();
        var promotion = HousingPromotion.Create(developerCompanyId, "Residencial Acacias", "Madrid", "Calle Mayor 1");
        _currentUser.DeveloperCompanyId.Returns(developerCompanyId);
        _promotionRepository.GetByIdAsync(promotion.Id, Arg.Any<CancellationToken>()).Returns(promotion);

        var command = new CreateHousingTypologyCommand(promotion.Id, "   ");

        var result = await _useCase.ExecuteAsync(command);

        result.IsFailure.Should().BeTrue();
        await _typologyRepository.DidNotReceive().AddAsync(Arg.Any<HousingTypology>(), Arg.Any<CancellationToken>());
    }
}

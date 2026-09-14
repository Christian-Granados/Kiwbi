using FluentAssertions;
using Kiwbi.Application.Common;
using Kiwbi.Application.RealEstate.CreateHousingUnit;
using Kiwbi.Domain.RealEstate;
using NSubstitute;

namespace Kiwbi.Application.Tests.RealEstate.CreateHousingUnit;

public class CreateHousingUnitUseCaseTests
{
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();
    private readonly IHousingPromotionRepository _promotionRepository = Substitute.For<IHousingPromotionRepository>();
    private readonly IHousingTypologyRepository _typologyRepository = Substitute.For<IHousingTypologyRepository>();
    private readonly IHousingUnitRepository _unitRepository = Substitute.For<IHousingUnitRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly CreateHousingUnitUseCase _useCase;

    public CreateHousingUnitUseCaseTests()
    {
        _useCase = new CreateHousingUnitUseCase(_currentUser, _promotionRepository, _typologyRepository, _unitRepository, _unitOfWork);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidCommand_ShouldCreateAndPersistUnit()
    {
        var developerCompanyId = Guid.NewGuid();
        var promotion = HousingPromotion.Create(developerCompanyId, "Residencial Acacias", "Madrid", "Calle Mayor 1");
        _currentUser.DeveloperCompanyId.Returns(developerCompanyId);
        _promotionRepository.GetByIdAsync(promotion.Id, Arg.Any<CancellationToken>()).Returns(promotion);
        _unitRepository.ExistsWithFloorAndDoorAsync(promotion.Id, "1", "A", Arg.Any<Guid?>(), Arg.Any<CancellationToken>()).Returns(false);

        var command = new CreateHousingUnitCommand(promotion.Id, null, "1", "A", 90m, 80m);

        var result = await _useCase.ExecuteAsync(command);

        result.IsSuccess.Should().BeTrue();
        await _unitRepository.Received(1).AddAsync(
            Arg.Is<HousingUnit>(u => u.Floor == "1" && u.Door == "A" && u.HousingPromotionId == promotion.Id),
            Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WithTypologyFromAnotherPromotion_ShouldReturnFailure()
    {
        var developerCompanyId = Guid.NewGuid();
        var promotion = HousingPromotion.Create(developerCompanyId, "Residencial Acacias", "Madrid", "Calle Mayor 1");
        var otherPromotion = HousingPromotion.Create(developerCompanyId, "Otra Promoción", "Madrid", "Calle Otra 1");
        var typology = HousingTypology.Create(otherPromotion.Id, "Ático Tipo A");
        _currentUser.DeveloperCompanyId.Returns(developerCompanyId);
        _promotionRepository.GetByIdAsync(promotion.Id, Arg.Any<CancellationToken>()).Returns(promotion);
        _typologyRepository.GetByIdAsync(typology.Id, Arg.Any<CancellationToken>()).Returns(typology);

        var command = new CreateHousingUnitCommand(promotion.Id, typology.Id, "1", "A", 90m, 80m);

        var result = await _useCase.ExecuteAsync(command);

        result.IsFailure.Should().BeTrue();
        await _unitRepository.DidNotReceive().AddAsync(Arg.Any<HousingUnit>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WithDuplicateFloorAndDoor_ShouldReturnFailure()
    {
        var developerCompanyId = Guid.NewGuid();
        var promotion = HousingPromotion.Create(developerCompanyId, "Residencial Acacias", "Madrid", "Calle Mayor 1");
        _currentUser.DeveloperCompanyId.Returns(developerCompanyId);
        _promotionRepository.GetByIdAsync(promotion.Id, Arg.Any<CancellationToken>()).Returns(promotion);
        _unitRepository.ExistsWithFloorAndDoorAsync(promotion.Id, "1", "A", Arg.Any<Guid?>(), Arg.Any<CancellationToken>()).Returns(true);

        var command = new CreateHousingUnitCommand(promotion.Id, null, "1", "A", 90m, 80m);

        var result = await _useCase.ExecuteAsync(command);

        result.IsFailure.Should().BeTrue();
        await _unitRepository.DidNotReceive().AddAsync(Arg.Any<HousingUnit>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenPromotionBelongsToAnotherTenant_ShouldReturnFailure()
    {
        var promotion = HousingPromotion.Create(Guid.NewGuid(), "Residencial Acacias", "Madrid", "Calle Mayor 1");
        _currentUser.DeveloperCompanyId.Returns(Guid.NewGuid());
        _promotionRepository.GetByIdAsync(promotion.Id, Arg.Any<CancellationToken>()).Returns(promotion);

        var command = new CreateHousingUnitCommand(promotion.Id, null, "1", "A", 90m, 80m);

        var result = await _useCase.ExecuteAsync(command);

        result.IsFailure.Should().BeTrue();
    }
}

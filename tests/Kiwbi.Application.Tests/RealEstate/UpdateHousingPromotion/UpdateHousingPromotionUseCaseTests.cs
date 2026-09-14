using FluentAssertions;
using Kiwbi.Application.Common;
using Kiwbi.Application.RealEstate.UpdateHousingPromotion;
using Kiwbi.Domain.RealEstate;
using NSubstitute;

namespace Kiwbi.Application.Tests.RealEstate.UpdateHousingPromotion;

public class UpdateHousingPromotionUseCaseTests
{
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();
    private readonly IHousingPromotionRepository _repository = Substitute.For<IHousingPromotionRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly UpdateHousingPromotionUseCase _useCase;

    public UpdateHousingPromotionUseCaseTests()
    {
        _useCase = new UpdateHousingPromotionUseCase(_currentUser, _repository, _unitOfWork);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidCommand_ShouldUpdateAndPersistPromotion()
    {
        var developerCompanyId = Guid.NewGuid();
        var promotion = HousingPromotion.Create(developerCompanyId, "Residencial Acacias", "Madrid", "Calle Mayor 1");
        _currentUser.DeveloperCompanyId.Returns(developerCompanyId);
        _repository.GetByIdAsync(promotion.Id, Arg.Any<CancellationToken>()).Returns(promotion);

        var command = new UpdateHousingPromotionCommand(promotion.Id, "Residencial Robles", "Barcelona", "Avenida Diagonal 20");

        var result = await _useCase.ExecuteAsync(command);

        result.IsSuccess.Should().BeTrue();
        promotion.Name.Should().Be("Residencial Robles");
        _repository.Received(1).Update(promotion);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WithInvalidData_ShouldReturnFailureWithoutPersisting()
    {
        var developerCompanyId = Guid.NewGuid();
        var promotion = HousingPromotion.Create(developerCompanyId, "Residencial Acacias", "Madrid", "Calle Mayor 1");
        _currentUser.DeveloperCompanyId.Returns(developerCompanyId);
        _repository.GetByIdAsync(promotion.Id, Arg.Any<CancellationToken>()).Returns(promotion);

        var command = new UpdateHousingPromotionCommand(promotion.Id, "   ", "Barcelona", "Avenida Diagonal 20");

        var result = await _useCase.ExecuteAsync(command);

        result.IsFailure.Should().BeTrue();
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenPromotionBelongsToAnotherTenant_ShouldReturnFailure()
    {
        var promotion = HousingPromotion.Create(Guid.NewGuid(), "Residencial Acacias", "Madrid", "Calle Mayor 1");
        _currentUser.DeveloperCompanyId.Returns(Guid.NewGuid());
        _repository.GetByIdAsync(promotion.Id, Arg.Any<CancellationToken>()).Returns(promotion);

        var command = new UpdateHousingPromotionCommand(promotion.Id, "Residencial Robles", "Barcelona", "Avenida Diagonal 20");

        var result = await _useCase.ExecuteAsync(command);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteAsync_WhenPromotionDoesNotExist_ShouldReturnFailure()
    {
        _currentUser.DeveloperCompanyId.Returns(Guid.NewGuid());
        _repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((HousingPromotion?)null);

        var command = new UpdateHousingPromotionCommand(Guid.NewGuid(), "Residencial Robles", "Barcelona", "Avenida Diagonal 20");

        var result = await _useCase.ExecuteAsync(command);

        result.IsFailure.Should().BeTrue();
    }
}

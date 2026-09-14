using FluentAssertions;
using Kiwbi.Application.Common;
using Kiwbi.Application.RealEstate.CreateHousingPromotion;
using Kiwbi.Domain.RealEstate;
using NSubstitute;

namespace Kiwbi.Application.Tests.RealEstate.CreateHousingPromotion;

public class CreateHousingPromotionUseCaseTests
{
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();
    private readonly IHousingPromotionRepository _repository = Substitute.For<IHousingPromotionRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly CreateHousingPromotionUseCase _useCase;

    public CreateHousingPromotionUseCaseTests()
    {
        _useCase = new CreateHousingPromotionUseCase(_currentUser, _repository, _unitOfWork);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidCommand_ShouldCreateAndPersistPromotion()
    {
        var developerCompanyId = Guid.NewGuid();
        _currentUser.DeveloperCompanyId.Returns(developerCompanyId);

        var command = new CreateHousingPromotionCommand("Residencial Acacias", "Madrid", "Calle Mayor 1");

        var result = await _useCase.ExecuteAsync(command);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
        await _repository.Received(1).AddAsync(
            Arg.Is<HousingPromotion>(p => p.Name == "Residencial Acacias" && p.DeveloperCompanyId == developerCompanyId),
            Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WithInvalidName_ShouldReturnFailureWithoutPersisting()
    {
        _currentUser.DeveloperCompanyId.Returns(Guid.NewGuid());

        var command = new CreateHousingPromotionCommand("   ", "Madrid", "Calle Mayor 1");

        var result = await _useCase.ExecuteAsync(command);

        result.IsFailure.Should().BeTrue();
        await _repository.DidNotReceive().AddAsync(Arg.Any<HousingPromotion>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenUserHasNoTenant_ShouldReturnFailure()
    {
        _currentUser.DeveloperCompanyId.Returns((Guid?)null);

        var command = new CreateHousingPromotionCommand("Residencial Acacias", "Madrid", "Calle Mayor 1");

        var result = await _useCase.ExecuteAsync(command);

        result.IsFailure.Should().BeTrue();
        await _repository.DidNotReceive().AddAsync(Arg.Any<HousingPromotion>(), Arg.Any<CancellationToken>());
    }
}

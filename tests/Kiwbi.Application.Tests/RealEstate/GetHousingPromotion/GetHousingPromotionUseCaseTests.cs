using FluentAssertions;
using Kiwbi.Application.Common;
using Kiwbi.Application.RealEstate.GetHousingPromotion;
using Kiwbi.Domain.RealEstate;
using NSubstitute;

namespace Kiwbi.Application.Tests.RealEstate.GetHousingPromotion;

public class GetHousingPromotionUseCaseTests
{
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();
    private readonly IHousingPromotionRepository _repository = Substitute.For<IHousingPromotionRepository>();
    private readonly GetHousingPromotionUseCase _useCase;

    public GetHousingPromotionUseCaseTests()
    {
        _useCase = new GetHousingPromotionUseCase(_currentUser, _repository);
    }

    [Fact]
    public async Task ExecuteAsync_WithOwnedPromotion_ShouldReturnDto()
    {
        var developerCompanyId = Guid.NewGuid();
        var promotion = HousingPromotion.Create(developerCompanyId, "Residencial Acacias", "Madrid", "Calle Mayor 1");
        _currentUser.DeveloperCompanyId.Returns(developerCompanyId);
        _repository.GetByIdAsync(promotion.Id, Arg.Any<CancellationToken>()).Returns(promotion);

        var result = await _useCase.ExecuteAsync(promotion.Id);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Name.Should().Be("Residencial Acacias");
    }

    [Fact]
    public async Task ExecuteAsync_WhenPromotionBelongsToAnotherTenant_ShouldReturnFailure()
    {
        var promotion = HousingPromotion.Create(Guid.NewGuid(), "Residencial Acacias", "Madrid", "Calle Mayor 1");
        _currentUser.DeveloperCompanyId.Returns(Guid.NewGuid());
        _repository.GetByIdAsync(promotion.Id, Arg.Any<CancellationToken>()).Returns(promotion);

        var result = await _useCase.ExecuteAsync(promotion.Id);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteAsync_WhenPromotionDoesNotExist_ShouldReturnFailure()
    {
        _currentUser.DeveloperCompanyId.Returns(Guid.NewGuid());
        _repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((HousingPromotion?)null);

        var result = await _useCase.ExecuteAsync(Guid.NewGuid());

        result.IsFailure.Should().BeTrue();
    }
}

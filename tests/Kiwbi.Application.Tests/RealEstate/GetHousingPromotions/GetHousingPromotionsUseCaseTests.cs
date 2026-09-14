using FluentAssertions;
using Kiwbi.Application.Common;
using Kiwbi.Application.RealEstate.GetHousingPromotions;
using Kiwbi.Domain.RealEstate;
using NSubstitute;

namespace Kiwbi.Application.Tests.RealEstate.GetHousingPromotions;

public class GetHousingPromotionsUseCaseTests
{
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();
    private readonly IHousingPromotionRepository _repository = Substitute.For<IHousingPromotionRepository>();
    private readonly GetHousingPromotionsUseCase _useCase;

    public GetHousingPromotionsUseCaseTests()
    {
        _useCase = new GetHousingPromotionsUseCase(_currentUser, _repository);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnPromotionsOfCurrentTenant()
    {
        var developerCompanyId = Guid.NewGuid();
        var promotion = HousingPromotion.Create(developerCompanyId, "Residencial Acacias", "Madrid", "Calle Mayor 1");
        _currentUser.DeveloperCompanyId.Returns(developerCompanyId);
        _repository.GetByDeveloperCompanyIdAsync(developerCompanyId, Arg.Any<CancellationToken>())
            .Returns(new List<HousingPromotion> { promotion });

        var result = await _useCase.ExecuteAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().ContainSingle(p => p.Name == "Residencial Acacias");
    }

    [Fact]
    public async Task ExecuteAsync_WhenUserHasNoTenant_ShouldReturnFailure()
    {
        _currentUser.DeveloperCompanyId.Returns((Guid?)null);

        var result = await _useCase.ExecuteAsync();

        result.IsFailure.Should().BeTrue();
    }
}

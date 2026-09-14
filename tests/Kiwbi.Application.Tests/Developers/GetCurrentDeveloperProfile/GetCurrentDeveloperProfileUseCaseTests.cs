using FluentAssertions;
using Kiwbi.Application.Common;
using Kiwbi.Application.Developers.GetCurrentDeveloperProfile;
using Kiwbi.Domain.Developers;
using NSubstitute;

namespace Kiwbi.Application.Tests.Developers.GetCurrentDeveloperProfile;

public class GetCurrentDeveloperProfileUseCaseTests
{
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();
    private readonly IDeveloperCompanyRepository _repository = Substitute.For<IDeveloperCompanyRepository>();
    private readonly GetCurrentDeveloperProfileUseCase _useCase;

    public GetCurrentDeveloperProfileUseCaseTests()
    {
        _useCase = new GetCurrentDeveloperProfileUseCase(_currentUser, _repository);
    }

    [Fact]
    public async Task ExecuteAsync_WhenUserHasNoTenant_ShouldReturnFailure()
    {
        _currentUser.DeveloperCompanyId.Returns((Guid?)null);

        var result = await _useCase.ExecuteAsync();

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteAsync_WhenCompanyDoesNotExist_ShouldReturnFailure()
    {
        var companyId = Guid.NewGuid();
        _currentUser.DeveloperCompanyId.Returns(companyId);
        _repository.GetByIdAsync(companyId, Arg.Any<CancellationToken>()).Returns((DeveloperCompany?)null);

        var result = await _useCase.ExecuteAsync();

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteAsync_WhenCompanyExists_ShouldReturnDto()
    {
        var company = DeveloperCompany.Create("Acme Promotora");
        _currentUser.DeveloperCompanyId.Returns(company.Id);
        _repository.GetByIdAsync(company.Id, Arg.Any<CancellationToken>()).Returns(company);

        var result = await _useCase.ExecuteAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value!.Name.Should().Be("Acme Promotora");
        result.Value.DeveloperCompanyId.Should().Be(company.Id);
    }
}

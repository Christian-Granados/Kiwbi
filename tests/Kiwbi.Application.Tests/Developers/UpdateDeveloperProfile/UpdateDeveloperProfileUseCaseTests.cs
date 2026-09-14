using FluentAssertions;
using Kiwbi.Application.Common;
using Kiwbi.Application.Developers.UpdateDeveloperProfile;
using Kiwbi.Domain.Developers;
using NSubstitute;

namespace Kiwbi.Application.Tests.Developers.UpdateDeveloperProfile;

public class UpdateDeveloperProfileUseCaseTests
{
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();
    private readonly IDeveloperCompanyRepository _repository = Substitute.For<IDeveloperCompanyRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly UpdateDeveloperProfileUseCase _useCase;

    public UpdateDeveloperProfileUseCaseTests()
    {
        _useCase = new UpdateDeveloperProfileUseCase(_currentUser, _repository, _unitOfWork);
    }

    [Fact]
    public async Task ExecuteAsync_WhenUserHasNoTenant_ShouldReturnFailure()
    {
        _currentUser.DeveloperCompanyId.Returns((Guid?)null);

        var result = await _useCase.ExecuteAsync(new UpdateDeveloperProfileCommand("Nueva Promotora"));

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteAsync_WithValidName_ShouldRenameAndPersist()
    {
        var company = DeveloperCompany.Create("Acme Promotora");
        _currentUser.DeveloperCompanyId.Returns(company.Id);
        _repository.GetByIdAsync(company.Id, Arg.Any<CancellationToken>()).Returns(company);

        var result = await _useCase.ExecuteAsync(new UpdateDeveloperProfileCommand("Nueva Promotora"));

        result.IsSuccess.Should().BeTrue();
        company.Name.Should().Be("Nueva Promotora");
        _repository.Received(1).Update(company);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WithInvalidName_ShouldReturnFailureWithoutPersisting()
    {
        var company = DeveloperCompany.Create("Acme Promotora");
        _currentUser.DeveloperCompanyId.Returns(company.Id);
        _repository.GetByIdAsync(company.Id, Arg.Any<CancellationToken>()).Returns(company);

        var result = await _useCase.ExecuteAsync(new UpdateDeveloperProfileCommand("   "));

        result.IsFailure.Should().BeTrue();
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}

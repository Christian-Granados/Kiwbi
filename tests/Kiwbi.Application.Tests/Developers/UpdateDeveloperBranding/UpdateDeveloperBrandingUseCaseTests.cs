using FluentAssertions;
using Kiwbi.Application.Common;
using Kiwbi.Application.Developers.UpdateDeveloperBranding;
using Kiwbi.Domain.Developers;
using NSubstitute;

namespace Kiwbi.Application.Tests.Developers.UpdateDeveloperBranding;

public class UpdateDeveloperBrandingUseCaseTests
{
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();
    private readonly IDeveloperCompanyRepository _repository = Substitute.For<IDeveloperCompanyRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly UpdateDeveloperBrandingUseCase _useCase;

    public UpdateDeveloperBrandingUseCaseTests()
    {
        _useCase = new UpdateDeveloperBrandingUseCase(_currentUser, _repository, _unitOfWork);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidColors_ShouldUpdateBrandingAndPersist()
    {
        var company = DeveloperCompany.Create("Acme Promotora");
        _currentUser.DeveloperCompanyId.Returns(company.Id);
        _repository.GetByIdAsync(company.Id, Arg.Any<CancellationToken>()).Returns(company);

        var result = await _useCase.ExecuteAsync(new UpdateDeveloperBrandingCommand("#ABCDEF", "#123456", "logos/acme.png"));

        result.IsSuccess.Should().BeTrue();
        company.Branding.PrimaryColor.Value.Should().Be("#ABCDEF");
        company.Branding.LogoPath.Should().Be("logos/acme.png");
        _repository.Received(1).Update(company);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WithInvalidColor_ShouldReturnFailureWithoutPersisting()
    {
        var company = DeveloperCompany.Create("Acme Promotora");
        _currentUser.DeveloperCompanyId.Returns(company.Id);
        _repository.GetByIdAsync(company.Id, Arg.Any<CancellationToken>()).Returns(company);

        var result = await _useCase.ExecuteAsync(new UpdateDeveloperBrandingCommand("not-a-color", null, null));

        result.IsFailure.Should().BeTrue();
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenUserHasNoTenant_ShouldReturnFailure()
    {
        _currentUser.DeveloperCompanyId.Returns((Guid?)null);

        var result = await _useCase.ExecuteAsync(new UpdateDeveloperBrandingCommand("#ABCDEF", null, null));

        result.IsFailure.Should().BeTrue();
    }
}

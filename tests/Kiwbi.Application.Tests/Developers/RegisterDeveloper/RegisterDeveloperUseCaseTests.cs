using FluentAssertions;
using Kiwbi.Application.Common;
using Kiwbi.Application.Developers;
using Kiwbi.Application.Developers.RegisterDeveloper;
using Kiwbi.Domain.Developers;
using NSubstitute;

namespace Kiwbi.Application.Tests.Developers.RegisterDeveloper;

public class RegisterDeveloperUseCaseTests
{
    private readonly IDeveloperCompanyRepository _repository = Substitute.For<IDeveloperCompanyRepository>();
    private readonly IAccountProvisioningService _accountProvisioningService = Substitute.For<IAccountProvisioningService>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly RegisterDeveloperUseCase _useCase;

    public RegisterDeveloperUseCaseTests()
    {
        _unitOfWork
            .ExecuteInTransactionAsync(Arg.Any<Func<CancellationToken, Task<Result<RegisterDeveloperResult>>>>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => callInfo.Arg<Func<CancellationToken, Task<Result<RegisterDeveloperResult>>>>().Invoke(CancellationToken.None));

        _useCase = new RegisterDeveloperUseCase(_repository, _accountProvisioningService, _unitOfWork);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidCommand_ShouldCreateCompanyAndAccount()
    {
        _accountProvisioningService
            .CreateDeveloperAdminAccountAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success("user-1"));

        var command = new RegisterDeveloperCommand("Acme Promotora", "admin@acme.com", "P@ssw0rd!");

        var result = await _useCase.ExecuteAsync(command);

        result.IsSuccess.Should().BeTrue();
        result.Value!.UserId.Should().Be("user-1");
        await _repository.Received(1).AddAsync(Arg.Is<DeveloperCompany>(c => c.Name == "Acme Promotora"), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WithInvalidCompanyName_ShouldReturnFailureWithoutTouchingPorts()
    {
        var command = new RegisterDeveloperCommand("   ", "admin@acme.com", "P@ssw0rd!");

        var result = await _useCase.ExecuteAsync(command);

        result.IsFailure.Should().BeTrue();
        await _repository.DidNotReceive().AddAsync(Arg.Any<DeveloperCompany>(), Arg.Any<CancellationToken>());
        await _accountProvisioningService.DidNotReceive().CreateDeveloperAdminAccountAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenAccountProvisioningFails_ShouldReturnFailure()
    {
        _accountProvisioningService
            .CreateDeveloperAdminAccountAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<string>("El correo ya está registrado."));

        var command = new RegisterDeveloperCommand("Acme Promotora", "admin@acme.com", "P@ssw0rd!");

        var result = await _useCase.ExecuteAsync(command);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("El correo ya está registrado.");
    }
}

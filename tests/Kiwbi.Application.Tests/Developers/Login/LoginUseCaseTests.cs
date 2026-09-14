using FluentAssertions;
using Kiwbi.Application.Common;
using Kiwbi.Application.Developers;
using Kiwbi.Application.Developers.Login;
using NSubstitute;

namespace Kiwbi.Application.Tests.Developers.Login;

public class LoginUseCaseTests
{
    private readonly IAuthenticationService _authenticationService = Substitute.For<IAuthenticationService>();
    private readonly LoginUseCase _useCase;

    public LoginUseCaseTests()
    {
        _useCase = new LoginUseCase(_authenticationService);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldDelegateToAuthenticationService()
    {
        _authenticationService
            .SignInAsync("admin@acme.com", "P@ssw0rd!", true, Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var result = await _useCase.ExecuteAsync(new LoginCommand("admin@acme.com", "P@ssw0rd!", true));

        result.IsSuccess.Should().BeTrue();
        await _authenticationService.Received(1).SignInAsync("admin@acme.com", "P@ssw0rd!", true, Arg.Any<CancellationToken>());
    }
}

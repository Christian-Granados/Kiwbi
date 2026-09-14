using Kiwbi.Application.Common;

namespace Kiwbi.Application.Developers.Login;

public class LoginUseCase
{
    private readonly IAuthenticationService _authenticationService;

    public LoginUseCase(IAuthenticationService authenticationService)
    {
        _authenticationService = authenticationService;
    }

    public Task<Result> ExecuteAsync(LoginCommand command, CancellationToken cancellationToken = default) =>
        _authenticationService.SignInAsync(command.Email, command.Password, command.RememberMe, cancellationToken);
}

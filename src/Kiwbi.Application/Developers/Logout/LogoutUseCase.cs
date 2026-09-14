using Kiwbi.Application.Common;

namespace Kiwbi.Application.Developers.Logout;

public class LogoutUseCase
{
    private readonly IAuthenticationService _authenticationService;

    public LogoutUseCase(IAuthenticationService authenticationService)
    {
        _authenticationService = authenticationService;
    }

    public Task ExecuteAsync(CancellationToken cancellationToken = default) =>
        _authenticationService.SignOutAsync(cancellationToken);
}

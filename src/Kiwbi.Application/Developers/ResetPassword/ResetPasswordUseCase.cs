using Kiwbi.Application.Common;

namespace Kiwbi.Application.Developers.ResetPassword;

public record ResetPasswordCommand(string Email, string Token, string NewPassword);

public class ResetPasswordUseCase
{
    private readonly IAuthenticationService _authenticationService;

    public ResetPasswordUseCase(IAuthenticationService authenticationService)
    {
        _authenticationService = authenticationService;
    }

    public Task<Result> ExecuteAsync(ResetPasswordCommand command, CancellationToken cancellationToken = default) =>
        _authenticationService.ResetPasswordAsync(command.Email, command.Token, command.NewPassword, cancellationToken);
}

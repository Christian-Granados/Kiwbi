using Kiwbi.Application.Common;

namespace Kiwbi.Application.Developers.ForgotPassword;

/// <summary>Requests a password reset email. Shared by both Promotora and Comprador accounts - Identity's reset
/// token mechanism doesn't distinguish role. Always returns success regardless of whether the email is registered,
/// to avoid leaking account existence (OWASP user enumeration).</summary>
public class ForgotPasswordUseCase
{
    private readonly IAuthenticationService _authenticationService;
    private readonly IEmailSender _emailSender;

    public ForgotPasswordUseCase(IAuthenticationService authenticationService, IEmailSender emailSender)
    {
        _authenticationService = authenticationService;
        _emailSender = emailSender;
    }

    public async Task<Result> ExecuteAsync(string email, CancellationToken cancellationToken = default)
    {
        var token = await _authenticationService.GeneratePasswordResetTokenAsync(email, cancellationToken);

        if (token is not null)
        {
            await _emailSender.SendPasswordResetEmailAsync(email, token, cancellationToken);
        }

        return Result.Success();
    }
}

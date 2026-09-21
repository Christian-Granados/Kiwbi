using Kiwbi.Application.Common;

namespace Kiwbi.Application.Developers;

/// <summary>Port for sign-in/sign-out operations, isolated from the concrete Identity implementation.</summary>
public interface IAuthenticationService
{
    Task<Result> SignInAsync(string email, string password, bool rememberMe, CancellationToken cancellationToken = default);

    Task SignOutAsync(CancellationToken cancellationToken = default);

    /// <summary>Returns a password reset token, or null if no account exists for that email (Epic 11, Feature 11.2).
    /// A null return must never be surfaced to the caller as an error - the Application use case treats both cases
    /// identically to avoid leaking whether an email is registered.</summary>
    Task<string?> GeneratePasswordResetTokenAsync(string email, CancellationToken cancellationToken = default);

    Task<Result> ResetPasswordAsync(string email, string token, string newPassword, CancellationToken cancellationToken = default);
}

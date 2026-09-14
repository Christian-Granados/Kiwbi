using Kiwbi.Application.Common;

namespace Kiwbi.Application.Developers;

/// <summary>Port for sign-in/sign-out operations, isolated from the concrete Identity implementation.</summary>
public interface IAuthenticationService
{
    Task<Result> SignInAsync(string email, string password, bool rememberMe, CancellationToken cancellationToken = default);

    Task SignOutAsync(CancellationToken cancellationToken = default);
}

using Kiwbi.Application.Common;
using Kiwbi.Application.Developers;
using Microsoft.AspNetCore.Identity;

namespace Kiwbi.Infrastructure.Identity;

/// <summary>Adapter that signs users in/out using SignInManager, hidden behind an Application port.</summary>
public class IdentityAuthenticationService : IAuthenticationService
{
    private readonly SignInManager<ApplicationUser> _signInManager;

    public IdentityAuthenticationService(SignInManager<ApplicationUser> signInManager)
    {
        _signInManager = signInManager;
    }

    public async Task<Result> SignInAsync(string email, string password, bool rememberMe, CancellationToken cancellationToken = default)
    {
        var result = await _signInManager.PasswordSignInAsync(email, password, rememberMe, lockoutOnFailure: true);

        if (result.Succeeded)
        {
            return Result.Success();
        }

        if (result.IsLockedOut)
        {
            return Result.Failure("La cuenta está bloqueada temporalmente. Inténtalo de nuevo más tarde.");
        }

        return Result.Failure("Correo electrónico o contraseña incorrectos.");
    }

    public Task SignOutAsync(CancellationToken cancellationToken = default) => _signInManager.SignOutAsync();
}

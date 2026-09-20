using Kiwbi.Application.Common;
using Kiwbi.Application.Developers;
using Microsoft.AspNetCore.Identity;

namespace Kiwbi.Infrastructure.Identity;

/// <summary>Adapter that signs users in/out using SignInManager, hidden behind an Application port.</summary>
public class IdentityAuthenticationService : IAuthenticationService
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public IdentityAuthenticationService(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
    {
        _signInManager = signInManager;
        _userManager = userManager;
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

    public async Task<string?> GeneratePasswordResetTokenAsync(string email, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(email);

        return user is null ? null : await _userManager.GeneratePasswordResetTokenAsync(user);
    }

    public async Task<Result> ResetPasswordAsync(string email, string token, string newPassword, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(email);

        if (user is null)
        {
            // Same generic message as an invalid/expired token, so this never reveals whether the email is registered.
            return Result.Failure("El enlace de restablecimiento no es válido o ha caducado.");
        }

        var result = await _userManager.ResetPasswordAsync(user, token, newPassword);

        if (result.Succeeded)
        {
            return Result.Success();
        }

        return Result.Failure("El enlace de restablecimiento no es válido o ha caducado.");
    }
}

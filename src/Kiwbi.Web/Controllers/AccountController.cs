using Kiwbi.Application.Developers.Login;
using Kiwbi.Application.Developers.Logout;
using Kiwbi.Application.Developers.RegisterDeveloper;
using Kiwbi.Web.Models.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kiwbi.Web.Controllers;

[AllowAnonymous]
public class AccountController : Controller
{
    private readonly RegisterDeveloperUseCase _registerDeveloperUseCase;
    private readonly LoginUseCase _loginUseCase;
    private readonly LogoutUseCase _logoutUseCase;

    public AccountController(
        RegisterDeveloperUseCase registerDeveloperUseCase,
        LoginUseCase loginUseCase,
        LogoutUseCase logoutUseCase)
    {
        _registerDeveloperUseCase = registerDeveloperUseCase;
        _loginUseCase = loginUseCase;
        _logoutUseCase = logoutUseCase;
    }

    [HttpGet]
    public IActionResult Register() => View(new RegisterViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var command = new RegisterDeveloperCommand(model.CompanyName, model.Email, model.Password);
        var result = await _registerDeveloperUseCase.ExecuteAsync(command, cancellationToken);

        if (result.IsFailure)
        {
            ModelState.AddModelError(string.Empty, result.Error!);
            return View(model);
        }

        // The new admin account is signed in immediately so registration flows straight into the dashboard.
        await _loginUseCase.ExecuteAsync(new LoginCommand(model.Email, model.Password, RememberMe: false), cancellationToken);

        return RedirectToAction("Index", "DeveloperProfile");
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null) => View(new LoginViewModel { ReturnUrl = returnUrl });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var command = new LoginCommand(model.Email, model.Password, model.RememberMe);
        var result = await _loginUseCase.ExecuteAsync(command, cancellationToken);

        if (result.IsFailure)
        {
            ModelState.AddModelError(string.Empty, result.Error!);
            return View(model);
        }

        if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
        {
            return Redirect(model.ReturnUrl);
        }

        return RedirectToAction("Index", "DeveloperProfile");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        await _logoutUseCase.ExecuteAsync(cancellationToken);
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult AccessDenied() => View();
}

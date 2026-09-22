using Kiwbi.Application.Developers.Login;
using Kiwbi.Application.Developers.Logout;
using Kiwbi.Application.Developers.ForgotPassword;
using Kiwbi.Application.Developers.ResetPassword;
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
    private readonly ForgotPasswordUseCase _forgotPasswordUseCase;
    private readonly ResetPasswordUseCase _resetPasswordUseCase;

    public AccountController(
        RegisterDeveloperUseCase registerDeveloperUseCase,
        LoginUseCase loginUseCase,
        LogoutUseCase logoutUseCase,
        ForgotPasswordUseCase forgotPasswordUseCase,
        ResetPasswordUseCase resetPasswordUseCase)
    {
        _registerDeveloperUseCase = registerDeveloperUseCase;
        _loginUseCase = loginUseCase;
        _logoutUseCase = logoutUseCase;
        _forgotPasswordUseCase = forgotPasswordUseCase;
        _resetPasswordUseCase = resetPasswordUseCase;
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

        // Login (as opposed to Register, which always lands on Mi promotora for first-time setup) always
        // goes straight to the working area: Promociones for a promotora, "Mis viviendas" for a buyer.
        if (User.IsInRole("Buyer"))
        {
            return RedirectToAction("Index", "Buyer");
        }

        return RedirectToAction("Index", "HousingPromotions");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        await _logoutUseCase.ExecuteAsync(cancellationToken);
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult ForgotPassword() => View(new ForgotPasswordViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // Always redirects to the same confirmation page regardless of whether the email is registered
        // (the use case itself never reveals it either) - avoids user enumeration.
        await _forgotPasswordUseCase.ExecuteAsync(model.Email, cancellationToken);

        return RedirectToAction(nameof(ForgotPasswordConfirmation));
    }

    [HttpGet]
    public IActionResult ForgotPasswordConfirmation() => View();

    [HttpGet]
    public IActionResult ResetPassword(string? email = null, string? token = null)
    {
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(token))
        {
            return RedirectToAction(nameof(ForgotPassword));
        }

        return View(new ResetPasswordViewModel { Email = email, Token = token });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var command = new ResetPasswordCommand(model.Email, model.Token, model.Password);
        var result = await _resetPasswordUseCase.ExecuteAsync(command, cancellationToken);

        if (result.IsFailure)
        {
            ModelState.AddModelError(string.Empty, result.Error!);
            return View(model);
        }

        return RedirectToAction(nameof(ResetPasswordConfirmation));
    }

    [HttpGet]
    public IActionResult ResetPasswordConfirmation() => View();

    [HttpGet]
    public IActionResult AccessDenied() => View();
}

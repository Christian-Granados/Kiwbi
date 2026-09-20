using Kiwbi.Application.Developers.GetBrandingForHousingUnit;
using Kiwbi.Application.Onboarding;
using Kiwbi.Application.Onboarding.AcceptBuyerInvitation;
using Kiwbi.Application.Onboarding.GetBuyerInvitationByToken;
using Kiwbi.Web.Models.Onboarding;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kiwbi.Web.Controllers;

/// <summary>Anonymous entry point for buyers accepting a Magic Link invitation (Feature 4.2). Trusts only the token in the URL.</summary>
[AllowAnonymous]
public class OnboardingController : Controller
{
    private readonly GetBuyerInvitationByTokenUseCase _getBuyerInvitationByTokenUseCase;
    private readonly AcceptBuyerInvitationUseCase _acceptBuyerInvitationUseCase;
    private readonly GetBrandingForHousingUnitUseCase _getBrandingForHousingUnitUseCase;

    public OnboardingController(
        GetBuyerInvitationByTokenUseCase getBuyerInvitationByTokenUseCase,
        AcceptBuyerInvitationUseCase acceptBuyerInvitationUseCase,
        GetBrandingForHousingUnitUseCase getBrandingForHousingUnitUseCase)
    {
        _getBuyerInvitationByTokenUseCase = getBuyerInvitationByTokenUseCase;
        _acceptBuyerInvitationUseCase = acceptBuyerInvitationUseCase;
        _getBrandingForHousingUnitUseCase = getBrandingForHousingUnitUseCase;
    }

    [HttpGet]
    public async Task<IActionResult> Accept(string token, CancellationToken cancellationToken)
    {
        var result = await _getBuyerInvitationByTokenUseCase.ExecuteAsync(token, cancellationToken);

        if (result.IsFailure)
        {
            return View("InvalidInvitation");
        }

        var model = ToViewModel(token, result.Value!);
        await ApplyInvitingCompanyBrandingAsync(model, result.Value!.HousingUnitId, cancellationToken);

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Accept(AcceptInvitationViewModel model, CancellationToken cancellationToken)
    {
        var invitationResult = await _getBuyerInvitationByTokenUseCase.ExecuteAsync(model.Token, cancellationToken);

        if (invitationResult.IsFailure)
        {
            return View("InvalidInvitation");
        }

        var refreshedModel = ToViewModel(model.Token, invitationResult.Value!);
        refreshedModel.Password = model.Password;
        refreshedModel.ConfirmPassword = model.ConfirmPassword;
        await ApplyInvitingCompanyBrandingAsync(refreshedModel, invitationResult.Value!.HousingUnitId, cancellationToken);

        if (!ModelState.IsValid)
        {
            return View(refreshedModel);
        }

        if (!refreshedModel.CanAccept)
        {
            return View(refreshedModel);
        }

        var command = new AcceptBuyerInvitationCommand(model.Token, model.Password);
        var acceptResult = await _acceptBuyerInvitationUseCase.ExecuteAsync(command, cancellationToken);

        if (acceptResult.IsFailure)
        {
            ModelState.AddModelError(string.Empty, acceptResult.Error!);
            return View(refreshedModel);
        }

        // The invitation/token no longer resolves anything useful once accepted, so the promotora's
        // brand is carried over via TempData for the Welcome screen instead of re-querying by token.
        TempData["WelcomeCompanyName"] = refreshedModel.CompanyName;
        TempData["WelcomeAccentColor"] = ViewBag.AuthAccentColor as string;

        return RedirectToAction(nameof(Welcome));
    }

    [HttpGet]
    public IActionResult Welcome()
    {
        ViewBag.CompanyName = TempData["WelcomeCompanyName"] as string;
        ViewBag.AuthAccentColor = TempData["WelcomeAccentColor"] as string;

        return View();
    }

    private static AcceptInvitationViewModel ToViewModel(string token, BuyerInvitationAcceptanceDto dto) => new()
    {
        Token = token,
        Email = dto.Email,
        Floor = dto.Floor,
        Door = dto.Door,
        HousingPromotionName = dto.HousingPromotionName,
        AccountAlreadyExists = dto.AccountAlreadyExists,
        CanAccept = dto.CanAccept,
        Status = dto.Status.ToString(),
        IsExpired = dto.IsExpired,
    };

    /// <summary>Resolves the inviting promotora's brand (Epic 9) to tint this anonymous screen, reusing the same use case already applied to the buyer's HousingUnit detail (Epic 8). Best-effort: leaves the model/ViewBag untouched if branding cannot be resolved.</summary>
    private async Task ApplyInvitingCompanyBrandingAsync(AcceptInvitationViewModel model, Guid housingUnitId, CancellationToken cancellationToken)
    {
        var brandingResult = await _getBrandingForHousingUnitUseCase.ExecuteAsync(housingUnitId, cancellationToken);

        if (brandingResult.IsFailure)
        {
            return;
        }

        model.CompanyName = brandingResult.Value!.Name;
        model.CompanyLogoPath = brandingResult.Value.LogoPath;
        ViewBag.AuthAccentColor = brandingResult.Value.PrimaryColor;
    }
}

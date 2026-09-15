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

    public OnboardingController(
        GetBuyerInvitationByTokenUseCase getBuyerInvitationByTokenUseCase,
        AcceptBuyerInvitationUseCase acceptBuyerInvitationUseCase)
    {
        _getBuyerInvitationByTokenUseCase = getBuyerInvitationByTokenUseCase;
        _acceptBuyerInvitationUseCase = acceptBuyerInvitationUseCase;
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

        return RedirectToAction(nameof(Welcome));
    }

    [HttpGet]
    public IActionResult Welcome() => View();

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
}

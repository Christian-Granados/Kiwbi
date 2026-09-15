using Kiwbi.Application.RealEstate.ChangeHousingUnitStatus;
using Kiwbi.Application.RealEstate.CreateHousingUnit;
using Kiwbi.Application.RealEstate.DeleteHousingUnit;
using Kiwbi.Application.RealEstate.GetHousingPromotion;
using Kiwbi.Application.RealEstate.GetHousingTypologies;
using Kiwbi.Application.RealEstate.GetHousingUnit;
using Kiwbi.Application.RealEstate.GetHousingUnits;
using Kiwbi.Application.RealEstate.UpdateHousingUnit;
using Kiwbi.Application.RealEstate.UpdateHousingUnitFloorPlan;
using Kiwbi.Application.Onboarding.InviteBuyerToHousingUnit;
using Kiwbi.Application.Onboarding.ResendBuyerInvitation;
using Kiwbi.Application.Onboarding.CancelBuyerInvitation;
using Kiwbi.Application.Onboarding.GetBuyerInvitationsForHousingUnit;
using Kiwbi.Application.Onboarding.GetHousingUnitBuyersForHousingUnit;
using Kiwbi.Domain.RealEstate;
using Kiwbi.Web.Models.HousingUnits;
using Kiwbi.Web.Models.Onboarding;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Kiwbi.Web.Controllers;

[Authorize(Roles = "DeveloperAdmin")]
public class HousingUnitsController : Controller
{
    private readonly CreateHousingUnitUseCase _createHousingUnitUseCase;
    private readonly UpdateHousingUnitUseCase _updateHousingUnitUseCase;
    private readonly UpdateHousingUnitFloorPlanUseCase _updateHousingUnitFloorPlanUseCase;
    private readonly ChangeHousingUnitStatusUseCase _changeHousingUnitStatusUseCase;
    private readonly DeleteHousingUnitUseCase _deleteHousingUnitUseCase;
    private readonly GetHousingUnitUseCase _getHousingUnitUseCase;
    private readonly GetHousingUnitsUseCase _getHousingUnitsUseCase;
    private readonly GetHousingTypologiesUseCase _getHousingTypologiesUseCase;
    private readonly GetHousingPromotionUseCase _getHousingPromotionUseCase;
    private readonly InviteBuyerToHousingUnitUseCase _inviteBuyerToHousingUnitUseCase;
    private readonly ResendBuyerInvitationUseCase _resendBuyerInvitationUseCase;
    private readonly CancelBuyerInvitationUseCase _cancelBuyerInvitationUseCase;
    private readonly GetBuyerInvitationsForHousingUnitUseCase _getBuyerInvitationsForHousingUnitUseCase;
    private readonly GetHousingUnitBuyersForHousingUnitUseCase _getHousingUnitBuyersForHousingUnitUseCase;

    public HousingUnitsController(
        CreateHousingUnitUseCase createHousingUnitUseCase,
        UpdateHousingUnitUseCase updateHousingUnitUseCase,
        UpdateHousingUnitFloorPlanUseCase updateHousingUnitFloorPlanUseCase,
        ChangeHousingUnitStatusUseCase changeHousingUnitStatusUseCase,
        DeleteHousingUnitUseCase deleteHousingUnitUseCase,
        GetHousingUnitUseCase getHousingUnitUseCase,
        GetHousingUnitsUseCase getHousingUnitsUseCase,
        GetHousingTypologiesUseCase getHousingTypologiesUseCase,
        GetHousingPromotionUseCase getHousingPromotionUseCase,
        InviteBuyerToHousingUnitUseCase inviteBuyerToHousingUnitUseCase,
        ResendBuyerInvitationUseCase resendBuyerInvitationUseCase,
        CancelBuyerInvitationUseCase cancelBuyerInvitationUseCase,
        GetBuyerInvitationsForHousingUnitUseCase getBuyerInvitationsForHousingUnitUseCase,
        GetHousingUnitBuyersForHousingUnitUseCase getHousingUnitBuyersForHousingUnitUseCase)
    {
        _createHousingUnitUseCase = createHousingUnitUseCase;
        _updateHousingUnitUseCase = updateHousingUnitUseCase;
        _updateHousingUnitFloorPlanUseCase = updateHousingUnitFloorPlanUseCase;
        _changeHousingUnitStatusUseCase = changeHousingUnitStatusUseCase;
        _deleteHousingUnitUseCase = deleteHousingUnitUseCase;
        _getHousingUnitUseCase = getHousingUnitUseCase;
        _getHousingUnitsUseCase = getHousingUnitsUseCase;
        _getHousingTypologiesUseCase = getHousingTypologiesUseCase;
        _getHousingPromotionUseCase = getHousingPromotionUseCase;
        _inviteBuyerToHousingUnitUseCase = inviteBuyerToHousingUnitUseCase;
        _resendBuyerInvitationUseCase = resendBuyerInvitationUseCase;
        _cancelBuyerInvitationUseCase = cancelBuyerInvitationUseCase;
        _getBuyerInvitationsForHousingUnitUseCase = getBuyerInvitationsForHousingUnitUseCase;
        _getHousingUnitBuyersForHousingUnitUseCase = getHousingUnitBuyersForHousingUnitUseCase;
    }

    [HttpGet]
    public async Task<IActionResult> Index(Guid promotionId, CancellationToken cancellationToken)
    {
        var promotionResult = await _getHousingPromotionUseCase.ExecuteAsync(promotionId, cancellationToken);

        if (promotionResult.IsFailure)
        {
            return NotFound();
        }

        var unitsResult = await _getHousingUnitsUseCase.ExecuteAsync(promotionId, cancellationToken);
        var typologiesResult = await _getHousingTypologiesUseCase.ExecuteAsync(promotionId, cancellationToken);

        if (unitsResult.IsFailure || typologiesResult.IsFailure)
        {
            return Forbid();
        }

        var typologyNames = typologiesResult.Value!.ToDictionary(t => t.Id, t => t.Name);

        var units = unitsResult.Value!
            .Select(u => HousingUnitListItemViewModel.FromDto(u, u.HousingTypologyId is { } id ? typologyNames.GetValueOrDefault(id) : null))
            .ToList();

        ViewBag.PromotionId = promotionId;
        ViewBag.PromotionName = promotionResult.Value!.Name;

        return View(units);
    }

    [HttpGet]
    public async Task<IActionResult> Create(Guid promotionId, CancellationToken cancellationToken)
    {
        var model = new CreateHousingUnitViewModel
        {
            HousingPromotionId = promotionId,
            TypologyOptions = await BuildTypologyOptionsAsync(promotionId, cancellationToken),
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateHousingUnitViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            model.TypologyOptions = await BuildTypologyOptionsAsync(model.HousingPromotionId, cancellationToken);
            return View(model);
        }

        var command = new CreateHousingUnitCommand(
            model.HousingPromotionId, model.HousingTypologyId, model.Floor, model.Door, model.BuiltAreaSqm, model.UsableAreaSqm);
        var result = await _createHousingUnitUseCase.ExecuteAsync(command, cancellationToken);

        if (result.IsFailure)
        {
            ModelState.AddModelError(string.Empty, result.Error!);
            model.TypologyOptions = await BuildTypologyOptionsAsync(model.HousingPromotionId, cancellationToken);
            return View(model);
        }

        if (model.FloorPlanImageFile is { Length: > 0 } file)
        {
            await using var stream = file.OpenReadStream();
            await _updateHousingUnitFloorPlanUseCase.ExecuteAsync(
                new UpdateHousingUnitFloorPlanCommand(result.Value, stream, file.FileName), cancellationToken);
        }

        return RedirectToAction(nameof(Index), new { promotionId = model.HousingPromotionId });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        var result = await _getHousingUnitUseCase.ExecuteAsync(id, cancellationToken);

        if (result.IsFailure)
        {
            return NotFound();
        }

        var dto = result.Value!;

        var model = new EditHousingUnitViewModel
        {
            Id = dto.Id,
            HousingPromotionId = dto.HousingPromotionId,
            HousingTypologyId = dto.HousingTypologyId,
            Floor = dto.Floor,
            Door = dto.Door,
            BuiltAreaSqm = dto.BuiltAreaSqm,
            UsableAreaSqm = dto.UsableAreaSqm,
            FloorPlanImagePath = dto.FloorPlanImagePath,
            TypologyOptions = await BuildTypologyOptionsAsync(dto.HousingPromotionId, cancellationToken),
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditHousingUnitViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            model.TypologyOptions = await BuildTypologyOptionsAsync(model.HousingPromotionId, cancellationToken);
            return View(model);
        }

        var command = new UpdateHousingUnitCommand(
            model.Id, model.HousingTypologyId, model.Floor, model.Door, model.BuiltAreaSqm, model.UsableAreaSqm);
        var result = await _updateHousingUnitUseCase.ExecuteAsync(command, cancellationToken);

        if (result.IsFailure)
        {
            ModelState.AddModelError(string.Empty, result.Error!);
            model.TypologyOptions = await BuildTypologyOptionsAsync(model.HousingPromotionId, cancellationToken);
            return View(model);
        }

        if (model.FloorPlanImageFile is { Length: > 0 } file)
        {
            await using var stream = file.OpenReadStream();
            var floorPlanResult = await _updateHousingUnitFloorPlanUseCase.ExecuteAsync(
                new UpdateHousingUnitFloorPlanCommand(model.Id, stream, file.FileName), cancellationToken);

            if (floorPlanResult.IsFailure)
            {
                ModelState.AddModelError(string.Empty, floorPlanResult.Error!);
                model.TypologyOptions = await BuildTypologyOptionsAsync(model.HousingPromotionId, cancellationToken);
                return View(model);
            }
        }

        return RedirectToAction(nameof(Index), new { promotionId = model.HousingPromotionId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeStatus(Guid id, Guid promotionId, HousingUnitStatus status, CancellationToken cancellationToken)
    {
        var result = await _changeHousingUnitStatusUseCase.ExecuteAsync(new ChangeHousingUnitStatusCommand(id, status), cancellationToken);

        if (result.IsFailure)
        {
            TempData["Error"] = result.Error;
        }

        return RedirectToAction(nameof(Index), new { promotionId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id, Guid promotionId, CancellationToken cancellationToken)
    {
        var result = await _deleteHousingUnitUseCase.ExecuteAsync(id, cancellationToken);

        if (result.IsFailure)
        {
            TempData["Error"] = result.Error;
        }

        return RedirectToAction(nameof(Index), new { promotionId });
    }

    [HttpGet]
    public async Task<IActionResult> Invitations(Guid id, CancellationToken cancellationToken)
    {
        var unitResult = await _getHousingUnitUseCase.ExecuteAsync(id, cancellationToken);

        if (unitResult.IsFailure)
        {
            return NotFound();
        }

        var invitationsResult = await _getBuyerInvitationsForHousingUnitUseCase.ExecuteAsync(id, cancellationToken);
        var buyersResult = await _getHousingUnitBuyersForHousingUnitUseCase.ExecuteAsync(id, cancellationToken);

        if (invitationsResult.IsFailure || buyersResult.IsFailure)
        {
            return Forbid();
        }

        ViewBag.HousingUnitId = id;
        ViewBag.PromotionId = unitResult.Value!.HousingPromotionId;
        ViewBag.UnitLabel = $"{unitResult.Value.Floor} {unitResult.Value.Door}";
        ViewBag.Buyers = buyersResult.Value!.Select(HousingUnitBuyerListItemViewModel.FromDto).ToList();

        var items = invitationsResult.Value!.Select(BuyerInvitationListItemViewModel.FromDto).ToList();

        return View(items);
    }

    [HttpGet]
    public async Task<IActionResult> InviteBuyer(Guid id, CancellationToken cancellationToken)
    {
        var unitResult = await _getHousingUnitUseCase.ExecuteAsync(id, cancellationToken);

        if (unitResult.IsFailure)
        {
            return NotFound();
        }

        ViewBag.UnitLabel = $"{unitResult.Value!.Floor} {unitResult.Value.Door}";

        return View(new InviteBuyerViewModel { HousingUnitId = id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> InviteBuyer(InviteBuyerViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var command = new InviteBuyerToHousingUnitCommand(model.HousingUnitId, model.Email);
        var result = await _inviteBuyerToHousingUnitUseCase.ExecuteAsync(command, cancellationToken);

        if (result.IsFailure)
        {
            ModelState.AddModelError(string.Empty, result.Error!);
            return View(model);
        }

        return RedirectToAction(nameof(Invitations), new { id = model.HousingUnitId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResendInvitation(Guid invitationId, Guid id, CancellationToken cancellationToken)
    {
        var result = await _resendBuyerInvitationUseCase.ExecuteAsync(invitationId, cancellationToken);

        if (result.IsFailure)
        {
            TempData["Error"] = result.Error;
        }

        return RedirectToAction(nameof(Invitations), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CancelInvitation(Guid invitationId, Guid id, CancellationToken cancellationToken)
    {
        var result = await _cancelBuyerInvitationUseCase.ExecuteAsync(invitationId, cancellationToken);

        if (result.IsFailure)
        {
            TempData["Error"] = result.Error;
        }

        return RedirectToAction(nameof(Invitations), new { id });
    }

    private async Task<List<SelectListItem>> BuildTypologyOptionsAsync(Guid promotionId, CancellationToken cancellationToken)
    {
        var result = await _getHousingTypologiesUseCase.ExecuteAsync(promotionId, cancellationToken);

        var options = new List<SelectListItem> { new("(Sin tipología)", string.Empty) };

        if (result.IsSuccess)
        {
            options.AddRange(result.Value!.Select(t => new SelectListItem(t.Name, t.Id.ToString())));
        }

        return options;
    }
}

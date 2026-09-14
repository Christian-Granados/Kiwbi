using Kiwbi.Application.RealEstate.CreateHousingPromotion;
using Kiwbi.Application.RealEstate.DeleteHousingPromotion;
using Kiwbi.Application.RealEstate.GetHousingPromotion;
using Kiwbi.Application.RealEstate.GetHousingPromotions;
using Kiwbi.Application.RealEstate.GetHousingPromotionSummary;
using Kiwbi.Application.RealEstate.UpdateHousingPromotion;
using Kiwbi.Application.RealEstate.UpdateHousingPromotionMasterPlan;
using Kiwbi.Web.Models.HousingPromotions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kiwbi.Web.Controllers;

[Authorize(Roles = "DeveloperAdmin")]
public class HousingPromotionsController : Controller
{
    private readonly CreateHousingPromotionUseCase _createHousingPromotionUseCase;
    private readonly UpdateHousingPromotionUseCase _updateHousingPromotionUseCase;
    private readonly UpdateHousingPromotionMasterPlanUseCase _updateHousingPromotionMasterPlanUseCase;
    private readonly GetHousingPromotionUseCase _getHousingPromotionUseCase;
    private readonly GetHousingPromotionsUseCase _getHousingPromotionsUseCase;
    private readonly GetHousingPromotionSummaryUseCase _getHousingPromotionSummaryUseCase;
    private readonly DeleteHousingPromotionUseCase _deleteHousingPromotionUseCase;

    public HousingPromotionsController(
        CreateHousingPromotionUseCase createHousingPromotionUseCase,
        UpdateHousingPromotionUseCase updateHousingPromotionUseCase,
        UpdateHousingPromotionMasterPlanUseCase updateHousingPromotionMasterPlanUseCase,
        GetHousingPromotionUseCase getHousingPromotionUseCase,
        GetHousingPromotionsUseCase getHousingPromotionsUseCase,
        GetHousingPromotionSummaryUseCase getHousingPromotionSummaryUseCase,
        DeleteHousingPromotionUseCase deleteHousingPromotionUseCase)
    {
        _createHousingPromotionUseCase = createHousingPromotionUseCase;
        _updateHousingPromotionUseCase = updateHousingPromotionUseCase;
        _updateHousingPromotionMasterPlanUseCase = updateHousingPromotionMasterPlanUseCase;
        _getHousingPromotionUseCase = getHousingPromotionUseCase;
        _getHousingPromotionsUseCase = getHousingPromotionsUseCase;
        _getHousingPromotionSummaryUseCase = getHousingPromotionSummaryUseCase;
        _deleteHousingPromotionUseCase = deleteHousingPromotionUseCase;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var result = await _getHousingPromotionsUseCase.ExecuteAsync(cancellationToken);

        if (result.IsFailure)
        {
            return Forbid();
        }

        var promotions = result.Value!.Select(HousingPromotionListItemViewModel.FromDto).ToList();

        return View(promotions);
    }

    [HttpGet]
    public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
    {
        var result = await _getHousingPromotionSummaryUseCase.ExecuteAsync(id, cancellationToken);

        if (result.IsFailure)
        {
            return NotFound();
        }

        return View(HousingPromotionViewModel.FromSummaryDto(result.Value!));
    }

    [HttpGet]
    public IActionResult Create() => View(new CreateHousingPromotionViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateHousingPromotionViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var command = new CreateHousingPromotionCommand(model.Name, model.City, model.Address);
        var result = await _createHousingPromotionUseCase.ExecuteAsync(command, cancellationToken);

        if (result.IsFailure)
        {
            ModelState.AddModelError(string.Empty, result.Error!);
            return View(model);
        }

        if (model.MasterPlanImageFile is { Length: > 0 } file)
        {
            await using var stream = file.OpenReadStream();
            var masterPlanCommand = new UpdateHousingPromotionMasterPlanCommand(result.Value, stream, file.FileName);
            var masterPlanResult = await _updateHousingPromotionMasterPlanUseCase.ExecuteAsync(masterPlanCommand, cancellationToken);

            if (masterPlanResult.IsFailure)
            {
                ModelState.AddModelError(string.Empty, masterPlanResult.Error!);
                return View(model);
            }
        }

        return RedirectToAction(nameof(Details), new { id = result.Value });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        var result = await _getHousingPromotionUseCase.ExecuteAsync(id, cancellationToken);

        if (result.IsFailure)
        {
            return NotFound();
        }

        var dto = result.Value!;

        return View(new EditHousingPromotionViewModel
        {
            Id = dto.Id,
            Name = dto.Name,
            City = dto.City,
            Address = dto.Address,
            MasterPlanImagePath = dto.MasterPlanImagePath,
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditHousingPromotionViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var command = new UpdateHousingPromotionCommand(model.Id, model.Name, model.City, model.Address);
        var result = await _updateHousingPromotionUseCase.ExecuteAsync(command, cancellationToken);

        if (result.IsFailure)
        {
            ModelState.AddModelError(string.Empty, result.Error!);
            return View(model);
        }

        if (model.MasterPlanImageFile is { Length: > 0 } file)
        {
            await using var stream = file.OpenReadStream();
            var masterPlanCommand = new UpdateHousingPromotionMasterPlanCommand(model.Id, stream, file.FileName);
            var masterPlanResult = await _updateHousingPromotionMasterPlanUseCase.ExecuteAsync(masterPlanCommand, cancellationToken);

            if (masterPlanResult.IsFailure)
            {
                ModelState.AddModelError(string.Empty, masterPlanResult.Error!);
                return View(model);
            }
        }

        return RedirectToAction(nameof(Details), new { id = model.Id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _deleteHousingPromotionUseCase.ExecuteAsync(id, cancellationToken);

        if (result.IsFailure)
        {
            TempData["Error"] = result.Error;
            return RedirectToAction(nameof(Details), new { id });
        }

        return RedirectToAction(nameof(Index));
    }
}

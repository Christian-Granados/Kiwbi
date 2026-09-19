using Kiwbi.Application.RealEstate.CreateHousingTypology;
using Kiwbi.Application.RealEstate.DeleteHousingTypology;
using Kiwbi.Application.RealEstate.GetHousingPromotion;
using Kiwbi.Application.RealEstate.GetHousingTypologies;
using Kiwbi.Application.RealEstate.GetHousingTypology;
using Kiwbi.Application.RealEstate.UpdateHousingTypology;
using Kiwbi.Web.Models.HousingTypologies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kiwbi.Web.Controllers;

[Authorize(Roles = "DeveloperAdmin")]
public class HousingTypologiesController : Controller
{
    private readonly CreateHousingTypologyUseCase _createHousingTypologyUseCase;
    private readonly UpdateHousingTypologyUseCase _updateHousingTypologyUseCase;
    private readonly DeleteHousingTypologyUseCase _deleteHousingTypologyUseCase;
    private readonly GetHousingTypologyUseCase _getHousingTypologyUseCase;
    private readonly GetHousingTypologiesUseCase _getHousingTypologiesUseCase;
    private readonly GetHousingPromotionUseCase _getHousingPromotionUseCase;

    public HousingTypologiesController(
        CreateHousingTypologyUseCase createHousingTypologyUseCase,
        UpdateHousingTypologyUseCase updateHousingTypologyUseCase,
        DeleteHousingTypologyUseCase deleteHousingTypologyUseCase,
        GetHousingTypologyUseCase getHousingTypologyUseCase,
        GetHousingTypologiesUseCase getHousingTypologiesUseCase,
        GetHousingPromotionUseCase getHousingPromotionUseCase)
    {
        _createHousingTypologyUseCase = createHousingTypologyUseCase;
        _updateHousingTypologyUseCase = updateHousingTypologyUseCase;
        _deleteHousingTypologyUseCase = deleteHousingTypologyUseCase;
        _getHousingTypologyUseCase = getHousingTypologyUseCase;
        _getHousingTypologiesUseCase = getHousingTypologiesUseCase;
        _getHousingPromotionUseCase = getHousingPromotionUseCase;
    }

    [HttpGet]
    public async Task<IActionResult> Index(Guid promotionId, CancellationToken cancellationToken)
    {
        var promotionResult = await _getHousingPromotionUseCase.ExecuteAsync(promotionId, cancellationToken);

        if (promotionResult.IsFailure)
        {
            return NotFound();
        }

        var result = await _getHousingTypologiesUseCase.ExecuteAsync(promotionId, cancellationToken);

        if (result.IsFailure)
        {
            return Forbid();
        }

        ViewBag.PromotionId = promotionId;
        ViewBag.PromotionName = promotionResult.Value!.Name;

        return View(result.Value!.Select(HousingTypologyListItemViewModel.FromDto).ToList());
    }

    [HttpGet]
    public IActionResult Create(Guid promotionId)
    {
        ViewBag.PromotionId = promotionId;
        return View(new CreateHousingTypologyViewModel { HousingPromotionId = promotionId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateHousingTypologyViewModel model, CancellationToken cancellationToken)
    {
        ViewBag.PromotionId = model.HousingPromotionId;

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var command = new CreateHousingTypologyCommand(model.HousingPromotionId, model.Name);
        var result = await _createHousingTypologyUseCase.ExecuteAsync(command, cancellationToken);

        if (result.IsFailure)
        {
            ModelState.AddModelError(string.Empty, result.Error!);
            return View(model);
        }

        return RedirectToAction(nameof(Index), new { promotionId = model.HousingPromotionId });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        var result = await _getHousingTypologyUseCase.ExecuteAsync(id, cancellationToken);

        if (result.IsFailure)
        {
            return NotFound();
        }

        var dto = result.Value!;
        ViewBag.PromotionId = dto.HousingPromotionId;

        return View(new EditHousingTypologyViewModel { Id = dto.Id, HousingPromotionId = dto.HousingPromotionId, Name = dto.Name });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditHousingTypologyViewModel model, CancellationToken cancellationToken)
    {
        ViewBag.PromotionId = model.HousingPromotionId;

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var command = new UpdateHousingTypologyCommand(model.Id, model.Name);
        var result = await _updateHousingTypologyUseCase.ExecuteAsync(command, cancellationToken);

        if (result.IsFailure)
        {
            ModelState.AddModelError(string.Empty, result.Error!);
            return View(model);
        }

        return RedirectToAction(nameof(Index), new { promotionId = model.HousingPromotionId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id, Guid promotionId, CancellationToken cancellationToken)
    {
        var result = await _deleteHousingTypologyUseCase.ExecuteAsync(id, cancellationToken);

        if (result.IsFailure)
        {
            TempData["Error"] = result.Error;
        }

        return RedirectToAction(nameof(Index), new { promotionId });
    }
}

using Kiwbi.Application.Choices.ConfirmHomeCustomizationChoice;
using Kiwbi.Application.Choices.GetHousingPromotionChoicesProgress;
using Kiwbi.Application.Choices.GetHousingUnitChoicesDetail;
using Kiwbi.Application.Choices.MarkHomeCustomizationChoiceAsPaid;
using Kiwbi.Application.RealEstate.GetHousingPromotion;
using Kiwbi.Application.RealEstate.GetHousingUnit;
using Kiwbi.Web.Models.HousingPromotionChoices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kiwbi.Web.Controllers;

/// <summary>Promotora-facing panel over buyers' Customization choices for a HousingPromotion (Feature 6.1/6.2).</summary>
[Authorize(Roles = "DeveloperAdmin")]
public class HousingPromotionChoicesController : Controller
{
    private readonly GetHousingPromotionChoicesProgressUseCase _getHousingPromotionChoicesProgressUseCase;
    private readonly GetHousingUnitChoicesDetailUseCase _getHousingUnitChoicesDetailUseCase;
    private readonly ConfirmHomeCustomizationChoiceUseCase _confirmHomeCustomizationChoiceUseCase;
    private readonly MarkHomeCustomizationChoiceAsPaidUseCase _markHomeCustomizationChoiceAsPaidUseCase;
    private readonly GetHousingPromotionUseCase _getHousingPromotionUseCase;
    private readonly GetHousingUnitUseCase _getHousingUnitUseCase;

    public HousingPromotionChoicesController(
        GetHousingPromotionChoicesProgressUseCase getHousingPromotionChoicesProgressUseCase,
        GetHousingUnitChoicesDetailUseCase getHousingUnitChoicesDetailUseCase,
        ConfirmHomeCustomizationChoiceUseCase confirmHomeCustomizationChoiceUseCase,
        MarkHomeCustomizationChoiceAsPaidUseCase markHomeCustomizationChoiceAsPaidUseCase,
        GetHousingPromotionUseCase getHousingPromotionUseCase,
        GetHousingUnitUseCase getHousingUnitUseCase)
    {
        _getHousingPromotionChoicesProgressUseCase = getHousingPromotionChoicesProgressUseCase;
        _getHousingUnitChoicesDetailUseCase = getHousingUnitChoicesDetailUseCase;
        _confirmHomeCustomizationChoiceUseCase = confirmHomeCustomizationChoiceUseCase;
        _markHomeCustomizationChoiceAsPaidUseCase = markHomeCustomizationChoiceAsPaidUseCase;
        _getHousingPromotionUseCase = getHousingPromotionUseCase;
        _getHousingUnitUseCase = getHousingUnitUseCase;
    }

    [HttpGet]
    public async Task<IActionResult> Index(Guid promotionId, CancellationToken cancellationToken)
    {
        var promotionResult = await _getHousingPromotionUseCase.ExecuteAsync(promotionId, cancellationToken);

        if (promotionResult.IsFailure)
        {
            return NotFound();
        }

        var result = await _getHousingPromotionChoicesProgressUseCase.ExecuteAsync(promotionId, cancellationToken);

        if (result.IsFailure)
        {
            return Forbid();
        }

        ViewBag.PromotionId = promotionId;
        ViewBag.PromotionName = promotionResult.Value!.Name;

        var units = result.Value!.Units.Select(HousingUnitChoicesProgressViewModel.FromDto).ToList();

        return View(units);
    }

    [HttpGet]
    public async Task<IActionResult> Details(Guid housingUnitId, CancellationToken cancellationToken)
    {
        var unitResult = await _getHousingUnitUseCase.ExecuteAsync(housingUnitId, cancellationToken);

        if (unitResult.IsFailure)
        {
            return NotFound();
        }

        var result = await _getHousingUnitChoicesDetailUseCase.ExecuteAsync(housingUnitId, cancellationToken);

        if (result.IsFailure)
        {
            return Forbid();
        }

        var unit = unitResult.Value!;
        ViewBag.PromotionId = unit.HousingPromotionId;
        ViewBag.HousingUnitId = unit.Id;
        ViewBag.HousingUnitLabel = $"Planta {unit.Floor}, puerta {unit.Door}";

        var tradeCategories = result.Value!.Select(TradeCategoryChoicesViewModel.FromDto).ToList();

        return View(tradeCategories);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Confirm(Guid housingUnitId, Guid customizationId, CancellationToken cancellationToken)
    {
        var command = new ConfirmHomeCustomizationChoiceCommand(housingUnitId, customizationId);
        var result = await _confirmHomeCustomizationChoiceUseCase.ExecuteAsync(command, cancellationToken);

        if (result.IsFailure)
        {
            TempData["Error"] = result.Error;
        }

        return RedirectToAction(nameof(Details), new { housingUnitId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAsPaid(Guid housingUnitId, Guid customizationId, CancellationToken cancellationToken)
    {
        var command = new MarkHomeCustomizationChoiceAsPaidCommand(housingUnitId, customizationId);
        var result = await _markHomeCustomizationChoiceAsPaidUseCase.ExecuteAsync(command, cancellationToken);

        if (result.IsFailure)
        {
            TempData["Error"] = result.Error;
        }

        return RedirectToAction(nameof(Details), new { housingUnitId });
    }
}

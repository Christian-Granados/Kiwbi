using Kiwbi.Application.Choices.GetHousingPromotionChoicesProgress;
using Kiwbi.Application.Choices.GetHousingUnitChoicesDetail;
using Kiwbi.Application.RealEstate.GetHousingPromotion;
using Kiwbi.Application.RealEstate.GetHousingUnit;
using Kiwbi.Web.Models.HousingPromotionChoices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kiwbi.Web.Controllers;

/// <summary>Promotora-facing panel over buyers' Customization choices for a HousingPromotion (Feature 6.1).</summary>
[Authorize(Roles = "DeveloperAdmin")]
public class HousingPromotionChoicesController : Controller
{
    private readonly GetHousingPromotionChoicesProgressUseCase _getHousingPromotionChoicesProgressUseCase;
    private readonly GetHousingUnitChoicesDetailUseCase _getHousingUnitChoicesDetailUseCase;
    private readonly GetHousingPromotionUseCase _getHousingPromotionUseCase;
    private readonly GetHousingUnitUseCase _getHousingUnitUseCase;

    public HousingPromotionChoicesController(
        GetHousingPromotionChoicesProgressUseCase getHousingPromotionChoicesProgressUseCase,
        GetHousingUnitChoicesDetailUseCase getHousingUnitChoicesDetailUseCase,
        GetHousingPromotionUseCase getHousingPromotionUseCase,
        GetHousingUnitUseCase getHousingUnitUseCase)
    {
        _getHousingPromotionChoicesProgressUseCase = getHousingPromotionChoicesProgressUseCase;
        _getHousingUnitChoicesDetailUseCase = getHousingUnitChoicesDetailUseCase;
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
        ViewBag.HousingUnitLabel = $"Planta {unit.Floor}, puerta {unit.Door}";

        var tradeCategories = result.Value!.Select(TradeCategoryChoicesViewModel.FromDto).ToList();

        return View(tradeCategories);
    }
}

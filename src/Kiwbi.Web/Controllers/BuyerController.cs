using Kiwbi.Application.Choices.GetHousingUnitCustomizationsForBuyer;
using Kiwbi.Application.Onboarding.GetHousingUnitsForCurrentBuyer;
using Kiwbi.Web.Models.Buyer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kiwbi.Web.Controllers;

/// <summary>Buyer-facing area (Epic 5): dashboard and customization visualizer, distinct from the promotora controllers.</summary>
[Authorize(Roles = "Buyer")]
public class BuyerController : Controller
{
    private readonly GetHousingUnitsForCurrentBuyerUseCase _getHousingUnitsForCurrentBuyerUseCase;
    private readonly GetHousingUnitCustomizationsForBuyerUseCase _getHousingUnitCustomizationsForBuyerUseCase;

    public BuyerController(
        GetHousingUnitsForCurrentBuyerUseCase getHousingUnitsForCurrentBuyerUseCase,
        GetHousingUnitCustomizationsForBuyerUseCase getHousingUnitCustomizationsForBuyerUseCase)
    {
        _getHousingUnitsForCurrentBuyerUseCase = getHousingUnitsForCurrentBuyerUseCase;
        _getHousingUnitCustomizationsForBuyerUseCase = getHousingUnitCustomizationsForBuyerUseCase;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var result = await _getHousingUnitsForCurrentBuyerUseCase.ExecuteAsync(cancellationToken);

        if (result.IsFailure)
        {
            return Forbid();
        }

        var units = result.Value!.Select(BuyerHousingUnitViewModel.FromDto).ToList();

        return View(units);
    }

    [HttpGet]
    public async Task<IActionResult> HousingUnit(Guid id, CancellationToken cancellationToken)
    {
        var result = await _getHousingUnitCustomizationsForBuyerUseCase.ExecuteAsync(id, cancellationToken);

        if (result.IsFailure)
        {
            return NotFound();
        }

        ViewData["HousingUnitId"] = id;
        var tradeCategories = result.Value!.Select(TradeCategoryCustomizationsViewModel.FromDto).ToList();

        return View(tradeCategories);
    }
}


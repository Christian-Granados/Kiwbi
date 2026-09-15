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

    public BuyerController(GetHousingUnitsForCurrentBuyerUseCase getHousingUnitsForCurrentBuyerUseCase)
    {
        _getHousingUnitsForCurrentBuyerUseCase = getHousingUnitsForCurrentBuyerUseCase;
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
}

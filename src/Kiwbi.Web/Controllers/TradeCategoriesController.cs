using Kiwbi.Application.Customizations.CreateTradeCategory;
using Kiwbi.Application.Customizations.DeleteTradeCategory;
using Kiwbi.Application.Customizations.GetTradeCategories;
using Kiwbi.Application.Customizations.GetTradeCategory;
using Kiwbi.Application.Customizations.UpdateTradeCategory;
using Kiwbi.Application.RealEstate.GetHousingPromotion;
using Kiwbi.Web.Models.TradeCategories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kiwbi.Web.Controllers;

[Authorize(Roles = "DeveloperAdmin")]
public class TradeCategoriesController : Controller
{
    private readonly CreateTradeCategoryUseCase _createTradeCategoryUseCase;
    private readonly UpdateTradeCategoryUseCase _updateTradeCategoryUseCase;
    private readonly DeleteTradeCategoryUseCase _deleteTradeCategoryUseCase;
    private readonly GetTradeCategoryUseCase _getTradeCategoryUseCase;
    private readonly GetTradeCategoriesUseCase _getTradeCategoriesUseCase;
    private readonly GetHousingPromotionUseCase _getHousingPromotionUseCase;

    public TradeCategoriesController(
        CreateTradeCategoryUseCase createTradeCategoryUseCase,
        UpdateTradeCategoryUseCase updateTradeCategoryUseCase,
        DeleteTradeCategoryUseCase deleteTradeCategoryUseCase,
        GetTradeCategoryUseCase getTradeCategoryUseCase,
        GetTradeCategoriesUseCase getTradeCategoriesUseCase,
        GetHousingPromotionUseCase getHousingPromotionUseCase)
    {
        _createTradeCategoryUseCase = createTradeCategoryUseCase;
        _updateTradeCategoryUseCase = updateTradeCategoryUseCase;
        _deleteTradeCategoryUseCase = deleteTradeCategoryUseCase;
        _getTradeCategoryUseCase = getTradeCategoryUseCase;
        _getTradeCategoriesUseCase = getTradeCategoriesUseCase;
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

        var result = await _getTradeCategoriesUseCase.ExecuteAsync(promotionId, cancellationToken);

        if (result.IsFailure)
        {
            return Forbid();
        }

        ViewBag.PromotionId = promotionId;
        ViewBag.PromotionName = promotionResult.Value!.Name;

        return View(result.Value!.Select(TradeCategoryListItemViewModel.FromDto).ToList());
    }

    [HttpGet]
    public IActionResult Create(Guid promotionId) => View(new CreateTradeCategoryViewModel { HousingPromotionId = promotionId });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateTradeCategoryViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var command = new CreateTradeCategoryCommand(model.HousingPromotionId, model.Name, model.SelectionCutOffDateUtc!.Value);
        var result = await _createTradeCategoryUseCase.ExecuteAsync(command, cancellationToken);

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
        var result = await _getTradeCategoryUseCase.ExecuteAsync(id, cancellationToken);

        if (result.IsFailure)
        {
            return NotFound();
        }

        var dto = result.Value!;

        return View(new EditTradeCategoryViewModel
        {
            Id = dto.Id,
            HousingPromotionId = dto.HousingPromotionId,
            Name = dto.Name,
            SelectionCutOffDateUtc = dto.SelectionCutOffDateUtc,
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditTradeCategoryViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var command = new UpdateTradeCategoryCommand(model.Id, model.Name, model.SelectionCutOffDateUtc!.Value);
        var result = await _updateTradeCategoryUseCase.ExecuteAsync(command, cancellationToken);

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
        var result = await _deleteTradeCategoryUseCase.ExecuteAsync(id, cancellationToken);

        if (result.IsFailure)
        {
            TempData["Error"] = result.Error;
        }

        return RedirectToAction(nameof(Index), new { promotionId });
    }
}

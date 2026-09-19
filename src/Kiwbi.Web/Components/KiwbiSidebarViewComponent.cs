using Kiwbi.Application.Developers.GetCurrentDeveloperProfile;
using Kiwbi.Application.RealEstate.GetHousingPromotion;
using Microsoft.AspNetCore.Mvc;

namespace Kiwbi.Web.Components;

/// <summary>Renders the Promotora sidebar (Feature 8.2) in one of two modes: GLOBAL (outside any Promoción context,
/// e.g. the Promociones listing or Mi promotora) or CONTEXTUAL (inside a specific Promoción). The mode is decided
/// from `ViewBag.PromotionId`/`ViewBag.PromotionName`, already set by the Controllers that operate within a
/// Promoción (TradeCategories/HousingUnits/HousingTypologies/Customizations/HousingPromotionChoices); if only the
/// id is present the promotion name is resolved here as a fallback, so a missing `PromotionName` never breaks the
/// contextual indicator.</summary>
public class KiwbiSidebarViewComponent : ViewComponent
{
    private readonly GetCurrentDeveloperProfileUseCase _getCurrentDeveloperProfileUseCase;
    private readonly GetHousingPromotionUseCase _getHousingPromotionUseCase;

    public KiwbiSidebarViewComponent(
        GetCurrentDeveloperProfileUseCase getCurrentDeveloperProfileUseCase,
        GetHousingPromotionUseCase getHousingPromotionUseCase)
    {
        _getCurrentDeveloperProfileUseCase = getCurrentDeveloperProfileUseCase;
        _getHousingPromotionUseCase = getHousingPromotionUseCase;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var model = new KiwbiSidebarViewModel
        {
            ActiveController = ViewContext.RouteData.Values["controller"] as string ?? string.Empty,
        };

        var profileResult = await _getCurrentDeveloperProfileUseCase.ExecuteAsync(HttpContext.RequestAborted);

        if (profileResult.IsSuccess)
        {
            model.CompanyName = profileResult.Value!.Name;
            model.LogoPath = profileResult.Value!.LogoPath;
        }

        var promotionId = TryGetPromotionId();

        if (promotionId is { } id)
        {
            model.IsContextual = true;
            model.PromotionId = id;
            model.PromotionName = ViewBag.PromotionName as string ?? await ResolvePromotionNameAsync(id);
        }

        return View(model);
    }

    private Guid? TryGetPromotionId() =>
        ViewBag.PromotionId switch
        {
            Guid id => id,
            string text when Guid.TryParse(text, out var parsed) => parsed,
            _ => null,
        };

    private async Task<string> ResolvePromotionNameAsync(Guid promotionId)
    {
        var result = await _getHousingPromotionUseCase.ExecuteAsync(promotionId, HttpContext.RequestAborted);
        return result.IsSuccess ? result.Value!.Name : string.Empty;
    }
}

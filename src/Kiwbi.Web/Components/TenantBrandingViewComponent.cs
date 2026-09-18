using Kiwbi.Application.Common;
using Kiwbi.Application.Developers.GetBrandingForHousingUnit;
using Kiwbi.Application.Developers.GetCurrentDeveloperProfile;
using Microsoft.AspNetCore.Mvc;

namespace Kiwbi.Web.Components;

/// <summary>Resolves and exposes the current tenant's branding accent (Epic 8) as CSS custom properties on every page, via `_Layout.cshtml`. Promotora views always have an unambiguous tenant; Buyer views only get an accent on the HousingUnit detail (a buyer can own units across multiple promotoras, so the aggregated dashboard stays neutral); anonymous/other pages get no branding.</summary>
public class TenantBrandingViewComponent : ViewComponent
{
    private readonly ICurrentUser _currentUser;
    private readonly GetCurrentDeveloperProfileUseCase _getCurrentDeveloperProfileUseCase;
    private readonly GetBrandingForHousingUnitUseCase _getBrandingForHousingUnitUseCase;

    public TenantBrandingViewComponent(
        ICurrentUser currentUser,
        GetCurrentDeveloperProfileUseCase getCurrentDeveloperProfileUseCase,
        GetBrandingForHousingUnitUseCase getBrandingForHousingUnitUseCase)
    {
        _currentUser = currentUser;
        _getCurrentDeveloperProfileUseCase = getCurrentDeveloperProfileUseCase;
        _getBrandingForHousingUnitUseCase = getBrandingForHousingUnitUseCase;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        if (UserClaimsPrincipal.IsInRole("Buyer"))
        {
            return View(await ResolveForBuyerAsync());
        }

        if (_currentUser.IsAuthenticated && _currentUser.DeveloperCompanyId is not null)
        {
            var result = await _getCurrentDeveloperProfileUseCase.ExecuteAsync(HttpContext.RequestAborted);
            return View(result.IsSuccess ? TenantBrandingViewModel.FromDto(result.Value!) : null);
        }

        return View((TenantBrandingViewModel?)null);
    }

    private async Task<TenantBrandingViewModel?> ResolveForBuyerAsync()
    {
        if (TryGetBuyerHousingUnitId() is not { } housingUnitId)
        {
            return null;
        }

        var result = await _getBrandingForHousingUnitUseCase.ExecuteAsync(housingUnitId, HttpContext.RequestAborted);

        return result.IsSuccess ? TenantBrandingViewModel.FromDto(result.Value!) : null;
    }

    private Guid? TryGetBuyerHousingUnitId()
    {
        var routeValues = ViewContext.RouteData.Values;

        var isBuyerHousingUnitAction =
            string.Equals(routeValues["controller"] as string, "Buyer", StringComparison.OrdinalIgnoreCase) &&
            string.Equals(routeValues["action"] as string, "HousingUnit", StringComparison.OrdinalIgnoreCase);

        if (!isBuyerHousingUnitAction)
        {
            return null;
        }

        return routeValues["id"] switch
        {
            Guid id => id,
            string text when Guid.TryParse(text, out var parsed) => parsed,
            _ => null,
        };
    }
}

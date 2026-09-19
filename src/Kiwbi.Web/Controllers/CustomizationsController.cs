using Kiwbi.Application.Customizations.AssignCustomizationToTypology;
using Kiwbi.Application.Customizations.AssignCustomizationToUnit;
using Kiwbi.Application.Customizations.CreateCustomization;
using Kiwbi.Application.Customizations.DeleteCustomization;
using Kiwbi.Application.Customizations.GetCustomization;
using Kiwbi.Application.Customizations.GetCustomizations;
using Kiwbi.Application.Customizations.GetTradeCategories;
using Kiwbi.Application.Customizations.GetTradeCategory;
using Kiwbi.Application.Customizations.RemoveCustomizationAssignment;
using Kiwbi.Application.Customizations.RenameCustomization;
using Kiwbi.Application.Customizations.AddCustomizationOption;
using Kiwbi.Application.Customizations.UpdateCustomizationOption;
using Kiwbi.Application.Customizations.SetDefaultCustomizationOption;
using Kiwbi.Application.Customizations.RemoveCustomizationOption;
using Kiwbi.Application.RealEstate.GetHousingPromotion;
using Kiwbi.Application.RealEstate.GetHousingTypologies;
using Kiwbi.Application.RealEstate.GetHousingUnits;
using Kiwbi.Web.Models.Customizations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Kiwbi.Web.Controllers;

[Authorize(Roles = "DeveloperAdmin")]
public class CustomizationsController : Controller
{
    private readonly CreateCustomizationUseCase _createCustomizationUseCase;
    private readonly RenameCustomizationUseCase _renameCustomizationUseCase;
    private readonly DeleteCustomizationUseCase _deleteCustomizationUseCase;
    private readonly GetCustomizationUseCase _getCustomizationUseCase;
    private readonly GetCustomizationsUseCase _getCustomizationsUseCase;
    private readonly GetTradeCategoryUseCase _getTradeCategoryUseCase;
    private readonly GetTradeCategoriesUseCase _getTradeCategoriesUseCase;
    private readonly GetHousingPromotionUseCase _getHousingPromotionUseCase;
    private readonly GetHousingTypologiesUseCase _getHousingTypologiesUseCase;
    private readonly GetHousingUnitsUseCase _getHousingUnitsUseCase;
    private readonly AssignCustomizationToTypologyUseCase _assignCustomizationToTypologyUseCase;
    private readonly AssignCustomizationToUnitUseCase _assignCustomizationToUnitUseCase;
    private readonly RemoveCustomizationAssignmentUseCase _removeCustomizationAssignmentUseCase;
    private readonly AddCustomizationOptionUseCase _addCustomizationOptionUseCase;
    private readonly UpdateCustomizationOptionUseCase _updateCustomizationOptionUseCase;
    private readonly SetDefaultCustomizationOptionUseCase _setDefaultCustomizationOptionUseCase;
    private readonly RemoveCustomizationOptionUseCase _removeCustomizationOptionUseCase;

    public CustomizationsController(
        CreateCustomizationUseCase createCustomizationUseCase,
        RenameCustomizationUseCase renameCustomizationUseCase,
        DeleteCustomizationUseCase deleteCustomizationUseCase,
        GetCustomizationUseCase getCustomizationUseCase,
        GetCustomizationsUseCase getCustomizationsUseCase,
        GetTradeCategoryUseCase getTradeCategoryUseCase,
        GetTradeCategoriesUseCase getTradeCategoriesUseCase,
        GetHousingPromotionUseCase getHousingPromotionUseCase,
        GetHousingTypologiesUseCase getHousingTypologiesUseCase,
        GetHousingUnitsUseCase getHousingUnitsUseCase,
        AssignCustomizationToTypologyUseCase assignCustomizationToTypologyUseCase,
        AssignCustomizationToUnitUseCase assignCustomizationToUnitUseCase,
        RemoveCustomizationAssignmentUseCase removeCustomizationAssignmentUseCase,
        AddCustomizationOptionUseCase addCustomizationOptionUseCase,
        UpdateCustomizationOptionUseCase updateCustomizationOptionUseCase,
        SetDefaultCustomizationOptionUseCase setDefaultCustomizationOptionUseCase,
        RemoveCustomizationOptionUseCase removeCustomizationOptionUseCase)
    {
        _createCustomizationUseCase = createCustomizationUseCase;
        _renameCustomizationUseCase = renameCustomizationUseCase;
        _deleteCustomizationUseCase = deleteCustomizationUseCase;
        _getCustomizationUseCase = getCustomizationUseCase;
        _getCustomizationsUseCase = getCustomizationsUseCase;
        _getTradeCategoryUseCase = getTradeCategoryUseCase;
        _getTradeCategoriesUseCase = getTradeCategoriesUseCase;
        _getHousingPromotionUseCase = getHousingPromotionUseCase;
        _getHousingTypologiesUseCase = getHousingTypologiesUseCase;
        _getHousingUnitsUseCase = getHousingUnitsUseCase;
        _assignCustomizationToTypologyUseCase = assignCustomizationToTypologyUseCase;
        _assignCustomizationToUnitUseCase = assignCustomizationToUnitUseCase;
        _removeCustomizationAssignmentUseCase = removeCustomizationAssignmentUseCase;
        _addCustomizationOptionUseCase = addCustomizationOptionUseCase;
        _updateCustomizationOptionUseCase = updateCustomizationOptionUseCase;
        _setDefaultCustomizationOptionUseCase = setDefaultCustomizationOptionUseCase;
        _removeCustomizationOptionUseCase = removeCustomizationOptionUseCase;
    }

    [HttpGet]
    public async Task<IActionResult> Index(Guid promotionId, Guid? tradeCategoryId, CancellationToken cancellationToken)
    {
        var promotionResult = await _getHousingPromotionUseCase.ExecuteAsync(promotionId, cancellationToken);

        if (promotionResult.IsFailure)
        {
            return NotFound();
        }

        var tradeCategoriesResult = await _getTradeCategoriesUseCase.ExecuteAsync(promotionId, cancellationToken);

        if (tradeCategoriesResult.IsFailure)
        {
            return Forbid();
        }

        var tradeCategories = tradeCategoriesResult.Value!;

        ViewBag.PromotionId = promotionId;
        ViewBag.PromotionName = promotionResult.Value!.Name;
        ViewBag.HasTradeCategories = tradeCategories.Count > 0;

        if (tradeCategoryId is { } filterId)
        {
            var tradeCategory = tradeCategories.FirstOrDefault(t => t.Id == filterId);

            if (tradeCategory is null)
            {
                return NotFound();
            }

            var filteredResult = await _getCustomizationsUseCase.ExecuteAsync(filterId, cancellationToken);

            if (filteredResult.IsFailure)
            {
                return Forbid();
            }

            ViewBag.TradeCategoryId = filterId;
            ViewBag.TradeCategoryName = tradeCategory.Name;

            return View(filteredResult.Value!.Select(c => CustomizationListItemViewModel.FromDto(c)).ToList());
        }

        ViewBag.TradeCategoryId = null;

        var items = new List<CustomizationListItemViewModel>();

        foreach (var tradeCategory in tradeCategories)
        {
            var result = await _getCustomizationsUseCase.ExecuteAsync(tradeCategory.Id, cancellationToken);

            if (result.IsFailure)
            {
                return Forbid();
            }

            items.AddRange(result.Value!.Select(c => CustomizationListItemViewModel.FromDto(c, tradeCategory.Name)));
        }

        return View(items);
    }

    [HttpGet]
    public async Task<IActionResult> Create(Guid promotionId, Guid? tradeCategoryId, CancellationToken cancellationToken)
    {
        var promotionResult = await _getHousingPromotionUseCase.ExecuteAsync(promotionId, cancellationToken);

        if (promotionResult.IsFailure)
        {
            return NotFound();
        }

        var tradeCategoriesResult = await _getTradeCategoriesUseCase.ExecuteAsync(promotionId, cancellationToken);

        if (tradeCategoriesResult.IsFailure)
        {
            return Forbid();
        }

        if (tradeCategoriesResult.Value!.Count == 0)
        {
            TempData["Error"] = "Antes de crear una personalización, da de alta al menos un gremio en esta promoción.";
            return RedirectToAction(nameof(Index), new { promotionId });
        }

        var model = new CreateCustomizationViewModel
        {
            HousingPromotionId = promotionId,
            TradeCategoryId = tradeCategoryId ?? tradeCategoriesResult.Value!.First().Id,
        };

        await PopulateCreateFormOptionsAsync(model, promotionId, cancellationToken);

        ViewBag.PromotionId = promotionId;

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateCustomizationViewModel model, CancellationToken cancellationToken)
    {
        var promotionResult = await _getHousingPromotionUseCase.ExecuteAsync(model.HousingPromotionId, cancellationToken);

        if (promotionResult.IsFailure)
        {
            return NotFound();
        }

        ViewBag.PromotionId = model.HousingPromotionId;

        if (!ModelState.IsValid)
        {
            await PopulateCreateFormOptionsAsync(model, model.HousingPromotionId, cancellationToken);
            return View(model);
        }

        var command = new CreateCustomizationCommand(
            model.TradeCategoryId,
            model.Name,
            model.DefaultOptionName,
            model.DefaultOptionSurchargeAmount,
            model.Scope,
            model.SelectedHousingTypologyIds,
            model.SelectedHousingUnitIds);

        var result = await _createCustomizationUseCase.ExecuteAsync(command, cancellationToken);

        if (result.IsFailure)
        {
            ModelState.AddModelError(string.Empty, result.Error!);
            await PopulateCreateFormOptionsAsync(model, model.HousingPromotionId, cancellationToken);
            return View(model);
        }

        return RedirectToAction(nameof(Index), new { promotionId = model.HousingPromotionId });
    }

    [HttpGet]
    public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
    {
        var result = await _getCustomizationUseCase.ExecuteAsync(id, cancellationToken);

        if (result.IsFailure)
        {
            return NotFound();
        }

        var dto = result.Value!;
        var tradeCategoryResult = await _getTradeCategoryUseCase.ExecuteAsync(dto.TradeCategoryId, cancellationToken);

        if (tradeCategoryResult.IsFailure)
        {
            return NotFound();
        }

        var housingPromotionId = tradeCategoryResult.Value!.HousingPromotionId;
        var (typologyNamesById, unitLabelsById) = await GetTargetLookupsAsync(housingPromotionId, cancellationToken);

        var model = CustomizationDetailsViewModel.FromDto(dto, housingPromotionId, typologyNamesById, unitLabelsById);
        await PopulateAvailableTargetsAsync(model, housingPromotionId, cancellationToken);

        ViewBag.PromotionId = housingPromotionId;

        if (TempData["Error"] is string error)
        {
            ModelState.AddModelError(string.Empty, error);
        }

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        var result = await _getCustomizationUseCase.ExecuteAsync(id, cancellationToken);

        if (result.IsFailure)
        {
            return NotFound();
        }

        var dto = result.Value!;

        var tradeCategoryResult = await _getTradeCategoryUseCase.ExecuteAsync(dto.TradeCategoryId, cancellationToken);

        if (tradeCategoryResult.IsSuccess)
        {
            ViewBag.PromotionId = tradeCategoryResult.Value!.HousingPromotionId;
        }

        return View(new EditCustomizationViewModel { Id = dto.Id, TradeCategoryId = dto.TradeCategoryId, Name = dto.Name });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditCustomizationViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            var tradeCategoryResult = await _getTradeCategoryUseCase.ExecuteAsync(model.TradeCategoryId, cancellationToken);

            if (tradeCategoryResult.IsSuccess)
            {
                ViewBag.PromotionId = tradeCategoryResult.Value!.HousingPromotionId;
            }

            return View(model);
        }

        var result = await _renameCustomizationUseCase.ExecuteAsync(new RenameCustomizationCommand(model.Id, model.Name), cancellationToken);

        if (result.IsFailure)
        {
            ModelState.AddModelError(string.Empty, result.Error!);
            return View(model);
        }

        return RedirectToAction(nameof(Details), new { id = model.Id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id, Guid promotionId, CancellationToken cancellationToken)
    {
        var result = await _deleteCustomizationUseCase.ExecuteAsync(id, cancellationToken);

        if (result.IsFailure)
        {
            TempData["Error"] = result.Error;
        }

        return RedirectToAction(nameof(Index), new { promotionId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddTypologyAssignment(Guid id, Guid housingTypologyId, CancellationToken cancellationToken)
    {
        var result = await _assignCustomizationToTypologyUseCase.ExecuteAsync(new AssignCustomizationToTypologyCommand(id, housingTypologyId), cancellationToken);

        if (result.IsFailure)
        {
            TempData["Error"] = result.Error;
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddUnitAssignment(Guid id, Guid housingUnitId, CancellationToken cancellationToken)
    {
        var result = await _assignCustomizationToUnitUseCase.ExecuteAsync(new AssignCustomizationToUnitCommand(id, housingUnitId), cancellationToken);

        if (result.IsFailure)
        {
            TempData["Error"] = result.Error;
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveAssignment(Guid id, Guid assignmentId, CancellationToken cancellationToken)
    {
        var result = await _removeCustomizationAssignmentUseCase.ExecuteAsync(id, assignmentId, cancellationToken);

        if (result.IsFailure)
        {
            TempData["Error"] = result.Error;
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddOption(Guid id, string name, decimal surchargeAmount, CancellationToken cancellationToken)
    {
        var result = await _addCustomizationOptionUseCase.ExecuteAsync(new AddCustomizationOptionCommand(id, name, surchargeAmount), cancellationToken);

        if (result.IsFailure)
        {
            TempData["Error"] = result.Error;
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpGet]
    public async Task<IActionResult> EditOption(Guid id, Guid optionId, CancellationToken cancellationToken)
    {
        var result = await _getCustomizationUseCase.ExecuteAsync(id, cancellationToken);

        if (result.IsFailure)
        {
            return NotFound();
        }

        var option = result.Value!.Options.FirstOrDefault(o => o.Id == optionId);

        if (option is null)
        {
            return NotFound();
        }

        var tradeCategoryResult = await _getTradeCategoryUseCase.ExecuteAsync(result.Value!.TradeCategoryId, cancellationToken);

        if (tradeCategoryResult.IsSuccess)
        {
            ViewBag.PromotionId = tradeCategoryResult.Value!.HousingPromotionId;
        }

        return View(new EditCustomizationOptionViewModel
        {
            CustomizationId = id,
            OptionId = optionId,
            Name = option.Name,
            SurchargeAmount = option.SurchargeAmount,
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditOption(EditCustomizationOptionViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            var customizationResult = await _getCustomizationUseCase.ExecuteAsync(model.CustomizationId, cancellationToken);

            if (customizationResult.IsSuccess)
            {
                var tradeCategoryResult = await _getTradeCategoryUseCase.ExecuteAsync(customizationResult.Value!.TradeCategoryId, cancellationToken);

                if (tradeCategoryResult.IsSuccess)
                {
                    ViewBag.PromotionId = tradeCategoryResult.Value!.HousingPromotionId;
                }
            }

            return View(model);
        }

        var command = new UpdateCustomizationOptionCommand(model.CustomizationId, model.OptionId, model.Name, model.SurchargeAmount);
        var result = await _updateCustomizationOptionUseCase.ExecuteAsync(command, cancellationToken);

        if (result.IsFailure)
        {
            ModelState.AddModelError(string.Empty, result.Error!);
            return View(model);
        }

        return RedirectToAction(nameof(Details), new { id = model.CustomizationId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetDefaultOption(Guid id, Guid optionId, CancellationToken cancellationToken)
    {
        var result = await _setDefaultCustomizationOptionUseCase.ExecuteAsync(id, optionId, cancellationToken);

        if (result.IsFailure)
        {
            TempData["Error"] = result.Error;
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveOption(Guid id, Guid optionId, CancellationToken cancellationToken)
    {
        var result = await _removeCustomizationOptionUseCase.ExecuteAsync(id, optionId, cancellationToken);

        if (result.IsFailure)
        {
            TempData["Error"] = result.Error;
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    private async Task PopulateCreateFormOptionsAsync(CreateCustomizationViewModel model, Guid housingPromotionId, CancellationToken cancellationToken)
    {
        var tradeCategoriesResult = await _getTradeCategoriesUseCase.ExecuteAsync(housingPromotionId, cancellationToken);
        model.AvailableTradeCategories = tradeCategoriesResult.IsSuccess
            ? tradeCategoriesResult.Value!.Select(t => new SelectListItem(t.Name, t.Id.ToString())).ToList()
            : new List<SelectListItem>();

        await PopulateAvailableTargetsAsync(model, housingPromotionId, cancellationToken);
    }

    private async Task PopulateAvailableTargetsAsync(CreateCustomizationViewModel model, Guid housingPromotionId, CancellationToken cancellationToken)
    {
        var (typologies, units) = await GetTargetOptionsAsync(housingPromotionId, cancellationToken);
        model.AvailableTypologies = typologies;
        model.AvailableUnits = units;
    }

    private async Task PopulateAvailableTargetsAsync(CustomizationDetailsViewModel model, Guid housingPromotionId, CancellationToken cancellationToken)
    {
        var (typologies, units) = await GetTargetOptionsAsync(housingPromotionId, cancellationToken);
        model.AvailableTypologies = typologies;
        model.AvailableUnits = units;
    }

    private async Task<(List<SelectListItem> Typologies, List<SelectListItem> Units)> GetTargetOptionsAsync(Guid housingPromotionId, CancellationToken cancellationToken)
    {
        var typologiesResult = await _getHousingTypologiesUseCase.ExecuteAsync(housingPromotionId, cancellationToken);
        var unitsResult = await _getHousingUnitsUseCase.ExecuteAsync(housingPromotionId, cancellationToken);

        var typologies = typologiesResult.IsSuccess
            ? typologiesResult.Value!.Select(t => new SelectListItem(t.Name, t.Id.ToString())).ToList()
            : new List<SelectListItem>();

        var units = unitsResult.IsSuccess
            ? unitsResult.Value!.Select(u => new SelectListItem($"Planta {u.Floor} - Puerta {u.Door}", u.Id.ToString())).ToList()
            : new List<SelectListItem>();

        return (typologies, units);
    }

    private async Task<(IReadOnlyDictionary<Guid, string> TypologyNamesById, IReadOnlyDictionary<Guid, string> UnitLabelsById)> GetTargetLookupsAsync(
        Guid housingPromotionId, CancellationToken cancellationToken)
    {
        var typologiesResult = await _getHousingTypologiesUseCase.ExecuteAsync(housingPromotionId, cancellationToken);
        var unitsResult = await _getHousingUnitsUseCase.ExecuteAsync(housingPromotionId, cancellationToken);

        var typologyNamesById = typologiesResult.IsSuccess
            ? typologiesResult.Value!.ToDictionary(t => t.Id, t => t.Name)
            : new Dictionary<Guid, string>();

        var unitLabelsById = unitsResult.IsSuccess
            ? unitsResult.Value!.ToDictionary(u => u.Id, u => $"Planta {u.Floor} - Puerta {u.Door}")
            : new Dictionary<Guid, string>();

        return (typologyNamesById, unitLabelsById);
    }
}

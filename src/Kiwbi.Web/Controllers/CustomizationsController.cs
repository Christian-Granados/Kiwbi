using Kiwbi.Application.Customizations.AssignCustomizationToTypology;
using Kiwbi.Application.Customizations.AssignCustomizationToUnit;
using Kiwbi.Application.Customizations.CreateCustomization;
using Kiwbi.Application.Customizations.DeleteCustomization;
using Kiwbi.Application.Customizations.GetCustomization;
using Kiwbi.Application.Customizations.GetCustomizations;
using Kiwbi.Application.Customizations.GetTradeCategory;
using Kiwbi.Application.Customizations.RemoveCustomizationAssignment;
using Kiwbi.Application.Customizations.RenameCustomization;
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
    private readonly GetHousingTypologiesUseCase _getHousingTypologiesUseCase;
    private readonly GetHousingUnitsUseCase _getHousingUnitsUseCase;
    private readonly AssignCustomizationToTypologyUseCase _assignCustomizationToTypologyUseCase;
    private readonly AssignCustomizationToUnitUseCase _assignCustomizationToUnitUseCase;
    private readonly RemoveCustomizationAssignmentUseCase _removeCustomizationAssignmentUseCase;

    public CustomizationsController(
        CreateCustomizationUseCase createCustomizationUseCase,
        RenameCustomizationUseCase renameCustomizationUseCase,
        DeleteCustomizationUseCase deleteCustomizationUseCase,
        GetCustomizationUseCase getCustomizationUseCase,
        GetCustomizationsUseCase getCustomizationsUseCase,
        GetTradeCategoryUseCase getTradeCategoryUseCase,
        GetHousingTypologiesUseCase getHousingTypologiesUseCase,
        GetHousingUnitsUseCase getHousingUnitsUseCase,
        AssignCustomizationToTypologyUseCase assignCustomizationToTypologyUseCase,
        AssignCustomizationToUnitUseCase assignCustomizationToUnitUseCase,
        RemoveCustomizationAssignmentUseCase removeCustomizationAssignmentUseCase)
    {
        _createCustomizationUseCase = createCustomizationUseCase;
        _renameCustomizationUseCase = renameCustomizationUseCase;
        _deleteCustomizationUseCase = deleteCustomizationUseCase;
        _getCustomizationUseCase = getCustomizationUseCase;
        _getCustomizationsUseCase = getCustomizationsUseCase;
        _getTradeCategoryUseCase = getTradeCategoryUseCase;
        _getHousingTypologiesUseCase = getHousingTypologiesUseCase;
        _getHousingUnitsUseCase = getHousingUnitsUseCase;
        _assignCustomizationToTypologyUseCase = assignCustomizationToTypologyUseCase;
        _assignCustomizationToUnitUseCase = assignCustomizationToUnitUseCase;
        _removeCustomizationAssignmentUseCase = removeCustomizationAssignmentUseCase;
    }

    [HttpGet]
    public async Task<IActionResult> Index(Guid tradeCategoryId, CancellationToken cancellationToken)
    {
        var tradeCategoryResult = await _getTradeCategoryUseCase.ExecuteAsync(tradeCategoryId, cancellationToken);

        if (tradeCategoryResult.IsFailure)
        {
            return NotFound();
        }

        var result = await _getCustomizationsUseCase.ExecuteAsync(tradeCategoryId, cancellationToken);

        if (result.IsFailure)
        {
            return Forbid();
        }

        ViewBag.TradeCategoryId = tradeCategoryId;
        ViewBag.TradeCategoryName = tradeCategoryResult.Value!.Name;
        ViewBag.PromotionId = tradeCategoryResult.Value!.HousingPromotionId;

        return View(result.Value!.Select(CustomizationListItemViewModel.FromDto).ToList());
    }

    [HttpGet]
    public async Task<IActionResult> Create(Guid tradeCategoryId, CancellationToken cancellationToken)
    {
        var tradeCategoryResult = await _getTradeCategoryUseCase.ExecuteAsync(tradeCategoryId, cancellationToken);

        if (tradeCategoryResult.IsFailure)
        {
            return NotFound();
        }

        var model = new CreateCustomizationViewModel { TradeCategoryId = tradeCategoryId };
        await PopulateAvailableTargetsAsync(model, tradeCategoryResult.Value!.HousingPromotionId, cancellationToken);

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateCustomizationViewModel model, CancellationToken cancellationToken)
    {
        var tradeCategoryResult = await _getTradeCategoryUseCase.ExecuteAsync(model.TradeCategoryId, cancellationToken);

        if (tradeCategoryResult.IsFailure)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            await PopulateAvailableTargetsAsync(model, tradeCategoryResult.Value!.HousingPromotionId, cancellationToken);
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
            await PopulateAvailableTargetsAsync(model, tradeCategoryResult.Value!.HousingPromotionId, cancellationToken);
            return View(model);
        }

        return RedirectToAction(nameof(Index), new { tradeCategoryId = model.TradeCategoryId });
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

        return View(new EditCustomizationViewModel { Id = dto.Id, TradeCategoryId = dto.TradeCategoryId, Name = dto.Name });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditCustomizationViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
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
    public async Task<IActionResult> Delete(Guid id, Guid tradeCategoryId, CancellationToken cancellationToken)
    {
        var result = await _deleteCustomizationUseCase.ExecuteAsync(id, cancellationToken);

        if (result.IsFailure)
        {
            TempData["Error"] = result.Error;
        }

        return RedirectToAction(nameof(Index), new { tradeCategoryId });
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

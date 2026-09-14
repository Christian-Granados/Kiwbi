using Kiwbi.Application.Developers.GetCurrentDeveloperProfile;
using Kiwbi.Application.Developers.UpdateDeveloperBranding;
using Kiwbi.Application.Developers.UpdateDeveloperProfile;
using Kiwbi.Web.Models.DeveloperProfile;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kiwbi.Web.Controllers;

[Authorize(Roles = "DeveloperAdmin")]
public class DeveloperProfileController : Controller
{
    private readonly GetCurrentDeveloperProfileUseCase _getCurrentDeveloperProfileUseCase;
    private readonly UpdateDeveloperProfileUseCase _updateDeveloperProfileUseCase;
    private readonly UpdateDeveloperBrandingUseCase _updateDeveloperBrandingUseCase;

    public DeveloperProfileController(
        GetCurrentDeveloperProfileUseCase getCurrentDeveloperProfileUseCase,
        UpdateDeveloperProfileUseCase updateDeveloperProfileUseCase,
        UpdateDeveloperBrandingUseCase updateDeveloperBrandingUseCase)
    {
        _getCurrentDeveloperProfileUseCase = getCurrentDeveloperProfileUseCase;
        _updateDeveloperProfileUseCase = updateDeveloperProfileUseCase;
        _updateDeveloperBrandingUseCase = updateDeveloperBrandingUseCase;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var result = await _getCurrentDeveloperProfileUseCase.ExecuteAsync(cancellationToken);

        if (result.IsFailure)
        {
            return Forbid();
        }

        return View(DeveloperProfileViewModel.FromDto(result.Value!));
    }

    [HttpGet]
    public async Task<IActionResult> EditProfile(CancellationToken cancellationToken)
    {
        var result = await _getCurrentDeveloperProfileUseCase.ExecuteAsync(cancellationToken);

        if (result.IsFailure)
        {
            return Forbid();
        }

        return View(new EditDeveloperProfileViewModel { Name = result.Value!.Name });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditProfile(EditDeveloperProfileViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _updateDeveloperProfileUseCase.ExecuteAsync(new UpdateDeveloperProfileCommand(model.Name), cancellationToken);

        if (result.IsFailure)
        {
            ModelState.AddModelError(string.Empty, result.Error!);
            return View(model);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> EditBranding(CancellationToken cancellationToken)
    {
        var result = await _getCurrentDeveloperProfileUseCase.ExecuteAsync(cancellationToken);

        if (result.IsFailure)
        {
            return Forbid();
        }

        var dto = result.Value!;

        return View(new EditDeveloperBrandingViewModel
        {
            PrimaryColor = dto.PrimaryColor,
            SecondaryColor = dto.SecondaryColor,
            LogoPath = dto.LogoPath,
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditBranding(EditDeveloperBrandingViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var command = new UpdateDeveloperBrandingCommand(model.PrimaryColor, model.SecondaryColor, model.LogoPath);
        var result = await _updateDeveloperBrandingUseCase.ExecuteAsync(command, cancellationToken);

        if (result.IsFailure)
        {
            ModelState.AddModelError(string.Empty, result.Error!);
            return View(model);
        }

        return RedirectToAction(nameof(Index));
    }
}

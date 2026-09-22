using Kiwbi.Application.Developers.GetCurrentDeveloperProfile;
using Kiwbi.Application.Developers.GetDeveloperCompanyOverview;
using Kiwbi.Application.Developers.UpdateDeveloperBranding;
using Kiwbi.Application.Developers.UploadDeveloperBrandingLogo;
using Kiwbi.Application.Developers.UpdateDeveloperProfile;
using Kiwbi.Web.Models.DeveloperProfile;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kiwbi.Web.Controllers;

[Authorize(Roles = "DeveloperAdmin")]
public class DeveloperProfileController : Controller
{
    private readonly GetCurrentDeveloperProfileUseCase _getCurrentDeveloperProfileUseCase;
    private readonly GetDeveloperCompanyOverviewUseCase _getDeveloperCompanyOverviewUseCase;
    private readonly UpdateDeveloperProfileUseCase _updateDeveloperProfileUseCase;
    private readonly UpdateDeveloperBrandingUseCase _updateDeveloperBrandingUseCase;
    private readonly UploadDeveloperBrandingLogoUseCase _uploadDeveloperBrandingLogoUseCase;

    public DeveloperProfileController(
        GetCurrentDeveloperProfileUseCase getCurrentDeveloperProfileUseCase,
        GetDeveloperCompanyOverviewUseCase getDeveloperCompanyOverviewUseCase,
        UpdateDeveloperProfileUseCase updateDeveloperProfileUseCase,
        UpdateDeveloperBrandingUseCase updateDeveloperBrandingUseCase,
        UploadDeveloperBrandingLogoUseCase uploadDeveloperBrandingLogoUseCase)
    {
        _getCurrentDeveloperProfileUseCase = getCurrentDeveloperProfileUseCase;
        _getDeveloperCompanyOverviewUseCase = getDeveloperCompanyOverviewUseCase;
        _updateDeveloperProfileUseCase = updateDeveloperProfileUseCase;
        _updateDeveloperBrandingUseCase = updateDeveloperBrandingUseCase;
        _uploadDeveloperBrandingLogoUseCase = uploadDeveloperBrandingLogoUseCase;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var result = await _getCurrentDeveloperProfileUseCase.ExecuteAsync(cancellationToken);

        if (result.IsFailure)
        {
            return Forbid();
        }

        var overviewResult = await _getDeveloperCompanyOverviewUseCase.ExecuteAsync(cancellationToken);
        var overview = overviewResult.IsSuccess ? overviewResult.Value! : new DeveloperCompanyOverviewDto(0, 0, 0, 0);

        return View(DeveloperProfileViewModel.FromDto(result.Value!, overview));
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

        if (model.LogoImageFile is { Length: > 0 } file)
        {
            await using var stream = file.OpenReadStream();
            var uploadResult = await _uploadDeveloperBrandingLogoUseCase.ExecuteAsync(stream, file.FileName, cancellationToken);

            if (uploadResult.IsFailure)
            {
                ModelState.AddModelError(string.Empty, uploadResult.Error!);
                return View(model);
            }

            model.LogoPath = uploadResult.Value;
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

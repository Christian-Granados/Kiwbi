using Kiwbi.Application.Common;

namespace Kiwbi.Application.Developers.UploadDeveloperBrandingLogo;

/// <summary>Uploads a logo image for the DeveloperCompany owned by the currently authenticated user and returns its
/// URL (Epic 11, Feature 11.4 - real file upload replacing the plain text/URL field). Does not persist the
/// resulting URL itself: the Web layer passes it into UpdateDeveloperBrandingUseCase right after, same two-step
/// pattern already used for HousingUnit/HousingPromotion image uploads.</summary>
public class UploadDeveloperBrandingLogoUseCase
{
    private const string StorageSubFolder = "branding";

    private readonly ICurrentUser _currentUser;
    private readonly IFileStorageService _fileStorageService;

    public UploadDeveloperBrandingLogoUseCase(ICurrentUser currentUser, IFileStorageService fileStorageService)
    {
        _currentUser = currentUser;
        _fileStorageService = fileStorageService;
    }

    public async Task<Result<string>> ExecuteAsync(Stream content, string fileName, CancellationToken cancellationToken = default)
    {
        if (_currentUser.DeveloperCompanyId is null)
        {
            return Result.Failure<string>("El usuario actual no está vinculado a ninguna promotora.");
        }

        return await _fileStorageService.SaveAsync(content, fileName, StorageSubFolder, cancellationToken);
    }
}

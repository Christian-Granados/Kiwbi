using Kiwbi.Application.Common;
using Kiwbi.Domain.Customizations;
using Kiwbi.Domain.Exceptions;
using Kiwbi.Domain.RealEstate;

namespace Kiwbi.Application.Customizations.UpdateCustomizationOptionThumbnail;

/// <summary>Uploads and sets the thumbnail image of a CustomizationOption whose Customization is owned by the currently authenticated tenant.</summary>
public class UpdateCustomizationOptionThumbnailUseCase
{
    private const string StorageSubFolder = "customization-options";

    private readonly ICurrentUser _currentUser;
    private readonly IHousingPromotionRepository _housingPromotionRepository;
    private readonly ITradeCategoryRepository _tradeCategoryRepository;
    private readonly ICustomizationRepository _customizationRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateCustomizationOptionThumbnailUseCase(
        ICurrentUser currentUser,
        IHousingPromotionRepository housingPromotionRepository,
        ITradeCategoryRepository tradeCategoryRepository,
        ICustomizationRepository customizationRepository,
        IFileStorageService fileStorageService,
        IUnitOfWork unitOfWork)
    {
        _currentUser = currentUser;
        _housingPromotionRepository = housingPromotionRepository;
        _tradeCategoryRepository = tradeCategoryRepository;
        _customizationRepository = customizationRepository;
        _fileStorageService = fileStorageService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> ExecuteAsync(UpdateCustomizationOptionThumbnailCommand command, CancellationToken cancellationToken = default)
    {
        if (_currentUser.DeveloperCompanyId is not { } developerCompanyId)
        {
            return Result.Failure("El usuario actual no está vinculado a ninguna promotora.");
        }

        var customization = await _customizationRepository.GetByIdAsync(command.CustomizationId, cancellationToken);

        if (customization is null)
        {
            return Result.Failure("No se ha encontrado la personalización.");
        }

        var tradeCategory = await _tradeCategoryRepository.GetByIdAsync(customization.TradeCategoryId, cancellationToken);

        if (tradeCategory is null)
        {
            return Result.Failure("No se ha encontrado la personalización.");
        }

        var promotion = await _housingPromotionRepository.GetByIdAsync(tradeCategory.HousingPromotionId, cancellationToken);

        if (promotion is null || promotion.DeveloperCompanyId != developerCompanyId)
        {
            return Result.Failure("No se ha encontrado la personalización.");
        }

        var option = customization.Options.FirstOrDefault(o => o.Id == command.OptionId);

        if (option is null)
        {
            return Result.Failure("No se ha encontrado la opción.");
        }

        var saveResult = await _fileStorageService.SaveAsync(command.Content, command.FileName, StorageSubFolder, cancellationToken);

        if (saveResult.IsFailure)
        {
            return Result.Failure(saveResult.Error!);
        }

        var previousImagePath = option.ThumbnailImagePath;

        try
        {
            customization.UpdateOptionThumbnail(command.OptionId, saveResult.Value);
        }
        catch (DomainException ex)
        {
            return Result.Failure(ex.Message);
        }

        _customizationRepository.Update(customization);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(previousImagePath))
        {
            _fileStorageService.Delete(previousImagePath);
        }

        return Result.Success();
    }
}

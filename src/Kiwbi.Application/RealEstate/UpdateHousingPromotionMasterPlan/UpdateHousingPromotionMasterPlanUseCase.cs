using Kiwbi.Application.Common;
using Kiwbi.Domain.RealEstate;

namespace Kiwbi.Application.RealEstate.UpdateHousingPromotionMasterPlan;

/// <summary>Uploads and sets the master plan image of a HousingPromotion owned by the currently authenticated tenant.</summary>
public class UpdateHousingPromotionMasterPlanUseCase
{
    private const string StorageSubFolder = "promotions";

    private readonly ICurrentUser _currentUser;
    private readonly IHousingPromotionRepository _housingPromotionRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateHousingPromotionMasterPlanUseCase(
        ICurrentUser currentUser,
        IHousingPromotionRepository housingPromotionRepository,
        IFileStorageService fileStorageService,
        IUnitOfWork unitOfWork)
    {
        _currentUser = currentUser;
        _housingPromotionRepository = housingPromotionRepository;
        _fileStorageService = fileStorageService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> ExecuteAsync(UpdateHousingPromotionMasterPlanCommand command, CancellationToken cancellationToken = default)
    {
        if (_currentUser.DeveloperCompanyId is not { } developerCompanyId)
        {
            return Result.Failure("El usuario actual no está vinculado a ninguna promotora.");
        }

        var promotion = await _housingPromotionRepository.GetByIdAsync(command.Id, cancellationToken);

        if (promotion is null || promotion.DeveloperCompanyId != developerCompanyId)
        {
            return Result.Failure("No se ha encontrado la promoción.");
        }

        var saveResult = await _fileStorageService.SaveAsync(command.Content, command.FileName, StorageSubFolder, cancellationToken);

        if (saveResult.IsFailure)
        {
            return Result.Failure(saveResult.Error!);
        }

        var previousImagePath = promotion.MasterPlanImagePath;

        promotion.UpdateMasterPlanImage(saveResult.Value);

        _housingPromotionRepository.Update(promotion);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(previousImagePath))
        {
            _fileStorageService.Delete(previousImagePath);
        }

        return Result.Success();
    }
}

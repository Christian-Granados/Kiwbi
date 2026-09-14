using Kiwbi.Application.Common;
using Kiwbi.Domain.RealEstate;

namespace Kiwbi.Application.RealEstate.UpdateHousingUnitFloorPlan;

/// <summary>Uploads and sets the floor plan image of a HousingUnit whose HousingPromotion is owned by the currently authenticated tenant.</summary>
public class UpdateHousingUnitFloorPlanUseCase
{
    private const string StorageSubFolder = "units";

    private readonly ICurrentUser _currentUser;
    private readonly IHousingPromotionRepository _housingPromotionRepository;
    private readonly IHousingUnitRepository _housingUnitRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateHousingUnitFloorPlanUseCase(
        ICurrentUser currentUser,
        IHousingPromotionRepository housingPromotionRepository,
        IHousingUnitRepository housingUnitRepository,
        IFileStorageService fileStorageService,
        IUnitOfWork unitOfWork)
    {
        _currentUser = currentUser;
        _housingPromotionRepository = housingPromotionRepository;
        _housingUnitRepository = housingUnitRepository;
        _fileStorageService = fileStorageService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> ExecuteAsync(UpdateHousingUnitFloorPlanCommand command, CancellationToken cancellationToken = default)
    {
        if (_currentUser.DeveloperCompanyId is not { } developerCompanyId)
        {
            return Result.Failure("El usuario actual no está vinculado a ninguna promotora.");
        }

        var unit = await _housingUnitRepository.GetByIdAsync(command.Id, cancellationToken);

        if (unit is null)
        {
            return Result.Failure("No se ha encontrado la vivienda.");
        }

        var promotion = await _housingPromotionRepository.GetByIdAsync(unit.HousingPromotionId, cancellationToken);

        if (promotion is null || promotion.DeveloperCompanyId != developerCompanyId)
        {
            return Result.Failure("No se ha encontrado la vivienda.");
        }

        var saveResult = await _fileStorageService.SaveAsync(command.Content, command.FileName, StorageSubFolder, cancellationToken);

        if (saveResult.IsFailure)
        {
            return Result.Failure(saveResult.Error!);
        }

        var previousImagePath = unit.FloorPlanImagePath;

        unit.UpdateFloorPlanImage(saveResult.Value);

        _housingUnitRepository.Update(unit);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(previousImagePath))
        {
            _fileStorageService.Delete(previousImagePath);
        }

        return Result.Success();
    }
}

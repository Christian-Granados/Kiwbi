using Kiwbi.Application.Common;
using Kiwbi.Domain.RealEstate;

namespace Kiwbi.Application.RealEstate.DeleteHousingPromotion;

/// <summary>Deletes a HousingPromotion owned by the currently authenticated tenant, blocking deletion if it has typologies or units.</summary>
public class DeleteHousingPromotionUseCase
{
    private readonly ICurrentUser _currentUser;
    private readonly IHousingPromotionRepository _housingPromotionRepository;
    private readonly IHousingTypologyRepository _housingTypologyRepository;
    private readonly IHousingUnitRepository _housingUnitRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteHousingPromotionUseCase(
        ICurrentUser currentUser,
        IHousingPromotionRepository housingPromotionRepository,
        IHousingTypologyRepository housingTypologyRepository,
        IHousingUnitRepository housingUnitRepository,
        IFileStorageService fileStorageService,
        IUnitOfWork unitOfWork)
    {
        _currentUser = currentUser;
        _housingPromotionRepository = housingPromotionRepository;
        _housingTypologyRepository = housingTypologyRepository;
        _housingUnitRepository = housingUnitRepository;
        _fileStorageService = fileStorageService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> ExecuteAsync(Guid promotionId, CancellationToken cancellationToken = default)
    {
        if (_currentUser.DeveloperCompanyId is not { } developerCompanyId)
        {
            return Result.Failure("El usuario actual no está vinculado a ninguna promotora.");
        }

        var promotion = await _housingPromotionRepository.GetByIdAsync(promotionId, cancellationToken);

        if (promotion is null || promotion.DeveloperCompanyId != developerCompanyId)
        {
            return Result.Failure("No se ha encontrado la promoción.");
        }

        if (await _housingTypologyRepository.ExistsByHousingPromotionIdAsync(promotionId, cancellationToken) ||
            await _housingUnitRepository.ExistsByHousingPromotionIdAsync(promotionId, cancellationToken))
        {
            return Result.Failure("No se puede eliminar la promoción porque tiene tipologías o viviendas asociadas.");
        }

        var masterPlanImagePath = promotion.MasterPlanImagePath;

        _housingPromotionRepository.Remove(promotion);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(masterPlanImagePath))
        {
            _fileStorageService.Delete(masterPlanImagePath);
        }

        return Result.Success();
    }
}

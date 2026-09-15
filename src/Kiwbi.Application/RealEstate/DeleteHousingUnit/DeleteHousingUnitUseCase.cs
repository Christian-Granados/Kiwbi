using Kiwbi.Application.Common;
using Kiwbi.Domain.Customizations;
using Kiwbi.Domain.Onboarding;
using Kiwbi.Domain.RealEstate;

namespace Kiwbi.Application.RealEstate.DeleteHousingUnit;

/// <summary>Deletes a HousingUnit whose HousingPromotion is owned by the currently authenticated tenant, blocking deletion if it has customization assignments or buyer invitations.</summary>
public class DeleteHousingUnitUseCase
{
    private readonly ICurrentUser _currentUser;
    private readonly IHousingPromotionRepository _housingPromotionRepository;
    private readonly IHousingUnitRepository _housingUnitRepository;
    private readonly ICustomizationRepository _customizationRepository;
    private readonly IBuyerInvitationRepository _buyerInvitationRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteHousingUnitUseCase(
        ICurrentUser currentUser,
        IHousingPromotionRepository housingPromotionRepository,
        IHousingUnitRepository housingUnitRepository,
        ICustomizationRepository customizationRepository,
        IBuyerInvitationRepository buyerInvitationRepository,
        IFileStorageService fileStorageService,
        IUnitOfWork unitOfWork)
    {
        _currentUser = currentUser;
        _housingPromotionRepository = housingPromotionRepository;
        _housingUnitRepository = housingUnitRepository;
        _customizationRepository = customizationRepository;
        _buyerInvitationRepository = buyerInvitationRepository;
        _fileStorageService = fileStorageService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> ExecuteAsync(Guid unitId, CancellationToken cancellationToken = default)
    {
        if (_currentUser.DeveloperCompanyId is not { } developerCompanyId)
        {
            return Result.Failure("El usuario actual no está vinculado a ninguna promotora.");
        }

        var unit = await _housingUnitRepository.GetByIdAsync(unitId, cancellationToken);

        if (unit is null)
        {
            return Result.Failure("No se ha encontrado la vivienda.");
        }

        var promotion = await _housingPromotionRepository.GetByIdAsync(unit.HousingPromotionId, cancellationToken);

        if (promotion is null || promotion.DeveloperCompanyId != developerCompanyId)
        {
            return Result.Failure("No se ha encontrado la vivienda.");
        }

        if (await _customizationRepository.ExistsByHousingUnitIdAsync(unitId, cancellationToken))
        {
            return Result.Failure("No se puede eliminar la vivienda porque tiene personalizaciones asignadas.");
        }

        if (await _buyerInvitationRepository.ExistsByHousingUnitIdAsync(unitId, cancellationToken))
        {
            return Result.Failure("No se puede eliminar la vivienda porque tiene invitaciones de comprador asociadas.");
        }

        var floorPlanImagePath = unit.FloorPlanImagePath;

        _housingUnitRepository.Remove(unit);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(floorPlanImagePath))
        {
            _fileStorageService.Delete(floorPlanImagePath);
        }

        return Result.Success();
    }
}

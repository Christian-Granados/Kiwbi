using Kiwbi.Application.Common;
using Kiwbi.Domain.Customizations;
using Kiwbi.Domain.RealEstate;

namespace Kiwbi.Application.RealEstate.DeleteHousingTypology;

/// <summary>Deletes a HousingTypology whose HousingPromotion is owned by the currently authenticated tenant, blocking deletion if it has units or customization assignments.</summary>
public class DeleteHousingTypologyUseCase
{
    private readonly ICurrentUser _currentUser;
    private readonly IHousingPromotionRepository _housingPromotionRepository;
    private readonly IHousingTypologyRepository _housingTypologyRepository;
    private readonly IHousingUnitRepository _housingUnitRepository;
    private readonly ICustomizationRepository _customizationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteHousingTypologyUseCase(
        ICurrentUser currentUser,
        IHousingPromotionRepository housingPromotionRepository,
        IHousingTypologyRepository housingTypologyRepository,
        IHousingUnitRepository housingUnitRepository,
        ICustomizationRepository customizationRepository,
        IUnitOfWork unitOfWork)
    {
        _currentUser = currentUser;
        _housingPromotionRepository = housingPromotionRepository;
        _housingTypologyRepository = housingTypologyRepository;
        _housingUnitRepository = housingUnitRepository;
        _customizationRepository = customizationRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> ExecuteAsync(Guid typologyId, CancellationToken cancellationToken = default)
    {
        if (_currentUser.DeveloperCompanyId is not { } developerCompanyId)
        {
            return Result.Failure("El usuario actual no está vinculado a ninguna promotora.");
        }

        var typology = await _housingTypologyRepository.GetByIdAsync(typologyId, cancellationToken);

        if (typology is null)
        {
            return Result.Failure("No se ha encontrado la tipología.");
        }

        var promotion = await _housingPromotionRepository.GetByIdAsync(typology.HousingPromotionId, cancellationToken);

        if (promotion is null || promotion.DeveloperCompanyId != developerCompanyId)
        {
            return Result.Failure("No se ha encontrado la tipología.");
        }

        if (await _housingUnitRepository.ExistsByHousingTypologyIdAsync(typologyId, cancellationToken))
        {
            return Result.Failure("No se puede eliminar la tipología porque tiene viviendas asociadas.");
        }

        if (await _customizationRepository.ExistsByHousingTypologyIdAsync(typologyId, cancellationToken))
        {
            return Result.Failure("No se puede eliminar la tipología porque tiene personalizaciones asignadas.");
        }

        _housingTypologyRepository.Remove(typology);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

using Kiwbi.Application.Common;
using Kiwbi.Domain.Customizations;
using Kiwbi.Domain.Exceptions;
using Kiwbi.Domain.RealEstate;

namespace Kiwbi.Application.Customizations.AssignCustomizationToUnit;

/// <summary>Adds a HousingUnit assignment to a Customization owned by the currently authenticated tenant.</summary>
public class AssignCustomizationToUnitUseCase
{
    private readonly ICurrentUser _currentUser;
    private readonly IHousingPromotionRepository _housingPromotionRepository;
    private readonly IHousingUnitRepository _housingUnitRepository;
    private readonly ITradeCategoryRepository _tradeCategoryRepository;
    private readonly ICustomizationRepository _customizationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AssignCustomizationToUnitUseCase(
        ICurrentUser currentUser,
        IHousingPromotionRepository housingPromotionRepository,
        IHousingUnitRepository housingUnitRepository,
        ITradeCategoryRepository tradeCategoryRepository,
        ICustomizationRepository customizationRepository,
        IUnitOfWork unitOfWork)
    {
        _currentUser = currentUser;
        _housingPromotionRepository = housingPromotionRepository;
        _housingUnitRepository = housingUnitRepository;
        _tradeCategoryRepository = tradeCategoryRepository;
        _customizationRepository = customizationRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> ExecuteAsync(AssignCustomizationToUnitCommand command, CancellationToken cancellationToken = default)
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

        var unit = await _housingUnitRepository.GetByIdAsync(command.HousingUnitId, cancellationToken);

        if (unit is null || unit.HousingPromotionId != promotion.Id)
        {
            return Result.Failure("No se ha encontrado la vivienda.");
        }

        try
        {
            customization.AssignToUnit(command.HousingUnitId);
        }
        catch (DomainException ex)
        {
            return Result.Failure(ex.Message);
        }

        _customizationRepository.Update(customization);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

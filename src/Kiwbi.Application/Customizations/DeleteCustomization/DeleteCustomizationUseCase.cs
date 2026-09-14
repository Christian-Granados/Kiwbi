using Kiwbi.Application.Common;
using Kiwbi.Domain.Customizations;
using Kiwbi.Domain.RealEstate;

namespace Kiwbi.Application.Customizations.DeleteCustomization;

/// <summary>Deletes a Customization whose TradeCategory/HousingPromotion is owned by the currently authenticated tenant.</summary>
public class DeleteCustomizationUseCase
{
    private readonly ICurrentUser _currentUser;
    private readonly IHousingPromotionRepository _housingPromotionRepository;
    private readonly ITradeCategoryRepository _tradeCategoryRepository;
    private readonly ICustomizationRepository _customizationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteCustomizationUseCase(
        ICurrentUser currentUser,
        IHousingPromotionRepository housingPromotionRepository,
        ITradeCategoryRepository tradeCategoryRepository,
        ICustomizationRepository customizationRepository,
        IUnitOfWork unitOfWork)
    {
        _currentUser = currentUser;
        _housingPromotionRepository = housingPromotionRepository;
        _tradeCategoryRepository = tradeCategoryRepository;
        _customizationRepository = customizationRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> ExecuteAsync(Guid customizationId, CancellationToken cancellationToken = default)
    {
        if (_currentUser.DeveloperCompanyId is not { } developerCompanyId)
        {
            return Result.Failure("El usuario actual no está vinculado a ninguna promotora.");
        }

        var customization = await _customizationRepository.GetByIdAsync(customizationId, cancellationToken);

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

        _customizationRepository.Remove(customization);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

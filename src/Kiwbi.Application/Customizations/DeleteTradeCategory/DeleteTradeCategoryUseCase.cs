using Kiwbi.Application.Common;
using Kiwbi.Domain.Customizations;
using Kiwbi.Domain.RealEstate;

namespace Kiwbi.Application.Customizations.DeleteTradeCategory;

/// <summary>Deletes a TradeCategory whose HousingPromotion is owned by the currently authenticated tenant.</summary>
/// <remarks>Will be extended in Feature 3.2 to block deletion if the TradeCategory has Customizations, once ICustomizationRepository exists.</remarks>
public class DeleteTradeCategoryUseCase
{
    private readonly ICurrentUser _currentUser;
    private readonly IHousingPromotionRepository _housingPromotionRepository;
    private readonly ITradeCategoryRepository _tradeCategoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteTradeCategoryUseCase(
        ICurrentUser currentUser,
        IHousingPromotionRepository housingPromotionRepository,
        ITradeCategoryRepository tradeCategoryRepository,
        IUnitOfWork unitOfWork)
    {
        _currentUser = currentUser;
        _housingPromotionRepository = housingPromotionRepository;
        _tradeCategoryRepository = tradeCategoryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> ExecuteAsync(Guid tradeCategoryId, CancellationToken cancellationToken = default)
    {
        if (_currentUser.DeveloperCompanyId is not { } developerCompanyId)
        {
            return Result.Failure("El usuario actual no está vinculado a ninguna promotora.");
        }

        var tradeCategory = await _tradeCategoryRepository.GetByIdAsync(tradeCategoryId, cancellationToken);

        if (tradeCategory is null)
        {
            return Result.Failure("No se ha encontrado el gremio.");
        }

        var promotion = await _housingPromotionRepository.GetByIdAsync(tradeCategory.HousingPromotionId, cancellationToken);

        if (promotion is null || promotion.DeveloperCompanyId != developerCompanyId)
        {
            return Result.Failure("No se ha encontrado el gremio.");
        }

        _tradeCategoryRepository.Remove(tradeCategory);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

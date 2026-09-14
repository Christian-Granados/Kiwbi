using Kiwbi.Application.Common;
using Kiwbi.Domain.Customizations;
using Kiwbi.Domain.RealEstate;

namespace Kiwbi.Application.Customizations.GetTradeCategory;

/// <summary>Resolves the detail of a TradeCategory whose HousingPromotion is owned by the currently authenticated tenant.</summary>
public class GetTradeCategoryUseCase
{
    private readonly ICurrentUser _currentUser;
    private readonly IHousingPromotionRepository _housingPromotionRepository;
    private readonly ITradeCategoryRepository _tradeCategoryRepository;

    public GetTradeCategoryUseCase(
        ICurrentUser currentUser,
        IHousingPromotionRepository housingPromotionRepository,
        ITradeCategoryRepository tradeCategoryRepository)
    {
        _currentUser = currentUser;
        _housingPromotionRepository = housingPromotionRepository;
        _tradeCategoryRepository = tradeCategoryRepository;
    }

    public async Task<Result<TradeCategoryDto>> ExecuteAsync(Guid tradeCategoryId, CancellationToken cancellationToken = default)
    {
        if (_currentUser.DeveloperCompanyId is not { } developerCompanyId)
        {
            return Result.Failure<TradeCategoryDto>("El usuario actual no está vinculado a ninguna promotora.");
        }

        var tradeCategory = await _tradeCategoryRepository.GetByIdAsync(tradeCategoryId, cancellationToken);

        if (tradeCategory is null)
        {
            return Result.Failure<TradeCategoryDto>("No se ha encontrado el gremio.");
        }

        var promotion = await _housingPromotionRepository.GetByIdAsync(tradeCategory.HousingPromotionId, cancellationToken);

        if (promotion is null || promotion.DeveloperCompanyId != developerCompanyId)
        {
            return Result.Failure<TradeCategoryDto>("No se ha encontrado el gremio.");
        }

        return Result.Success(new TradeCategoryDto(
            tradeCategory.Id,
            tradeCategory.HousingPromotionId,
            tradeCategory.Name,
            tradeCategory.SelectionCutOffDateUtc,
            tradeCategory.CreatedAtUtc,
            tradeCategory.UpdatedAtUtc));
    }
}

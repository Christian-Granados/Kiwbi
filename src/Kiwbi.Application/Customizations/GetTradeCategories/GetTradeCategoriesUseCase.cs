using Kiwbi.Application.Common;
using Kiwbi.Domain.Customizations;
using Kiwbi.Domain.RealEstate;

namespace Kiwbi.Application.Customizations.GetTradeCategories;

/// <summary>Lists the TradeCategories of a HousingPromotion owned by the currently authenticated tenant.</summary>
public class GetTradeCategoriesUseCase
{
    private readonly ICurrentUser _currentUser;
    private readonly IHousingPromotionRepository _housingPromotionRepository;
    private readonly ITradeCategoryRepository _tradeCategoryRepository;

    public GetTradeCategoriesUseCase(
        ICurrentUser currentUser,
        IHousingPromotionRepository housingPromotionRepository,
        ITradeCategoryRepository tradeCategoryRepository)
    {
        _currentUser = currentUser;
        _housingPromotionRepository = housingPromotionRepository;
        _tradeCategoryRepository = tradeCategoryRepository;
    }

    public async Task<Result<IReadOnlyList<TradeCategoryDto>>> ExecuteAsync(Guid housingPromotionId, CancellationToken cancellationToken = default)
    {
        if (_currentUser.DeveloperCompanyId is not { } developerCompanyId)
        {
            return Result.Failure<IReadOnlyList<TradeCategoryDto>>("El usuario actual no está vinculado a ninguna promotora.");
        }

        var promotion = await _housingPromotionRepository.GetByIdAsync(housingPromotionId, cancellationToken);

        if (promotion is null || promotion.DeveloperCompanyId != developerCompanyId)
        {
            return Result.Failure<IReadOnlyList<TradeCategoryDto>>("No se ha encontrado la promoción.");
        }

        var tradeCategories = await _tradeCategoryRepository.GetByHousingPromotionIdAsync(housingPromotionId, cancellationToken);

        var dtos = tradeCategories
            .Select(t => new TradeCategoryDto(t.Id, t.HousingPromotionId, t.Name, t.SelectionCutOffDateUtc, t.CreatedAtUtc, t.UpdatedAtUtc))
            .ToList();

        return Result.Success<IReadOnlyList<TradeCategoryDto>>(dtos);
    }
}

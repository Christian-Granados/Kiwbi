using Kiwbi.Domain.Interfaces;

namespace Kiwbi.Domain.Customizations;

/// <summary>Repository for the TradeCategory aggregate, extending the generic repository with promotion-specific queries.</summary>
public interface ITradeCategoryRepository : IRepository<TradeCategory>
{
    Task<IReadOnlyList<TradeCategory>> GetByHousingPromotionIdAsync(Guid housingPromotionId, CancellationToken cancellationToken = default);

    Task<bool> ExistsByHousingPromotionIdAsync(Guid housingPromotionId, CancellationToken cancellationToken = default);
}

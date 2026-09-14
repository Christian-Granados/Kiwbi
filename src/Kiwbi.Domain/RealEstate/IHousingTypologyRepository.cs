using Kiwbi.Domain.Interfaces;

namespace Kiwbi.Domain.RealEstate;

/// <summary>Repository for the HousingTypology aggregate, extending the generic repository with promotion-specific queries.</summary>
public interface IHousingTypologyRepository : IRepository<HousingTypology>
{
    Task<IReadOnlyList<HousingTypology>> GetByHousingPromotionIdAsync(Guid housingPromotionId, CancellationToken cancellationToken = default);

    Task<bool> ExistsByHousingPromotionIdAsync(Guid housingPromotionId, CancellationToken cancellationToken = default);
}

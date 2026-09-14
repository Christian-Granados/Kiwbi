using Kiwbi.Domain.Interfaces;

namespace Kiwbi.Domain.RealEstate;

/// <summary>Repository for the HousingPromotion aggregate, extending the generic repository with tenant-specific queries.</summary>
public interface IHousingPromotionRepository : IRepository<HousingPromotion>
{
    Task<IReadOnlyList<HousingPromotion>> GetByDeveloperCompanyIdAsync(Guid developerCompanyId, CancellationToken cancellationToken = default);
}

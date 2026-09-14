using Kiwbi.Domain.Interfaces;

namespace Kiwbi.Domain.RealEstate;

/// <summary>Repository for the HousingUnit aggregate, extending the generic repository with promotion/typology-specific queries.</summary>
public interface IHousingUnitRepository : IRepository<HousingUnit>
{
    Task<IReadOnlyList<HousingUnit>> GetByHousingPromotionIdAsync(Guid housingPromotionId, CancellationToken cancellationToken = default);

    Task<bool> ExistsByHousingPromotionIdAsync(Guid housingPromotionId, CancellationToken cancellationToken = default);

    Task<bool> ExistsByHousingTypologyIdAsync(Guid housingTypologyId, CancellationToken cancellationToken = default);

    Task<bool> ExistsWithFloorAndDoorAsync(
        Guid housingPromotionId,
        string floor,
        string door,
        Guid? excludeUnitId = null,
        CancellationToken cancellationToken = default);
}

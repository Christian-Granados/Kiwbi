using Kiwbi.Domain.Interfaces;

namespace Kiwbi.Domain.Onboarding;

/// <summary>Repository for the HousingUnitBuyer aggregate, extending the generic repository with unit/buyer-specific queries.</summary>
public interface IHousingUnitBuyerRepository : IRepository<HousingUnitBuyer>
{
    Task<IReadOnlyList<HousingUnitBuyer>> GetByHousingUnitIdAsync(Guid housingUnitId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<HousingUnitBuyer>> GetByHousingUnitIdsAsync(IEnumerable<Guid> housingUnitIds, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<HousingUnitBuyer>> GetByBuyerUserIdAsync(string buyerUserId, CancellationToken cancellationToken = default);

    Task<bool> ExistsByHousingUnitIdAndBuyerUserIdAsync(Guid housingUnitId, string buyerUserId, CancellationToken cancellationToken = default);

    Task<bool> ExistsByHousingUnitIdAsync(Guid housingUnitId, CancellationToken cancellationToken = default);
}

using Kiwbi.Domain.Interfaces;

namespace Kiwbi.Domain.Onboarding;

/// <summary>Repository for the BuyerInvitation aggregate, extending the generic repository with unit/token-specific queries.</summary>
public interface IBuyerInvitationRepository : IRepository<BuyerInvitation>
{
    Task<IReadOnlyList<BuyerInvitation>> GetByHousingUnitIdAsync(Guid housingUnitId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<BuyerInvitation>> GetByHousingUnitIdsAsync(IEnumerable<Guid> housingUnitIds, CancellationToken cancellationToken = default);

    Task<BuyerInvitation?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);

    Task<bool> ExistsPendingByHousingUnitIdAndEmailAsync(Guid housingUnitId, string email, CancellationToken cancellationToken = default);

    Task<bool> ExistsByHousingUnitIdAsync(Guid housingUnitId, CancellationToken cancellationToken = default);
}

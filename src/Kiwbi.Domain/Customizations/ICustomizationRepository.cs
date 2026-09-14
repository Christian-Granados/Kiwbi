using Kiwbi.Domain.Interfaces;

namespace Kiwbi.Domain.Customizations;

/// <summary>Repository for the Customization aggregate, extending the generic repository with trade-category-specific queries.</summary>
public interface ICustomizationRepository : IRepository<Customization>
{
    Task<IReadOnlyList<Customization>> GetByTradeCategoryIdAsync(Guid tradeCategoryId, CancellationToken cancellationToken = default);

    Task<bool> ExistsByTradeCategoryIdAsync(Guid tradeCategoryId, CancellationToken cancellationToken = default);

    Task<bool> ExistsByHousingTypologyIdAsync(Guid housingTypologyId, CancellationToken cancellationToken = default);

    Task<bool> ExistsByHousingUnitIdAsync(Guid housingUnitId, CancellationToken cancellationToken = default);
}

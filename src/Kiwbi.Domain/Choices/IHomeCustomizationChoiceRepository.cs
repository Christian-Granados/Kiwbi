using Kiwbi.Domain.Interfaces;

namespace Kiwbi.Domain.Choices;

/// <summary>Repository for the HomeCustomizationChoice aggregate, extending the generic repository with unit/customization-specific queries.</summary>
public interface IHomeCustomizationChoiceRepository : IRepository<HomeCustomizationChoice>
{
    Task<IReadOnlyList<HomeCustomizationChoice>> GetByHousingUnitIdAsync(Guid housingUnitId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<HomeCustomizationChoice>> GetByHousingUnitIdsAsync(IEnumerable<Guid> housingUnitIds, CancellationToken cancellationToken = default);

    Task<HomeCustomizationChoice?> GetByHousingUnitIdAndCustomizationIdAsync(Guid housingUnitId, Guid customizationId, CancellationToken cancellationToken = default);

    Task<bool> ExistsByCustomizationIdAsync(Guid customizationId, CancellationToken cancellationToken = default);
}

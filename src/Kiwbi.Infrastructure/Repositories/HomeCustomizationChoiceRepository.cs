using Kiwbi.Domain.Choices;
using Kiwbi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Kiwbi.Infrastructure.Repositories;

public class HomeCustomizationChoiceRepository : IHomeCustomizationChoiceRepository
{
    private readonly KiwbiDbContext _dbContext;

    public HomeCustomizationChoiceRepository(KiwbiDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<HomeCustomizationChoice?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _dbContext.HomeCustomizationChoices.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public async Task<IReadOnlyList<HomeCustomizationChoice>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await _dbContext.HomeCustomizationChoices.ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<HomeCustomizationChoice>> GetByHousingUnitIdAsync(Guid housingUnitId, CancellationToken cancellationToken = default) =>
        await _dbContext.HomeCustomizationChoices
            .Where(c => c.HousingUnitId == housingUnitId)
            .ToListAsync(cancellationToken);

    public Task<HomeCustomizationChoice?> GetByHousingUnitIdAndCustomizationIdAsync(Guid housingUnitId, Guid customizationId, CancellationToken cancellationToken = default) =>
        _dbContext.HomeCustomizationChoices
            .FirstOrDefaultAsync(c => c.HousingUnitId == housingUnitId && c.CustomizationId == customizationId, cancellationToken);

    public Task<bool> ExistsByCustomizationIdAsync(Guid customizationId, CancellationToken cancellationToken = default) =>
        _dbContext.HomeCustomizationChoices.AnyAsync(c => c.CustomizationId == customizationId, cancellationToken);

    public async Task AddAsync(HomeCustomizationChoice entity, CancellationToken cancellationToken = default) =>
        await _dbContext.HomeCustomizationChoices.AddAsync(entity, cancellationToken);

    public void Update(HomeCustomizationChoice entity) => _dbContext.HomeCustomizationChoices.Update(entity);

    public void Remove(HomeCustomizationChoice entity) => _dbContext.HomeCustomizationChoices.Remove(entity);
}

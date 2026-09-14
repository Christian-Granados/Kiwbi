using Kiwbi.Domain.Customizations;
using Kiwbi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Kiwbi.Infrastructure.Repositories;

public class CustomizationRepository : ICustomizationRepository
{
    private readonly KiwbiDbContext _dbContext;

    public CustomizationRepository(KiwbiDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Customization?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _dbContext.Customizations.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Customization>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await _dbContext.Customizations.ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Customization>> GetByTradeCategoryIdAsync(Guid tradeCategoryId, CancellationToken cancellationToken = default) =>
        await _dbContext.Customizations
            .Where(c => c.TradeCategoryId == tradeCategoryId)
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);

    public Task<bool> ExistsByTradeCategoryIdAsync(Guid tradeCategoryId, CancellationToken cancellationToken = default) =>
        _dbContext.Customizations.AnyAsync(c => c.TradeCategoryId == tradeCategoryId, cancellationToken);

    public Task<bool> ExistsByHousingTypologyIdAsync(Guid housingTypologyId, CancellationToken cancellationToken = default) =>
        _dbContext.Customizations
            .SelectMany(c => c.Assignments)
            .AnyAsync(a => a.HousingTypologyId == housingTypologyId, cancellationToken);

    public Task<bool> ExistsByHousingUnitIdAsync(Guid housingUnitId, CancellationToken cancellationToken = default) =>
        _dbContext.Customizations
            .SelectMany(c => c.Assignments)
            .AnyAsync(a => a.HousingUnitId == housingUnitId, cancellationToken);

    public async Task AddAsync(Customization entity, CancellationToken cancellationToken = default) =>
        await _dbContext.Customizations.AddAsync(entity, cancellationToken);

    public void Update(Customization entity) => _dbContext.Customizations.Update(entity);

    public void Remove(Customization entity) => _dbContext.Customizations.Remove(entity);
}

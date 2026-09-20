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

    // EF Core's automatic change detection decides Added vs Modified for entities discovered via
    // navigation-collection mutation (not an explicit Add/AddAsync) based on whether their key already
    // has a non-default value. Since CustomizationOption/CustomizationAssignment ids are Guids assigned
    // by the domain constructor (BaseEntity), they're never the CLR default, so a brand-new item added to
    // Customization.Options/Assignments after loading the aggregate gets misdetected as an existing row to
    // UPDATE (DbUpdateConcurrencyException, 0 rows affected) instead of INSERT. Fix: compare against what's
    // actually persisted (a fresh no-tracking read) and explicitly mark anything not found there as Added.
    public void Update(Customization entity)
    {
        var persisted = _dbContext.Customizations
            .AsNoTracking()
            .Where(c => c.Id == entity.Id)
            .Select(c => new
            {
                OptionIds = c.Options.Select(o => o.Id).ToList(),
                AssignmentIds = c.Assignments.Select(a => a.Id).ToList(),
            })
            .FirstOrDefault();

        var persistedOptionIds = persisted?.OptionIds ?? [];
        var persistedAssignmentIds = persisted?.AssignmentIds ?? [];

        foreach (var option in entity.Options)
        {
            if (!persistedOptionIds.Contains(option.Id))
            {
                _dbContext.Entry(option).State = EntityState.Added;
            }
        }

        foreach (var assignment in entity.Assignments)
        {
            if (!persistedAssignmentIds.Contains(assignment.Id))
            {
                _dbContext.Entry(assignment).State = EntityState.Added;
            }
        }
    }

    public void Remove(Customization entity) => _dbContext.Customizations.Remove(entity);
}

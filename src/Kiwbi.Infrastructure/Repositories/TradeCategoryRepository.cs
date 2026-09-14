using Kiwbi.Domain.Customizations;
using Kiwbi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Kiwbi.Infrastructure.Repositories;

public class TradeCategoryRepository : ITradeCategoryRepository
{
    private readonly KiwbiDbContext _dbContext;

    public TradeCategoryRepository(KiwbiDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<TradeCategory?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _dbContext.TradeCategories.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

    public async Task<IReadOnlyList<TradeCategory>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await _dbContext.TradeCategories.ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<TradeCategory>> GetByHousingPromotionIdAsync(Guid housingPromotionId, CancellationToken cancellationToken = default) =>
        await _dbContext.TradeCategories
            .Where(t => t.HousingPromotionId == housingPromotionId)
            .OrderBy(t => t.Name)
            .ToListAsync(cancellationToken);

    public Task<bool> ExistsByHousingPromotionIdAsync(Guid housingPromotionId, CancellationToken cancellationToken = default) =>
        _dbContext.TradeCategories.AnyAsync(t => t.HousingPromotionId == housingPromotionId, cancellationToken);

    public async Task AddAsync(TradeCategory entity, CancellationToken cancellationToken = default) =>
        await _dbContext.TradeCategories.AddAsync(entity, cancellationToken);

    public void Update(TradeCategory entity) => _dbContext.TradeCategories.Update(entity);

    public void Remove(TradeCategory entity) => _dbContext.TradeCategories.Remove(entity);
}

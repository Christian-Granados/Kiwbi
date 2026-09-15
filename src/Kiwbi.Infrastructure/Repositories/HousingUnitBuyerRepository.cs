using Kiwbi.Domain.Onboarding;
using Kiwbi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Kiwbi.Infrastructure.Repositories;

public class HousingUnitBuyerRepository : IHousingUnitBuyerRepository
{
    private readonly KiwbiDbContext _dbContext;

    public HousingUnitBuyerRepository(KiwbiDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<HousingUnitBuyer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _dbContext.HousingUnitBuyers.FirstOrDefaultAsync(b => b.Id == id, cancellationToken);

    public async Task<IReadOnlyList<HousingUnitBuyer>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await _dbContext.HousingUnitBuyers.ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<HousingUnitBuyer>> GetByHousingUnitIdAsync(Guid housingUnitId, CancellationToken cancellationToken = default) =>
        await _dbContext.HousingUnitBuyers
            .Where(b => b.HousingUnitId == housingUnitId)
            .OrderBy(b => b.CreatedAtUtc)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<HousingUnitBuyer>> GetByBuyerUserIdAsync(string buyerUserId, CancellationToken cancellationToken = default) =>
        await _dbContext.HousingUnitBuyers
            .Where(b => b.BuyerUserId == buyerUserId)
            .ToListAsync(cancellationToken);

    public Task<bool> ExistsByHousingUnitIdAndBuyerUserIdAsync(Guid housingUnitId, string buyerUserId, CancellationToken cancellationToken = default) =>
        _dbContext.HousingUnitBuyers.AnyAsync(b => b.HousingUnitId == housingUnitId && b.BuyerUserId == buyerUserId, cancellationToken);

    public Task<bool> ExistsByHousingUnitIdAsync(Guid housingUnitId, CancellationToken cancellationToken = default) =>
        _dbContext.HousingUnitBuyers.AnyAsync(b => b.HousingUnitId == housingUnitId, cancellationToken);

    public async Task AddAsync(HousingUnitBuyer entity, CancellationToken cancellationToken = default) =>
        await _dbContext.HousingUnitBuyers.AddAsync(entity, cancellationToken);

    public void Update(HousingUnitBuyer entity) => _dbContext.HousingUnitBuyers.Update(entity);

    public void Remove(HousingUnitBuyer entity) => _dbContext.HousingUnitBuyers.Remove(entity);
}

using Kiwbi.Domain.RealEstate;
using Kiwbi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Kiwbi.Infrastructure.Repositories;

public class HousingPromotionRepository : IHousingPromotionRepository
{
    private readonly KiwbiDbContext _dbContext;

    public HousingPromotionRepository(KiwbiDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<HousingPromotion?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _dbContext.HousingPromotions.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public async Task<IReadOnlyList<HousingPromotion>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await _dbContext.HousingPromotions.ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<HousingPromotion>> GetByDeveloperCompanyIdAsync(Guid developerCompanyId, CancellationToken cancellationToken = default) =>
        await _dbContext.HousingPromotions
            .Where(p => p.DeveloperCompanyId == developerCompanyId)
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(HousingPromotion entity, CancellationToken cancellationToken = default) =>
        await _dbContext.HousingPromotions.AddAsync(entity, cancellationToken);

    public void Update(HousingPromotion entity) => _dbContext.HousingPromotions.Update(entity);

    public void Remove(HousingPromotion entity) => _dbContext.HousingPromotions.Remove(entity);
}

using Kiwbi.Domain.RealEstate;
using Kiwbi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Kiwbi.Infrastructure.Repositories;

public class HousingTypologyRepository : IHousingTypologyRepository
{
    private readonly KiwbiDbContext _dbContext;

    public HousingTypologyRepository(KiwbiDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<HousingTypology?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _dbContext.HousingTypologies.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

    public async Task<IReadOnlyList<HousingTypology>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await _dbContext.HousingTypologies.ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<HousingTypology>> GetByHousingPromotionIdAsync(Guid housingPromotionId, CancellationToken cancellationToken = default) =>
        await _dbContext.HousingTypologies
            .Where(t => t.HousingPromotionId == housingPromotionId)
            .OrderBy(t => t.Name)
            .ToListAsync(cancellationToken);

    public Task<bool> ExistsByHousingPromotionIdAsync(Guid housingPromotionId, CancellationToken cancellationToken = default) =>
        _dbContext.HousingTypologies.AnyAsync(t => t.HousingPromotionId == housingPromotionId, cancellationToken);

    public async Task AddAsync(HousingTypology entity, CancellationToken cancellationToken = default) =>
        await _dbContext.HousingTypologies.AddAsync(entity, cancellationToken);

    public void Update(HousingTypology entity) => _dbContext.HousingTypologies.Update(entity);

    public void Remove(HousingTypology entity) => _dbContext.HousingTypologies.Remove(entity);
}

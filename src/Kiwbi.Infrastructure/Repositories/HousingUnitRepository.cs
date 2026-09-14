using Kiwbi.Domain.RealEstate;
using Kiwbi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Kiwbi.Infrastructure.Repositories;

public class HousingUnitRepository : IHousingUnitRepository
{
    private readonly KiwbiDbContext _dbContext;

    public HousingUnitRepository(KiwbiDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<HousingUnit?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _dbContext.HousingUnits.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

    public async Task<IReadOnlyList<HousingUnit>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await _dbContext.HousingUnits.ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<HousingUnit>> GetByHousingPromotionIdAsync(Guid housingPromotionId, CancellationToken cancellationToken = default) =>
        await _dbContext.HousingUnits
            .Where(u => u.HousingPromotionId == housingPromotionId)
            .OrderBy(u => u.Floor).ThenBy(u => u.Door)
            .ToListAsync(cancellationToken);

    public Task<bool> ExistsByHousingPromotionIdAsync(Guid housingPromotionId, CancellationToken cancellationToken = default) =>
        _dbContext.HousingUnits.AnyAsync(u => u.HousingPromotionId == housingPromotionId, cancellationToken);

    public Task<bool> ExistsByHousingTypologyIdAsync(Guid housingTypologyId, CancellationToken cancellationToken = default) =>
        _dbContext.HousingUnits.AnyAsync(u => u.HousingTypologyId == housingTypologyId, cancellationToken);

    public Task<bool> ExistsWithFloorAndDoorAsync(
        Guid housingPromotionId,
        string floor,
        string door,
        Guid? excludeUnitId = null,
        CancellationToken cancellationToken = default) =>
        _dbContext.HousingUnits.AnyAsync(
            u => u.HousingPromotionId == housingPromotionId
                && u.Floor == floor
                && u.Door == door
                && (excludeUnitId == null || u.Id != excludeUnitId),
            cancellationToken);

    public async Task AddAsync(HousingUnit entity, CancellationToken cancellationToken = default) =>
        await _dbContext.HousingUnits.AddAsync(entity, cancellationToken);

    public void Update(HousingUnit entity) => _dbContext.HousingUnits.Update(entity);

    public void Remove(HousingUnit entity) => _dbContext.HousingUnits.Remove(entity);
}

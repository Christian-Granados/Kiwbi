using Kiwbi.Domain.Onboarding;
using Kiwbi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Kiwbi.Infrastructure.Repositories;

public class BuyerInvitationRepository : IBuyerInvitationRepository
{
    private readonly KiwbiDbContext _dbContext;

    public BuyerInvitationRepository(KiwbiDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<BuyerInvitation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _dbContext.BuyerInvitations.FirstOrDefaultAsync(i => i.Id == id, cancellationToken);

    public async Task<IReadOnlyList<BuyerInvitation>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await _dbContext.BuyerInvitations.ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<BuyerInvitation>> GetByHousingUnitIdAsync(Guid housingUnitId, CancellationToken cancellationToken = default) =>
        await _dbContext.BuyerInvitations
            .Where(i => i.HousingUnitId == housingUnitId)
            .OrderByDescending(i => i.CreatedAtUtc)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<BuyerInvitation>> GetByHousingUnitIdsAsync(IEnumerable<Guid> housingUnitIds, CancellationToken cancellationToken = default) =>
        await _dbContext.BuyerInvitations
            .Where(i => housingUnitIds.Contains(i.HousingUnitId))
            .ToListAsync(cancellationToken);

    public Task<BuyerInvitation?> GetByTokenAsync(string token, CancellationToken cancellationToken = default) =>
        _dbContext.BuyerInvitations.FirstOrDefaultAsync(i => i.Token == token, cancellationToken);

    public Task<bool> ExistsPendingByHousingUnitIdAndEmailAsync(Guid housingUnitId, string email, CancellationToken cancellationToken = default) =>
        _dbContext.BuyerInvitations.AnyAsync(
            i => i.HousingUnitId == housingUnitId && i.Email == email && i.Status == BuyerInvitationStatus.Pending,
            cancellationToken);

    public Task<bool> ExistsByHousingUnitIdAsync(Guid housingUnitId, CancellationToken cancellationToken = default) =>
        _dbContext.BuyerInvitations.AnyAsync(i => i.HousingUnitId == housingUnitId, cancellationToken);

    public async Task AddAsync(BuyerInvitation entity, CancellationToken cancellationToken = default) =>
        await _dbContext.BuyerInvitations.AddAsync(entity, cancellationToken);

    public void Update(BuyerInvitation entity) => _dbContext.BuyerInvitations.Update(entity);

    public void Remove(BuyerInvitation entity) => _dbContext.BuyerInvitations.Remove(entity);
}

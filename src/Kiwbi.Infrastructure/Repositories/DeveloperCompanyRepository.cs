using Kiwbi.Domain.Developers;
using Kiwbi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Kiwbi.Infrastructure.Repositories;

public class DeveloperCompanyRepository : IDeveloperCompanyRepository
{
    private readonly KiwbiDbContext _dbContext;

    public DeveloperCompanyRepository(KiwbiDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<DeveloperCompany?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _dbContext.DeveloperCompanies.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public async Task<IReadOnlyList<DeveloperCompany>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await _dbContext.DeveloperCompanies.ToListAsync(cancellationToken);

    public async Task AddAsync(DeveloperCompany entity, CancellationToken cancellationToken = default) =>
        await _dbContext.DeveloperCompanies.AddAsync(entity, cancellationToken);

    public void Update(DeveloperCompany entity) => _dbContext.DeveloperCompanies.Update(entity);

    public void Remove(DeveloperCompany entity) => _dbContext.DeveloperCompanies.Remove(entity);
}

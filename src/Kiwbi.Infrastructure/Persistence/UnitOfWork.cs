using Kiwbi.Application.Common;
using Microsoft.EntityFrameworkCore;

namespace Kiwbi.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly KiwbiDbContext _dbContext;

    public UnitOfWork(KiwbiDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _dbContext.SaveChangesAsync(cancellationToken);

    public async Task<TResult> ExecuteInTransactionAsync<TResult>(
        Func<CancellationToken, Task<TResult>> operation,
        CancellationToken cancellationToken = default)
        where TResult : Result
    {
        var strategy = _dbContext.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            var result = await operation(cancellationToken);

            if (result.IsFailure)
            {
                await transaction.RollbackAsync(cancellationToken);
                return result;
            }

            await transaction.CommitAsync(cancellationToken);

            return result;
        });
    }
}

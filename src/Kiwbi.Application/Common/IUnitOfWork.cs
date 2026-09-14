namespace Kiwbi.Application.Common;

/// <summary>Unit of work abstraction to confirm transactional operations without depending on EF Core.</summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>Runs the given operation atomically, committing only if the resulting Result is successful.</summary>
    Task<TResult> ExecuteInTransactionAsync<TResult>(
        Func<CancellationToken, Task<TResult>> operation,
        CancellationToken cancellationToken = default)
        where TResult : Result;
}

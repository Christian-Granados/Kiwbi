namespace Kiwbi.Application.Common;

/// <summary>Port for persisting uploaded files (images/plans) without coupling Application to a concrete storage provider.</summary>
public interface IFileStorageService
{
    Task<Result<string>> SaveAsync(Stream content, string fileName, string subFolder, CancellationToken cancellationToken = default);

    void Delete(string relativePath);
}

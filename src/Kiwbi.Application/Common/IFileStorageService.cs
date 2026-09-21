namespace Kiwbi.Application.Common;

/// <summary>Port for persisting uploaded files (images/plans) without coupling Application to a concrete storage provider.</summary>
public interface IFileStorageService
{
    /// <summary>Saves the file and returns a URL directly usable in a view's src/href (root-relative for local
    /// disk, e.g. "/uploads/x.png", or fully-qualified for a cloud provider, e.g. "https://.../x.png").</summary>
    Task<Result<string>> SaveAsync(Stream content, string fileName, string subFolder, CancellationToken cancellationToken = default);

    /// <summary>Deletes a previously saved file, identified by the same URL SaveAsync returned. A no-op if the
    /// path/URL doesn't correspond to anything this provider manages (e.g. a manually-typed external URL).</summary>
    void Delete(string relativePath);
}

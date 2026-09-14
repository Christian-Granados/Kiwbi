using Kiwbi.Application.Common;
using Microsoft.AspNetCore.Hosting;

namespace Kiwbi.Infrastructure.Storage;

/// <summary>Stores uploaded files on local disk under wwwroot/uploads. Kept behind IFileStorageService so a future cloud provider only needs a new adapter here.</summary>
public class LocalFileStorageService : IFileStorageService
{
    private const long MaxFileSizeBytes = 10 * 1024 * 1024;
    private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".webp", ".pdf"];

    private readonly IWebHostEnvironment _environment;

    public LocalFileStorageService(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public async Task<Result<string>> SaveAsync(Stream content, string fileName, string subFolder, CancellationToken cancellationToken = default)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(extension) || !AllowedExtensions.Contains(extension))
        {
            return Result.Failure<string>("Formato de archivo no permitido. Usa JPG, PNG, WEBP o PDF.");
        }

        if (content.Length > MaxFileSizeBytes)
        {
            return Result.Failure<string>("El archivo supera el tamaño máximo permitido (10 MB).");
        }

        var uploadsRoot = Path.Combine(_environment.WebRootPath, "uploads", subFolder);
        Directory.CreateDirectory(uploadsRoot);

        // Server-generated file name: the original name is never trusted for the path.
        var storedFileName = $"{Guid.NewGuid():N}{extension}";
        var absolutePath = Path.Combine(uploadsRoot, storedFileName);

        content.Position = 0;
        await using (var fileStream = new FileStream(absolutePath, FileMode.Create))
        {
            await content.CopyToAsync(fileStream, cancellationToken);
        }

        var relativePath = Path.Combine("uploads", subFolder, storedFileName).Replace('\\', '/');

        return Result.Success(relativePath);
    }

    public void Delete(string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
        {
            return;
        }

        var absolutePath = Path.Combine(_environment.WebRootPath, relativePath.Replace('/', Path.DirectorySeparatorChar));

        if (File.Exists(absolutePath))
        {
            File.Delete(absolutePath);
        }
    }
}

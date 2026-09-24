using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Kiwbi.Application.Common;
using Microsoft.Extensions.Configuration;

namespace Kiwbi.Infrastructure.Storage;

/// <summary>Stores uploaded files in an S3-compatible bucket (Cloudflare R2 in production - Epic 11, Feature 11.4).
/// Config keys (read directly from IConfiguration, same convention as LoggingBuyerInvitationEmailSender's AppBaseUrl):
/// Storage:S3:ServiceUrl, Storage:S3:AccessKeyId, Storage:S3:SecretAccessKey, Storage:S3:BucketName, Storage:S3:PublicBaseUrl.</summary>
public class S3FileStorageService : IFileStorageService
{
    private const long MaxFileSizeBytes = 10 * 1024 * 1024;
    private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".webp"];

    private readonly IAmazonS3 _s3Client;
    private readonly string _bucketName;
    private readonly string _publicBaseUrl;

    public S3FileStorageService(IConfiguration configuration)
    {
        var serviceUrl = RequireConfig(configuration, "Storage:S3:ServiceUrl");
        var accessKeyId = RequireConfig(configuration, "Storage:S3:AccessKeyId");
        var secretAccessKey = RequireConfig(configuration, "Storage:S3:SecretAccessKey");
        _bucketName = RequireConfig(configuration, "Storage:S3:BucketName");
        _publicBaseUrl = RequireConfig(configuration, "Storage:S3:PublicBaseUrl").TrimEnd('/');

        _s3Client = new AmazonS3Client(accessKeyId, secretAccessKey, new AmazonS3Config
        {
            ServiceURL = serviceUrl,
            ForcePathStyle = true, // required by R2 and most other S3-compatible providers.
            // AWSSDK.S3's newer default (WHEN_SUPPORTED) streams a trailing checksum
            // (STREAMING-AWS4-HMAC-SHA256-PAYLOAD-TRAILER) that R2 rejects with 501 NotImplemented.
            RequestChecksumCalculation = RequestChecksumCalculation.WHEN_REQUIRED,
            ResponseChecksumValidation = ResponseChecksumValidation.WHEN_REQUIRED,
        });
    }

    public async Task<Result<string>> SaveAsync(Stream content, string fileName, string subFolder, CancellationToken cancellationToken = default)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(extension) || !AllowedExtensions.Contains(extension))
        {
            return Result.Failure<string>("Formato de archivo no permitido. Usa JPG, PNG o WEBP.");
        }

        if (content.Length > MaxFileSizeBytes)
        {
            return Result.Failure<string>("El archivo supera el tamaño máximo permitido (10 MB).");
        }

        // Server-generated file name: the original name is never trusted for the object key.
        var key = $"{subFolder}/{Guid.NewGuid():N}{extension}";

        content.Position = 0;
        await _s3Client.PutObjectAsync(new PutObjectRequest
        {
            BucketName = _bucketName,
            Key = key,
            InputStream = content,
            ContentType = GetContentType(extension),
            // R2 doesn't support the SDK's default chunked/streaming SigV4 signature either; our streams are
            // always seekable so a normal single-pass signed hash works fine.
            UseChunkEncoding = false,
        }, cancellationToken);

        return Result.Success($"{_publicBaseUrl}/{key}");
    }

    public void Delete(string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath) || !relativePath.StartsWith(_publicBaseUrl, StringComparison.OrdinalIgnoreCase))
        {
            return; // Not one of ours (e.g. a manually-typed external URL) - safe no-op, same contract as the local adapter.
        }

        var key = relativePath[(_publicBaseUrl.Length + 1)..];

        // The port's Delete is synchronous (matches the local disk adapter); blocking here is acceptable since
        // it's only called from rare admin delete actions, never a hot path.
        _s3Client.DeleteObjectAsync(_bucketName, key).GetAwaiter().GetResult();
    }

    private static string RequireConfig(IConfiguration configuration, string key) =>
        configuration[key] ?? throw new InvalidOperationException($"Falta la configuración obligatoria '{key}' para el almacenamiento S3.");

    private static string GetContentType(string extension) => extension switch
    {
        ".jpg" or ".jpeg" => "image/jpeg",
        ".png" => "image/png",
        ".webp" => "image/webp",
        _ => "application/octet-stream",
    };
}

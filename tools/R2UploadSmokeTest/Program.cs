using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using static Kiwbi.Tools.Shared.ConsoleInput;

// Standalone diagnostic tool (Feature 11.4, Cloudflare R2 smoke test). Intentionally NOT part of Kiwbi.slnx and NOT
// referencing Kiwbi.Infrastructure: it only exists to confirm real R2 credentials/bucket work end-to-end, without
// ever writing them to appsettings.json, user-secrets, or any file. Every value below lives only in local variables
// for the lifetime of this process; nothing is logged or persisted. Run it yourself in your own terminal - never
// paste real credentials into a chat session.

Console.WriteLine("=== Kiwbi - prueba de subida a Cloudflare R2 ===");
Console.WriteLine("Los valores que introduzcas solo se usan en memoria durante esta ejecución. No se guardan en ningún fichero.");
Console.WriteLine();

var serviceUrl = ReadValue("Storage:S3:ServiceUrl (https://<account-id>.r2.cloudflarestorage.com)");
var accessKeyId = ReadValue("Storage:S3:AccessKeyId");
var secretAccessKey = ReadSecret("Storage:S3:SecretAccessKey (oculto)");
var bucketName = ReadValue("Storage:S3:BucketName");
var publicBaseUrl = ReadValue("Storage:S3:PublicBaseUrl (https://pub-xxxx.r2.dev)").TrimEnd('/');

Console.WriteLine();
Console.WriteLine("Subiendo un objeto de prueba...");

using var client = new AmazonS3Client(accessKeyId, secretAccessKey, new AmazonS3Config
{
    ServiceURL = serviceUrl,
    ForcePathStyle = true, // required by R2 and most other S3-compatible providers, same as S3FileStorageService.
    // AWSSDK.S3's newer default (WHEN_SUPPORTED) streams a trailing checksum
    // (STREAMING-AWS4-HMAC-SHA256-PAYLOAD-TRAILER) that R2 rejects with 501 NotImplemented.
    RequestChecksumCalculation = RequestChecksumCalculation.WHEN_REQUIRED,
    ResponseChecksumValidation = ResponseChecksumValidation.WHEN_REQUIRED,
});

var key = $"kiwbi-smoke-test/{Guid.NewGuid():N}.txt";
var body = $"Kiwbi R2 smoke test - {DateTime.UtcNow:O}";

try
{
    using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(body));
    await client.PutObjectAsync(new PutObjectRequest
    {
        BucketName = bucketName,
        Key = key,
        InputStream = stream,
        ContentType = "text/plain",
        // Same fix as S3FileStorageService: R2 rejects the SDK's default chunked/streaming SigV4 signature.
        UseChunkEncoding = false,
    });

    Console.WriteLine();
    Console.WriteLine("Subida correcta.");
    Console.WriteLine($"  Object key: {key}");
    Console.WriteLine($"  URL pública esperada: {publicBaseUrl}/{key}");
    Console.WriteLine();
    Console.WriteLine("Revisa el bucket en el panel de Cloudflare para confirmar que el objeto aparece.");
    Console.Write("¿Borrar ya el objeto de prueba? (s/n): ");

    if ((Console.ReadLine() ?? string.Empty).Trim().StartsWith('s'))
    {
        await client.DeleteObjectAsync(bucketName, key);
        Console.WriteLine("Objeto borrado.");
    }
    else
    {
        Console.WriteLine("Objeto conservado - bórralo manualmente cuando quieras.");
    }
}
catch (Exception ex)
{
    Console.WriteLine();
    Console.WriteLine("La subida ha fallado.");
    Console.WriteLine($"  Tipo: {ex.GetType().Name}");
    Console.WriteLine($"  Mensaje: {ex.Message}");

    if (ex is AmazonS3Exception s3Exception)
    {
        Console.WriteLine($"  Código S3: {s3Exception.ErrorCode}");
        Console.WriteLine($"  Status HTTP: {(int)s3Exception.StatusCode}");
    }
}

Console.WriteLine();
Console.WriteLine("Fin. Esta herramienta no ha escrito ninguna credencial en disco.");

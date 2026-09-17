namespace Kiwbi.Application.Common;

/// <summary>Result of generating a HousingPromotion report file, ready to be returned to the browser (Feature 6.3).</summary>
public sealed record GeneratedReportDto(byte[] Content, string FileName, string ContentType);

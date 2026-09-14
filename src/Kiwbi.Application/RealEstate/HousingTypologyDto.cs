namespace Kiwbi.Application.RealEstate;

public sealed record HousingTypologyDto(
    Guid Id,
    Guid HousingPromotionId,
    string Name,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);

namespace Kiwbi.Application.RealEstate;

public sealed record HousingPromotionDto(
    Guid Id,
    string Name,
    string City,
    string Address,
    string? MasterPlanImagePath,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);

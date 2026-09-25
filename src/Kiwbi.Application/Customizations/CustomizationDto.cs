using Kiwbi.Domain.Customizations;

namespace Kiwbi.Application.Customizations;

public sealed record CustomizationOptionDto(Guid Id, string Name, decimal SurchargeAmount, bool IsDefault, string? ThumbnailImagePath);

public sealed record CustomizationAssignmentDto(Guid Id, CustomizationScope Scope, Guid? HousingTypologyId, Guid? HousingUnitId);

public sealed record CustomizationDto(
    Guid Id,
    Guid TradeCategoryId,
    string Name,
    IReadOnlyList<CustomizationOptionDto> Options,
    IReadOnlyList<CustomizationAssignmentDto> Assignments,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc)
{
    public static CustomizationDto FromEntity(Customization customization) => new(
        customization.Id,
        customization.TradeCategoryId,
        customization.Name,
        customization.Options.Select(o => new CustomizationOptionDto(o.Id, o.Name, o.SurchargeAmount, o.IsDefault, o.ThumbnailImagePath)).ToList(),
        customization.Assignments.Select(a => new CustomizationAssignmentDto(a.Id, a.Scope, a.HousingTypologyId, a.HousingUnitId)).ToList(),
        customization.CreatedAtUtc,
        customization.UpdatedAtUtc);
}

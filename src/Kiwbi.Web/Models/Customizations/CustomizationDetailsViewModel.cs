using Kiwbi.Application.Customizations;
using Kiwbi.Domain.Customizations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Kiwbi.Web.Models.Customizations;

public class CustomizationOptionViewModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal SurchargeAmount { get; set; }
    public bool IsDefault { get; set; }
}

public class CustomizationAssignmentViewModel
{
    public Guid Id { get; set; }
    public CustomizationScope Scope { get; set; }
    public string TargetLabel { get; set; } = string.Empty;
}

public class CustomizationDetailsViewModel
{
    public Guid Id { get; set; }
    public Guid TradeCategoryId { get; set; }
    public Guid HousingPromotionId { get; set; }
    public string Name { get; set; } = string.Empty;

    public List<CustomizationOptionViewModel> Options { get; set; } = new();
    public List<CustomizationAssignmentViewModel> Assignments { get; set; } = new();

    public bool IsAssignedToWholePromotion { get; set; }

    public List<SelectListItem> AvailableTypologies { get; set; } = new();
    public List<SelectListItem> AvailableUnits { get; set; } = new();

    public static CustomizationDetailsViewModel FromDto(
        CustomizationDto dto,
        Guid housingPromotionId,
        IReadOnlyDictionary<Guid, string> typologyNamesById,
        IReadOnlyDictionary<Guid, string> unitLabelsById)
    {
        return new CustomizationDetailsViewModel
        {
            Id = dto.Id,
            TradeCategoryId = dto.TradeCategoryId,
            HousingPromotionId = housingPromotionId,
            Name = dto.Name,
            Options = dto.Options
                .Select(o => new CustomizationOptionViewModel { Id = o.Id, Name = o.Name, SurchargeAmount = o.SurchargeAmount, IsDefault = o.IsDefault })
                .ToList(),
            Assignments = dto.Assignments
                .Select(a => new CustomizationAssignmentViewModel
                {
                    Id = a.Id,
                    Scope = a.Scope,
                    TargetLabel = a.Scope switch
                    {
                        CustomizationScope.WholePromotion => "Toda la promoción",
                        CustomizationScope.Typology => typologyNamesById.GetValueOrDefault(a.HousingTypologyId!.Value, "Tipología eliminada"),
                        CustomizationScope.Unit => unitLabelsById.GetValueOrDefault(a.HousingUnitId!.Value, "Vivienda eliminada"),
                        _ => "-",
                    },
                })
                .ToList(),
            IsAssignedToWholePromotion = dto.Assignments.Any(a => a.Scope == CustomizationScope.WholePromotion),
        };
    }
}

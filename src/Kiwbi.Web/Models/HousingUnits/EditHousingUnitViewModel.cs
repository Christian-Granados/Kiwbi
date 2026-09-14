using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Kiwbi.Web.Models.HousingUnits;

public class EditHousingUnitViewModel
{
    public Guid Id { get; set; }
    public Guid HousingPromotionId { get; set; }

    [Display(Name = "Tipología")]
    public Guid? HousingTypologyId { get; set; }

    [Required(ErrorMessage = "La planta es obligatoria.")]
    [StringLength(50)]
    [Display(Name = "Planta")]
    public string Floor { get; set; } = string.Empty;

    [Required(ErrorMessage = "La puerta es obligatoria.")]
    [StringLength(50)]
    [Display(Name = "Puerta")]
    public string Door { get; set; } = string.Empty;

    [Required(ErrorMessage = "La superficie construida es obligatoria.")]
    [Range(0.01, double.MaxValue, ErrorMessage = "La superficie construida debe ser mayor que cero.")]
    [Display(Name = "Superficie construida (m²)")]
    public decimal BuiltAreaSqm { get; set; }

    [Display(Name = "Superficie útil (m²)")]
    public decimal? UsableAreaSqm { get; set; }

    public string? FloorPlanImagePath { get; set; }

    [Display(Name = "Reemplazar plano específico")]
    public IFormFile? FloorPlanImageFile { get; set; }

    public List<SelectListItem> TypologyOptions { get; set; } = [];
}

using System.ComponentModel.DataAnnotations;
using Kiwbi.Domain.Customizations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Kiwbi.Web.Models.Customizations;

public class CreateCustomizationViewModel
{
    public Guid HousingPromotionId { get; set; }

    [Display(Name = "Gremio")]
    public Guid TradeCategoryId { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(200)]
    [Display(Name = "Nombre")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre de la opción por defecto es obligatorio.")]
    [StringLength(200)]
    [Display(Name = "Nombre de la opción por defecto")]
    public string DefaultOptionName { get; set; } = string.Empty;

    [Range(0, 1_000_000, ErrorMessage = "El sobrecoste no puede ser negativo.")]
    [Display(Name = "Sobrecoste de la opción por defecto")]
    public decimal DefaultOptionSurchargeAmount { get; set; }

    [Display(Name = "Se aplica a")]
    public CustomizationScope Scope { get; set; } = CustomizationScope.WholePromotion;

    [Display(Name = "Tipologías")]
    public List<Guid> SelectedHousingTypologyIds { get; set; } = new();

    [Display(Name = "Viviendas")]
    public List<Guid> SelectedHousingUnitIds { get; set; } = new();

    public List<SelectListItem> AvailableTradeCategories { get; set; } = new();

    public List<SelectListItem> AvailableScopes { get; set; } = new();

    public List<SelectListItem> AvailableTypologies { get; set; } = new();

    public List<SelectListItem> AvailableUnits { get; set; } = new();
}

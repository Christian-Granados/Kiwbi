using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Kiwbi.Web.Models.HousingPromotions;

public class EditHousingPromotionViewModel
{
    public Guid Id { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(200)]
    [Display(Name = "Nombre")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "La ciudad es obligatoria.")]
    [StringLength(150)]
    [Display(Name = "Ciudad")]
    public string City { get; set; } = string.Empty;

    [Required(ErrorMessage = "La dirección es obligatoria.")]
    [StringLength(300)]
    [Display(Name = "Dirección")]
    public string Address { get; set; } = string.Empty;

    public string? MasterPlanImagePath { get; set; }

    [Display(Name = "Reemplazar plano general")]
    public IFormFile? MasterPlanImageFile { get; set; }
}

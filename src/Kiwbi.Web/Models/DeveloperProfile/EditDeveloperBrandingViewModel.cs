using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Kiwbi.Web.Models.DeveloperProfile;

public class EditDeveloperBrandingViewModel
{
    [Required(ErrorMessage = "El color primario es obligatorio.")]
    [Display(Name = "Color primario")]
    [RegularExpression("^#[0-9A-Fa-f]{6}$", ErrorMessage = "El color debe tener el formato #RRGGBB.")]
    public string PrimaryColor { get; set; } = "#000000";

    [Display(Name = "Color secundario")]
    [RegularExpression("^(#[0-9A-Fa-f]{6})?$", ErrorMessage = "El color debe tener el formato #RRGGBB.")]
    public string? SecondaryColor { get; set; }

    [Display(Name = "Ruta del logo")]
    [StringLength(500)]
    public string? LogoPath { get; set; }

    public IFormFile? LogoImageFile { get; set; }
}

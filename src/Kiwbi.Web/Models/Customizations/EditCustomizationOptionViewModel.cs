using System.ComponentModel.DataAnnotations;

namespace Kiwbi.Web.Models.Customizations;

public class EditCustomizationOptionViewModel
{
    public Guid CustomizationId { get; set; }
    public Guid OptionId { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(200)]
    [Display(Name = "Nombre")]
    public string Name { get; set; } = string.Empty;

    [Range(0, 1_000_000, ErrorMessage = "El sobrecoste no puede ser negativo.")]
    [Display(Name = "Sobrecoste")]
    public decimal SurchargeAmount { get; set; }
}

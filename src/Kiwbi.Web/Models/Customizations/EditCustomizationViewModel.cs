using System.ComponentModel.DataAnnotations;

namespace Kiwbi.Web.Models.Customizations;

public class EditCustomizationViewModel
{
    public Guid Id { get; set; }
    public Guid TradeCategoryId { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(200)]
    [Display(Name = "Nombre")]
    public string Name { get; set; } = string.Empty;
}

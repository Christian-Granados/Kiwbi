using System.ComponentModel.DataAnnotations;

namespace Kiwbi.Web.Models.DeveloperProfile;

public class EditDeveloperProfileViewModel
{
    [Required(ErrorMessage = "El nombre de la promotora es obligatorio.")]
    [StringLength(200)]
    [Display(Name = "Nombre de la promotora")]
    public string Name { get; set; } = string.Empty;
}

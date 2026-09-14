using System.ComponentModel.DataAnnotations;

namespace Kiwbi.Web.Models.HousingTypologies;

public class CreateHousingTypologyViewModel
{
    public Guid HousingPromotionId { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(200)]
    [Display(Name = "Nombre")]
    public string Name { get; set; } = string.Empty;
}

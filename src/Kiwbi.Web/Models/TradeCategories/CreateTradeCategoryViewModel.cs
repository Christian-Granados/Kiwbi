using System.ComponentModel.DataAnnotations;

namespace Kiwbi.Web.Models.TradeCategories;

public class CreateTradeCategoryViewModel
{
    public Guid HousingPromotionId { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(200)]
    [Display(Name = "Nombre")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "La fecha límite de selección es obligatoria.")]
    [Display(Name = "Fecha límite de selección (UTC)")]
    [DataType(DataType.DateTime)]
    public DateTime? SelectionCutOffDateUtc { get; set; }
}

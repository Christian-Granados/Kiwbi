using System.ComponentModel.DataAnnotations;

namespace Kiwbi.Web.Models.Onboarding;

public class InviteBuyerViewModel
{
    public Guid HousingUnitId { get; set; }

    [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
    [EmailAddress(ErrorMessage = "El correo electrónico no es válido.")]
    public string Email { get; set; } = string.Empty;
}

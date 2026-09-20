using System.ComponentModel.DataAnnotations;

namespace Kiwbi.Web.Models.Onboarding;

public class AcceptInvitationViewModel
{
    public string Token { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "La contraseña debe tener al menos {2} caracteres.")]
    [DataType(DataType.Password)]
    [Display(Name = "Contraseña")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Confirma la contraseña.")]
    [DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "Las contraseñas no coinciden.")]
    [Display(Name = "Confirmar contraseña")]
    public string ConfirmPassword { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;
    public string Floor { get; set; } = string.Empty;
    public string Door { get; set; } = string.Empty;
    public string HousingPromotionName { get; set; } = string.Empty;
    public bool AccountAlreadyExists { get; set; }
    public bool CanAccept { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool IsExpired { get; set; }

    /// <summary>Nombre/logo de la promotora invitante (Epic 9), para el acento de marca de la pantalla de aceptación.</summary>
    public string CompanyName { get; set; } = string.Empty;
    public string? CompanyLogoPath { get; set; }
}

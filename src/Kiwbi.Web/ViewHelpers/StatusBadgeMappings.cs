using Kiwbi.Domain.Choices;
using Kiwbi.Domain.RealEstate;

namespace Kiwbi.Web.ViewHelpers;

/// <summary>A (CssClass, Label) pair for rendering `&lt;span class="kiwbi-badge {CssClass}"&gt;{Label}&lt;/span&gt;`.</summary>
public readonly record struct StatusBadge(string CssClass, string Label);

/// <summary>Maps the real domain status enums to the `kiwbi-badge-*` CSS classes/Spanish labels already validated in
/// wwwroot/design-preview (Feature 8.2 Fase 1). HomeCustomizationChoiceStatus and BuyerInvitationStatus colors are
/// deliberately fixed (not tenant-accent) so they read consistently across every promotora brand (Ronda 4/5 decision).</summary>
public static class StatusBadgeMappings
{
    public static StatusBadge ToBadge(this HousingUnitStatus status) => status switch
    {
        HousingUnitStatus.Available => new StatusBadge("kiwbi-badge-success", "Disponible"),
        HousingUnitStatus.Reserved => new StatusBadge("kiwbi-badge-amber", "Reservada"),
        HousingUnitStatus.Sold => new StatusBadge("kiwbi-badge-neutral", "Vendida"),
        _ => throw new ArgumentOutOfRangeException(nameof(status)),
    };

    /// <summary>Convenience for ViewModels that already expose the status as a string (`enum.ToString()`).</summary>
    public static StatusBadge ToHousingUnitStatusBadge(this string status) => Enum.Parse<HousingUnitStatus>(status).ToBadge();

    public static StatusBadge ToBadge(this HomeCustomizationChoiceStatus status) => status switch
    {
        HomeCustomizationChoiceStatus.Pending => new StatusBadge("kiwbi-badge-pendiente", "Pendiente"),
        HomeCustomizationChoiceStatus.Selected => new StatusBadge("kiwbi-badge-seleccionada", "Seleccionada"),
        HomeCustomizationChoiceStatus.Confirmed => new StatusBadge("kiwbi-badge-confirmada", "Confirmada"),
        HomeCustomizationChoiceStatus.Paid => new StatusBadge("kiwbi-badge-pagada", "Pagada"),
        _ => throw new ArgumentOutOfRangeException(nameof(status)),
    };

    /// <summary>BuyerInvitationStatus only persists Pending/Accepted/Cancelled; "Caducada" is a 4th computed visual
    /// state (Pending + IsExpired), not a persisted status (see BuyerInvitation.IsExpired).</summary>
    public static StatusBadge ToBuyerInvitationStatusBadge(this string status, bool isExpired) => status switch
    {
        "Accepted" => new StatusBadge("kiwbi-badge-success", "Aceptada"),
        "Cancelled" => new StatusBadge("kiwbi-badge-neutral", "Cancelada"),
        "Pending" when isExpired => new StatusBadge("kiwbi-badge-danger", "Caducada"),
        "Pending" => new StatusBadge("kiwbi-badge-amber", "Pendiente"),
        _ => throw new ArgumentOutOfRangeException(nameof(status)),
    };
}

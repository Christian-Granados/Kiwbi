namespace Kiwbi.Web.Components;

/// <summary>View model for <see cref="KiwbiSidebarViewComponent"/>: decides Global vs Contextual nav and carries the brand mark data.</summary>
public class KiwbiSidebarViewModel
{
    public bool IsContextual { get; set; }
    public Guid PromotionId { get; set; }
    public string PromotionName { get; set; } = string.Empty;
    public string ActiveController { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string? LogoPath { get; set; }
}

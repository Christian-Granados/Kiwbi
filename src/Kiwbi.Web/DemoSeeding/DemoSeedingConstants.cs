namespace Kiwbi.Web.DemoSeeding;

/// <summary>Fixed identifiers for the demo tenant, used both to seed and to detect/clean up previous demo runs (idempotency).</summary>
public static class DemoSeedingConstants
{
    public const string DeveloperCompanyName = "Kiwbi Demo";
    public const string DeveloperAdminEmail = "demo@kiwbi.test";
    public const string DeveloperAdminPassword = "DemoKiwbi!2026";

    public const string Buyer1Email = "buyer1@kiwbi.test";
    public const string Buyer2Email = "buyer2@kiwbi.test";
    public const string Buyer3Email = "buyer3@kiwbi.test";
    public const string Buyer4Email = "buyer4@kiwbi.test";
    public const string BuyerPassword = "DemoBuyer!2026";
}

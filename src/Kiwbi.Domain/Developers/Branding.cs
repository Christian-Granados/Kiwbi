namespace Kiwbi.Domain.Developers;

/// <summary>Value Object grouping the basic visual branding configuration of a developer company.</summary>
public sealed class Branding : IEquatable<Branding>
{
    public string? LogoPath { get; }
    public BrandColor PrimaryColor { get; }
    public BrandColor? SecondaryColor { get; }

    private Branding(string? logoPath, BrandColor primaryColor, BrandColor? secondaryColor)
    {
        LogoPath = logoPath;
        PrimaryColor = primaryColor;
        SecondaryColor = secondaryColor;
    }

    public static Branding Create(string primaryColor, string? secondaryColor = null, string? logoPath = null)
    {
        var primary = BrandColor.Create(primaryColor);
        var secondary = string.IsNullOrWhiteSpace(secondaryColor) ? null : BrandColor.Create(secondaryColor);

        return new Branding(NormalizeLogoPath(logoPath), primary, secondary);
    }

    public static Branding CreateDefault() => Create(primaryColor: "#000000");

    private static string? NormalizeLogoPath(string? logoPath) =>
        string.IsNullOrWhiteSpace(logoPath) ? null : logoPath.Trim();

    public bool Equals(Branding? other) =>
        other is not null &&
        LogoPath == other.LogoPath &&
        PrimaryColor.Equals(other.PrimaryColor) &&
        Equals(SecondaryColor, other.SecondaryColor);

    public override bool Equals(object? obj) => Equals(obj as Branding);

    public override int GetHashCode() => HashCode.Combine(LogoPath, PrimaryColor, SecondaryColor);
}

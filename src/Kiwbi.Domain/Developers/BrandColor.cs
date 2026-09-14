using System.Text.RegularExpressions;
using Kiwbi.Domain.Exceptions;

namespace Kiwbi.Domain.Developers;

/// <summary>Value Object representing a validated hexadecimal brand color (#RRGGBB).</summary>
public sealed partial class BrandColor : IEquatable<BrandColor>
{
    public string Value { get; }

    private BrandColor(string value)
    {
        Value = value;
    }

    public static BrandColor Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException("El color de marca es obligatorio.");
        }

        var normalized = value.Trim().ToUpperInvariant();

        if (!HexColorRegex().IsMatch(normalized))
        {
            throw new DomainException("El color de marca debe tener el formato hexadecimal #RRGGBB.");
        }

        return new BrandColor(normalized);
    }

    public bool Equals(BrandColor? other) => other is not null && Value == other.Value;

    public override bool Equals(object? obj) => Equals(obj as BrandColor);

    public override int GetHashCode() => Value.GetHashCode();

    public override string ToString() => Value;

    [GeneratedRegex("^#[0-9A-F]{6}$")]
    private static partial Regex HexColorRegex();
}

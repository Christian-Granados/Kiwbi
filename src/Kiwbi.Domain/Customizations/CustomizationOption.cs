using Kiwbi.Domain.Common;
using Kiwbi.Domain.Exceptions;

namespace Kiwbi.Domain.Customizations;

/// <summary>Child entity of the Customization aggregate: a selectable option with its surcharge over the base price.</summary>
public class CustomizationOption : BaseEntity
{
    public string Name { get; private set; } = null!;
    public decimal SurchargeAmount { get; private set; }
    public bool IsDefault { get; private set; }
    public string? ThumbnailImagePath { get; private set; }

    private CustomizationOption()
    {
    }

    internal CustomizationOption(string name, decimal surchargeAmount, bool isDefault)
    {
        Rename(name);
        UpdateSurcharge(surchargeAmount);
        IsDefault = isDefault;
    }

    internal void Rename(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("El nombre de la opción es obligatorio.");
        }

        Name = name.Trim();
    }

    internal void UpdateSurcharge(decimal surchargeAmount)
    {
        if (surchargeAmount < 0)
        {
            throw new DomainException("El sobrecoste de la opción no puede ser negativo.");
        }

        SurchargeAmount = surchargeAmount;
    }

    internal void MarkAsDefault() => IsDefault = true;

    internal void UnmarkAsDefault() => IsDefault = false;

    internal void UpdateThumbnail(string? thumbnailImagePath) =>
        ThumbnailImagePath = string.IsNullOrWhiteSpace(thumbnailImagePath) ? null : thumbnailImagePath.Trim();
}

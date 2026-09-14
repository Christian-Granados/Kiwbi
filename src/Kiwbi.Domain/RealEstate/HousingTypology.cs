using Kiwbi.Domain.Common;
using Kiwbi.Domain.Exceptions;

namespace Kiwbi.Domain.RealEstate;

/// <summary>Entity representing an optional grouping (Tipología) of housing units within a HousingPromotion.</summary>
public class HousingTypology : BaseEntity
{
    public Guid HousingPromotionId { get; private set; }
    public string Name { get; private set; } = null!;
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    private HousingTypology()
    {
    }

    public static HousingTypology Create(Guid housingPromotionId, string name)
    {
        if (housingPromotionId == Guid.Empty)
        {
            throw new DomainException("La tipología debe pertenecer a una promoción.");
        }

        var typology = new HousingTypology
        {
            HousingPromotionId = housingPromotionId,
        };

        typology.Rename(name);
        typology.CreatedAtUtc = typology.UpdatedAtUtc;

        return typology;
    }

    public void Rename(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("El nombre de la tipología es obligatorio.");
        }

        Name = name.Trim();
        UpdatedAtUtc = DateTime.UtcNow;
    }
}

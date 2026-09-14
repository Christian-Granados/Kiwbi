using Kiwbi.Domain.Common;
using Kiwbi.Domain.Exceptions;

namespace Kiwbi.Domain.Customizations;

/// <summary>Entity representing a trade/guild (Gremio) of a HousingPromotion, defining the selection cut-off date for its Customizations.</summary>
public class TradeCategory : BaseEntity
{
    public Guid HousingPromotionId { get; private set; }
    public string Name { get; private set; } = null!;
    public DateTime SelectionCutOffDateUtc { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    private TradeCategory()
    {
    }

    public static TradeCategory Create(Guid housingPromotionId, string name, DateTime selectionCutOffDateUtc)
    {
        if (housingPromotionId == Guid.Empty)
        {
            throw new DomainException("El gremio debe pertenecer a una promoción.");
        }

        var tradeCategory = new TradeCategory
        {
            HousingPromotionId = housingPromotionId,
        };

        tradeCategory.Rename(name);
        tradeCategory.Reschedule(selectionCutOffDateUtc);
        tradeCategory.CreatedAtUtc = tradeCategory.UpdatedAtUtc;

        return tradeCategory;
    }

    public void Rename(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("El nombre del gremio es obligatorio.");
        }

        Name = name.Trim();
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void Reschedule(DateTime selectionCutOffDateUtc)
    {
        if (selectionCutOffDateUtc == default)
        {
            throw new DomainException("La fecha límite de selección es obligatoria.");
        }

        SelectionCutOffDateUtc = DateTime.SpecifyKind(selectionCutOffDateUtc, DateTimeKind.Utc);
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public bool IsExpired(DateTime utcNow) => utcNow > SelectionCutOffDateUtc;
}

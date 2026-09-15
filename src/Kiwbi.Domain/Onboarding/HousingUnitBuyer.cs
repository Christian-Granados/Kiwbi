using Kiwbi.Domain.Common;
using Kiwbi.Domain.Exceptions;

namespace Kiwbi.Domain.Onboarding;

/// <summary>Entity representing the confirmed link between a Buyer (Identity user) and a HousingUnit, created when a BuyerInvitation is accepted.</summary>
public class HousingUnitBuyer : BaseEntity
{
    public Guid HousingUnitId { get; private set; }
    public string BuyerUserId { get; private set; } = null!;
    public DateTime CreatedAtUtc { get; private set; }

    private HousingUnitBuyer()
    {
    }

    public static HousingUnitBuyer Create(Guid housingUnitId, string buyerUserId)
    {
        if (housingUnitId == Guid.Empty)
        {
            throw new DomainException("El enlace debe pertenecer a una vivienda.");
        }

        if (string.IsNullOrWhiteSpace(buyerUserId))
        {
            throw new DomainException("El comprador vinculado es obligatorio.");
        }

        return new HousingUnitBuyer
        {
            HousingUnitId = housingUnitId,
            BuyerUserId = buyerUserId,
            CreatedAtUtc = DateTime.UtcNow,
        };
    }
}

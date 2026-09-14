using Kiwbi.Domain.Common;
using Kiwbi.Domain.Exceptions;

namespace Kiwbi.Domain.RealEstate;

/// <summary>Entity representing a Promoción (real estate development) owned by a DeveloperCompany tenant.</summary>
public class HousingPromotion : BaseEntity
{
    public Guid DeveloperCompanyId { get; private set; }
    public string Name { get; private set; } = null!;
    public string City { get; private set; } = null!;
    public string Address { get; private set; } = null!;
    public string? MasterPlanImagePath { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    private HousingPromotion()
    {
    }

    public static HousingPromotion Create(Guid developerCompanyId, string name, string city, string address)
    {
        if (developerCompanyId == Guid.Empty)
        {
            throw new DomainException("La promoción debe pertenecer a una promotora.");
        }

        var promotion = new HousingPromotion
        {
            DeveloperCompanyId = developerCompanyId,
        };

        promotion.UpdateDetails(name, city, address);
        promotion.CreatedAtUtc = promotion.UpdatedAtUtc;

        return promotion;
    }

    public void UpdateDetails(string name, string city, string address)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("El nombre de la promoción es obligatorio.");
        }

        if (string.IsNullOrWhiteSpace(city))
        {
            throw new DomainException("La ciudad de la promoción es obligatoria.");
        }

        if (string.IsNullOrWhiteSpace(address))
        {
            throw new DomainException("La dirección de la promoción es obligatoria.");
        }

        Name = name.Trim();
        City = city.Trim();
        Address = address.Trim();
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void UpdateMasterPlanImage(string? masterPlanImagePath)
    {
        MasterPlanImagePath = string.IsNullOrWhiteSpace(masterPlanImagePath) ? null : masterPlanImagePath.Trim();
        UpdatedAtUtc = DateTime.UtcNow;
    }
}

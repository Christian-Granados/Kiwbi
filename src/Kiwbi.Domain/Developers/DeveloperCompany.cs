using Kiwbi.Domain.Common;
using Kiwbi.Domain.Exceptions;

namespace Kiwbi.Domain.Developers;

/// <summary>Aggregate root representing a Promotora (Developer Company). Root of a tenant.</summary>
public class DeveloperCompany : BaseEntity
{
    public string Name { get; private set; } = null!;
    public Branding Branding { get; private set; } = null!;
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    private DeveloperCompany()
    {
    }

    public static DeveloperCompany Create(string name)
    {
        var company = new DeveloperCompany
        {
            Branding = Branding.CreateDefault(),
        };

        company.Rename(name);
        company.CreatedAtUtc = company.UpdatedAtUtc;

        return company;
    }

    public void Rename(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("El nombre de la promotora es obligatorio.");
        }

        Name = name.Trim();
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void UpdateBranding(Branding branding)
    {
        ArgumentNullException.ThrowIfNull(branding);

        Branding = branding;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}

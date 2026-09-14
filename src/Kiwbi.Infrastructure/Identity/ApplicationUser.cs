using Kiwbi.Domain.Developers;
using Microsoft.AspNetCore.Identity;

namespace Kiwbi.Infrastructure.Identity;

/// <summary>ASP.NET Core Identity user, extended with Kiwbi-specific profile data as needed.</summary>
public class ApplicationUser : IdentityUser
{
    /// <summary>Tenant the user belongs to. Nullable because buyers (Epic 4) will not have a DeveloperCompany.</summary>
    public Guid? DeveloperCompanyId { get; set; }

    public DeveloperCompany? DeveloperCompany { get; set; }
}

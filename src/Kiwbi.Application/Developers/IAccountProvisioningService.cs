using Kiwbi.Application.Common;

namespace Kiwbi.Application.Developers;

/// <summary>Port for provisioning Identity accounts, isolated from the concrete Identity implementation.</summary>
public interface IAccountProvisioningService
{
    /// <summary>Creates an Identity user linked to the given tenant and assigns the DeveloperAdmin role.</summary>
    Task<Result<string>> CreateDeveloperAdminAccountAsync(
        string email,
        string password,
        Guid developerCompanyId,
        CancellationToken cancellationToken = default);
}

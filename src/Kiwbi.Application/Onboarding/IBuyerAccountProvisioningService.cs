using Kiwbi.Application.Common;

namespace Kiwbi.Application.Onboarding;

/// <summary>Port for provisioning/looking up Buyer Identity accounts, isolated from the concrete Identity implementation. Parallel to Developers.IAccountProvisioningService, but for buyers (no DeveloperCompanyId, role Buyer instead of DeveloperAdmin).</summary>
public interface IBuyerAccountProvisioningService
{
    Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default);

    Task<string?> FindUserIdByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>Creates an Identity user with no DeveloperCompanyId and assigns the Buyer role.</summary>
    Task<Result<string>> CreateBuyerAccountAsync(string email, string password, CancellationToken cancellationToken = default);

    Task<string?> GetEmailByUserIdAsync(string userId, CancellationToken cancellationToken = default);
}

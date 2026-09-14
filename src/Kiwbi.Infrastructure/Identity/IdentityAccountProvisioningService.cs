using Kiwbi.Application.Common;
using Kiwbi.Application.Developers;
using Microsoft.AspNetCore.Identity;

namespace Kiwbi.Infrastructure.Identity;

/// <summary>Adapter that provisions Identity accounts using UserManager/RoleManager, hidden behind an Application port.</summary>
public class IdentityAccountProvisioningService : IAccountProvisioningService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public IdentityAccountProvisioningService(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<Result<string>> CreateDeveloperAdminAccountAsync(
        string email,
        string password,
        Guid developerCompanyId,
        CancellationToken cancellationToken = default)
    {
        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            DeveloperCompanyId = developerCompanyId,
        };

        var createResult = await _userManager.CreateAsync(user, password);

        if (!createResult.Succeeded)
        {
            return Result.Failure<string>(string.Join(" ", createResult.Errors.Select(e => e.Description)));
        }

        await EnsureRoleExistsAsync(ApplicationRoles.DeveloperAdmin);
        await _userManager.AddToRoleAsync(user, ApplicationRoles.DeveloperAdmin);
        await _userManager.AddClaimAsync(user, new System.Security.Claims.Claim(
            ApplicationClaimTypes.DeveloperCompanyId, developerCompanyId.ToString()));

        return Result.Success(user.Id);
    }

    private async Task EnsureRoleExistsAsync(string roleName)
    {
        if (!await _roleManager.RoleExistsAsync(roleName))
        {
            await _roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }
}

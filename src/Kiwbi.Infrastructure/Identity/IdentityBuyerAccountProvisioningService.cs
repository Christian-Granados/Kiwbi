using Kiwbi.Application.Common;
using Kiwbi.Application.Onboarding;
using Microsoft.AspNetCore.Identity;

namespace Kiwbi.Infrastructure.Identity;

/// <summary>Adapter that provisions/looks up Buyer Identity accounts using UserManager/RoleManager, hidden behind an Application port. Parallel to IdentityAccountProvisioningService, but for buyers.</summary>
public class IdentityBuyerAccountProvisioningService : IBuyerAccountProvisioningService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public IdentityBuyerAccountProvisioningService(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default) =>
        await _userManager.FindByEmailAsync(email) is not null;

    public async Task<string?> FindUserIdByEmailAsync(string email, CancellationToken cancellationToken = default) =>
        (await _userManager.FindByEmailAsync(email))?.Id;

    public async Task<Result<string>> CreateBuyerAccountAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
        };

        var createResult = await _userManager.CreateAsync(user, password);

        if (!createResult.Succeeded)
        {
            return Result.Failure<string>(string.Join(" ", createResult.Errors.Select(e => e.Description)));
        }

        await EnsureRoleExistsAsync(ApplicationRoles.Buyer);
        await _userManager.AddToRoleAsync(user, ApplicationRoles.Buyer);

        return Result.Success(user.Id);
    }

    public async Task<string?> GetEmailByUserIdAsync(string userId, CancellationToken cancellationToken = default) =>
        (await _userManager.FindByIdAsync(userId))?.Email;

    private async Task EnsureRoleExistsAsync(string roleName)
    {
        if (!await _roleManager.RoleExistsAsync(roleName))
        {
            await _roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }
}

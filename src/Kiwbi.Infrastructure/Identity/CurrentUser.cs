using Kiwbi.Application.Common;
using Microsoft.AspNetCore.Http;

namespace Kiwbi.Infrastructure.Identity;

/// <summary>Resolves the current user's identity from the HTTP context's claims principal.</summary>
public class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private System.Security.Claims.ClaimsPrincipal? Principal => _httpContextAccessor.HttpContext?.User;

    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated ?? false;

    public string? UserId => Principal?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

    public Guid? DeveloperCompanyId =>
        Guid.TryParse(Principal?.FindFirst(ApplicationClaimTypes.DeveloperCompanyId)?.Value, out var id) ? id : null;
}

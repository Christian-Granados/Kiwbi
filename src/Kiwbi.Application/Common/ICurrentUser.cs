namespace Kiwbi.Application.Common;

/// <summary>Resolves identity information for the currently authenticated principal, derived from the auth context.</summary>
public interface ICurrentUser
{
    bool IsAuthenticated { get; }
    string? UserId { get; }
    Guid? DeveloperCompanyId { get; }
}

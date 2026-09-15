using Kiwbi.Application.Onboarding;

namespace Kiwbi.Web.Models.Onboarding;

public class BuyerInvitationListItemViewModel
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public DateTime ExpiresAtUtc { get; set; }
    public bool IsExpired { get; set; }
    public DateTime? AcceptedAtUtc { get; set; }

    public static BuyerInvitationListItemViewModel FromDto(BuyerInvitationDto dto) => new()
    {
        Id = dto.Id,
        Email = dto.Email,
        Status = dto.Status.ToString(),
        CreatedAtUtc = dto.CreatedAtUtc,
        ExpiresAtUtc = dto.ExpiresAtUtc,
        IsExpired = dto.IsExpired,
        AcceptedAtUtc = dto.AcceptedAtUtc,
    };
}

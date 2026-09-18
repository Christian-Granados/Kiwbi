using Kiwbi.Application.Developers.GetCurrentDeveloperProfile;

namespace Kiwbi.Web.Components;

public class TenantBrandingViewModel
{
    public string CompanyName { get; set; } = string.Empty;
    public string? LogoPath { get; set; }
    public string PrimaryColor { get; set; } = string.Empty;
    public string? SecondaryColor { get; set; }

    public static TenantBrandingViewModel FromDto(DeveloperProfileDto dto) => new()
    {
        CompanyName = dto.Name,
        LogoPath = dto.LogoPath,
        PrimaryColor = dto.PrimaryColor,
        SecondaryColor = dto.SecondaryColor,
    };
}

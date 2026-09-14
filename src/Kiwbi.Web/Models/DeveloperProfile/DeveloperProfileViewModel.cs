using Kiwbi.Application.Developers.GetCurrentDeveloperProfile;

namespace Kiwbi.Web.Models.DeveloperProfile;

public class DeveloperProfileViewModel
{
    public string Name { get; set; } = string.Empty;
    public string? LogoPath { get; set; }
    public string PrimaryColor { get; set; } = string.Empty;
    public string? SecondaryColor { get; set; }

    public static DeveloperProfileViewModel FromDto(DeveloperProfileDto dto) => new()
    {
        Name = dto.Name,
        LogoPath = dto.LogoPath,
        PrimaryColor = dto.PrimaryColor,
        SecondaryColor = dto.SecondaryColor,
    };
}

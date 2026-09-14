namespace Kiwbi.Application.Developers.GetCurrentDeveloperProfile;

public sealed record DeveloperProfileDto(
    Guid DeveloperCompanyId,
    string Name,
    string? LogoPath,
    string PrimaryColor,
    string? SecondaryColor);

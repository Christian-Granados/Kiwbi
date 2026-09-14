namespace Kiwbi.Application.Developers.UpdateDeveloperBranding;

public sealed record UpdateDeveloperBrandingCommand(string PrimaryColor, string? SecondaryColor, string? LogoPath);

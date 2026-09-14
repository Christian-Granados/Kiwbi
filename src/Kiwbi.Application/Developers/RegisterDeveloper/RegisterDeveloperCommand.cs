namespace Kiwbi.Application.Developers.RegisterDeveloper;

public sealed record RegisterDeveloperCommand(string CompanyName, string Email, string Password);

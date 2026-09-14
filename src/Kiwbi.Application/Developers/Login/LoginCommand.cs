namespace Kiwbi.Application.Developers.Login;

public sealed record LoginCommand(string Email, string Password, bool RememberMe);

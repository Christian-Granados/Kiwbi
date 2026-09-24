namespace Kiwbi.Tools.Shared;

// Shared by the standalone diagnostic tools (SmtpSendTest, R2UploadSmokeTest) via a linked Compile
// item in each .csproj - kept dependency-free and deliberately outside Kiwbi.Infrastructure, see the
// comments at the top of each tool's Program.cs for why.
internal static class ConsoleInput
{
    public static string ReadValue(string label)
    {
        Console.Write($"{label}: ");
        return (Console.ReadLine() ?? string.Empty).Trim();
    }

    public static string ReadSecret(string label)
    {
        Console.Write($"{label}: ");
        var secret = new System.Text.StringBuilder();

        var key = Console.ReadKey(intercept: true);
        while (key.Key != ConsoleKey.Enter)
        {
            if (key.Key == ConsoleKey.Backspace)
            {
                if (secret.Length > 0)
                {
                    secret.Remove(secret.Length - 1, 1);
                    Console.Write("\b \b");
                }
            }
            else if (!char.IsControl(key.KeyChar))
            {
                secret.Append(key.KeyChar);
                Console.Write('*');
            }

            key = Console.ReadKey(intercept: true);
        }

        Console.WriteLine();
        return secret.ToString();
    }
}

using System.Net;
using System.Net.Mail;

// Standalone diagnostic tool (Feature 11.5, Brevo SMTP smoke test). Intentionally NOT part of Kiwbi.slnx and NOT
// referencing Kiwbi.Infrastructure: it only exists to confirm real SMTP credentials work end-to-end, without ever
// writing them to appsettings.json, user-secrets, or any file. Every value below lives only in local variables for
// the lifetime of this process; nothing is logged or persisted. Run it yourself in your own terminal - never paste
// real credentials into a chat session.

Console.WriteLine("=== Kiwbi - prueba de envío SMTP (Brevo) ===");
Console.WriteLine("Los valores que introduzcas solo se usan en memoria durante esta ejecución. No se guardan en ningún fichero.");
Console.WriteLine();

var host = ReadValue("Email:Smtp:Host (p.ej. smtp-relay.brevo.com)");
var port = int.Parse(ReadValue("Email:Smtp:Port (587 para STARTTLS - System.Net.Mail no soporta TLS implícito en 465)"));
var username = ReadValue("Email:Smtp:Username");
var password = ReadSecret("Email:Smtp:Password (oculto)");
var fromEmail = ReadValue("Email:Smtp:FromEmail (remitente autorizado en Brevo)");
var fromName = ReadValue("Email:Smtp:FromName (opcional, Enter para \"Kiwbi\")");
if (string.IsNullOrWhiteSpace(fromName)) fromName = "Kiwbi";
var toEmail = ReadValue("Correo de destino donde quieres recibir el email de prueba");

Console.WriteLine();
Console.WriteLine("Enviando correo de prueba (timeout de 20s, para no esperar el timeout de 100s por defecto)...");

using var client = new SmtpClient(host, port)
{
    Credentials = new NetworkCredential(username, password),
    EnableSsl = true,
    Timeout = 20000,
};

using var message = new MailMessage
{
    From = new MailAddress(fromEmail, fromName),
    Subject = "Kiwbi - correo de prueba SMTP",
    Body = $"<p>Prueba de envío SMTP de Kiwbi - {DateTime.UtcNow:O}</p>",
    IsBodyHtml = true,
};
message.To.Add(toEmail);

try
{
    await client.SendMailAsync(message);
    Console.WriteLine();
    Console.WriteLine("Correo enviado correctamente. Revisa la bandeja de entrada (y spam) de " + toEmail + ".");
}
catch (Exception ex)
{
    Console.WriteLine();
    Console.WriteLine("El envío ha fallado.");
    Console.WriteLine($"  Tipo: {ex.GetType().Name}");
    Console.WriteLine($"  Mensaje: {ex.Message}");

    if (ex is SmtpException smtpException)
    {
        Console.WriteLine($"  StatusCode: {smtpException.StatusCode}");
    }

    if (ex.InnerException is not null)
    {
        Console.WriteLine($"  Inner: {ex.InnerException.GetType().Name} - {ex.InnerException.Message}");
    }

    if (ex is TimeoutException || ex.Message.Contains("time", StringComparison.OrdinalIgnoreCase))
    {
        Console.WriteLine();
        Console.WriteLine("Un timeout aquí normalmente significa: puerto equivocado (usa 587, no 465 - System.Net.Mail");
        Console.WriteLine("solo soporta STARTTLS, no TLS implícito), o el proveedor de hosting bloquea ese puerto saliente.");
    }
}

Console.WriteLine();
Console.WriteLine("Fin. Esta herramienta no ha escrito ninguna credencial en disco.");

static string ReadValue(string label)
{
    Console.Write($"{label}: ");
    return (Console.ReadLine() ?? string.Empty).Trim();
}

static string ReadSecret(string label)
{
    Console.Write($"{label}: ");
    var secret = new System.Text.StringBuilder();
    ConsoleKeyInfo key;

    while ((key = Console.ReadKey(intercept: true)).Key != ConsoleKey.Enter)
    {
        if (key.Key == ConsoleKey.Backspace)
        {
            if (secret.Length > 0)
            {
                secret.Remove(secret.Length - 1, 1);
                Console.Write("\b \b");
            }

            continue;
        }

        if (!char.IsControl(key.KeyChar))
        {
            secret.Append(key.KeyChar);
            Console.Write('*');
        }
    }

    Console.WriteLine();
    return secret.ToString();
}

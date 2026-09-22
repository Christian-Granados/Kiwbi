using System.Diagnostics;
using System.Net.Sockets;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Kiwbi.Web.Controllers;

/// <summary>Manual, promotora-only diagnostics for infra-level issues that don't show useful info in the browser
/// (e.g. an SMTP send hanging without a clear error). Not a permanent monitoring feature - remove once the
/// email/Brevo connectivity investigation is done.</summary>
[Authorize(Roles = "DeveloperAdmin")]
public class DiagnosticsController : Controller
{
    private readonly IConfiguration _configuration;

    public DiagnosticsController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    /// <summary>Pure TCP reachability check against the configured Email:Smtp:Host/Port - no credentials involved,
    /// no email sent. Answers "can this deployment reach Brevo's SMTP port at all?" in a few seconds instead of
    /// waiting for SmtpClient's send-and-auth timeout.</summary>
    [HttpGet]
    public async Task<IActionResult> SmtpReachability(CancellationToken cancellationToken)
    {
        var host = _configuration["Email:Smtp:Host"];
        var portValue = _configuration["Email:Smtp:Port"];

        if (string.IsNullOrWhiteSpace(host) || !int.TryParse(portValue, out var port))
        {
            return Content("No hay configuración SMTP (Email:Smtp:Host / Email:Smtp:Port) en esta instancia.");
        }

        using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);
        var stopwatch = Stopwatch.StartNew();

        try
        {
            using var tcpClient = new TcpClient();
            await tcpClient.ConnectAsync(host, port, linkedCts.Token);
            stopwatch.Stop();
            return Content($"OK: conexión TCP a {host}:{port} establecida en {stopwatch.ElapsedMilliseconds} ms.");
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            return Content($"FALLO tras {stopwatch.ElapsedMilliseconds} ms conectando a {host}:{port} - {ex.GetType().Name}: {ex.Message}");
        }
    }
}

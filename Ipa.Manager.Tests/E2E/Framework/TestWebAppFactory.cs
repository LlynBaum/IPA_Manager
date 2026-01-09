using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace Ipa.Manager.Tests.E2E.Framework;

/// <summary>
/// Spins up the Blazor Web App as an external process.
/// The app is available via <c>127.0.0.1</c> on a random free port chosen at start up.
/// </summary>
internal class TestWebAppFactory(string dbConnectionString) : IDisposable, IAsyncDisposable
{
    private Process? process;

    private Uri? baseAddress;
    public Uri BaseAddress => baseAddress ?? throw new InvalidOperationException("Call StartAsync() to start the WebApp first.");

    public string DbConnectionString => dbConnectionString;

    public async Task StartAsync()
    {
        // Reserve a free loopback port so Kestrel can bind to a fixed address.
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();

        baseAddress = new Uri($"http://127.0.0.1:{port}");

        // Resolve project folder relative to the test assembly location.
        var projectPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "Ipa.Manager"));

        var psi = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = "run --no-launch-profile",
            WorkingDirectory = projectPath,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        // Environment to control the started app
        psi.Environment["ASPNETCORE_URLS"] = baseAddress.ToString();
        psi.Environment["ASPNETCORE_ENVIRONMENT"] = "Development";
        psi.Environment["ConnectionStrings__DefaultConnection"] = dbConnectionString;

        process = Process.Start(psi) ?? throw new InvalidOperationException("Failed to start external process.");

        // Optional: stream app logs into the test runner debug output (best-effort)
        _ = Task.Run(() =>
        {
            try
            {
                if (process is null) return;
                while (!process.HasExited)
                {
                    var line = process.StandardOutput.ReadLine();
                    if (line is not null) Debug.WriteLine($"[App] {line}");
                }
            }
            catch { /* best-effort logging */ }
        });

        // Wait for the app to become responsive.
        using var http = new HttpClient();
        http.BaseAddress = baseAddress;

        var timeout = TimeSpan.FromSeconds(30);
        var stopAt = DateTime.UtcNow + timeout;
        while (DateTime.UtcNow < stopAt)
        {
            try
            {
                var r = await http.GetAsync("/");
                if (r.IsSuccessStatusCode) return; // ready
            }
            catch { /* not ready yet */ }

            await Task.Delay(250);
        }

        // Not ready in time: stop process and fail.
        await DisposeAsync();
        throw new TimeoutException("External web host did not become ready in time.");
    }

    public ValueTask DisposeAsync()
    {
        if (process is null) return ValueTask.CompletedTask;

        try
        {
            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: true);
                process.WaitForExit(5000);
            }
        }
        catch { /* best-effort */ }
        finally
        {
            process.Dispose();
            process = null;
        }

        return ValueTask.CompletedTask;
    }

    void IDisposable.Dispose()
    {
        // Best-effort synchronous disposal for callers that call Dispose().
        DisposeAsync().GetAwaiter().GetResult();
    }
}
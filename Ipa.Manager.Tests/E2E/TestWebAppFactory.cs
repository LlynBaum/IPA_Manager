using System.Diagnostics;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;

namespace Ipa.Manager.Tests.E2E;

/// <summary>
/// Spins up the Blazor Web App.
/// The app is available via <c>127.0.0.1</c> on a random free port chosen at start up.
/// </summary>
internal class TestWebAppFactory(string dbConnectionString) : WebApplicationFactory<Program>
{
    private IHost? host;
    
    public override IServiceProvider Services
        => host?.Services
           ?? throw new InvalidOperationException("Call StartAsync() first to start host.");

    public string ServerAddress => host is not null
        ? ClientOptions.BaseAddress.ToString()
        : throw new InvalidOperationException("Call StartAsync() first to start host.");
    
    public async Task StartAsync()
    {
        // Triggers CreateHost() getting called. Else the host isn't yet created when calling StartAsync().
        _ = base.Services;

        Debug.Assert(host is not null);

        // Spins the host app up.
        await host.StartAsync();

        // Extract the selected dynamic port out of the Kestrel server
        // and assign it onto the client options for convenience so it
        // "just works" as otherwise it'll be the default http://localhost
        // URL, which won't route to the Kestrel-hosted HTTP server.
        var server = host.Services.GetRequiredService<IServer>();
        var addresses = server.Features.Get<IServerAddressesFeature>();
        ClientOptions.BaseAddress = addresses!.Addresses
            .Select(x => x.Replace("127.0.0.1", "localhost", StringComparison.Ordinal))
            .Select(x => new Uri(x))
            .Last();
    }
    
    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.ConfigureHostConfiguration(cfg =>
        {
            cfg.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = dbConnectionString
            });
        });
        
        builder.ConfigureWebHost(webHost =>
        {
            webHost.UseKestrel();
            webHost.UseEnvironment("Test");
            webHost.UseUrls("http://127.0.0.1:0");
        });

        host = builder.Build();

        return new DummyHost();
    }
    
    // The DummyHost is returned to avoid the
    // TProgram project being started twice.
    private sealed class DummyHost : IHost
    {
        public IServiceProvider Services { get; }

        public DummyHost()
        {
            Services = new ServiceCollection()
                .AddSingleton<IServer>((s) => new TestServer(s))
                .BuildServiceProvider();
        }

        public void Dispose()
        {
        }

        public Task StartAsync(CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task StopAsync(CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }
}
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;

namespace Ipa.Manager.Tests.E2E;

internal class TestWebAppFactory(string dbConnectionString) : WebApplicationFactory<Program>
{
    public string BaseUrl = string.Empty;
    
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
            webHost.UseTestServer();
            webHost.UseEnvironment("Test");
            webHost.UseUrls("http://127.0.0.1:5000");
        });

        var host = builder.Build();
        host.Start();

        var server = host.Services.GetRequiredService<IServer>();
        var addresses = server.Features.Get<IServerAddressesFeature>()?.Addresses;

        BaseUrl = addresses?.First() ?? throw new InvalidOperationException("Could not determine server address.");

        return host;
    }
}
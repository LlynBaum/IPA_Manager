using NUnit.Framework;
using Testcontainers.MySql;

namespace Ipa.Manager.Tests.E2E;

[SetUpFixture]
internal class PlaywrightServerFixture
{
    public static TestWebAppFactory Factory = null!;

    [OneTimeSetUp]
    public async Task StartServer()
    {
        var mySqlContainer = new MySqlBuilder("mysql:8.0").Build();
        await mySqlContainer.StartAsync();
        var connectionString = mySqlContainer.GetConnectionString();
        
        Factory = new TestWebAppFactory(connectionString);
        _ = Factory.CreateClient(); // Force the host to be built + started so CreateHost() runs
    }
    
    [OneTimeTearDown]
    public void StopServer()
    {
        Factory.Dispose();
    }
}
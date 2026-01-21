using DotNet.Testcontainers.Builders;
using Ipa.Manager.Tests.E2E.Framework;
using NUnit.Framework;
using Testcontainers.MySql;

namespace Ipa.Manager.Tests.E2E;

[SetUpFixture]
internal class PlaywrightServerFixture
{
    public static TestWebAppFactory Factory => factory!;
    private static TestWebAppFactory? factory;

    [OneTimeSetUp]
    public async Task StartServer()
    {
        string connectionString;
        try
        {
            var mySqlContainer = new MySqlBuilder("mysql:8.0").Build();
            await mySqlContainer.StartAsync();
            connectionString = mySqlContainer.GetConnectionString();
        }
        catch (Exception e)
        {
            throw new DockerUnavailableException("Failed to start Testcontainers for MySQL DB. Make sure docker is running.", e);
        }

        factory = new TestWebAppFactory(connectionString);
        await factory.StartAsync();
    }

    [OneTimeTearDown]
    public void StopServer()
    {
        factory?.Dispose();
    }
}
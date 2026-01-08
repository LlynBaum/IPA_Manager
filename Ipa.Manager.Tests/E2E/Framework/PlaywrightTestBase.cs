//using Ipa.Manager.Database;
//using Microsoft.EntityFrameworkCore;

using Microsoft.Playwright.NUnit;
using NUnit.Framework;

namespace Ipa.Manager.Tests.E2E.Framework;

[NonParallelizable]
public class PlaywrightTestBase : PageTest
{
    /// <summary>
    /// Use this code <code>protected override bool EnableTracing => true;</code> to Enable Playwright tracing.
    /// Playwright Tracing can be used to view the Playwright Test Snapshots. Use the following command:
    /// <code>npx playwright show-trace bin/Debug/net9.0/trace.zip</code>
    /// </summary>
    protected virtual bool EnableTracing => false;
    
    /// <summary>
    /// The URL where the Blazor host is available.
    /// </summary>
    protected string BaseUrl = string.Empty;
    
    /// <summary>
    /// A HttpClient to interact with the Web App for 'Integration' Tests.
    /// </summary>
    protected HttpClient Client => PlaywrightServerFixture.Factory.Server.CreateClient();
    
    /// <summary>
    /// A DB Context for the current DB Instance. The data in the DB will be cleared after every Test run.
    /// </summary>
    //protected ApplicationDbContext Db;

    /// <summary>
    /// The ServiceProvider to access the DI Container.
    /// </summary>
    protected IServiceProvider ServiceProvider => scope.ServiceProvider;
    
    private IServiceScope scope;
    
    [SetUp]
    public async Task Setup()
    {
        if(!EnableTracing) return;
        await Context.Tracing.StartAsync(new()
        {
            Screenshots = true,
            Snapshots = true,
            Sources = true
        });
    }
    
    [TearDown]
    public async Task TearDown()
    {
        if(!EnableTracing) return;
        await Context.Tracing.StopAsync(new()
        {
            Path = "trace.zip"
        });
    }
    
    [SetUp]
    public async Task StartServer()
    {
        BaseUrl = PlaywrightServerFixture.Factory.ServerAddress;
        
        scope = PlaywrightServerFixture.Factory.Services.CreateScope();
        //Db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        //await Db.Database.EnsureDeletedAsync();
        //await Db.Database.MigrateAsync();
    }

    [TearDown]
    public void DisposeScope()
    {
        //Db.Dispose();
        scope.Dispose();
    }
}
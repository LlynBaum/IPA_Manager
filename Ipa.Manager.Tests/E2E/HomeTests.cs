using Ipa.Manager.Models;
using Ipa.Manager.Tests.E2E.Framework;
using Microsoft.AspNetCore.Identity;
using Microsoft.Playwright;
using NUnit.Framework;

namespace Ipa.Manager.Tests.E2E;

[TestFixture]
public class HomeTests : PlaywrightTestBase
{
    private const string Username = "TestUser";
    private const string Password = "TestPassword123!";

    protected override bool EnableTracing => true;

    [Test]
    public async Task Home_ShowsEmptyState_WhenUserHasNoProjects()
    {
        await CreateUserAndLoginAsync();

        await Page.GotoSaveAsync(BaseUrl);

        await Expect(Page.GetByText("No projects yet")).ToBeVisibleAsync();
        await Expect(Page.GetByText("Create your first project to get started")).ToBeVisibleAsync();
    }

    [Test]
    public async Task Home_ShowsProjects_WhenUserHasProjects()
    {
        var user = await CreateUserAndLoginAsync();

        // Create a project in DB
        var project = new Project
        {
            Name = "My Existing Project",
            Topic = "C# Mastery",
            UserId = user.Id,
            CreatedAt = DateTime.UtcNow
        };
        await Db.Projects.AddAsync(project);

        // Add some criteria progress
        await Db.CriteriaProgress.AddAsync(new CriteriaProgress
        {
            Project = project,
            CriteriaId = "A01",
            FulfilledRequirementIds = [],
            Notes = ""
        });
        await Db.SaveChangesAsync();

        await Page.GotoSaveAsync(BaseUrl);

        // Verify project card
        await Expect(Page.GetByText("My Existing Project")).ToBeVisibleAsync();
        await Expect(Page.GetByText("C# Mastery")).ToBeVisibleAsync();
        await Expect(Page.GetByText("1 criteria")).ToBeVisibleAsync();

        // Verify total count in header
        await Expect(Page.GetByText("1 project in total")).ToBeVisibleAsync();
    }

    [Test]
    public async Task Home_CanDeleteProject()
    {
        var user = await CreateUserAndLoginAsync();

        // Create a project
        var project = new Project
        {
            Name = "To Be Deleted",
            Topic = "Delete Me",
            UserId = user.Id
        };
        await Db.Projects.AddAsync(project);
        await Db.SaveChangesAsync();

        await Page.GotoSaveAsync(BaseUrl);

        // Handle dialog
        Page.Dialog += async (_, dialog) => await dialog.AcceptAsync();

        // Click delete button (title="Delete Project")
        await Page.GetByTitle("Delete Project").ClickAsync();

        // Verify it's gone
        await Expect(Page.GetByText("To Be Deleted")).Not.ToBeVisibleAsync();

        // Verify DB
        Assert.That(Db.Projects.Any(p => p.Id == project.Id), Is.False);
    }

    private async Task<User> CreateUserAndLoginAsync()
    {
        // Create user in DB directly
        var passwordHasher = ServiceProvider.GetRequiredService<IPasswordHasher<User>>();
        var user = new User
        {
            Username = Username,
            PasswordHash = passwordHasher.HashPassword(null!, Password)
        };
        await Db.Users.AddAsync(user);
        await Db.SaveChangesAsync();

        // Login via API (bypass UI)
        var formContent = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Username"] = Username,
            ["Password"] = Password,
            ["ReturnUrl"] = "/"
        });
        var formData = await formContent.ReadAsStringAsync();

        var response = await Context.APIRequest.PostAsync(BaseUrl + "auth/login", new()
        {
            Headers = new Dictionary<string, string> { ["Content-Type"] = "application/x-www-form-urlencoded" },
            Data = formData
        });

        Assert.That(response.Status, Is.EqualTo(200).Or.EqualTo(302)); // Redirect or OK

        return user;
    }
}
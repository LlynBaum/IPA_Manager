using Ipa.Manager.Auth;
using Ipa.Manager.Database;
using Ipa.Manager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;

namespace Ipa.Manager.Pages;

[Authorize]
public partial class Home(
    ApplicationDbContext context,
    NavigationManager navigationManager,
    IJSRuntime jsRuntime)
{
    [CascadingParameter]
    public required UserContext UserContext { get; set; }

    private IReadOnlyList<Project> projects = [];

    protected override async Task OnInitializedAsync()
    {
        await LoadProjects();
    }

    private async Task LoadProjects()
    {
        projects = await context.Projects
            .Include(p => p.CriteriaProgress)
            .Where(p => p.UserId == UserContext.UserId)
            .ToListAsync();
    }

    private void ViewProject(int id)
    {
        navigationManager.NavigateTo($"/project/{id}");
    }

    private async Task DeleteProject(Project project)
    {
        bool confirmed = await jsRuntime.InvokeAsync<bool>("confirm", $"Are you sure you want to delete '{project.Name}'?");

        if (confirmed)
        {
            context.Projects.Remove(project);
            await context.SaveChangesAsync();
            await LoadProjects();
        }
    }
}
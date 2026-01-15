using Ipa.Manager.Auth;
using Ipa.Manager.Database;
using Ipa.Manager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;

namespace Ipa.Manager.Pages;

[Authorize]
public partial class Home(
    ApplicationDbContext context,
    NavigationManager navigationManager)
{
    [CascadingParameter]
    public required UserContext UserContext { get; set; }
    
    private IReadOnlyList<Project> projects = [];
    
    protected override async Task OnInitializedAsync()
    {
        projects = await context.Projects
            .Where(p => p.UserId == UserContext.UserId)
            .ToListAsync();
    }

    private void ViewProject(int id)
    {
        navigationManager.NavigateTo($"/project/{id}");
    }
}
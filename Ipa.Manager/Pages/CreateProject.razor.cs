using Ipa.Manager.Auth;
using Ipa.Manager.Database;
using Ipa.Manager.Models;
using Ipa.Manager.Services.Criterias;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Ipa.Manager.Pages;

[Authorize]
public partial class CreateProject(
    ApplicationDbContext context,
    ICriteriaProgressService criteriaProgressService,
    IStaticCriteriaService staticCriteriaService,
    NavigationManager navigationManager, 
    ILogger<CreateProject> logger)
{
    [CascadingParameter]
    public required UserContext UserContext { get; set; }

    private readonly Project projectModel = new() { Name = "", Topic = "" };
    private IReadOnlyList<Criteria>? availableCriteria;
    private readonly HashSet<string> selectedCriteriaIds = [];
    private bool isSubmitting;
    
    private string searchTerm = "";
    private string bulkIdsInput = "";

    private IEnumerable<IGrouping<string, Criteria>> GetGroupedCriteria()
    {
        if (availableCriteria == null) return [];
            
        var filtered = availableCriteria;

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            filtered = filtered.Where(c => 
                c.Id.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) || 
                c.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                c.Description.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        return filtered.GroupBy(c => c.Section);
    }

    protected override void OnInitialized()
    {
        // GetAll() already returns criteria in JSON order (by section, then by criteria order within section)
        availableCriteria = staticCriteriaService.GetAll();
    }

    private void ToggleCriteria(string criteriaId, object? checkedValue)
    {
        if (checkedValue is true)
        {
            selectedCriteriaIds.Add(criteriaId);
        }
        else
        {
            selectedCriteriaIds.Remove(criteriaId);
        }
    }
    
    private void HandleBulkInputKeyUp(KeyboardEventArgs e)
    {
        if (e.Key == "Enter")
        {
            ApplyBulkSelection();
        }
    }

    private void ApplyBulkSelection()
    {
        if (string.IsNullOrWhiteSpace(bulkIdsInput)) return;

        var ids = bulkIdsInput.Split([',', ' ', ';'], StringSplitOptions.RemoveEmptyEntries);
        
        foreach (var id in ids)
        {
            var criteria = availableCriteria?.FirstOrDefault(c => c.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
            if (criteria != null)
            {
                selectedCriteriaIds.Add(criteria.Id);
            }
        }
        
        bulkIdsInput = "";
    }

    private async Task HandleSubmit()
    {
        isSubmitting = true;
        try
        {
            projectModel.UserId = UserContext.UserId;
            
            context.Projects.Add(projectModel);
            await context.SaveChangesAsync();

            if (selectedCriteriaIds.Any())
            {
                await criteriaProgressService.CreateAsync(projectModel.Id, selectedCriteriaIds.ToList());
            }

            navigationManager.NavigateTo("/");
        }
        catch (Exception ex)
        {
            logger.LogError("Error creating project: {Message}", ex.Message);
        }
        finally
        {
            isSubmitting = false;
        }
    }
}
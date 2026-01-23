using Ipa.Manager.Auth;
using Ipa.Manager.Database;
using Ipa.Manager.Models;
using Ipa.Manager.Services.Criterias;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;

namespace Ipa.Manager.Pages;

[Authorize]
public partial class ProjectDetail(
    ApplicationDbContext context,
    ICriteriaProgressService criteriaProgressService,
    IStaticCriteriaService staticCriteriaService,
    NavigationManager navigationManager,
    IJSRuntime jsRuntime)
{
    [Parameter]
    public int Id { get; set; }

    [CascadingParameter]
    public required UserContext UserContext { get; set; }

    private Project? project;
    private IReadOnlyList<ProjectCriteria> criteriaList = [];
    private bool isEditing;
    private string editName = "";
    private string editTopic = "";

    private double currentGrade = 1.0;
    private int totalPoints = 0;
    private int maxPoints = 0;

    private bool showCriteriaModal;
    private string criteriaSearchTerm = "";
    private HashSet<string> selectedCriteriaIds = [];
    private IReadOnlyList<Criteria> allCriteria = [];

    protected override async Task OnInitializedAsync()
    {
        await LoadDataAsync();
        CalculateGrade();
    }

    private async Task LoadDataAsync()
    {
        project = await context.Projects
            .Include(p => p.CriteriaProgress)
            .FirstOrDefaultAsync(p => p.Id == Id && p.UserId == UserContext.UserId);

        if (project is null)
        {
            navigationManager.NavigateTo("/");
            return;
        }

        editName = project.Name;
        editTopic = project.Topic;

        criteriaList = await criteriaProgressService.GetByProject(Id);
    }

    private void CalculateGrade()
    {
        if (criteriaList.Count == 0)
        {
            currentGrade = 1.0;
            totalPoints = 0;
            maxPoints = 0;
            return;
        }

        totalPoints = criteriaList.Sum(c => c.CriteriaProgress.FulfilledRequirementIds.DefaultIfEmpty(0).Max());
        maxPoints = criteriaList.Count * 3;

        if (maxPoints > 0)
        {
            currentGrade = Math.Round(((double)totalPoints / maxPoints) * 5 + 1, 1);
        }
        else
        {
            currentGrade = 1.0;
        }
    }

    private async Task ToggleRequirement(ProjectCriteria item, int requirementIndex)
    {
        bool isChecking = !item.CriteriaProgress.FulfilledRequirementIds.Contains(requirementIndex);

        if (isChecking)
        {
            // Check this level and all levels below it
            for (int i = 0; i <= requirementIndex; i++)
            {
                if (!item.CriteriaProgress.FulfilledRequirementIds.Contains(i))
                {
                    item.CriteriaProgress.FulfilledRequirementIds.Add(i);
                }
            }
        }
        else
        {
            // Uncheck this level and all levels above it
            for (int i = requirementIndex; i < item.Criteria.QualityLevels.Count; i++)
            {
                if (item.CriteriaProgress.FulfilledRequirementIds.Contains(i))
                {
                    item.CriteriaProgress.FulfilledRequirementIds.Remove(i);
                }
            }
        }

        await criteriaProgressService.UpdateAsync(item.CriteriaProgress);
        CalculateGrade();
    }

    private async Task UpdateNotes(ProjectCriteria item, ChangeEventArgs e)
    {
        item.CriteriaProgress.Notes = e.Value?.ToString();
        await criteriaProgressService.UpdateAsync(item.CriteriaProgress);
    }

    private void ToggleEditMode()
    {
        isEditing = !isEditing;
        if (!isEditing)
        {
            // Reset if cancelled
            editName = project!.Name;
            editTopic = project!.Topic;
        }
    }

    private async Task SaveProjectDetails()
    {
        if (project is null) return;

        project.Name = editName;
        project.Topic = editTopic;

        context.Projects.Update(project);
        await context.SaveChangesAsync();

        isEditing = false;
    }

    private async Task DeleteProject()
    {
        if (project is null) return;

        bool confirmed = await jsRuntime.InvokeAsync<bool>("confirm", "Are you sure you want to delete this project? This action cannot be undone.");

        if (confirmed)
        {
            context.Projects.Remove(project);
            await context.SaveChangesAsync();
            navigationManager.NavigateTo("/");
        }
    }

    private async Task ResetAllProgress()
    {
        bool confirmed = await jsRuntime.InvokeAsync<bool>("confirm", "Are you sure you want to reset all progress? All checkboxes will be unchecked.");

        if (confirmed)
        {
            foreach (var item in criteriaList)
            {
                item.CriteriaProgress.FulfilledRequirementIds.Clear();
                await criteriaProgressService.UpdateAsync(item.CriteriaProgress);
            }
            CalculateGrade();
        }
    }

    private void OpenCriteriaModal()
    {
        allCriteria = staticCriteriaService.GetAll();
        selectedCriteriaIds = criteriaList.Select(c => c.Criteria.Id).ToHashSet();
        criteriaSearchTerm = "";
        showCriteriaModal = true;
    }

    private void CloseCriteriaModal()
    {
        showCriteriaModal = false;
    }

    private void ToggleCriteriaSelection(string criteriaId)
    {
        if (selectedCriteriaIds.Contains(criteriaId))
        {
            selectedCriteriaIds.Remove(criteriaId);
        }
        else
        {
            selectedCriteriaIds.Add(criteriaId);
        }
    }

    private IEnumerable<IGrouping<string, Criteria>> GetGroupedAvailableCriteria()
    {
        var filtered = allCriteria.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(criteriaSearchTerm))
        {
            filtered = filtered.Where(c =>
                c.Id.Contains(criteriaSearchTerm, StringComparison.OrdinalIgnoreCase) ||
                c.Name.Contains(criteriaSearchTerm, StringComparison.OrdinalIgnoreCase) ||
                c.Description.Contains(criteriaSearchTerm, StringComparison.OrdinalIgnoreCase));
        }

        return filtered.GroupBy(c => c.Section);
    }

    private async Task SaveCriteriaChanges()
    {
        if (project is null) return;

        var currentCriteriaIds = criteriaList.Select(c => c.Criteria.Id).ToHashSet();

        // Add new criteria
        var toAdd = selectedCriteriaIds.Except(currentCriteriaIds);
        foreach (var criteriaId in toAdd)
        {
            var newProgress = new CriteriaProgress
            {
                ProjectId = project.Id,
                CriteriaId = criteriaId,
                FulfilledRequirementIds = [],
                Notes = ""
            };
            await context.CriteriaProgress.AddAsync(newProgress);
        }

        // Remove deselected criteria
        var toRemove = currentCriteriaIds.Except(selectedCriteriaIds);
        foreach (var criteriaId in toRemove)
        {
            var progressToRemove = criteriaList.First(c => c.Criteria.Id == criteriaId).CriteriaProgress;
            context.CriteriaProgress.Remove(progressToRemove);
        }

        await context.SaveChangesAsync();

        // Reload data
        criteriaList = await criteriaProgressService.GetByProject(Id);
        CalculateGrade();

        showCriteriaModal = false;
    }
}
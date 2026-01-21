using Ipa.Manager.Database;
using Ipa.Manager.Models;
using Microsoft.EntityFrameworkCore;

namespace Ipa.Manager.Services.Criterias;

public class CriteriaProgressService(ApplicationDbContext context, IStaticCriteriaService staticCriteria) : ICriteriaProgressService
{
    public async Task CreateAsync(int projectId, IReadOnlyList<string> criteriaIds)
    {
        foreach (var id in criteriaIds)
        {
            var criteriaProgress = new CriteriaProgress
            {
                ProjectId = projectId,
                CriteriaId = id
            };
            await context.CriteriaProgress.AddAsync(criteriaProgress);
        }

        await context.SaveChangesAsync();
    }

    public async Task<IReadOnlyList<ProjectCriteria>> GetByProject(int projectId)
    {
        var criteriasProgress = await context.CriteriaProgress
            .Where(c => c.ProjectId == projectId)
            .ToListAsync();

        return criteriasProgress
            .Select(c => new ProjectCriteria(c, staticCriteria.GetById(c.CriteriaId)))
            .ToList();
    }
    
    public async Task<ProjectCriteria?> GetById(int id)
    {
        var criteriaProgress = await context.CriteriaProgress
            .SingleOrDefaultAsync(c => c.Id == id);

        return criteriaProgress is not null 
            ? new ProjectCriteria(criteriaProgress, staticCriteria.GetById(criteriaProgress.CriteriaId)) 
            : null;
    }

    public async Task UpdateAsync(CriteriaProgress criteriaProgress)
    {
        context.CriteriaProgress.Update(criteriaProgress);
        await context.SaveChangesAsync();
    }
}
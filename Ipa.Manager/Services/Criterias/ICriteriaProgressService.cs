namespace Ipa.Manager.Services.Criterias;

public interface ICriteriaProgressService
{
    Task CreateAsync(int projectId, IReadOnlyList<string> criteriaIds);

    Task<IReadOnlyList<ProjectCriteria>> GetByProject(int projectId);

    Task<ProjectCriteria?> GetById(int id);
}
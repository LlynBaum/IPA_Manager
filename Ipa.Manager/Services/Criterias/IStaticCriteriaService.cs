namespace Ipa.Manager.Services.Criterias;

public interface IStaticCriteriaService
{
    IReadOnlyList<Criteria> GetAll();
    
    Criteria GetById(string id);

    IReadOnlyList<Criteria> GetAllById(IEnumerable<string> ids);

    IReadOnlyDictionary<string, Criteria> GetLookupTableByIds(IEnumerable<string> ids);

    Task InitializeAsync(string fileName, CancellationToken cancellationToken = default);
}
using System.Reflection;

namespace Ipa.Manager.Services.Criterias;

public interface ICriteriaService
{
    IReadOnlyList<Criteria> GetAll();
    
    Criteria GetById(string id);

    IReadOnlyList<Criteria> GetAllById(IEnumerable<string> ids);

    IReadOnlyDictionary<string, Criteria> GetLookupTableByIds(IEnumerable<string> ids);

    Task InitializeAsync(Assembly assembly, string fileName, CancellationToken cancellationToken = default);
}
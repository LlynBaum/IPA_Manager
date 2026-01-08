using System.Reflection;

namespace Ipa.Manager.Services.Criterias;

public interface ICriteriaService
{
    Criteria GetById(string id);

    IReadOnlyList<Criteria> GetAllById(IEnumerable<string> ids);

    IReadOnlyDictionary<string, Criteria> GetLookupTableByIds(IEnumerable<string> ids);

    Task InitializeAsync(Assembly assembly, CancellationToken cancellationToken = default);
}
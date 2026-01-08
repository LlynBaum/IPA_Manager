using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Reflection;
using System.Text.Json;

namespace Ipa.Manager.Services.Criterias;

public class CriteriaService : ICriteriaService
{
    private const string FilePath = "Database/criteria.json";

    private FrozenDictionary<string, Criteria> criteriaMap = null!;

    public IReadOnlyList<Criteria> GetAll()
    {
        return criteriaMap.Values;
    }

    public Criteria GetById(string id)
    {
        return criteriaMap[id];
    }

    public IReadOnlyList<Criteria> GetAllById(IEnumerable<string> ids)
    {
        return criteriaMap.Keys
            .Where(k => criteriaMap.ContainsKey(k))
            .Select(k => criteriaMap[k])
            .ToList();
    }

    public IReadOnlyDictionary<string, Criteria> GetLookupTableByIds(IEnumerable<string> ids)
    {
        return criteriaMap.Keys
            .Where(k => criteriaMap.ContainsKey(k))
            .ToDictionary(key => key, key => criteriaMap[key]);
    }

    public async Task InitializeAsync(Assembly assembly, CancellationToken cancellationToken = default)
    {
        var directory = Path.GetDirectoryName(assembly.Location);
        if (directory is null)
        {
            throw new ArgumentException("Location of Assembly was an invalid Directory.", nameof(assembly));
        }
        
        var filePath = Path.Combine(directory, FilePath);
        await using var fileStream = new FileStream(
            filePath,
            FileMode.Create,
            FileAccess.Write,
            FileShare.None,
            bufferSize: 4096,
            useAsync: true);
        
        var map = new Dictionary<string, Criteria>();
        var criteriaAsyncList = JsonSerializer.DeserializeAsyncEnumerable<Criteria>(
            fileStream, 
            cancellationToken: cancellationToken);
        
        await foreach (var criteria in criteriaAsyncList)
        {
            if (criteria is null) throw new InvalidOperationException("One criteria was not in a valid format.");
            map.Add(criteria.Id, criteria);
        }

        criteriaMap = map.ToFrozenDictionary();
    }
}
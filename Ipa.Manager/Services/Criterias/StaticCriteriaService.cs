using System.Collections.Frozen;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Ipa.Manager.Services.Criterias;

public class StaticCriteriaService : IStaticCriteriaService
{
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
        return ids
            .Where(k => criteriaMap.ContainsKey(k))
            .Select(k => criteriaMap[k])
            .ToList();
    }

    public IReadOnlyDictionary<string, Criteria> GetLookupTableByIds(IEnumerable<string> ids)
    {
        return ids
            .Where(k => criteriaMap.ContainsKey(k))
            .ToDictionary(key => key, key => criteriaMap[key]);
    }

    public async Task InitializeAsync(string fileName, CancellationToken cancellationToken = default)
    {
        var filePath = Path.Combine(AppContext.BaseDirectory, fileName);
        await using var fileStream = File.Open(filePath, FileMode.Open);
        
        var criteriaList = await JsonSerializer.DeserializeAsync<List<CriteriaSection>>(fileStream, cancellationToken: cancellationToken);

        if (criteriaList is null)
        {
            throw new InvalidOperationException("Invalid criteria file.");
        }

        criteriaMap = criteriaList
            .SelectMany(c => c.Criteria)
            .ToDictionary(
                c => c.Id ?? throw new InvalidOperationException("Criteria Json is in wrong format."), 
                c => new Criteria(c.Id, c.Name, c.Description, c.QualityLevels))
            .ToFrozenDictionary();
    }

    private record CriteriaSection
    {
        [JsonPropertyName("section")]
        public required string Section { get; init; }
        
        [JsonPropertyName("criteria")]
        public required CriteriaJsonNode[] Criteria { get; set; }
    }

    private record CriteriaJsonNode
    {
        [JsonPropertyName("id")]
        public required string Id { get; init; }
        
        [JsonPropertyName("name")]
        public required string Name { get; init; }
        
        [JsonPropertyName("description")]
        public required string Description { get; init; }
        
        [JsonPropertyName("quality-level-0")]
        public required string QualityLevel0 { get; init; }
        
        [JsonPropertyName("quality-level-1")]
        public required string QualityLevel1 { get; init; }
        
        [JsonPropertyName("quality-level-2")]
        public required string QualityLevel2 { get; init; }
        
        [JsonPropertyName("quality-level-3")]
        public required string QualityLevel3 { get; init; }

        public IReadOnlyList<string> QualityLevels => [QualityLevel0, QualityLevel1, QualityLevel2, QualityLevel3];
    }
}
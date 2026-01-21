namespace Ipa.Manager.Services.Criterias;

public record Criteria(string Id, string Name, string Description, string Section, IReadOnlyList<string> QualityLevels)
{
    public string QualityLevel1 => QualityLevels[0];
    public string QualityLevel2 => QualityLevels[1];
    public string QualityLevel3 => QualityLevels[2];
    public string QualityLevel4 => QualityLevels[3];
}
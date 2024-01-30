namespace Core;

public class FamFeederOptions
{
    public string? SearchIndexEndpoint { get; set; }
    public string? SearchIndexName { get; set; }
    public string? SearchIndexAccessKey { get; set; }
    public string? SearchIndexBatchSize { get; set; }
    public string? SearchIndexBatchDelay { get; set; }
    public string? DbStatusAiCs { get; set; }
    public string? FamFeederPlantFilter { get; set; }
    public List<string> FamFeederPlantFilterList => FamFeederPlantFilter?.Split(",", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList() ?? throw new Exception($"There is no config value for {nameof(FamFeederPlantFilter)}. It should be a comma separated list of PCS projectschemas like PCS$GRANE");
}
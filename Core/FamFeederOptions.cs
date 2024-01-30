namespace Core;

public class FamFeederOptions
{
    public string? SearchIndexEndpoint { get; set; }
    public string? SearchIndexName { get; set; }
    public string? SearchIndexAccessKey { get; set; }
    public string? SearchIndexBatchSize { get; set; }
    public string? SearchIndexBatchDelay { get; set; }
    public string? DbStatusAiCs { get; set; }
    public string? PlantFilter { get; set; }
    public List<string> PlantFilterList => PlantFilter?.Split(",", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList() ?? throw new Exception($"There is no config value for {nameof(PlantFilter)}. It should be a comma separated list of PCS projectschemas like PCS$GRANE");
}
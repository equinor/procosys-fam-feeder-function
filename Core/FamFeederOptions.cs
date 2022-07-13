namespace Core;

public class FamFeederOptions
{
    public FamFeederOptions()
    {
    }

    public FamFeederOptions(string? proCoSysConnectionString)
    {
        ProCoSysConnectionString = proCoSysConnectionString;
    }

    public string? ProCoSysConnectionString { get; set; }
    public string? SearchIndexEndpoint { get; set; }
    public string? SearchIndexName { get; set; }
    public string? SearchIndexAccessKey { get; set; }
    public string? SearchIndexBatchSize { get; set; }
    public string? SearchIndexBatchDelay { get; set; }

}
namespace Core.Models;

public class QueryParameters
{
    //Empty constructor for JSON parsing
    public QueryParameters() : this(new List<string>(), new List<string>()){}

    public QueryParameters(string plant, string pcsTopic, bool shouldAddToQueue = false) : this(new List<string> { plant }, new List<string> { pcsTopic }, shouldAddToQueue) { }
    public QueryParameters(List<string> plants, List<string> pcsTopics, bool shouldAddToQueue = false)
    {
        Plants = plants;
        PcsTopics = pcsTopics;
        ShouldAddToQueue = shouldAddToQueue;
    }

    public List<string> Plants { get; }

    public List<string> PcsTopics { get; }
    
    public bool ShouldAddToQueue { get; set; }
  
}
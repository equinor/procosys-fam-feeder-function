namespace Core.Models;

public class QueryParameters
{
    //Empty constructor for JSON parsing
    public QueryParameters() : this(new List<string>(), new List<string>()){}

    public QueryParameters(string plant, string pcsTopic) : this(new List<string> { plant }, new List<string> { pcsTopic }) { }
    public QueryParameters(List<string> plants, List<string> pcsTopics)
    {
        Plants = plants;
        PcsTopics = pcsTopics;
    }

    public List<string> Plants { get; }

    public List<string> PcsTopics { get; }
}
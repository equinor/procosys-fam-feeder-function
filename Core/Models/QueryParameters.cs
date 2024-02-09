namespace Core.Models;

public class QueryParameters
{
    public QueryParameters(string plants, string pcsTopics) : this(new List<string> { plants }, new List<string> { pcsTopics }) { }
    public QueryParameters(List<string> plants, List<string> pcsTopics)
    {
        Plants = plants;
        PcsTopics = pcsTopics;
    }

    public List<string> Plants { get; }

    public List<string> PcsTopics { get; }
}
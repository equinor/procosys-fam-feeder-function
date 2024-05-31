namespace Core.Models;

public class QueryParameters
{
    //Empty constructor for JSON parsing
    public QueryParameters() : this( new List<string>(), ""){}

    public QueryParameters(string plant, string pcsTopic, bool shouldAddToQueue = false) : this(new List<string> { plant },pcsTopic, shouldAddToQueue) { }
    public QueryParameters(List<string> plants, string topic, bool shouldAddToQueue = false)
    {
        Plants = plants;
        PcsTopic = topic;
        ShouldAddToQueue = shouldAddToQueue;
    }

    public string PcsTopic { get; set; }

    public List<string> Plants { get; }
    
    
    public bool ShouldAddToQueue { get; set; }
  
}
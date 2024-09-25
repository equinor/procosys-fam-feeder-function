namespace Core.Models;

public class QueryParameters
{
    //Empty constructor for JSON parsing
    public QueryParameters() : this( new List<string>(), ""){}

    public QueryParameters(string plant, QueryParameters param) : this(new List<string> { plant },param) { }
    public QueryParameters(string plant, string pcsTopic, bool shouldAddToQueue = false) : this(new List<string> { plant }, pcsTopic, shouldAddToQueue) { }
    public QueryParameters(List<string> plants, string topic, bool shouldAddToQueue = false, DateTime? checkAfterDate = null)
    {
        Plants = plants;
        PcsTopic = topic;
        ShouldAddToQueue = shouldAddToQueue;
        CheckAfterDate = checkAfterDate;
    }   
    
    public QueryParameters(List<string> plants, QueryParameters param)
    {
        Plants = plants;
        PcsTopic = param.PcsTopic;
        ShouldAddToQueue = param.ShouldAddToQueue;
        CheckAfterDate = param.CheckAfterDate;
    }

    public DateTime? CheckAfterDate { get; set; }

    public string PcsTopic { get; set; }

    public List<string> Plants { get; }
    
    public bool ShouldAddToQueue { get; set; }
  
}
using Equinor.ProCoSys.PcsServiceBus;

namespace Core.Models;

public class QueryParameters
{
    public QueryParameters(string plant, string pcsTopic)
    {
        Plant = plant;
        PcsTopic = pcsTopic;
    }

    public string Plant { get; }

    public string PcsTopic { get; }
}
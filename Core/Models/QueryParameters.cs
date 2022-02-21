using Equinor.ProCoSys.PcsServiceBus;

namespace Core.Models;

public class QueryParameters
{
    public QueryParameters(string plant, PcsTopic pcsTopic)
    {
        Plant = plant;
        PcsTopic = pcsTopic;
    }

    public string Plant { get; }

    public PcsTopic PcsTopic { get; }
}
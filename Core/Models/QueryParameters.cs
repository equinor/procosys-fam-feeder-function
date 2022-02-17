

using Equinor.ProCoSys.PcsServiceBus;

namespace Core.Models;

public class QueryParameters
{

    public QueryParameters(string facility, PcsTopic pcsTopic)
    {
        Facility = facility;
        PcsTopic = pcsTopic;
    }
    public string Facility { get;  }

    public PcsTopic PcsTopic  { get;  }
}
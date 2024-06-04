using Equinor.ProCoSys.PcsServiceBus.Interfaces;

namespace Core.Models;

public class Person : IPersonEventV1
{
    public Guid ProCoSysGuid { get; init; }
    public Guid? AzureOid { get; init; }
    public string FirstName { get; init; }
    public string LastName { get; init; }
    public string UserName { get; init; }
    public string Email { get; init; }
    public bool SuperUser { get; init; }
    public DateTime LastUpdated { get; init; }
    public string EventType { get; init; }
}

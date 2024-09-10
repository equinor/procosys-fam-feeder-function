using Equinor.ProCoSys.PcsServiceBus.Interfaces;
using JetBrains.Annotations;

namespace Core.Models;

#pragma warning disable CS8618
[UsedImplicitly]
public class NotificationSignature : INotificationSignatureEventV1
{
    public string EventType => "NotificationSignatureEvent";
    public string Plant { get; init; }
    public Guid ProCoSysGuid { get; init; }
    public string ProjectName { get; init; }
    public Guid ProjectGuid { get; init; }
    public Guid NotificationGuid { get; init; }
    public string? SignatureRoleCode { get; init; }
    public long Sequence { get; init; }
    public Guid? SignerPersonOid { get; init; }
    public string? SignerFunctionalRoleCode { get; init; }
    public Guid? SignedByOid { get; init; }
    public DateTime? SignedAt { get; init; }
    public DateTime LastUpdated { get; init; }
}

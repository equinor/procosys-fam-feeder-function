using Equinor.ProCoSys.PcsServiceBus.Interfaces;

namespace Core.Models;
#pragma warning disable CS8618
public class SwcrSignature : ISwcrSignatureEventV1
{
    public string EventType { get; }
    public string Plant { get; init; }
    public Guid ProCoSysGuid { get; init; }
    public long SwcrSignatureId { get; init; }
    public string ProjectName { get; init; }
    public string SwcrNo { get; init; }
    public Guid SwcrGuid { get; init; }
    public string SignatureRoleCode { get; init; }
    public string? SignatureRoleDescription { get; init; }
    public int Sequence { get; init; }
    public Guid? SignedByAzureOid { get; init; }
    public string? FunctionalRoleCode { get; init; }
    public string? FunctionalRoleDescription { get; init; }
    public DateTime? SignedDate { get; init; }
    public string? StatusCode { get; init; }
    public DateTime LastUpdated { get; init; }
}
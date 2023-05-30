using System.Data;
using Dapper;

namespace Infrastructure.Handlers;

public class GuidTypeHandler : SqlMapper.TypeHandler<Guid>
{
    public static readonly GuidTypeHandler Default = new();

    public override Guid Parse(object value) =>
        Guid.TryParse(value.ToString(), out var guid)
            ? guid
            : new Guid((byte[])value);

    public override void SetValue(IDbDataParameter parameter, Guid value)
        => throw new NotImplementedException();
}

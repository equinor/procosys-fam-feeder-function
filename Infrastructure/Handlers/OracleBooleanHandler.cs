using System.Data;
using Dapper;

namespace Infrastructure.Handlers;

internal class OracleBooleanHandler : SqlMapper.TypeHandler<bool>
{
    public static readonly OracleBooleanHandler Default = new();

    public override bool Parse(object value) => value.ToString() == "Y";

    public override void SetValue(IDbDataParameter parameter, bool value) => parameter.Value = value ? 'Y' : 'N';
}

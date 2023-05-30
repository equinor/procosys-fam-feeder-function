using System.Data;
using Dapper;

namespace Infrastructure.Handlers;

internal class BooleanHandler : SqlMapper.TypeHandler<bool>
{
    public static readonly BooleanHandler Default = new();

    public override bool Parse(object value) => value.ToString() == "Y";

    public override void SetValue(IDbDataParameter parameter, bool value) => parameter.Value = value ? 'Y' : 'N';
}

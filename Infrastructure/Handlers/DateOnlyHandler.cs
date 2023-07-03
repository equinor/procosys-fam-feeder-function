using System.Data;
using Dapper;

namespace Infrastructure.Handlers;

public class DateOnlyHandler : SqlMapper.TypeHandler<DateOnly>
{
    public static readonly DateOnlyHandler Default = new();

    public override DateOnly Parse(object value)
    {
        var dateTime = DateTime.SpecifyKind((DateTime)value, DateTimeKind.Utc);
        return new DateOnly(dateTime.Year, dateTime.Month, dateTime.Day);
    }

    public override void SetValue(IDbDataParameter parameter, DateOnly value)
        => parameter.Value = value.ToString("yyyy-MM-dd");
}

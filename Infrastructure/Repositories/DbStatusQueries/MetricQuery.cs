namespace Infrastructure.Repositories.DbStatusQueries;

public static class MetricQuery
{
    internal static string GetQuery()
    {
        return @"select username, program , a.sid, a.serial#, b.name, c.value
from v$session a, v$statname b, v$sesstat c
where b.STATISTIC# =c.STATISTIC#
and c.sid=a.sid and b.name like 'redo%'
and c.value>0";
    }
}
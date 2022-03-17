using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories.Queries;

public class StockQuery
{
    internal static string GetQuery(string schema)
    {
        return @$"select
            '{{""Plant"" : ""' || s.projectschema || 
             '"", ""StockId"" : ' || s.id || 
            '"", ""StockNo"" : ' ||regexp_replace(s.stockno, '([""\])', '\\\1')  || 
            '"", ""Description"" : ""' || regexp_replace(s.description, '([""\])', '\\\1') || 
            '"", ""LastUpdated"" : ""' || TO_CHAR(s.LAST_UPDATED, 'YYYY-MM-DD hh:mm:ss')  ||
            '""}}'  as message
            from stock s
            where s.projectschema = '{schema}'";
    }


}


namespace Infrastructure.Repositories.Queries;

public class WorkOrderMaterialQuery
{
    internal static string GetQuery(string schema)
    {
        return @$"select
         '{{""Plant"" : ""' || wm.projectschema || 
         '"", ""ProjectName"" : ""' || p.NAME || 
         '"", ""WoNo"" : ""' || wo.wono ||
         '"", ""WoId"" : ""' || wo.wo_id ||
         '"", ""ItemNo"" : ""' || wm.itemno || 
         '"", ""TagNo"" : ""' || regexp_replace(t.tagno, '([""\])', '\\\1') ||
         '"", ""TagId"" : ""' || wm.tag_id ||
         '"", ""TagRegisterId"" : ""' || t.register_id ||
         '"", ""StockId"" : ""' || wm.stock_id ||
         '"", ""Quantity"" : ""' || wm.quantity ||
         '"", ""UnitName"" : ""' || regexp_replace(u.name, '([""\])', '\\\1') ||
         '"", ""UnitDescription"" : ""' || regexp_replace(u.description, '([""\])', '\\\1') ||
         '"", ""AdditionalInformation"" : ""' || regexp_replace(wm.description, '([""\])', '\\\1') ||
         '"", ""RequiredDate"" : ""' || TO_CHAR(wm.requireddate, 'YYYY-MM-DD hh:mm:ss') ||
         '"", ""EstimatedAvailableDate"" : ""' || TO_CHAR(wm.ESTIMATEDAVAILABLEDATE, 'YYYY-MM-DD hh:mm:ss') ||
         '"", ""Available"" : ""' || decode(wm.AVAILABLE,'Y', 'true', 'N', 'false') ||
         '"", ""MaterialStatus"" : ""' ||regexp_replace(ms.code, '([""\])', '\\\1') ||       
         '"", ""StockLocation"" : ""' || regexp_replace(sl.code, '([""\])', '\\\1') ||
         '"", ""LastUpdated"" : ""' || TO_CHAR(wm.last_updated, 'YYYY-MM-DD hh:mm:ss') ||        
         '""}}' as message
         from wo_material wm
            join wo on wo.wo_id = wm.wo_id
            join project p on p.project_id = wo.project_id       
            left join tag t on t.tag_id = wm.tag_id
            left join library ms  on ms.library_id = wm.materialstatus_id
            left join library sl on sl.library_id = wm.stocklocation_id
            left join unit u on u.unit_id = wm.unit_id 
         where wm.projectschema = '{schema}'";
    }
}
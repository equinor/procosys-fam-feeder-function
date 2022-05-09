

namespace Infrastructure.Repositories.Queries;

public class CommPkgOperationQuery
{

    internal static string GetQuery(string schema)
    {
        return $@"select   '{{""Plant"" : ""' || :New.projectschema || '"",
    ""ProjectName"" : ""' || p.NAME || '"",
    ""CommPkgNo"" : ""' || c.commpkgno || '"",
    ""InOperation"" : ' || decode(:new.inoperation,'Y', 'true', 'N', 'false') || ', 
    ""ReadyForProduction"" : ' || decode(:new.readyforproduction,'Y', 'true', 'N', 'false') || ', 
    ""MaintenanceProgram"" : ' || decode(:new.maintenanceprog,'Y', 'true', 'N', 'false') || ', 
    ""YellowLine"" : ' || decode(:new.yellowline,'Y', 'true', 'N', 'false') || ', 
    ""BlueLine"" : ' || decode(:new.blueline,'Y', 'true', 'N', 'false') || ', 
    ""YellowLineStatus"" : ""' || regexp_replace(:new.yellowlinestatus, '([""\])', '\\\1') || '"",
    ""BlueLineStatus"" : ""' || regexp_replace(:new.bluelinestatus, '([""\])', '\\\1') || '"",
    ""TemperaryOperationEst"" : ' || decode(:new.temporaryoperation_est,'Y', 'true', 'N', 'false') || ',
    ""PmRoutine"" : ' || decode(:new.pmroutine,'Y', 'true', 'N', 'false') || ', 
    ""CommissioningResp"" : ' ||  decode(:new.commissioningresp,'Y', 'true', 'N', 'false') || ', 
    ""ValveBlindingList"" : ""' || decode(:new.valveblindinglist,'Y', 'true', 'N', 'false') ||  '"", 
    ""LastUpdated"" : ""' || TO_CHAR(:New.LAST_UPDATED, 'yyyy-mm-dd hh24:mi:ss')  || '""
    }}'
  from commpkg_operation co
    join commpkg c on c.commpkg_id = co.commpkg_id;
    join project p ON p.project_id = c.project_id
  where co.projectschema  =  '{schema}' ";
    }
}
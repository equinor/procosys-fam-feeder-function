namespace Infrastructure.Repositories.Queries;

public static class PipingRevisionQuery
{

    internal static string GetQuery(string schema)
    {
           return @$"select
              '{{""Plant"" : ""' || pr.projectschema || 
              '"", ""PipingRevisionId"" : ""' || pr.pipingrevision_id ||
              '"", ""Revision"" : ""' || pr.testrevisionno || 
              '"", ""McPkgNo"" : ""' || m.mcpkgno ||
              '"", ""ProjectName"" : ""' || p.name || 
              '"", ""MaxDesignPressure"" : ""' || pr.maxdesignpressure || 
              '"", ""MaxTestPressure"" : ""' || pr.maxtestpressure || 
              '"", ""Comments"" : ""' || regexp_replace(pr.comments, '([""\])', '\\\1') ||
              '"", ""TestISODocumentNo"" : ""' || ti.documentno ||
              '"", ""TestISORevision"" : ""' || pr.TEST_ISO_REVISIONNO ||
              '"", ""PurchaseOrderNo"" : ""' || po.packageno || 
              '"", ""CallOffNo"" : ""' || co.calloffno ||
              '"", ""LastUpdated"" : ""' || TO_CHAR(pr.LAST_UPDATED, 'YYYY-MM-DD hh:mm:ss') || 
              '""}}' as message
                from pipingrevision pr              
                    join mcpkg m on m.mcpkg_id = pr.mcpkg_id
                    join project p on p.project_id=m.project_id
                    left join document ti on ti.document_id = pr.document_id
                    left join purchaseorder po on po.package_id = pr.package_id
                    left join calloff co on co.calloff_id = pr.calloff_id
                where pr.projectschema = '{schema}'";

    }
}
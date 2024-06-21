using Dapper;
using Equinor.ProCoSys.PcsServiceBus.Queries;

namespace Infrastructure.CompletionQueries;

public class PunchAttachmentQuery
{
    public static (string query, DynamicParameters parameters) GetQuery(string? plant = null)
    {
        QueryHelper.DetectFaultyPlantInput(plant);
        long? discard = null;
        var whereClause = QueryHelper.CreateWhereClause(discard, plant, "aa", "irrelevant");

        var query = @$"SELECT 
               aa.projectSchema AS Plant, 
               pi.procosys_guid AS PunchItemGuid,
               aa.procosys_guid AS AttachmentGuid,
               pr.name AS ProjectName,
               aa.name AS FileName,
               aa.uri,
               aa.file_id AS FileId,
               aa.title,
               per.azure_oid AS CreatedByGuid,
               aa.createdat,
               aa.last_updated as LastUpdated,
               aa.last_updatedbyuser as LastUpdatedByUser
           FROM attachment aa
               JOIN attachmentLink al on al.attachment_id = aa.id
               JOIN punchListItem pi on pi.punchListItem_id = al.punchListItem_id
               JOIN tagCheck tc ON tc.tagcheck_id = pi.tagCheck_id
               JOIN tagFormularType tf ON tc.tagFormularType_id = tf.tagFormularType_id
               JOIN tag t ON tf.tag_id = t.tag_id
               JOIN project pr ON t.project_id = pr.project_id
               JOIN person per on per.person_id = aa.createdby_id
               JOIN projectschema psc on psc.projectschema = aa.projectschema
           {whereClause.clause}
               AND pr.isVoided = 'N'
               AND psc.isVoided = 'N'
                ";

        return (query, whereClause.parameters);

    }
}
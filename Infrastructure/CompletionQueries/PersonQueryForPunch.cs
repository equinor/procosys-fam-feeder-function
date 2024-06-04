namespace Infrastructure.CompletionQueries;

public static class PersonQueryForPunch
{
    public static string GetQuery()
    {
        return @"SELECT DISTINCT
                        p.procosy_guid as ProCoSysGuid,
                        p.azure_oid as AzureOid, 
                        p.firstname as FirstName, 
                        p.lastname as LastName, 
                        p.username as UserName, 
                        p.emailaddress as Email,
                        p.super as SuperUser, 
                        p.last_updated as LastUpdated,
                        'CreateForPunch' as EventType
                        FROM Person p
                        WHERE (EXISTS (
                            SELECT 1 FROM PunchListItem pl_created WHERE p.person_id = pl_created.createdby_id
                        ) OR EXISTS (
                            SELECT 1 FROM PunchListItem pl_updated WHERE p.person_id = pl_updated.updatedby_id
                        ) OR EXISTS (
                            SELECT 1 FROM PunchListItem pl_verified WHERE p.person_id = pl_verified.verifiedby_id
                        ) OR EXISTS (
                            SELECT 1 FROM PunchListItem pl_rejected WHERE p.person_id = pl_rejected.rejectedby_id
                        ) OR EXISTS (
                            SELECT 1 FROM PunchListItem pl_cleared WHERE p.person_id = pl_cleared.clearedby_id
                        ) OR EXISTS (
                            SELECT 1 FROM PunchListItem pl_action WHERE p.person_id = pl_action.actionbyperson_id
                        ))";
    }
}


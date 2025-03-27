﻿using Core.Interfaces;
using Equinor.ProCoSys.PcsServiceBus.Queries;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Text.Json;
using Core.Models;
using Dapper;
using Equinor.ProCoSys.PcsServiceBus.Interfaces;
using Infrastructure.CompletionQueries;
using Infrastructure.Handlers;
using Action = Core.Models.Action;
using CommPkg = Core.Models.CommPkg;
using CommPkgQuery = Core.Models.CommPkgQuery;
using Document = Core.Models.Document;
using McPkg = Core.Models.McPkg;
using Tag = Core.Models.Tag;

namespace Infrastructure.Repositories;

public class EventRepository : IEventRepository
{
    private readonly AppDbContext _context;
    public EventRepository(AppDbContext context) 
        => _context = context;

    public async Task<List<string>> GetSwcrAttachments(string plant) => await Query<SwcrAttachment>(SwcrAttachmentQuery.GetQuery(null, plant));
    public async Task<List<string>> GetSwcrOtherReferences(string plant) => await Query<SwcrOtherReference>(SwcrOtherReferenceQuery.GetQuery(null, plant));
    public async Task<List<string>> GetSwcrTypes(string plant) => await Query<SwcrType>(SwcrTypeQuery.GetQuery(null, plant));
    public async Task<List<string>> GetActions(string plant) => await Query<Action>(ActionQuery.GetQuery(null, plant));
    public async Task<List<string>> GetCommPkgTasks(string plant) => await Query<CommPkgTask>(CommPkgTaskQuery.GetQuery(null, null, plant));
    public async Task<List<string>> GetTasks(string plant) => await Query<TaskEvent>(TaskQuery.GetQuery(null, plant));
    public async Task<List<string>> GetMcPackages(string plant) => await Query<McPkg>(McPkgQuery.GetQuery(null,plant));
    public async Task<List<string>> GetCommPackages(string plant) => await Query<CommPkg>(Equinor.ProCoSys.PcsServiceBus.Queries.CommPkgQuery.GetQuery(null,plant));
    public async Task<List<string>> GetCommPkgOperations(string plant) => await Query<CommPkgOperation>(CommPkgOperationQuery.GetQuery(null,plant));
    public async Task<List<string>> GetPunchItems(string plant) => await Query<PunchListItem>(PunchListItemQuery.GetQuery(null,plant));
    public async Task<List<string>> GetWorkOrders(string plant) => await Query<WorkOrder>(WorkOrderQuery.GetQuery(null, plant));
    public async Task<List<string>> GetCheckLists(string plant) => await Query<Checklist>(ChecklistQuery.GetQuery(null,plant));
    public async Task<List<string>> GetTags(string plant) => await Query<Tag>(TagQuery.GetQuery(null,plant));
    public async Task<List<string>> GetTagEquipments(string plant) => await Query<TagEquipment>(TagEquipmentQuery.GetQuery(null!,plant));
    public async Task<List<string>> GetMcPkgMilestones(string plant) => await Query<McPkgMilestone>(McPkgMilestoneQuery.GetQuery(null,plant));
    public async Task<List<string>> GetProjects(string plant) => await Query<Project>(ProjectQuery.GetQuery(null, plant));
    public async Task<List<string>> GetSwcrs(string plant) => await Query<Swcr>(SwcrQuery.GetQuery(null,plant));
    public async Task<List<string>> GetSwcrSignatures(string plant) => await Query<SwcrSignature>(SwcrSignatureQuery.GetQuery(null, plant));
    public async Task<List<string>> GetWoChecklists(string plant) => await Query<WorkOrderChecklist>(WorkOrderChecklistQuery.GetQuery(null,null, plant));
    public async Task<List<string>> GetQueries(string plant) => await Query<Query>(QueryQuery.GetQuery(null,plant));
    public async Task<List<string>> GetQuerySignatures(string plant) => await Query<QuerySignature>(QuerySignatureQuery.GetQuery(null, plant));
    public async Task<List<string>> GetPipingRevisions(string plant) => await Query<PipingRevision>(PipingRevisionQuery.GetQuery(null,plant));
    public async Task<List<string>> GetPipingSpools(string plant) => await Query<PipingSpool>(PipingSpoolQuery.GetQuery(null,plant));
    public async Task<List<string>> GetWoMilestones(string plant) => await Query<WorkOrderMilestone>(WorkOrderMilestoneQuery.GetQuery(null,null,plant));
    public async Task<List<string>> GetWoMaterials(string plant) => await Query<WorkOrderMaterial>(WorkOrderMaterialQuery.GetQuery(null,plant));
    public async Task<List<string>> GetStocks(string plant) => await Query<Stock>(StockQuery.GetQuery(null,plant));
    public async Task<List<string>> GetResponsibles(string plant) => await Query<Responsible>(ResponsibleQuery.GetQuery(null, plant));
    public async Task<List<string>> GetLibraries(string plant) => await Query<Library>(LibraryQuery.GetQuery(null, plant));
    public async Task<List<string>> GetDocument(string plant) => await Query<Document>(DocumentQuery.GetQuery(null,plant));
    public async Task<List<string>> GetLoopContents(string plant) => await Query<LoopContent>(LoopContentQuery.GetQuery(null, plant));
    public async Task<List<string>> GetCallOffs(string plant) => await Query<CallOff>(CallOffQuery.GetQuery(null,plant));
    public async Task<List<string>> GetCommPkgQueries(string plant) => await Query<CommPkgQuery>(CommPkgQueryQuery.GetQuery(null, null, plant));
    public async Task<List<string>> GetWoCutoffsByWeekAndPlant(string cutoffWeek, string plant) => await Query<WorkOrderCutoff>(WorkOrderCutoffQuery.GetQuery(null, cutoffWeek, plant));
    public async Task<List<string>> GetWoCutoffsByWeekAndProjectIds(string cutoffWeek, string plant,IEnumerable<long> projectIds) => await Query<WorkOrderCutoff>(WorkOrderCutoffQuery.GetQuery(null, cutoffWeek, plant,null,projectIds));
    public async Task<List<string>> GetHeatTraces(string plant) => await Query<HeatTrace>(HeatTraceQuery.GetQuery(null, plant));
    public async Task<List<string>> GetLibraryFields(string plant) => await Query<LibraryField>(LibraryFieldQuery.GetQuery(null, plant));
    public async Task<List<string>> GetCommPkgMilestones(string plant) => await Query<CommPkgMilestone>(CommPkgMilestoneQuery.GetQuery(null, plant));
    public async Task<List<string>> GetHeatTracePipeTests(string plant) => await Query<HeatTracePipeTest>(HeatTracePipeTestQuery.GetQuery(null, plant));


    // Punch/Completion related queries, these are used when seeding data into completion, and not used for FAM
    public async Task<IEnumerable<string>> GetLibrariesForPunch(string plant) => await Query<Library>(LibraryForPunchQuery.GetQuery(null, plant));
    public async Task<IEnumerable<string>> GetPunchItemsForCompletion(string plant, DateTime? checkAfterDate) => await Query<PunchListItem>(PunchListItemQuery.GetQuery(null, plant, CreateWhereClauseForCompletionPunch(checkAfterDate)));

    public async Task<List<string>> GetWorkOrdersForCompletion(string plant, DateTime? checkAfterDate) => await Query<WorkOrder>(WorkOrderQuery.GetQuery(null, plant, CreateWhereClauseForCompletionWorkOrder(checkAfterDate)));

    public async Task<List<string>> GetDocumentsForCompletion(string plant, DateTime? checkAfterDate) => await Query<Document>(DocumentQuery.GetQuery(null, plant, CreateWhereClauseForCompletionDocument(checkAfterDate)));

    private static string? CreateWhereClauseForCompletionWorkOrder(DateTime? checkAfterDate)
    {
        return !checkAfterDate.HasValue ? null : $" and w.last_Updated > TO_DATE('{checkAfterDate.Value:MM-dd-yyyy}', 'MM-DD-YYYY')";
    }
    private static string? CreateWhereClauseForCompletionDocument(DateTime? checkAfterDate)
    {
        return !checkAfterDate.HasValue ? null : $" and d.last_Updated > TO_DATE('{checkAfterDate.Value:MM-dd-yyyy}', 'MM-DD-YYYY')";
    }

    private static string CreateWhereClauseForCompletionPunch(DateTime? checkAfterDate)
    {
        const string notVoided = " and p.isVoided = 'N'";
        if (!checkAfterDate.HasValue)
        {
            return notVoided;
        }

        var checkBeforeQuery = $" and pl.LAST_UPDATED > TO_DATE('{checkAfterDate.Value:MM-dd-yyyy}', 'MM-DD-YYYY')";
        return notVoided + checkBeforeQuery;
    }

    public async Task<IEnumerable<string>> GetPunchItemHistory(string plant, DateTime? checkAfterDate) => await Query<PunchItemHistory>(PunchHistoryQuery.GetQuery(plant, CreateWhereClauseForCompletionPunchHistory(checkAfterDate)));
    
    private static string? CreateWhereClauseForCompletionPunchHistory(DateTime? checkAfterDate)
    {
        return !checkAfterDate.HasValue ? null : $" and plh.CHANGEDAT > TO_DATE('{checkAfterDate.Value:MM-dd-yyyy}', 'MM-DD-YYYY')";
    }

    public async Task<IEnumerable<string>> GetPunchItemComments(string plant, DateTime? checkAfterDate) => await Query<PunchItemComment>(PunchCommentsQuery.GetQuery(plant, CreateWhereClauseForCompletionPunchComment(checkAfterDate)));
   
    private static string? CreateWhereClauseForCompletionPunchComment(DateTime? checkAfterDate)
    {
        return !checkAfterDate.HasValue ? null : $" and pc.LAST_UPDATED > TO_DATE('{checkAfterDate.Value:MM-dd-yyyy}', 'MM-DD-YYYY')";
    }

    public async Task<IEnumerable<string>> GetPersonsForPunch() => await Query<Person>((PersonQueryForPunch.GetQuery(), new DynamicParameters()));

    public async Task<IEnumerable<string>> GetAttachmentsForCompletion(string plant, DateTime? checkAfterDate) =>
        await Query<PunchItemAttachment>(PunchAttachmentQuery.GetQuery(plant, CreateWhereClauseForCompletionPunchAttachment(checkAfterDate)));

    private static string? CreateWhereClauseForCompletionPunchAttachment(DateTime? checkAfterDate)
    {
        return !checkAfterDate.HasValue ? null : $" and aa.LAST_UPDATED > TO_DATE('{checkAfterDate.Value:MM-dd-yyyy}', 'MM-DD-YYYY')";
    }

    public async Task<IEnumerable<string>> GetPunchPriorityLibRelations(string plant) => 
        await Query<PunchPriorityLibRelation>(PunchPriorityLibraryRelationQuery.GetQuery(null,plant));

    public async Task<IEnumerable<string>> GetNotifications(string plant) => await Query<Notification>(NotificationQuery.GetQuery(null, plant));
    public async Task<IEnumerable<string>> GetNotificationWorkOrders(string plant) => await Query<NotificationWorkOrder>(NotificationWorkOrderQuery.GetQuery(null, plant));
    public async Task<IEnumerable<string>> GetNotificationCommPkgs(string plant)
    {
        var boundaryCommPkg = await Query<NotificationCommPkg>(NotificationCommPkgBoundaryQuery.GetQuery(null, plant));
        var otherCommPkg = await Query<NotificationCommPkg>(NotificationCommPkgOtherQuery.GetQuery(null, plant));
        var combined = boundaryCommPkg.Concat(otherCommPkg);
        return combined;
    }
    public async Task<IEnumerable<string>> GetNotificationSignatures(string plant) => await Query<NotificationSignature>(NotificationSignatureQuery.GetQuery(null, plant));


    private async Task<List<string>> Query<T>((string queryString, DynamicParameters parameters) query) where T : IHasEventType
    {
        var connection = _context.Database.GetDbConnection();
        var connectionWasClosed = connection.State != ConnectionState.Open;
        if (connectionWasClosed)
        {
            await _context.Database.OpenConnectionAsync();
        }
        try
        {
            var events = connection.Query<T>(query.queryString, query.parameters).ToList();
            if (events.Count == 0)
            {
                return new List<string>();
            }

            var list = events.Select(e => JsonSerializer.Serialize(e, DefaultSerializerHelper.SerializerOptions)).ToList();
            return list;
        }
        finally
        {
            //If we open it, we have to close it.
            if (connectionWasClosed)
            {
                await _context.Database.CloseConnectionAsync();
            }
        }
    }

}
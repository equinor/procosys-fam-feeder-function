using Core.Interfaces;
using Core.Models;
using Equinor.ProCoSys.PcsServiceBus.Queries;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;
using System.Data;
using Dapper;

namespace Infrastructure.Repositories;

public class FamEventRepository : IFamEventRepository
{
    private readonly AppDbContext _context;
    public FamEventRepository(AppDbContext context) 
        => _context = context;
    public async Task<List<string>> GetSwcrAttachments(string plant) => await Query(SwcrAttachmentQuery.GetQuery(null, plant));
    public async Task<List<string>> GetSwcrOtherReferences(string plant) => await Query(SwcrOtherReferenceQuery.GetQuery(null, plant));
    public async Task<List<string>> GetSwcrType(string plant) => await Query(SwcrTypeQuery.GetQuery(null, plant));
    public async Task<List<string>> GetActions(string plant) => await Query(ActionQuery.GetQuery(null, plant));
    public async Task<List<string>> GetCommPkgTasks(string plant) => await Query(CommPkgTaskQuery.GetQuery(null, null, plant));
    public async Task<List<string>> GetTasks(string plant) => await Query(TaskQuery.GetQuery(null, plant));
    public async Task<List<string>> GetMcPackages(string plant) => await Query(McPkgQuery.GetQuery(null,plant));
    public async Task<List<string>> GetCommPackages(string plant) => await Query(CommPkgQuery.GetQuery(null,plant));
    public async Task<List<string>> GetCommPkgOperations(string plant) => await Query(CommPkgOperationQuery.GetQuery(null,plant));
    public async Task<List<string>> GetPunchItems(string plant) => await Query(PunchListItemQuery.GetQuery(null,plant));
    public async Task<List<string>> GetWorkOrders(string plant) => await Query(WorkOrderQuery.GetQuery(null, plant));
    public async Task<List<string>> GetCheckLists(string plant) => await Query(ChecklistQuery.GetQuery(null,plant));
    public async Task<List<string>> GetTags(string plant) => await Query(TagQuery.GetQuery(null,plant));
    public async Task<List<string>> GetMcPkgMilestones(string plant) => await Query(McPkgMilestoneQuery.GetQuery(null,plant));
    public async Task<List<string>> GetProjects(string plant) => await Query(ProjectQuery.GetQuery(null, plant));
    public async Task<List<string>> GetSwcr(string plant) => await Query(SwcrQuery.GetQuery(null,plant));
    public async Task<List<string>> GetSwcrSignature(string plant) => await Query(SwcrSignatureQuery.GetQuery(null, plant));
    public async Task<List<string>> GetWoChecklists(string plant) => await Query(WorkOrderChecklistQuery.GetQuery(null,null, plant));
    public async Task<List<string>> GetQuery(string plant) => await Query(QueryQuery.GetQuery(null,plant));
    public async Task<List<string>> GetQuerySignature(string plant) => await Query(QuerySignatureQuery.GetQuery(null, plant));
    public async Task<List<string>> GetPipingRevision(string plant) => await Query(PipingRevisionQuery.GetQuery(null,plant));
    public async Task<List<string>> GetPipingSpool(string plant) => await Query(PipingSpoolQuery.GetQuery(null,plant));
    public async Task<List<string>> GetWoMilestones(string plant) => await Query(WorkOrderMilestoneQuery.GetQuery(null,null,plant));
    public async Task<List<string>> GetWoMaterials(string plant) => await Query(WorkOrderMaterialQuery.GetQuery(null,plant));
    public async Task<List<string>> GetStock(string plant) => await Query(StockQuery.GetQuery(null,plant));
    public async Task<List<string>> GetResponsible(string plant) => await Query(ResponsibleQuery.GetQuery(null, plant));
    public async Task<List<string>> GetLibrary(string plant) => await Query(LibraryQuery.GetQuery(null, plant));
    public async Task<List<string>> GetDocument(string plant) => await Query(DocumentQuery.GetQuery(null,plant));
    public async Task<List<string>> GetLoopContent(string plant) => await Query(LoopContentQuery.GetQuery(null, plant));
    public async Task<List<string>> GetCallOff(string plant) => await Query(CallOffQuery.GetQuery(null,plant));
    public async Task<List<string>> GetCommPkgQuery(string plant) => await Query(CommPkgQueryQuery.GetQuery(null, null, plant));
    public async Task<List<string>> GetWoCutoffsByWeekAndPlant(string cutoffWeek, string plant) => await Query(WorkOrderCutoffQuery.GetQuery(null, cutoffWeek, plant,null));
    public async Task<List<string>> GetHeatTrace(string plant) => await Query(HeatTraceQuery.GetQuery(null, plant));
    public async Task<List<string>> GetLibraryField(string plant) => await Query(LibraryFieldQuery.GetQuery(null, plant));


    public async Task<List<string>> Query((string queryString, DynamicParameters parameters) query) 
    {
        var connection = _context.Database.GetDbConnection();
        var connectionWasClosed = connection.State != ConnectionState.Open;
        if (connectionWasClosed)
        {
            await _context.Database.OpenConnectionAsync();
        }

        try
        {
            var events = connection.Query<string>(query.queryString, query.parameters).ToList();
            if (events.Count == 0)
            {
              //  _logger.LogError("Object/Entity with id {ObjectId} did not return anything", objectId);
                return default;
            }

            return events;
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
    
    
    private async Task<List<string>> ExecuteQuery(string query)
    {
        var dbConnection = _context.Database.GetDbConnection();
        var connectionWasClosed = dbConnection.State != ConnectionState.Open;
        if (connectionWasClosed)
        {
            await _context.Database.OpenConnectionAsync();
        }
        try
        {
            await using var command = dbConnection.CreateCommand();
            command.CommandText = query;
            await using var result = await command.ExecuteReaderAsync();
            var entities = new List<string>();

            while (await result.ReadAsync())
            {
                //Last row
                if (!result.HasRows)
                {
                    continue;
                }

                var s = (string)result[0];

                //For local debugging
                entities.Add(s.Replace("\"WoNo\" : \"���\"", "\"WoNo\" : \"ÆØÅ\""));
            }

            return entities;
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
namespace Core.Interfaces;

public interface IEventRepository
{
    Task<List<string>> GetTagEquipments(string plant);
    Task<List<string>> GetSwcrAttachments(string plant);
    Task<List<string>> GetSwcrOtherReferences(string plant);
    Task<List<string>> GetSwcrTypes(string plant);
    Task<List<string>> GetActions(string plant);
    Task<List<string>> GetCommPkgTasks(string plant);
    Task<List<string>> GetTasks(string plant);
    Task<List<string>> GetProjects(string plant);
    Task<List<string>> GetMcPackages(string plant);
    Task<List<string>> GetCommPackages(string plant);
    Task<List<string>> GetCommPkgOperations(string plant);
    Task<List<string>> GetPunchItems(string plant);
    Task<List<string>> GetWorkOrders(string plant);
    Task<List<string>> GetCheckLists(string plant);
    Task<List<string>> GetTags(string plant);
    Task<List<string>> GetMcPkgMilestones(string plant);
    Task<List<string>> GetWoChecklists(string plant);
    Task<List<string>> GetSwcrs(string plant);
    Task<List<string>> GetSwcrSignatures(string plant);
    Task<List<string>> GetQueries(string plant);
    Task<List<string>> GetQuerySignatures(string plant);
    Task<List<string>> GetPipingRevisions(string plant);
    Task<List<string>> GetPipingSpools(string plant);
    Task<List<string>> GetWoMilestones(string plant);
    Task<List<string>> GetWoMaterials(string plant);
    Task<List<string>> GetStocks(string plant);
    Task<List<string>> GetResponsibles(string plant);
    Task<List<string>> GetLibraries(string plant);
    Task<List<string>> GetDocument(string plant);
    Task<List<string>> GetLoopContents(string plant);
    Task<List<string>> GetCallOffs(string plant);
    Task<List<string>> GetCommPkgQueries(string plant);
    Task<List<string>> GetWoCutoffsByWeekAndPlant(string cutoffWeek, string plant);
    Task<List<string>> GetWoCutoffsByWeekAndProjectIds(string cutoffWeek, string plant, IEnumerable<long> projectIds);
    Task<List<string>> GetHeatTraces(string plant);
    Task<List<string>> GetLibraryFields(string plant);
    Task<List<string>> GetCommPkgMilestones(string plant);
    Task<List<string>> GetHeatTracePipeTests(string plant);
    Task<IEnumerable<string>> GetLibrariesForPunch(string plant);
    Task<IEnumerable<string>> GetPersonsForPunch();
    Task<IEnumerable<string>> GetPunchItemsForCompletion(string plant);
    Task<IEnumerable<string>> GetPunchItemHistory(string plant);
    Task<IEnumerable<string>> GetPunchItemComments(string plant);
    Task<IEnumerable<string>> GetAttachmentsForCompletion(string plant);
}
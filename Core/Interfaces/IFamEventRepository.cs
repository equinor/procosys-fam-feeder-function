namespace Core.Interfaces;

public interface IFamEventRepository
{
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
    Task<List<string>> GetMilestones(string plant);
    Task<List<string>> GetWoCutoffs(string month, string plant, string? connectionString);
    Task<List<string>> GetWoChecklists(string plant);
    Task<List<string>> GetSwcr(string plant);
    Task<List<string>> GetSwcrSignature(string plant);
    Task<List<string>> GetQuery(string plant);
    Task<List<string>> GetQuerySignature(string plant);
    Task<List<string>> GetPipingRevision(string plant);
    Task<List<string>> GetPipingSpool(string plant);
    Task<List<string>> GetWoMilestones(string plant);
    Task<List<string>> GetWoMaterials(string plant);
    Task<List<string>> GetStock(string plant);
    Task<List<string>> GetResponsible(string plant);
    Task<List<string>> GetLibrary(string plant);
    Task<List<string>> GetDocument(string plant);
    Task<List<string>> GetLoopContent(string plant);
    Task<List<string>> GetCallOff(string plant);
    Task<List<string>> GetCommPkgQuery(string plant);
    Task<List<string>> GetWoCutoffsByWeekAndPlant(string cutoffWeek, string plant);
    Task<List<string>> GetHeatTrace(string plant);
}
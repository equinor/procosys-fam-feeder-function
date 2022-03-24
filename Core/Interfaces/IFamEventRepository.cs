using Core.Models;

namespace Core.Interfaces;

public interface IFamEventRepository
{
    Task<List<FamEvent>> GetProjects(string plant);

    Task<List<FamEvent>> GetMcPackages(string plant);

    Task<List<FamEvent>> GetCommPackages(string plant);

    Task<List<FamEvent>> GetPunchItems(string plant);

    Task<List<FamEvent>> GetWorkOrders(string plant);

    Task<List<FamEvent>> GetCheckLists(string plant);

    Task<List<FamEvent>> GetTags(string plant);

    Task<List<FamEvent>> GetMilestones(string plant);

    Task<List<FamEvent>> GetWoCutoffs(string month, string plant, string? connectionString);

    Task<List<FamEvent>> GetWoChecklists(string plant);

    Task<List<FamEvent>> GetSwcr(string plant);

    Task<List<FamEvent>> GetSwcrSignature(string plant);

    Task<List<FamEvent>> GetQuery(string plant);

    Task<List<FamEvent>> GetQuerySignature(string plant);
    Task<List<FamEvent>> GetPipingRevision(string plant);
    Task<List<FamEvent>> GetWoMilestones(string plant);
    Task<List<FamEvent>> GetWoMaterials(string plant);
    Task<List<FamEvent>> GetStock(string plant);
    Task<List<FamEvent>> GetResponsible(string plant);
    Task<List<FamEvent>> GetLibrary(string plant);
}
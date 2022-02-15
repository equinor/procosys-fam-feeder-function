using Core.Models;

namespace Core.Interfaces
{
    public interface IFamEventRepository
    {
        Task<List<FamEvent>> GetProjects();

        Task<List<FamEvent>> GetMcPackages();

        Task<List<FamEvent>> GetCommPackages();

        Task<List<FamEvent>> GetPunchItems();

        Task<List<FamEvent>> GetWorkOrders();

        Task<List<FamEvent>> GetCheckLists();

        Task<List<FamEvent>> GetTags();

        Task<List<FamEvent>> GetMilestones();

        Task<List<FamEvent>> GetWoCutoffs(string month, string connectionString);

        Task<List<FamEvent>> GetWoChecklists();

        Task<List<FamEvent>> GetSwcr();

        Task<List<FamEvent>> GetSwcrSignature();

        Task<List<FamEvent>> GetQuery();

        Task<List<FamEvent>> GetQuerySignature();
    }
}
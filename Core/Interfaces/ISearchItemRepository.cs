using Core.Models.Search;

namespace Core.Interfaces;

public interface ISearchItemRepository
{
    Task<List<IndexDocument>> GetMcPackages(string plant);
    Task<List<IndexDocument>> GetCommPackages(string plant);
    Task<List<IndexDocument>> GetPunchItems(string plant);
    Task<List<IndexDocument>> GetTags(string plant);
}
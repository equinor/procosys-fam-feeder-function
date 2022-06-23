using Core.Models;

namespace Core.Interfaces;

public interface IWoCutoffEventRepository
{
    Task<List<string>> GetWoCutoffs(string month);
}
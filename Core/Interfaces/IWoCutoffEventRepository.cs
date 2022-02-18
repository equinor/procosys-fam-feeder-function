using Core.Models;

namespace Core.Interfaces;

public interface IWoCutoffEventRepository
{
    Task<List<FamEvent>> GetWoCutoffs(string month);
}
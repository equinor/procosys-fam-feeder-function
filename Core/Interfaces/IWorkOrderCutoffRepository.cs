namespace Core.Interfaces;

public interface IWorkOrderCutoffRepository
{
    Task<List<string>> GetWoCutoffs(string month, string plant);
}
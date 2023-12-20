namespace Core.Interfaces;

public interface IWorkOrderCutoffRepository
{
    Task<List<string>> GetWoCutoffs(string weekNumber, string plant);
}
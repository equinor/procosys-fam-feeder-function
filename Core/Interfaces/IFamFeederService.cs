using Core.Models;
using Microsoft.Extensions.Logging;

namespace Core.Interfaces;

public interface IFamFeederService
{
    Task<string> RunFeeder(QueryParameters queryParams, ILogger logger);
    Task<List<string>> GetAllPlants();
    Task<string> WoCutoff(string plant, string month, ILogger logger);
    Task<string> RunForCutoffWeek(string cutoffWeek, string plant, ILogger logger);
    Task<string> RunForCutoffWeek(string cutoffWeek, IEnumerable<long> projectIds, string plant, ILogger logger);
}
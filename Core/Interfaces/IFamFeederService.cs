using Core.Models;
using Microsoft.Extensions.Logging;

namespace Core.Interfaces;

public interface IFamFeederService
{
    Task<string> RunFeeder(QueryParameters queryParams, ILogger logger);
    Task<List<string>> GetAllPlants();
}
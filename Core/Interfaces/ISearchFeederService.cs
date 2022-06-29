using Core.Models;
using Equinor.TI.CommonLibrary.Mapper.Core;
using Microsoft.Extensions.Logging;

namespace Core.Interfaces;

public interface ISearchFeederService
{
    Task<string> RunFeeder(QueryParameters queryParams, ILogger logger);
    Task<List<string>> GetAllPlants();
}
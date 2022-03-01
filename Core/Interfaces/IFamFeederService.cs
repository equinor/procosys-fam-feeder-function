using Core.Models;
using Equinor.TI.CommonLibrary.Mapper.Core;
using Microsoft.Extensions.Logging;

namespace Core.Interfaces;

public interface IFamFeederService
{
    Task<string> RunFeeder(QueryParameters queryParams, ILogger logger);
    Task<List<string>> GetAllPlants();
    Task<string> WoCutoff(string plant, string month, ILogger logger);
}
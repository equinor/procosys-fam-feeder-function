using Core.Models;
using Equinor.TI.CommonLibrary.Mapper.Core;
using Microsoft.Extensions.Logging;

namespace Core.Interfaces;

public interface IDbStatusFeederService
{
    Task<string> RunFeeder(ILogger logger);
}
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Core.Interfaces;
using Core.Models;
using Core.Services;
using Equinor.ProCoSys.PcsServiceBus;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using static System.Enum;
namespace DbStatusFeederFunction;

public class DbStatusFeederFunction
{
    private readonly IDbStatusFeederService _dbStatusFeederService;

    public DbStatusFeederFunction(IDbStatusFeederService dbStatusFeederService)
    {
        _dbStatusFeederService = dbStatusFeederService;
    }

    [FunctionName("DbStatusFeederFunction")]
    public void Run([TimerTrigger("0 */5 * * * *")] TimerInfo myTimer, ILogger log)
    {
        _dbStatusFeederService.RunFeeder(log);
        
    }
}
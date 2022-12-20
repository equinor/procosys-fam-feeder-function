using Core.Interfaces;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace DbStatusFeederFunction;

public class DbStatusFeederFunction
{
    private readonly IDbStatusFeederService _dbStatusFeederService;

    public DbStatusFeederFunction(IDbStatusFeederService dbStatusFeederService)
    {
        _dbStatusFeederService = dbStatusFeederService;
    }

    [FunctionName("DbStatusFeederFunction")]
    public void Run([TimerTrigger("0 */5 * * * *")] TimerInfo myTimer, ILogger log) // cron expression for every 5 minutes
    {
        _dbStatusFeederService.RunFeeder(log);
    }
}
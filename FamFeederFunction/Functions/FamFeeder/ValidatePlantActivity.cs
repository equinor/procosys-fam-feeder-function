using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Core.Interfaces;

namespace FamFeederFunction.Functions.FamFeeder;

public  class ValidatePlantActivity
{
    private readonly IFamFeederService _famFeederService;

    public ValidatePlantActivity(IFamFeederService famFeederService)
    {
        _famFeederService = famFeederService;
    }

    [FunctionName(nameof(ValidatePlantActivity))]
    public async Task<List<string>> RunFeeder([ActivityTrigger] IDurableActivityContext context, ILogger logger)
    {
        var plants = await _famFeederService.GetAllPlants();
        return plants;
    }
}
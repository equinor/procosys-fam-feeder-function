using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Core.Interfaces;

namespace FamFeederFunction.Functions.FamFeeder;

public  class ValidatePlantActivity
{
    private readonly IFamFeederService _famFeederService;

    public ValidatePlantActivity(IFamFeederService famFeederService) 
        => _famFeederService = famFeederService;

    [FunctionName(nameof(ValidatePlantActivity))]
    public async Task<List<string>> RunFeeder([ActivityTrigger] IDurableActivityContext context)
        => await _famFeederService.GetAllPlants();
}
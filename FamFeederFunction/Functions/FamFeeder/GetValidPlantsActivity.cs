using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Core.Interfaces;

namespace FamFeederFunction.Functions.FamFeeder;

public  class GetValidPlantsActivity
{
    private readonly IFamFeederService _famFeederService;

    public GetValidPlantsActivity(IFamFeederService famFeederService) 
        => _famFeederService = famFeederService;

    [FunctionName(nameof(GetValidPlantsActivity))]
    public async Task<List<string>> RunFeeder([ActivityTrigger] IDurableActivityContext context)
        => await _famFeederService.GetAllPlants();
}
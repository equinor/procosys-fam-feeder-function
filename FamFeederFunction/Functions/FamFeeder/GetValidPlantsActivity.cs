using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Core.Interfaces;

namespace FamFeederFunction.Functions.FamFeeder;

public  class GetValidPlantsActivity
{
    private readonly IFeederService _feederService;

    public GetValidPlantsActivity(IFeederService feederService) 
        => _feederService = feederService;

    [FunctionName(nameof(GetValidPlantsActivity))]
    public async Task<List<string>> RunFeeder([ActivityTrigger] IDurableActivityContext context)
        => await _feederService.GetAllPlants();
}
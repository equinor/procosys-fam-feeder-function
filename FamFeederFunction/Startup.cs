using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Core;
using Core.Interfaces;
using Core.Services;
using Fam.Core.EventHubs.Extensions;
using Infrastructure;
using Infrastructure.Repositories;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MoreLinq.Extensions;

[assembly: FunctionsStartup(typeof(FamFeederFunction.Startup))]

namespace FamFeederFunction;

public class Startup : FunctionsStartup
{
    public override void Configure(IFunctionsHostBuilder builder)
    {
        var services = builder.Services;
   
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
            .AddUserSecrets(Assembly.GetExecutingAssembly(),true)
            .AddEnvironmentVariables()
            .Build();


      
        
  
        services.AddOptions<CommonLibConfig>().Configure<IConfiguration>((settings, configuration) =>
        {
            configuration.GetSection("CommonLibConfig");
        });
      
        services.Configure<CommonLibConfig>(config.GetSection("CommonLibConfig"));
        services.Configure<FamFeederOptions>(config.GetSection("FamFeederOptions"));

        services.AddEventHubProducer(configBuilder
            => config.Bind("EventHubProducerConfig", configBuilder));

        services.AddDbContext(config.GetSection("FamFeederOptions")["ProCoSysConnectionString"]);
        services.AddScoped<IFamEventRepository, FamEventRepository>();
        services.AddScoped<IFamFeederService, FamFeederService>();
    }

   
}
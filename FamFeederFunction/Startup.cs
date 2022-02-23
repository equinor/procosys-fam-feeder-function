using System.IO;
using System.Reflection;
using Core;
using Core.Interfaces;
using Core.Services;
using Fam.Core.EventHubs.Extensions;
using FamFeederFunction;
using Infrastructure;
using Infrastructure.Repositories;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(Startup))]
namespace FamFeederFunction;

public class Startup : FunctionsStartup
{
    public override void Configure(IFunctionsHostBuilder builder)
    {
        var services = builder.Services;

        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("local.settings.json", true, true)
            .AddUserSecrets(Assembly.GetExecutingAssembly(), true)
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

        AddWalletToDirectory(config);

        services.AddDbContext(config.GetSection("FamFeederOptions")["ProCoSysConnectionString"]);
        services.AddScoped<IFamEventRepository, FamEventRepository>();
        services.AddScoped<IFamFeederService, FamFeederService>();
        services.AddScoped<IPlantRepository, PlantRepository>();
    }

    private static void AddWalletToDirectory(IConfiguration config)
    {
        var rep = new BlobRepository(config["BlobStorage:ConnectionString"], config["BlobStorage:ContainerName"]);
        var walletPath = config["WalletFileDir"];
        Directory.CreateDirectory(walletPath);
        rep.Download(config["BlobStorage:WalletFileName"], walletPath + "\\cwallet.sso");
    }
}
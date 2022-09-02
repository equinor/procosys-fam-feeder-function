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

        services.Configure<CommonLibConfig>(config.GetSection("CommonLibConfig"));
        services.Configure<FamFeederOptions>(config.GetSection("FamFeederOptions"));
        services.AddEventHubProducer(configBuilder
            => config.Bind("EventHubProducerConfig", configBuilder));

        services.AddLogging();
        AddWalletToDirectory(config);

        services.AddDbContext(config.GetSection("FamFeederOptions")["ProCoSysConnectionString"]);
        services.AddScoped<IFamEventRepository, FamEventRepository>();
        services.AddScoped<IFamFeederService, FamFeederService>();
        services.AddScoped<ISearchItemRepository, SearchItemRepository>();
        services.AddScoped<ISearchFeederService, SearchFeederService>();
        services.AddScoped<IPlantRepository, PlantRepository>();
    }

    private static void AddWalletToDirectory(IConfiguration config)
    {
        if (config["BlobStorage:ConnectionString"] == null)
        {
            return;
        }

        var rep = new BlobRepository(config["BlobStorage:ConnectionString"], config["BlobStorage:ContainerName"]);
        const string walletPath = "/home/site/wwwroot/wallet";
        Directory.CreateDirectory(walletPath);
        rep.Download(config["BlobStorage:WalletFileName"], walletPath + "/cwallet.sso");
    }
}
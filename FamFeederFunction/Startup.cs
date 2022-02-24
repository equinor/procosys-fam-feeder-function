using System;
using System.Configuration;
using System.IO;
using System.Reflection;
using Azure.Storage.Blobs;
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
using Microsoft.Extensions.Logging;

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

        var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole();  
        });
        ILogger logger = loggerFactory.CreateLogger<Startup>();

        GetWalletFileFromBlobStorage(builder.GetContext().ApplicationRootPath,logger);

        //AddWalletToDirectory(config);

        services.AddDbContext(config.GetSection("FamFeederOptions")["ProCoSysConnectionString"]);
        services.AddScoped<IFamEventRepository, FamEventRepository>();
        services.AddScoped<IFamFeederService, FamFeederService>();
        services.AddScoped<IPlantRepository, PlantRepository>();
    }

    private static void AddWalletToDirectory(IConfiguration config)
    {
        var rep = new BlobRepository(config["BlobStorage:ConnectionString"], config["BlobStorage:ContainerName"]);
        var walletPath = "/Users/test/wallet";// config["WalletFileDir"];
        Directory.CreateDirectory(walletPath);
        Console.WriteLine("Created wallet file at: " + walletPath);
        rep.Download(config["BlobStorage:WalletFileName"], walletPath + "/cwallet.sso");
    }

    private static async void GetWalletFileFromBlobStorage(string rootPath, ILogger log)
    {
        var connectionString = ConfigurationManager.AppSettings["BlobStorage:ConnectionString"];
        var blobContainerName = ConfigurationManager.AppSettings["BlobStorage:ContainerName"];
        var blobName = ConfigurationManager.AppSettings["BlobStorage:WalletFileName"];
        if (string.IsNullOrWhiteSpace(connectionString) 
            || string.IsNullOrWhiteSpace(blobContainerName) ||
            string.IsNullOrWhiteSpace(blobName)) return;

        var fileName = rootPath + @"/cwallet.sso";
        log.LogInformation("Created wallet file at: " + fileName);
        var blobClient = new BlobClient(connectionString, blobContainerName, blobName);
        var response = await blobClient.DownloadAsync();
        await using var outputFileStream = new FileStream(fileName, FileMode.Create);
        await response.Value.Content.CopyToAsync(outputFileStream);
    }
}
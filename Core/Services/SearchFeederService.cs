using Core.Interfaces;
using Core.Models;
using Core.Models.Search;
using Azure;
using Equinor.ProCoSys.PcsServiceBus;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MoreLinq;
using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using Task = System.Threading.Tasks.Task;

namespace Core.Services;

public class SearchFeederService : ISearchFeederService
{
    private readonly FamFeederOptions _famFeederOptions;
    private ILogger? _logger;
    private readonly IPlantRepository _plantRepository;
    private readonly ISearchItemRepository _searchItemRepo;

    public SearchFeederService( ISearchItemRepository searchItemRepo,
        IOptions<FamFeederOptions> famFeederOptions, IPlantRepository plantRepository)
    {
        _searchItemRepo = searchItemRepo;
        _plantRepository = plantRepository;
        _famFeederOptions = famFeederOptions.Value;
    }

    public async Task<string> RunFeeder(QueryParameters queryParameters, ILogger logger)
    {
        _logger = logger;
        var batchSize = int.Parse(_famFeederOptions.SearchIndexBatchSize ?? "20");
        var items = await GetItemsBasedOnTopicAndPlant(queryParameters);

        if (items.Count == 0)
        {
            _logger.LogInformation("found no items");
            return "found no items";
        }

        _logger.LogInformation(
            "Found {events} events for topic {topic} and plant {plant}",items.Count,queryParameters.PcsTopics,queryParameters.Plants);

        foreach (var batch in items.Batch(batchSize))
        {
            var batchList = batch.ToList();
            _logger.LogInformation($"Sending {batchList.Count} items to Search Index {queryParameters.Plants} {queryParameters.PcsTopics}");
            await SendIndexDocuments(batchList);
        }

        _logger.LogInformation("Finished adding {topic} to Index",queryParameters.PcsTopics);

        return $"finished successfully sending {items.Count} documents to Search Index {queryParameters.Plants} {queryParameters.PcsTopics}";
    }

    public Task<List<string>> GetAllPlants() => _plantRepository.GetAllPlants();

    private async Task SendIndexDocuments(IEnumerable<IndexDocument> docs)
    {
        var endPoint = _famFeederOptions.SearchIndexEndpoint;
        var indexName = _famFeederOptions.SearchIndexName;
        var accessKey = _famFeederOptions.SearchIndexAccessKey;
        var batchDelay = int.Parse(_famFeederOptions.SearchIndexBatchDelay ?? "1000");


        if (indexName == null || endPoint == null || accessKey == null)
        {
            _logger?.LogError($"Missing required env value (required: SearchIndexEndpoint, SearchIndexName and SearchIndexAccessKey");
            return;
        }

        try
        {
            var endPointUri = new Uri($"https://{endPoint}.search.windows.net/");
            var credential = new AzureKeyCredential(accessKey);
            var client = new SearchClient(endPointUri, indexName, credential);
            // Add batch to index
            var batch = IndexDocumentsBatch.MergeOrUpload(docs);
            var options1 = new IndexDocumentsOptions { ThrowOnAnyError = true };
            var resp = await client.IndexDocumentsAsync(batch, options1);
            
            Console.WriteLine($"Index Document batch finished. Results: {resp.Value.Results.Count}");
            Thread.Sleep(batchDelay);

        }
        catch (Exception e)
        {
            throw new Exception("Error: Could not send message.", e);
        }
    }
    private async Task<List<IndexDocument>> GetItemsBasedOnTopicAndPlant(QueryParameters queryParameters)
    {
        var events = new List<IndexDocument>();
        // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
        foreach (var plant in queryParameters.Plants)
        {
            foreach (var topic in queryParameters.PcsTopics)
            {
                switch (topic)
                {
                    case PcsTopicConstants.CommPkg:
                        events.AddRange(await _searchItemRepo.GetCommPackages(plant));
                        break;
                    case PcsTopicConstants.McPkg:
                        events.AddRange(await _searchItemRepo.GetMcPackages(plant));
                        break;
                    case PcsTopicConstants.Tag:
                        events.AddRange(await _searchItemRepo.GetTags(plant));
                        break;
                    case PcsTopicConstants.PunchListItem:
                        events.AddRange(await _searchItemRepo.GetPunchItems(plant));
                        break;
                    default:
                    {
                        _logger?.LogInformation("{Topic} not included in switch statement",topic);
                        break;
                    }
                }
            }
        }
        return events;
    }
}
using Core.Interfaces;
using Core.Models;
using Core.Models.Search;
using Azure;
using Equinor.ProCoSys.FamWebJob.Core.Mappers;
using Equinor.ProCoSys.PcsServiceBus;
using Equinor.TI.Common.Messaging;
using Equinor.TI.CommonLibrary.Mapper;
using Equinor.TI.CommonLibrary.Mapper.Core;
using Fam.Core.EventHubs.Contracts;
using Fam.Models.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MoreLinq;
using Azure.Search.Documents;
using Azure.Search.Documents.Models;

namespace Core.Services;

public class SearchFeederService : ISearchFeederService
{
    private readonly CommonLibConfig _commonLibConfig;
    private readonly IEventHubProducerService _eventHubProducerService;
    private readonly FamFeederOptions _famFeederOptions;
    private ILogger? _logger;
    private readonly IPlantRepository _plantRepository;
    private readonly ISearchItemRepository _searchItemRepo;

    public SearchFeederService(IEventHubProducerService eventHubProducerService, ISearchItemRepository searchItemRepo,
        IOptions<CommonLibConfig> commonLibConfig, IOptions<FamFeederOptions> famFeederOptions,
        IPlantRepository plantRepository)
    {
        _eventHubProducerService = eventHubProducerService;
        _searchItemRepo = searchItemRepo;
        _plantRepository = plantRepository;
        _commonLibConfig = commonLibConfig.Value;
        _famFeederOptions = famFeederOptions.Value;
    }

    public async Task<string> RunFeeder(QueryParameters queryParameters, ILogger logger)
    {
        _logger = logger;
        
        var items = await GetItemsBasedOnTopicAndPlant(queryParameters);

        if (items.Count == 0)
        {
            _logger.LogInformation("found no items");
            return "found no items";
        }

        _logger.LogInformation(
            "Found {events} events for topic {topic} and plant {plant}",items.Count,queryParameters.PcsTopic,queryParameters.Plant);

        foreach (var batch in items.Batch(20))
        {
            _logger.LogInformation($"Sending {batch.Count()} items to Search Index");
            await SendIndexDocuments(batch);
        }

        _logger.LogInformation("Finished adding {topic} to Index",queryParameters.PcsTopic);

        return $"finished successfully sending {items.Count} documents to Search Index {queryParameters.PcsTopic}";
    }

    public Task<List<string>> GetAllPlants() => _plantRepository.GetAllPlants();

    private async Task SendIndexDocuments(IEnumerable<IndexDocument> docs)
    {
        var endPoint = _famFeederOptions.SearchIndexEndpoint;
        var indexName = _famFeederOptions.SearchIndexName;
        var accessKey = _famFeederOptions.SearchIndexAccessKey;

        if (indexName == null || endPoint == null || accessKey == null)
        {
            _logger.LogError($"Missing required env value (required: SearchIndexEndpoint, SearchIndexName and SearchIndexAccessKey");
            return;
        }

        try
        {
            var endPointUri = new Uri($"https://{endPoint}.search.windows.net/");
            var credential = new AzureKeyCredential(accessKey);
            var client = new SearchClient(endPointUri, indexName, credential);
            // Add batch to index
            IndexDocumentsBatch<IndexDocument> batch = IndexDocumentsBatch.MergeOrUpload(docs);
            IndexDocumentsOptions options1 = new IndexDocumentsOptions { ThrowOnAnyError = true };
            var resp = await client.IndexDocumentsAsync(batch, options1);
            
            Console.WriteLine($"Index Document batch finished. Results: {resp.Value.Results.Count}");
            Thread.Sleep(1000);

        }
        catch (Exception e)
        {
            throw new Exception("Error: Could not send message.", e);
        }
    }
    private async Task<List<IndexDocument>> GetItemsBasedOnTopicAndPlant(QueryParameters queryParameters)
    {
        var events = new List<IndexDocument>();
        switch (queryParameters.PcsTopic)
        {
            case PcsTopic.CommPkg:
                events = await _searchItemRepo.GetCommPackages(queryParameters.Plant);
                break;
            case PcsTopic.McPkg:
                events = await _searchItemRepo.GetMcPackages(queryParameters.Plant);
                break;
            case PcsTopic.Tag:
                events = await _searchItemRepo.GetTags(queryParameters.Plant);
                break;
            case PcsTopic.PunchListItem:
                events = await _searchItemRepo.GetPunchItems(queryParameters.Plant);
                break;
            default:
            {
                _logger.LogInformation("{topic} not included in switch statement",queryParameters.PcsTopic);
                break;
            }
        }
        return events;
    }
}
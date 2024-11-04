using Equinor.TI.CommonLibrary.Mapper;
using Equinor.TI.CommonLibrary.Mapper.Core;
using Equinor.TI.CommonLibrary.SchemaModel;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Core.Services;
/// <summary>
/// Takes IDistributedCache and ISchemaCacheSource as input and returns a SchemaDTO
/// Do not copy this class for other purposes unless you know that the use case is the same.
/// When used in FAMFeeder we keep the scope of the class just for the lifetime of the function.
/// We check cache once, then store the result in memory and use that in all subsequent uses of the class.
/// That means that we should call the cache once per function call.
/// This will not work if the class is long-lived as the schema could get outdated.
/// </summary>
public class DistributedCacheSource : ISchemaSource
{
    private SchemaDTO? _schemaDto;
    private readonly IDistributedCache _distributedCache;
    private readonly ISchemaCacheSource _schemaSource;
    private readonly ILogger? _logger;
    private readonly TimeSpan _maxCacheAge = TimeSpan.FromDays(1);

    public DistributedCacheSource(
        IDistributedCache distributedCache,
        ISchemaCacheSource schemaSource,
        ILogger? logger)
    {
        _distributedCache = distributedCache;
        _schemaSource = schemaSource;
        _logger = logger;
    }

    public SchemaDTO Get(string schemaFrom, string schemaTo)
    {
        if(_schemaDto != null)
        {
            return _schemaDto;
        }
        const string key = "CommonLib--FamFeederFunction";
        if (TryGetCacheItemFromCache(key, out var item))
        {
            _schemaDto = item;
            return item!; 
        }

        var schemaDto = GetAndCache(schemaFrom, schemaTo, key);
        _schemaDto = schemaDto;
        return schemaDto;
    }

    private SchemaDTO GetAndCache(string schemaFrom, string schemaTo, string key)
    {
        try
        {
            var schema = _schemaSource.Get(schemaFrom, schemaTo);
            //Using System.Text.Json to Deserialize did not work.
            //Got an error due to a dictionary with key,value <object,object> in the SchemaDto class.
            //However, it seems to work with newtonsoft.json, so not spending more time on it.
            _distributedCache.SetString(key, JsonConvert.SerializeObject(schema), new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = _maxCacheAge
            });

            return schema;
        }
        catch (Exception e)
        {
            _logger?.LogError(e,"Was not able to fetch commonLib schema from source. {From}, {Too}",schemaFrom,schemaTo);
            throw;
        }
    }

    private bool TryGetCacheItemFromCache(string key, out SchemaDTO? schema)
    {
        schema = new SchemaDTO();
        try
        {
            var cacheString = _distributedCache.GetString(key);
            if (string.IsNullOrWhiteSpace(cacheString))
            {
                return false;
            }
            schema = JsonConvert.DeserializeObject<SchemaDTO>(cacheString);
            if(schema == null)
            {
                _logger?.LogWarning("Failed to deserialize schema from cache. Key: {Key}", key);
                return false;
            }
            return true;
        }
        catch (Exception e)
        {
            _logger?.LogWarning(e,"Failed to get schema from cache. Key: {Key}", key);
            return false;
        }
        
   
    }
}
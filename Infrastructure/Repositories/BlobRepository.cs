using Azure.Storage.Blobs;

namespace Infrastructure.Repositories;

public class BlobRepository
{
    private readonly BlobContainerClient _client;

    public BlobRepository(string connectionString, string containerName)
    {
        _client = new BlobContainerClient(connectionString, containerName);
    }

    public async void Download(string pathAndFileName, string downloadPath)
    {
        var blobClient = _client.GetBlobClient(pathAndFileName);
        if (await blobClient.ExistsAsync())
        {
            await blobClient.DownloadToAsync(downloadPath);
        }
    }
}
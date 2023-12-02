using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;

namespace AzureTaskManagement.Database.Services;

public interface IBlobStorageService
{
    Task<string> UploadBlob(IFormFile file, string fileName);
    Task<string> GetBlobUrl(string imageName);
    Task RemoveBlob(string imageName);
}

public class BlobStorageService : IBlobStorageService
{
    private readonly string _connectionString;
    private readonly string _containerName;

    public BlobStorageService(IConfiguration configuration)
    {
        _connectionString = configuration["StorageAccount:ConnectionString"]!;
        _containerName = configuration["StorageAccount:ContainerName"]!;
    }

    public async Task<string> UploadBlob(IFormFile file, string fileName)
    {
        var blobName = $"{fileName}{Path.GetExtension(file.FileName)}";
        var client = await GetBlobContainerClient();
        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream);
        memoryStream.Position = 0;
        var blobContentInfo = await client.UploadBlobAsync(blobName, memoryStream);
        return blobName;
    }

    public async Task<string> GetBlobUrl(string imageName)
    {
        var client = await GetBlobContainerClient();
        var blobClient = client.GetBlobClient(imageName);

        BlobSasBuilder blobSasBuilder = new()
        {
            BlobContainerName = blobClient.BlobContainerName,
            BlobName = blobClient.Name,
            ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(2),
            Protocol = SasProtocol.Https,
            Resource = "b"
        };
        blobSasBuilder.SetPermissions(BlobSasPermissions.Read);

        return blobClient.GenerateSasUri(blobSasBuilder).ToString();
    }

    public async Task RemoveBlob(string imageName)
    {
        var client = await GetBlobContainerClient();
        var blob = client.GetBlobClient(imageName);
        await blob.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots);
    }

    private async Task<BlobContainerClient> GetBlobContainerClient()
    {
        BlobContainerClient container = new BlobContainerClient(_connectionString, _containerName);
        await container.CreateIfNotExistsAsync();
        return container;
    }
}
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace brainbeats_backend {
  public class StorageConnection {
    public static StorageConnection Instance { get; set; }

    private BlobServiceClient blobServiceClient { get; set; }

    public static void Init(IConfiguration configuration) {
      Instance = new StorageConnection(configuration);
    }

    private StorageConnection(IConfiguration configuration) {
      blobServiceClient = new BlobServiceClient(configuration["Storage:ConnectionString"]);
    }

    public async Task<string> UploadSample(IFormFile file) {
      return await UploadFileAsync(file, "samples");
    }

    public async Task<string> UploadBeat(IFormFile file) {
      return await UploadFileAsync(file, "beats");
    }

    private async Task<string> UploadFileAsync(IFormFile file, string containerName) {
      BlobContainerClient containerClient;

      try {
        containerClient = await blobServiceClient.CreateBlobContainerAsync(containerName);
      } catch {
        containerClient = blobServiceClient.GetBlobContainerClient(containerName);
      }

      BlobClient blobClient = containerClient.GetBlobClient(file.FileName);

      try {
        using (var stream = file.OpenReadStream()) {
          await blobClient.UploadAsync(stream, true);
        }

        return blobClient.Uri.AbsoluteUri;
      } catch {
        return null;
      }
    }
  }
}

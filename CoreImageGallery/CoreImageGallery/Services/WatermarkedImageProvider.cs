using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ImageGallery.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace CoreImageGallery.Services
{
    public class WatermarkedImageProvider : IImageProvider
    {
        private readonly CloudBlobContainer _publicContainer;

        public WatermarkedImageProvider(IConfiguration config)
        {
            var connectionString = config["AzureStorageConnection"];
            var account = CloudStorageAccount.Parse(connectionString);
            var client = account.CreateCloudBlobClient();
            _publicContainer = client.GetContainerReference(Config.WatermarkedContainer);
        }

        public async Task<byte[]> GetImageAsync(string name)
        {
            var blob = _publicContainer.GetBlockBlobReference(name);
            using (var stream = new MemoryStream())
            {
                await blob.DownloadToStreamAsync(stream);
                return stream.ToArray();
            }
        }

    }
}

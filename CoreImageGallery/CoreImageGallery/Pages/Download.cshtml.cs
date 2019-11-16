using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using CoreImageGallery.Services;
using ImageGallery.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CoreImageGallery.Pages
{
    public class DownloadModel : PageModel
    {
        private readonly IStorageService _storageService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly SemaphoreSlim _zipEntrySemaphore = new SemaphoreSlim(1);
        private readonly SemaphoreSlim _httpThreadSemaphore = new SemaphoreSlim(5);

        public DownloadModel(IStorageService storageService,
            IHttpClientFactory httpClientFactory)
        {
            _storageService = storageService ?? throw new System.ArgumentNullException(nameof(storageService));
            _httpClientFactory = httpClientFactory ?? throw new System.ArgumentNullException(nameof(httpClientFactory));
        }

        public async Task<IActionResult> OnGetAsync()
        {
            IEnumerable<UploadedImage> images = await _storageService.GetImagesAsync().ConfigureAwait(false);

            List<Task> streamTasks = new List<Task>();

            using (var memoryStream = new MemoryStream())
            {
                using (ZipArchive zipArchive = new ZipArchive(memoryStream, ZipArchiveMode.Create))
                {
                    foreach (UploadedImage image in images)
                    {
                        streamTasks.Add(ZipImage(image, zipArchive));
                    }

                    await Task.WhenAll(streamTasks).ConfigureAwait(false);
                }

                return File(memoryStream.ToArray(), MediaTypeNames.Application.Zip, Path.ChangeExtension(Guid.NewGuid().ToString(), ".zip"));
            }
        }

        private async Task ZipImage(UploadedImage image, ZipArchive targetArchive)
        {
            try
            {
                await _httpThreadSemaphore.WaitAsync().ConfigureAwait(false);
                HttpClient httpClient = _httpClientFactory.CreateClient();
                httpClient.BaseAddress = new Uri($"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}{HttpContext.Request.PathBase}");

                Stream streamReader = await httpClient.GetStreamAsync(image.ImagePath).ConfigureAwait(false);
                try
                {
                    await _zipEntrySemaphore.WaitAsync().ConfigureAwait(false);
                    var zipArchiveEntry = targetArchive.CreateEntry(image.FileName, CompressionLevel.NoCompression);
                    using (Stream zipArchiveEntryStream = zipArchiveEntry.Open())
                    {
                        await streamReader.CopyToAsync(zipArchiveEntryStream).ConfigureAwait(false);
                        await streamReader.FlushAsync().ConfigureAwait(false);
                    }
                }
                finally
                {
                    _zipEntrySemaphore.Release();
                    httpClient.Dispose();
                }
            }
            finally
            {
                _httpThreadSemaphore.Release();
            }
        }
    }
}
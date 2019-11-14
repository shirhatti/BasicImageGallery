using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Net.Mime;
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

        public DownloadModel(IStorageService storageService,
            IHttpClientFactory httpClientFactory)
        {
            _storageService = storageService ?? throw new System.ArgumentNullException(nameof(storageService));
            _httpClientFactory = httpClientFactory ?? throw new System.ArgumentNullException(nameof(httpClientFactory));
        }

        public async Task<IActionResult> OnGetAsync()
        {
            IEnumerable<UploadedImage> images = await _storageService.GetImagesAsync();

            HttpClient httpClient = _httpClientFactory.CreateClient();
            httpClient.BaseAddress = new Uri($"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}{HttpContext.Request.PathBase}");

            using (var memoryStream = new MemoryStream())
            {
                using (ZipArchive zipArchive = new ZipArchive(memoryStream, ZipArchiveMode.Create))
                {
                    // Potential perf improvement: parallel the streaming.
                    foreach (UploadedImage image in images)
                    {
                        using (Stream streamReader = await httpClient.GetStreamAsync(image.ImagePath))
                        {
                            var zipArchiveEntry = zipArchive.CreateEntry(image.FileName, CompressionLevel.NoCompression);
                            using (Stream zipArchiveEntryStream = zipArchiveEntry.Open())
                            {
                                await streamReader.CopyToAsync(zipArchiveEntryStream);
                                await streamReader.FlushAsync();
                            }
                        }
                    }
                }

                return File(memoryStream.ToArray(), MediaTypeNames.Application.Zip, Path.ChangeExtension(Guid.NewGuid().ToString(), ".zip"));
            }
        }
    }
}
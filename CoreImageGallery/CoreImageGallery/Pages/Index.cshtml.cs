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
    public class IndexModel : PageModel
    {
        public IEnumerable<UploadedImage> Images { get; private set; }
        private readonly IStorageService _storageService;
        private readonly IHttpClientFactory _httpClientFactory;

        public IndexModel(
            IStorageService storageService,
            IHttpClientFactory httpClientFactory,
            IStreamValidationService streamValidationService)
        {
            _storageService = storageService ?? throw new ArgumentNullException(nameof(storageService));
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        }

        public async Task OnGetAsync()
        {
            var images = await _storageService.GetImagesAsync();
            this.Images = images;
        }

        public async Task<IActionResult> OnPostDownloadAsync()
        {
            var images = await _storageService.GetImagesAsync();

            HttpClient httpClient = _httpClientFactory.CreateClient();
            httpClient.BaseAddress = new Uri($"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}{HttpContext.Request.PathBase}");

            using (var memoryStream = new MemoryStream())
            {
                using (ZipArchive zipArchive = new ZipArchive(memoryStream, ZipArchiveMode.Create))
                {
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

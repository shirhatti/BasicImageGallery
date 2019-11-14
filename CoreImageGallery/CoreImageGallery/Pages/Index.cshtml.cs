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
            IHttpClientFactory httpClientFactory)
        {
            _storageService = storageService ?? throw new ArgumentNullException(nameof(storageService));
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        }

        public async Task OnGetAsync()
        {
            var images = await _storageService.GetImagesAsync();
            this.Images = images;
        }
    }
}

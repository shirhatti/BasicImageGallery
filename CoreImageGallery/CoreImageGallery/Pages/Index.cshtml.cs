using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using CoreImageGallery.Services;
using ImageGallery.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CoreImageGallery.Pages
{
    public class IndexModel : PageModel
    {
        public IEnumerable<UploadedImage> Images { get; private set; }

        private readonly IStorageService _storageService;

        public IndexModel(IStorageService storageService)
        {
            _storageService = storageService;
        }

        public async Task OnGetAsync()
        {
            var images = await _storageService.GetImagesAsync();

            List<UploadedImage> results = new List<UploadedImage>();
            using (HttpClient client = new HttpClient())
            {
                foreach (var image in images)
                {
                    HttpResponseMessage response = await client.GetAsync(image.ImagePath);
                    if (response.IsSuccessStatusCode)
                    {
                        results.Add(image);
                    }
                }
            }

            this.Images = results;
        }
    }
}

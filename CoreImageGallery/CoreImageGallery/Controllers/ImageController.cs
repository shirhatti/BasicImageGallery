using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoreImageGallery.Services;
using Microsoft.AspNetCore.Mvc;

namespace CoreImageGallery.Controllers
{
    public class ImageController : Controller
    {
        private IImageProvider _imageProvider;

        public ImageController(IImageProvider imageProvider)
        {
            _imageProvider = imageProvider;
        }

        [Route("image/{id}")]
        [HttpGet]
        public async Task<IActionResult> Index(string id)
        {
            byte[] imageBytes = await _imageProvider.GetImageAsync(id);
            return new FileContentResult(imageBytes, "image/jpg");
        }
    }
}

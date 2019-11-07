using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreImageGallery.Services
{
    public interface IImageProvider
    {
        Task<byte[]> GetImageAsync(string name);
    }
}

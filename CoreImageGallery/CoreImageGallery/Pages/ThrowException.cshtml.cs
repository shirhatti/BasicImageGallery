using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CoreImageGallery.Pages
{
    public class ThrowExceptionModel : PageModel
    {
        public void OnGet()
        {
            throw new ApplicationException();
        }
    }
}

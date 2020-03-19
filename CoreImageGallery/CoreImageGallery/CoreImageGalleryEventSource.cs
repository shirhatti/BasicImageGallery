using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CoreImageGallery
{

    public class CoreImageGalleryEventSource : EventSource
    {
        public static readonly CoreImageGalleryEventSource Log = new CoreImageGalleryEventSource();

        private PollingCounter _totalImagesDownloaded;

        private long _totalImages;

        internal CoreImageGalleryEventSource()
            : base("CoreImageGallery")
        {

        }

        internal void ImagesDownloaded(int numberOfImages)
        {
            Interlocked.Add(ref _totalImages, numberOfImages);
        }

        protected override void OnEventCommand(EventCommandEventArgs command)
        {
            if (command.Command == EventCommand.Enable)
            {
                _totalImagesDownloaded ??= new PollingCounter("images-downloaded", this, () => _totalImages)
                {
                    DisplayName = "Total Images Downloaded"
                };
            }
        }
    }
}

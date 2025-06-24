using System.Collections.Generic;

namespace GoBangladesh.Application.ViewModels
{
    public class MediaDataWithIdVm
    {
        public List<ImageData> ImageData { get; set; }
        public List<VideoData> VideoData { get; set; }
    }

    public class ImageData
    {
        public string Id { get; set; }
        public string ImageUrl { get; set; }
    }

    public class VideoData
    {
        public string Id { get; set; }
        public string VideoUrl { get; set; }
    }
}

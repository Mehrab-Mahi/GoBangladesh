using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace GoBangladesh.Application.ViewModels
{
    public class MediaVm
    {
        public string ModelId { get; set; }
        public string ModelName { get; set; }
        public List<IFormFile> Images { get; set; }
        public List<string> ImageUrls { get; set; }
        public List<string> VideoUrls { get; set; }
    }
}

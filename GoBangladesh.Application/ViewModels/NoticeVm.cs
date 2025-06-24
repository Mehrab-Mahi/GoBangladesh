using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace GoBangladesh.Application.ViewModels
{
    public class NoticeVm
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<string> FileUrls { get; set; }
        public List<IFormFile> Files { get; set; }
        public DateTime PublishDate { get; set; }
    }
}

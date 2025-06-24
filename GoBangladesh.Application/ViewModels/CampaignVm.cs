using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace GoBangladesh.Application.ViewModels
{
    public class CampaignVm
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Address { get; set; }
        public string Institute { get; set; }
        public string BannerUrl { get; set; }
        public IFormFile Banner { get; set; }
        public List<string> VolunteerList { get; set; }
    }
}

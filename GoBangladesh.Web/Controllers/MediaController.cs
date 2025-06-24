using GoBangladesh.Application.Helper;
using GoBangladesh.Application.Interfaces;
using GoBangladesh.Application.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoBangladesh.Web.Controllers
{
    [Route("api/media")]
    public class MediaController : Controller
    {
        private readonly IMediaService _mediaService;

        public MediaController(IMediaService mediaService)
        {
            _mediaService = mediaService;
        }

        [GoBangladeshAuth]
        [HttpPost("uploadcampaignmedia")]
        public IActionResult UploadCampaignMedia([FromForm] MediaVm mediaData)
        {
            var response = _mediaService.UploadCampaignMedia(mediaData);
            return Ok(new {data = response});
        }

        [AllowAnonymous]
        [HttpPost("getallmedia")]
        public IActionResult GetAllMedia([FromBody] MediaDataSizeVm mediaDataSize)
        {
            var response = _mediaService.GetAllMedia(mediaDataSize);
            return Ok(new {data = response});
        }

        [GoBangladeshAuth]
        [HttpDelete("deletecampaignmedia")]
        public IActionResult DeleteCampaignMedia([FromBody] MediaDeleteVm mediaDeleteData)
        {
            var response = _mediaService.DeleteCampaignMedia(mediaDeleteData);
            return Ok(new {data = response});
        }

        [GoBangladeshAuth]
        [HttpGet("getcampaignmedia")]
        public IActionResult GetCampaignMedia(string campaignId)
        {
            var response = _mediaService.GetCampaignMedia(campaignId);
            return Ok(new { data = response });
        }
    }
}

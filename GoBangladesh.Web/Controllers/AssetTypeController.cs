using BloodDonation.Application.Helper;
using BloodDonation.Application.Interfaces;
using BloodDonation.Application.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace BloodDonation.Web.Controllers
{
    [BloodDonationAuth]
    [Route("assettypes")]
    public class AssetTypeController : Controller
    {
        private readonly IAssetTypeService _service;

        public AssetTypeController(IAssetTypeService service)
        {
            _service = service;
        }

        //public IActionResult Index()
        //{
        //    return View();
        //}

        [HttpGet("getall")]
        public IActionResult GetAll()
        {
            var list = _service.GetAll();
            return Ok(new { data = list });
        }

        [HttpGet("get/{id}")]
        public IActionResult GetById(string id)
        {
            var list = _service.GetById(id);
            return Ok(new { data = list });
        }

        [HttpPost("create")]
        public IActionResult Create([FromBody] AssetTypeVm model)
        {
            var data = _service.Create(model);
            return Ok(new { data });
        }

        [HttpPut("update/{id}")]
        public IActionResult Update(string id, [FromBody] AssetTypeVm model)
        {
            var data = _service.Update(id, model);
            return Ok(new { data });
        }
    }
}
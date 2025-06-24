using BloodDonation.Application.Helper;
using BloodDonation.Application.Interfaces;
using BloodDonation.Application.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace BloodDonation.Web.Controllers
{
    [BloodDonationAuth]
    [Route("roles")]
    public class RoleController : Controller
    {
        private readonly IRoleService _roleService;

        public RoleController(IRoleService roleService)
        {
            _roleService = roleService;
        }

        //public IActionResult Index()
        //{
        //    return View();
        //}

        [HttpGet("getall")]
        public IActionResult GetAll()
        {
            var list = _roleService.GetAll();
            return Ok(new { data = list });
        }

        [HttpPost("getmenutree")]
        public IActionResult GetMenuTree()
        {
            var data = _roleService.GetMenuTreeData();
            return Json(data);
        }

        [HttpPost("create")]
        public IActionResult Create([FromBody] RoleVm roleVm)
        {
            var status = _roleService.CreateRole(roleVm);
            return Json(new { isSuccess = status });
        }

        [HttpPost("update")]
        public IActionResult Update([FromBody] RoleVm roleVm)
        {
            var status = _roleService.UpdateRole(roleVm);
            return Json(new { isSuccess = status });
        }

        [HttpGet("getroletree/{id}")]
        public IActionResult GetRoleTree(string id)
        {
            var data = _roleService.GetMenuTreeData();
            var menus = _roleService.GetRoleMenuIds(id);
            var role = _roleService.GetRole(id);
            return Json(new { data = data, menuIds = menus, role = role });
        }
    }
}
using GoBangladesh.Application.DTOs.Agent;
using GoBangladesh.Application.Helper;
using GoBangladesh.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoBangladesh.Web.Controllers
{
    [Route("api/agent/")]
    public class AgentController : Controller
    {
        private readonly IAgentService _agentService;

        public AgentController(IAgentService agentService)
        {
            _agentService = agentService;
        }

        [AllowAnonymous]
        [HttpPost("registration")]
        public IActionResult AgentInsert([FromForm] AgentCreateRequest model)
        {
            var data = _agentService.AgentInsert(model);
            return Ok(new { data });
        }

        [GoBangladeshAuth]
        [HttpPut("update")]
        public IActionResult UpdateAgent([FromForm] AgentUpdateRequest model)
        {
            var data = _agentService.UpdateAgent(model);
            return Ok(new { data });
        }

        [GoBangladeshAuth]
        [HttpGet("getById")]
        public IActionResult GetAgentById(string id)
        {
            var data = _agentService.GetAgentById(id);
            return Ok(new { data });
        }
    }
}
using GoBangladesh.Application.DTOs.Agent;
using GoBangladesh.Application.Helper;
using GoBangladesh.Application.Interfaces;
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

        [GoBangladeshAuth]
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

        [GoBangladeshAuth]
        [HttpPost("getAll")]
        public IActionResult GetAll([FromBody] AgentDataFilter filter)
        {
            var data = _agentService.GetAll(filter);
            return Ok(new { data });
        }

        [GoBangladeshAuth]
        [HttpDelete("delete")]
        public IActionResult Delete(string id)
        {
            var data = _agentService.Delete(id);
            return Ok(new { data });
        }

        [GoBangladeshAuth]
        [HttpGet("getRechargeData")]
        public IActionResult GetRechargeData(string id)
        {
            var data = _agentService.GetRechargeData(id);
            return Ok(new { data });
        }
    }
}
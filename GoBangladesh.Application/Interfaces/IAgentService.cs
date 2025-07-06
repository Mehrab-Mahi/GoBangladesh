using GoBangladesh.Application.DTOs.Agent;
using GoBangladesh.Application.ViewModels;

namespace GoBangladesh.Application.Interfaces;

public interface IAgentService
{
    PayloadResponse AgentInsert(AgentCreateRequest user);
    PayloadResponse UpdateAgent(AgentUpdateRequest user);
    PayloadResponse GetAgentById(string id);
    PayloadResponse GetAll(AgentDataFilter filter);
    PayloadResponse Delete(string id);
    PayloadResponse GetRechargeData(string id);
}
using GoBangladesh.Domain.Entities;

namespace GoBangladesh.Application.Interfaces;

public interface ISettlementService
{
    void AddOrganizationWiseCardBalance(string organizationId, string cardId, int amount);
    void SettleReturn(Card card, string agentOrganizationId, decimal amount, string transactionId);
}
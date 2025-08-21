using GoBangladesh.Domain.Entities;

namespace GoBangladesh.Application.Interfaces;

public interface ISettlementTransactionService
{
    void SettleRecharge(string organizationId, string cardId, decimal amount);
    void SettleReturn(Card card, string agentOrganizationId, decimal amount, string transactionId);
    void SettleTrip(Card card, string staffOrganizationId, decimal amount, string transactionId);
}
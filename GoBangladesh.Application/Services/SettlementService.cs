using System.Linq;
using GoBangladesh.Application.Interfaces;
using GoBangladesh.Application.Util;
using GoBangladesh.Domain.Entities;
using GoBangladesh.Domain.Interfaces;

namespace GoBangladesh.Application.Services;

public class SettlementService : ISettlementService
{
    private readonly IRepository<OrganizationCardBalance> _organizationCardBalanceRepository;
    private readonly IRepository<OrganizationSettlement> _organizationSettlementRepository;

    public SettlementService(IRepository<OrganizationCardBalance> organizationCardBalanceRepository,
        IRepository<OrganizationSettlement> organizationSettlementRepository)
    {
        _organizationCardBalanceRepository = organizationCardBalanceRepository;
        _organizationSettlementRepository = organizationSettlementRepository;
    }

    public void AddOrganizationWiseCardBalance(string organizationId, string cardId, int modelAmount)
    {
        var organizationCardBalance = _organizationCardBalanceRepository
            .GetAll()
            .FirstOrDefault(o => o.OrganizationId == organizationId && o.CardId == cardId);

        if (organizationCardBalance != null)
        {
            organizationCardBalance.Balance += modelAmount;
            _organizationCardBalanceRepository.Update(organizationCardBalance);
        }
        else
        {
            _organizationCardBalanceRepository.Insert(new OrganizationCardBalance
            {
                CardId = cardId,
                OrganizationId = organizationId,
                Balance = modelAmount
            });
        }

        _organizationCardBalanceRepository.SaveChanges();
    }

    public void SettleReturn(Card card, string agentOrganizationId, decimal amount, string transactionId)
    {
        var organizationWiseBalance =
            _organizationCardBalanceRepository
                .GetAll()
                .Where(o => o.CardId == card.Id)
                .ToList();

        var remainingAmount = amount;

        var checkIfSameOrg = organizationWiseBalance
            .FirstOrDefault(o => o.OrganizationId == agentOrganizationId);

        if (checkIfSameOrg != null)
        {
            if(checkIfSameOrg.Balance >= remainingAmount)
            {
                checkIfSameOrg.Balance -= remainingAmount;
                _organizationCardBalanceRepository.Update(checkIfSameOrg);
                remainingAmount = 0;
            }
            else
            {
                remainingAmount -= checkIfSameOrg.Balance;
                checkIfSameOrg.Balance = 0;
                _organizationCardBalanceRepository.Delete(checkIfSameOrg);
            }
            _organizationCardBalanceRepository.SaveChanges();
        }

        if (remainingAmount > 0)
        {
            var otherOrganizations = organizationWiseBalance
                .Where(o => o.OrganizationId != agentOrganizationId)
                .OrderBy(o => o.Balance)
                .ToList();

            foreach (var organization in otherOrganizations)
            {
                if (organization.Balance >= remainingAmount)
                {
                    organization.Balance -= remainingAmount;
                    _organizationCardBalanceRepository.Update(organization);
                    _organizationSettlementRepository.Insert(new OrganizationSettlement
                    {
                        FromOrganizationId = organization.OrganizationId,
                        ToOrganizationId = agentOrganizationId,
                        Amount = remainingAmount,
                        TransactionId = transactionId,
                        TransactionType = TransactionType.Return
                    });
                    break;
                }

                _organizationSettlementRepository.Insert(new OrganizationSettlement
                {
                    FromOrganizationId = organization.OrganizationId,
                    ToOrganizationId = agentOrganizationId,
                    Amount = organization.Balance,
                    TransactionId = transactionId,
                    TransactionType = TransactionType.Return
                });
                remainingAmount -= organization.Balance;
                organization.Balance = 0;
                _organizationCardBalanceRepository.Delete(organization);
            }

            _organizationCardBalanceRepository.SaveChanges();
            _organizationSettlementRepository.SaveChanges();
        }
    }
}
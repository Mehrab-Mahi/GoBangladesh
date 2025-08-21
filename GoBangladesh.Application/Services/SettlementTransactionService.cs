using System.Linq;
using GoBangladesh.Application.Interfaces;
using GoBangladesh.Application.Util;
using GoBangladesh.Domain.Entities;
using GoBangladesh.Domain.Interfaces;

namespace GoBangladesh.Application.Services;

public class SettlementTransactionService : ISettlementTransactionService
{
    private readonly IRepository<OrganizationCardBalance> _organizationCardBalanceRepository;
    private readonly IRepository<OrganizationSettlement> _organizationSettlementRepository;
    private readonly IRepository<CardDue> _cardDueRepository;

    public SettlementTransactionService(IRepository<OrganizationCardBalance> organizationCardBalanceRepository,
        IRepository<OrganizationSettlement> organizationSettlementRepository,
        IRepository<CardDue> cardDueRepository)
    {
        _organizationCardBalanceRepository = organizationCardBalanceRepository;
        _organizationSettlementRepository = organizationSettlementRepository;
        _cardDueRepository = cardDueRepository;
    }

    public void SettleRecharge(string organizationId, string cardId, decimal modelAmount)
    {
        var organizationCardBalance = _organizationCardBalanceRepository
            .GetAll()
            .FirstOrDefault(o => o.OrganizationId == organizationId && o.CardId == cardId);

        var remainingAmount = SettleCardDues(organizationId, cardId, modelAmount);

        if (organizationCardBalance != null)
        {
            organizationCardBalance.Balance += remainingAmount;
            _organizationCardBalanceRepository.Update(organizationCardBalance);
        }
        else
        {
            _organizationCardBalanceRepository.Insert(new OrganizationCardBalance
            {
                CardId = cardId,
                OrganizationId = organizationId,
                Balance = remainingAmount
            });
        }

        _organizationCardBalanceRepository.SaveChanges();
    }

    private decimal SettleCardDues(string organizationId, string cardId, decimal amount)
    {
        var cardDue = _cardDueRepository.GetAll().FirstOrDefault(c => c.CardId == cardId);

        if (cardDue == null)
        {
            return amount;
        }
        
        if (cardDue.OrganizationId == organizationId)
        {
            if (cardDue.Amount <= amount)
            {
                amount -= cardDue.Amount;
                _cardDueRepository.Delete(cardDue);
                _cardDueRepository.SaveChanges();
                return amount;
            }

            cardDue.Amount -= amount;
            _cardDueRepository.Update(cardDue);
            _cardDueRepository.SaveChanges();
            return 0;

        }

        if (cardDue.Amount <= amount)
        {
            amount -= cardDue.Amount;
            _cardDueRepository.Delete(cardDue);
            _organizationSettlementRepository.Insert(new OrganizationSettlement()
            {
                FromOrganizationId = organizationId,
                ToOrganizationId = cardDue.OrganizationId,
                Amount = cardDue.Amount,
                TransactionId = cardDue.TransactionId,
                TransactionType = TransactionType.BusFare
            });
            _organizationSettlementRepository.SaveChanges();
            _cardDueRepository.SaveChanges();
            return amount;
        }

        _organizationSettlementRepository.Insert(new OrganizationSettlement()
        {
            FromOrganizationId = organizationId,
            ToOrganizationId = cardDue.OrganizationId,
            Amount = amount,
            TransactionId = cardDue.TransactionId,
            TransactionType = TransactionType.BusFare
        });
        _organizationSettlementRepository.SaveChanges();

        cardDue.Amount -= amount;
        _cardDueRepository.Update(cardDue);
        _cardDueRepository.SaveChanges();
        return 0;
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

    public void SettleTrip(Card card, string staffOrganizationId, decimal amount, string transactionId)
    {
        var organizationWiseBalance =
           _organizationCardBalanceRepository
               .GetAll()
               .Where(o => o.CardId == card.Id)
               .ToList();

        var remainingAmount = amount;

        var checkIfSameOrg = organizationWiseBalance
            .FirstOrDefault(o => o.OrganizationId == staffOrganizationId);

        if (checkIfSameOrg != null)
        {
            if (checkIfSameOrg.Balance >= remainingAmount)
            {
                checkIfSameOrg.Balance -= remainingAmount;
                _organizationCardBalanceRepository.Update(checkIfSameOrg);
                _organizationCardBalanceRepository.SaveChanges();
                remainingAmount = 0;
            }
            else
            {
                remainingAmount -= checkIfSameOrg.Balance;
                checkIfSameOrg.Balance = 0;
                _organizationCardBalanceRepository.Delete(checkIfSameOrg);
                _organizationCardBalanceRepository.SaveChanges();

                if(organizationWiseBalance.Count == 1)
                {
                    _cardDueRepository.Insert(new CardDue()
                    {
                        CardId = card.Id,
                        OrganizationId = staffOrganizationId,
                        Amount = remainingAmount,
                        TransactionId = transactionId
                    });
                    return;
                }
            }
        }

        if (remainingAmount > 0)
        {
            var otherOrganizations = organizationWiseBalance
                .Where(o => o.OrganizationId != staffOrganizationId)
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
                        ToOrganizationId = staffOrganizationId,
                        Amount = remainingAmount,
                        TransactionId = transactionId,
                        TransactionType = TransactionType.BusFare
                    });
                    break;
                }

                _organizationSettlementRepository.Insert(new OrganizationSettlement
                {
                    FromOrganizationId = organization.OrganizationId,
                    ToOrganizationId = staffOrganizationId,
                    Amount = organization.Balance,
                    TransactionId = transactionId,
                    TransactionType = TransactionType.Return
                });
                remainingAmount -= organization.Balance;
                organization.Balance = 0;
                _organizationCardBalanceRepository.Delete(organization);
            }

            if (remainingAmount > 0)
            {
                _cardDueRepository.Insert(new CardDue()
                {
                    CardId = card.Id,
                    OrganizationId = staffOrganizationId,
                    Amount = remainingAmount,
                    TransactionId = transactionId
                });

                _cardDueRepository.SaveChanges();
            }

            _organizationCardBalanceRepository.SaveChanges();
            _organizationSettlementRepository.SaveChanges();
        }
    }
}
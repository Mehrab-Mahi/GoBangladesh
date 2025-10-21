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
    private readonly IRepository<Transaction> _transactionRepository;
    private readonly IRepository<Trip> _tripRepository;
    private readonly IPromoService _promoService;

    public SettlementTransactionService(IRepository<OrganizationCardBalance> organizationCardBalanceRepository,
        IRepository<OrganizationSettlement> organizationSettlementRepository,
        IRepository<CardDue> cardDueRepository,
        IRepository<Transaction> transactionRepository,
        IRepository<Trip> tripRepository,
        IPromoService promoService)
    {
        _organizationCardBalanceRepository = organizationCardBalanceRepository;
        _organizationSettlementRepository = organizationSettlementRepository;
        _cardDueRepository = cardDueRepository;
        _transactionRepository = transactionRepository;
        _tripRepository = tripRepository;
        _promoService = promoService;
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
                RemainingAmount = cardDue.Amount,
                TransactionId = cardDue.TransactionId,
                TransactionType = TransactionType.Due,
                Status = SettlementStatus.Pending
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
            RemainingAmount = amount,
            TransactionId = cardDue.TransactionId,
            TransactionType = TransactionType.Due, 
            Status = SettlementStatus.Pending
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
            if(checkIfSameOrg.Balance > remainingAmount)
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
                        RemainingAmount = remainingAmount,
                        TransactionId = transactionId,
                        TransactionType = TransactionType.Return, 
                        Status = SettlementStatus.Pending
                    });
                    break;
                }

                _organizationSettlementRepository.Insert(new OrganizationSettlement
                {
                    FromOrganizationId = organization.OrganizationId,
                    ToOrganizationId = agentOrganizationId,
                    Amount = organization.Balance,
                    RemainingAmount = organization.Balance,
                    TransactionId = transactionId,
                    TransactionType = TransactionType.Return, 
                    Status = SettlementStatus.Pending
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
            if (checkIfSameOrg.Balance > remainingAmount)
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
                        RemainingAmount = remainingAmount,
                        TransactionId = transactionId,
                        TransactionType = TransactionType.BusFare,
                        Status = SettlementStatus.Pending
                    });
                    remainingAmount = 0;
                    break;
                }

                _organizationSettlementRepository.Insert(new OrganizationSettlement
                {
                    FromOrganizationId = organization.OrganizationId,
                    ToOrganizationId = staffOrganizationId,
                    Amount = organization.Balance,
                    RemainingAmount = organization.Balance,
                    TransactionId = transactionId,
                    TransactionType = TransactionType.Return,
                    Status = SettlementStatus.Pending
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

    public void UpdateCardDetail(Card newCard, Card previousCard)
    {
        UpdateOrganizationWiseCardBalance(newCard.Id, previousCard.Id);
        UpdatePreviousCardDue(newCard.Id, previousCard.Id);
    }

    public void SettlePromoAmount(string cardId, string busOrganizationId, decimal promoAmount, string transactionId)
    {
        var promoCard = _promoService.GetPromoCardByCardId(cardId);

        if(promoCard is null) return;

        if (promoCard.Promo.OrganizationId != busOrganizationId)
        {
            _organizationSettlementRepository.Insert(new OrganizationSettlement()
            {
                FromOrganizationId = promoCard.Promo.OrganizationId,
                ToOrganizationId = busOrganizationId,
                Amount = promoAmount,
                RemainingAmount = promoAmount,
                TransactionId = transactionId,
                TransactionType = TransactionType.Promo,
                Status = SettlementStatus.Pending
            });

            _organizationSettlementRepository.SaveChanges();
        }
    }

    private void UpdatePreviousCardDue(string newCardId, string previousCardId)
    {
        var cardDues = _cardDueRepository
            .GetAll()
            .Where(c => c.CardId == previousCardId)
            .ToList();

        if(!cardDues.Any()) return;

        foreach (var cardDue in cardDues)
        {
            cardDue.CardId = newCardId;
            _cardDueRepository.Update(cardDue);
        }
        _cardDueRepository.SaveChanges();

        var transactionIds = cardDues
            .Select(c => c.TransactionId)
            .Distinct()
            .ToList();

        var transactions = _transactionRepository
            .GetAll()
            .Where(t => transactionIds.Contains(t.TransactionId))
            .ToList();

        foreach (var transaction in transactions)
        {
            transaction.CardId = newCardId;
            _transactionRepository.Update(transaction);
        }
        _transactionRepository.SaveChanges();

        var tripIds = transactions
            .Select(t => t.TripId)
            .ToList();

        var trips = _tripRepository
            .GetAll()
            .Where(t => tripIds.Contains(t.Id))
            .ToList();

        foreach (var trip in trips)
        {
            trip.CardId = newCardId;
            _tripRepository.Update(trip);
        }
        _tripRepository.SaveChanges();
    }

    private void UpdateOrganizationWiseCardBalance(string newCardId, string previousCardId)
    {
        var organizationCardBalances = _organizationCardBalanceRepository
            .GetAll()
            .Where(o => o.CardId == previousCardId)
            .ToList();
        foreach (var organizationCardBalance in organizationCardBalances)
        {
            organizationCardBalance.CardId = newCardId;
            _organizationCardBalanceRepository.Update(organizationCardBalance);
        }
        _organizationCardBalanceRepository.SaveChanges();
    }
}
using System;
using System.Linq;
using System.Net.Http;
using GoBangladesh.Application.DTOs.Notification;
using GoBangladesh.Application.DTOs.Transaction;
using GoBangladesh.Application.Interfaces;
using GoBangladesh.Application.Util;
using GoBangladesh.Application.ViewModels;
using GoBangladesh.Application.ViewModels.Transaction;
using GoBangladesh.Domain.Entities;
using GoBangladesh.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Route = GoBangladesh.Domain.Entities.Route;

namespace GoBangladesh.Application.Services;

public class TransactionService : ITransactionService
{
    private readonly IRepository<Transaction> _transactionRepository;
    private readonly ILoggedInUserService _loggedInUserService;
    private readonly IRepository<Trip> _tripRepository;
    private readonly IRepository<Session> _sessionRepository;
    private readonly DistanceMatrixApiSettings _distanceMatrixApiSettings;
    private readonly IRepository<Card> _cardRepository;
    private readonly ISettlementTransactionService _settlementService;
    private readonly IPromoService _promoService;
    private readonly INotificationService _notificationService;
    private readonly ICardService _cardService;

    public TransactionService(IRepository<Transaction> transactionRepository,
        ILoggedInUserService loggedInUserService, 
        IRepository<Trip> tripRepository,
        IRepository<Session> sessionRepository, 
        IOptions<DistanceMatrixApiSettings> distanceMatrixApiSettings, 
        IRepository<Card> cardRepository,
        ISettlementTransactionService settlementService,
        IPromoService promoService,
        INotificationService notificationService,
        ICardService cardService)
    {
        _transactionRepository = transactionRepository;
        _loggedInUserService = loggedInUserService;
        _tripRepository = tripRepository;
        _sessionRepository = sessionRepository;
        _cardRepository = cardRepository;
        _settlementService = settlementService;
        _promoService = promoService;
        _notificationService = notificationService;
        _cardService = cardService;
        _distanceMatrixApiSettings = distanceMatrixApiSettings.Value;
    }

    public PayloadResponse Recharge(RechargeRequest model)
    {
        var currentUser = _loggedInUserService.GetLoggedInUser();

        if (currentUser == null)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                Message = "User not found!"
            };
        }

        string medium;

        if (currentUser.UserType == UserTypes.Agent)
        {
            medium = RechargeMedium.Agent;
        }
        else if (currentUser.UserType == UserTypes.TicketExaminer)
        {
            medium = RechargeMedium.TicketExaminer;
        }
        else
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                Message = "User does not have permission to recharge!"
            };
        }

        var card = _cardRepository
            .GetConditional(c => c.CardNumber == model.CardNumber);

        if(card.Status is CardStatus.Obsolete or CardStatus.Paused)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                PayloadType = "Recharge",
                Message = $"Recharge is not possible on {card.Status} card!"
            };
        }

        var transaction = new Transaction();

        try
        {
            transaction = AddRechargeTransaction(model, TransactionType.Recharge, card.Id);
            transaction.Medium = medium;
        }
        catch (Exception ex)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                Message = $"Transaction failed because {ex.Message}",
            };
        }

        try
        {
            UpdateCardDatabase(model.Amount, card);
            _settlementService
                .SettleRecharge(currentUser.OrganizationId, card.Id, model.Amount);

            var cardOwner = _cardService.GetUserByCardNumber(card.CardNumber);

            if (cardOwner is not null)
            {
                _notificationService.InsertEventNotification(new EventNotificationCreateRequest()
                {
                    UserId = cardOwner.Id,
                    Title = "Card Recharged Successfully",
                    Message = $"Your card {card.CardNumber} has been successfully recharged with amount {model.Amount}."
                });
            }

            return new PayloadResponse()
            {
                IsSuccess = true,
                PayloadType = "Recharge",
                Message = "Recharge has been successful!"
            };
        }
        catch (Exception ex)
        {
            DeleteTransaction(transaction);

            return new PayloadResponse()
            {
                IsSuccess = false,
                PayloadType = "Recharge",
                Message = $"Recharge has been failed because {ex.Message}!"
            };
        }
    }

    private void UpdateCardDatabase(decimal amount, Card card)
    {
        if (card == null) { return; }

        if (card.Status == CardStatus.NotUsed)
        {
            card.Status = CardStatus.InUse;
        }
        card.Balance += amount;

        _cardRepository.Update(card);
        _cardRepository.SaveChanges();
    }

    public PayloadResponse Tap(TapRequest tapRequest)
    {
        var currentUser = _loggedInUserService.GetLoggedInUser();

        if (currentUser == null)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                Message = "User not found!"
            };
        }

        var card = _cardRepository.GetAll()
            .Where(c => c.CardNumber == tapRequest.CardNumber)
            .Include(c => c.Organization)
            .FirstOrDefault();

        if (card == null)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                PayloadType = "Tap",
                Message = "Card not found!"
            };
        }

        if (card.Status == CardStatus.NotUsed)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                PayloadType = "Tap",
                Message = "Need to recharge!"
            };
        }

        if (card.Status != CardStatus.InUse)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                PayloadType = "Tap",
                Message = "Card is not in use!"
            };
        }

        var session = _sessionRepository
            .GetAll()
            .Where(s => s.Id == tapRequest.SessionId)
            .Include(s => s.User)
            .Include(s => s.User.Organization)
            .Include(s => s.Bus)
            .Include(s => s.Bus.Organization)
            .Include(s => s.Bus.Route)
            .FirstOrDefault();

        if (session == null)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                PayloadType = "Tap",
                Message = "Session not found!"
            };
        }

        if (session.Bus.Route == null)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                PayloadType = "Tap",
                Message = "No route is assigned to this bus!"
            };
        }

        if (card.Organization.OrganizationType == OrganizationTypes.Public &&
            session.Bus.Organization.OrganizationType == OrganizationTypes.Private)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                PayloadType = "Tap",
                Message = "Organization is not same!"
            };
        }

        if(card.Organization.OrganizationType == OrganizationTypes.Private &&
           session.Bus.Organization.OrganizationType == OrganizationTypes.Private
           && card.Organization.Id != session.Bus.Organization.Id)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                PayloadType = "Tap",
                Message = "Organization is not same!"
            };
        }

        var minimumBalanceCheck = tapRequest.TapType == "Penalty" ?
            IsMinimumBalanceAvailable(card, session.Bus.Route.PenaltyAmount) :
            IsMinimumBalanceAvailable(card, session.Bus.Route.MinimumBalance);

        var cardSessionVerification = IfCardIsOnAnyOngoingTripOnAnotherSession(card, tapRequest, currentUser);

        if (!cardSessionVerification.IsSuccess) return cardSessionVerification;

        if (!minimumBalanceCheck.IsSuccess) return minimumBalanceCheck;

        var trip = _tripRepository
            .GetAll()
            .Include(t => t.Session)
            .Include(t => t.Session.Bus)
            .Include(t => t.Session.Bus)
            .OrderByDescending(t => t.CreateTime)
            .FirstOrDefault(t => t.CardId == card.Id && t.SessionId == tapRequest.SessionId);

        if (trip is null)
        {
            try
            {
                AddTrip(tapRequest, card);

                return new PayloadResponse()
                {
                    IsSuccess = true,
                    PayloadType = "Tap",
                    Message = "Trip started!"
                };
            }
            catch (Exception ex)
            {
                return new PayloadResponse()
                {
                    IsSuccess = false,
                    PayloadType = "Tap",
                    Message = $"Bus fare transaction has been failed because {ex.Message}"
                };
            }
        }

        if (!trip.IsRunning)
        {
            try
            {
                AddTrip(tapRequest, card);

                return new PayloadResponse()
                {
                    IsSuccess = true,
                    PayloadType = "Tap",
                    Message = "Trip started!"
                };
            }
            catch (Exception ex)
            {
                return new PayloadResponse()
                {
                    IsSuccess = false,
                    PayloadType = "Tap",
                    Message = $"Bus fare transaction has been failed because {ex.Message}"
                };
            }
        }

        if (IfTripTimeDifferenceIsLessThanOneMin(trip.TripStartTime))
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                PayloadType = "Tap",
                Message = "Card has punched multiple times within one minute!"
            };
        }

        try
        {
            trip.EndingLatitude = tapRequest.Latitude;
            trip.EndingLongitude = tapRequest.Longitude;

            var tripFare = GetTripFareAndDistance(trip, session.Bus.Route);

            trip.TripEndTime = DateTime.UtcNow;
            trip.IsRunning = false;
            trip.Distance = tripFare.Distance;
            trip.Amount = tripFare.Fare;
            trip.TapOutStatus = tapRequest.TapType;
            trip.PromoAmount = tripFare.PromoAmount;

            _tripRepository.Update(trip);
            _tripRepository.SaveChanges();
        }
        catch (Exception ex)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                PayloadType = "Tap",
                Message = $"Trip fare addition has been failed because {ex.Message}"
            };
        }

        return BusFareTransaction(card, trip, currentUser.OrganizationId);
    }

    public PayloadResponse ForceTripStop(ForceStopTripDto forceStop)
    {
        var card = _cardRepository.GetConditional(c => c.Id == forceStop.CardId);

        if (card == null)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                PayloadType = "Trip",
                Message = "Card not found!"
            };
        }
        
        var trip = _tripRepository.GetConditional(t => t.Id == forceStop.TripId && t.IsRunning);

        if (trip == null)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                PayloadType = "Trip",
                Message = "No running trip not found!"
            };
        }

        var session = _sessionRepository
            .GetAll()
            .Where(s => s.Id == forceStop.SessionId)
            .Include(s => s.Bus)
            .Include(r => r.Bus.Route)
            .FirstOrDefault();

        if (session == null)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                PayloadType = "Trip",
                Message = "Session not found!"
            };
        }

        try
        {
            trip.TripEndTime = DateTime.UtcNow;
            trip.IsRunning = false;
            trip.Distance = 0;
            trip.Amount = session.Bus.Route.PenaltyAmount;
            trip.TapOutStatus = forceStop.TripCloseStatus;

            _tripRepository.Update(trip);
            _tripRepository.SaveChanges();
        }
        catch (Exception ex)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                PayloadType = "Trip",
                Message = $"Force stop failed because {ex.Message}!"
            };
        }

        return BusFareTransaction(card, trip, session.Bus.OrganizationId);
    }

    private PayloadResponse IfCardIsOnAnyOngoingTripOnAnotherSession(Card card, TapRequest tapRequest, User currentUser)
    {
        var trip = _tripRepository
            .GetAll()
            .Where(t => t.CardId == card.Id && t.SessionId != tapRequest.SessionId && t.IsRunning)
            .Include(t => t.Session)
            .Include(t => t.Session.Bus)
            .Include(t => t.Session.Bus.Route)
            .FirstOrDefault();

        if (trip == null) return new PayloadResponse()
        {
            IsSuccess = true
        };

        try
        {
            trip.EndingLatitude = tapRequest.Latitude;
            trip.EndingLongitude = tapRequest.Longitude;
            trip.TripEndTime = DateTime.UtcNow;
            trip.IsRunning = false;
            trip.Amount = trip.Session.Bus.Route.PenaltyAmount;
            trip.TapOutStatus = TapOutStatus.Penalty;

            _tripRepository.Update(trip);
            _tripRepository.SaveChanges();
        }
        catch (Exception ex)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                PayloadType = "Tap",
                Message = $"Trip fare addition has been failed because {ex.Message}"
            };
        }

        return BusFareTransaction(card, trip, currentUser.OrganizationId);
    }
    
    private PayloadResponse BusFareTransaction(Card card, Trip trip, string settlementOrganizationId)
    {
        var transaction = new Transaction();

        try
        {
            transaction = AddBusFareTransaction(TransactionType.BusFare, card.Id, trip);
        }
        catch (Exception ex)
        {
            RollBackTrip(trip);
            return new PayloadResponse()
            {
                IsSuccess = false,
                PayloadType = "Tap",
                Message = $"Bus fare transaction has been failed because {ex.Message}"
            };
        }

        try
        {
            UpdateCardAmount(card, trip.Amount, TransactionOperation.Subtract);
            _settlementService.SettleTrip(card, settlementOrganizationId, transaction.Amount, transaction.TransactionId);
            _settlementService.SettlePromoAmount(trip.CardId, trip.Session.Bus.OrganizationId, trip.PromoAmount, transaction.TransactionId);

            var cardOwner = _cardService.GetUserByCardNumber(card.CardNumber);

            if (cardOwner is not null)
            {
                _notificationService.InsertEventNotification(new EventNotificationCreateRequest()
                {
                    UserId = cardOwner.Id,
                    Title = "Trip Ended",
                    Message = $"Your trip has been ended successfully. Fare: {trip.Amount}."
                });
            }

            return new PayloadResponse()
            {
                IsSuccess = true,
                PayloadType = "Tap",
                Message = "Bus fare deduction has been successful!"
            };
        }
        catch (Exception ex)
        {
            RollBackTrip(trip);
            DeleteTransaction(transaction);

            return new PayloadResponse()
            {
                IsSuccess = false,
                PayloadType = "Tap",
                Message = $"Bus fare deduction has been failed because {ex.Message}!"
            };
        }
    }

    private TripFareDistanceDto GetTripFareAndDistance(Trip trip, Route route)
    {
        var distance = GetDistance(trip);
        var fare = trip.TapInType == "Penalty" ?
            route.PenaltyAmount :
            GetCalculatedAmount(distance, route);
        var promoAmount = (decimal)0.0;

        var promoCard = GetAvailablePromo(trip.CardId);

        if (promoCard != null)
        {
            promoAmount = _promoService.GetPromoAmountByCardId(trip.CardId, fare);
            fare -= promoAmount;
            _promoService.MarkPromoAsUsedByCardId(trip.CardId);
            _promoService.UpdatePromoUsageAmount(promoCard.Id, promoAmount);
        }

        return new TripFareDistanceDto()
        {
            Distance = distance,
            Fare = fare,
            PromoAmount = promoAmount
        };
    }

    private PromoCard GetAvailablePromo(string cardId)
    {
        var promoCard = _promoService.GetPromoCardByCardId(cardId);
        
        return promoCard;
    }

    private decimal GetCalculatedAmount(decimal distance, Route route)
    {
        var fare = distance * route.PerKmFare;

        return fare < route.BaseFare ? route.BaseFare : fare;
    }

    private decimal GetDistance(Trip trip)
    {
        var url = $"{_distanceMatrixApiSettings.BaseUrl}{trip.StartingLongitude},{trip.StartingLatitude};{trip.EndingLongitude},{trip.EndingLatitude}?overview=false";
        using var httpClient = new HttpClient();
        var response = httpClient.GetStringAsync(url).Result;
        var data = JsonConvert.DeserializeObject<DistanceApiDto>(response);
        return data.Routes.FirstOrDefault()!.Distance / 1000;
    }

    private static bool IfTripTimeDifferenceIsLessThanOneMin(DateTime tripStartTime)
    {
        var timeDifference = (DateTime.UtcNow - tripStartTime).TotalSeconds;

        return Math.Abs(timeDifference) < 60;
    }

    public PayloadResponse IsMinimumBalanceAvailable(Card card, decimal minimumBalance)
    {
        try
        {
            if (card.Balance < minimumBalance)
            {
                return new PayloadResponse()
                {
                    IsSuccess = false,
                    Content = false,
                    PayloadType = "Transaction",
                    Message = $"Passenger's balance is only {card.Balance}; minimum balance is {minimumBalance}"
                };
            }

            return new PayloadResponse()
            {
                IsSuccess = true,
                Content = true,
                PayloadType = "Transaction",
                Message = "Passenger balance is upper than the limit"
            };
        }
        catch (Exception ex)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                Content = false,
                PayloadType = "Transaction",
                Message = $"Balance checking failed because {ex.Message}"
            };
        }
    }

    private void RollBackTrip(Trip trip)
    {
        trip.EndingLatitude = null;
        trip.EndingLongitude = null;
        trip.TripEndTime = null;
        trip.IsRunning = false;
        trip.Distance = 0;
        trip.Amount = 0;

        _tripRepository.Update(trip);
        _tripRepository.SaveChanges();
    }

    private void AddTrip(TapRequest tapRequest, Card card)
    {
        _tripRepository.Insert(new Trip()
        {
            CardId = card.Id,
            SessionId = tapRequest.SessionId,
            StartingLatitude = tapRequest.Latitude,
            StartingLongitude = tapRequest.Longitude,
            TripStartTime = DateTime.UtcNow,
            TapInType = tapRequest.TapType
        });

        _tripRepository.SaveChanges();

        var cardOwner = _cardService.GetUserByCardNumber(card.CardNumber);

        if (cardOwner is not null)
        {
            _notificationService.InsertEventNotification(new EventNotificationCreateRequest()
            {
                UserId = cardOwner.Id,
                Title = "Trip Started",
                Message = "Your trip has been started."
            });
        }
    }

    private Transaction AddBusFareTransaction(string transactionType,
        string cardId,
        Trip trip)
    {
        var transaction = new Transaction()
        {
            TransactionType = transactionType,
            Amount = trip.Amount,
            CardId = cardId,
            TripId = trip.Id,
            PromoAmount = trip.PromoAmount
        };

        _transactionRepository.Insert(transaction);
        _transactionRepository.SaveChanges();

        return transaction;
    }

    private void DeleteTransaction(Transaction transaction)
    {
        _transactionRepository.Delete(transaction);
        _transactionRepository.SaveChanges();
    }

    private void UpdateCardAmount(Card card, decimal amount, string operation)
    {
        if (operation == TransactionOperation.Add)
        {
            card.Balance += amount;
        }
        else
        {
            card.Balance -= amount;
        }

        _cardRepository.Update(card);
        _cardRepository.SaveChanges();
    }

    private Transaction AddRechargeTransaction(RechargeRequest model, string transactionType, string cardId)
    {
        var agentId = _loggedInUserService.GetLoggedInUser();
        var transaction = new Transaction()
        {
            TransactionType = transactionType,
            Amount = model.Amount,
            CardId = cardId,
            AgentId = agentId.Id
        };

        _transactionRepository.Insert(transaction);
        _transactionRepository.SaveChanges();

        return transaction;
    }

    public void ForceTripStopLinkedWIthSession(Trip trip, Route route, string latitude, string longitude, string tapOutStatus)
    {
        var card = _cardRepository.GetConditional(c => c.Id == trip.CardId);
        var currentUser = _loggedInUserService.GetLoggedInUser();
        try
        {
            trip.EndingLatitude = latitude;
            trip.EndingLongitude = longitude;

            var tripFare = GetTripFareAndDistance(trip, route);

            trip.TripEndTime = DateTime.UtcNow;
            trip.IsRunning = false;
            trip.Distance = tripFare.Distance;
            trip.Amount = tripFare.Fare;
            trip.TapOutStatus = tapOutStatus;
            trip.PromoAmount = tripFare.PromoAmount;

            _tripRepository.Update(trip);
            _tripRepository.SaveChanges();
        }
        catch
        {
            return;
        }

        var transaction = new Transaction();

        try
        {
            transaction = AddBusFareTransaction(TransactionType.BusFare, trip.Card.Id, trip);
        }
        catch
        {
            RollBackTrip(trip);
            return;
        }

        try
        {
            UpdateCardAmount(trip.Card, trip.Amount, TransactionOperation.Subtract);
            _settlementService.SettleTrip(card, currentUser.OrganizationId, transaction.Amount, transaction.TransactionId);
        }
        catch
        {
            RollBackTrip(trip);
            DeleteTransaction(transaction);
        }
    }

    public PayloadResponse Return(ReturnRequest model)
    {
        var currentUser = _loggedInUserService.GetLoggedInUser();

        if (currentUser == null)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                Message = "User not found!"
            };
        }

        var card = _cardRepository
            .GetConditional(c => c.CardNumber == model.CardNumber);

        if (card.Status != CardStatus.InUse)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                PayloadType = "Return",
                Message = "Card is not in use!"
            };
        }

        if (model.Amount <= 0)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                PayloadType = "Return",
                Message = "Amount must be greater than 0!"
            };
        }

        if (model.Amount > card.Balance)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                PayloadType = "Return",
                Message = "Amount must be less than or equal card balance!"
            };
        }

        var transaction = new Transaction();

        try
        {
            transaction = AddReturnTransaction(model, TransactionType.Return, card.Id, currentUser.UserType);
        }
        catch (Exception ex)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                Message = $"Transaction failed because {ex.Message}",
            };
        }

        try
        {
            UpdateCardAmount(card, model.Amount, TransactionOperation.Subtract);
            _settlementService
                .SettleReturn(card, currentUser.OrganizationId, model.Amount, transaction.TransactionId);

            var cardOwner = _cardService.GetUserByCardNumber(card.CardNumber);

            if (cardOwner is not null)
            {
                _notificationService.InsertEventNotification(new EventNotificationCreateRequest()
                {
                    UserId = cardOwner.Id,
                    Title = "Card Returned Successfully",
                    Message = $"Amount - {model.Amount} from your card - {card.CardNumber} has been returned!"
                });
            }

            return new PayloadResponse()
            {
                IsSuccess = true,
                PayloadType = "Return",
                Message = "Return has been successful!"
            };
        }
        catch (Exception ex)
        {
            DeleteTransaction(transaction);

            return new PayloadResponse()
            {
                IsSuccess = false,
                PayloadType = "Return",
                Message = $"Return has been failed because {ex.Message}!"
            };
        }
    }

    private Transaction AddReturnTransaction(ReturnRequest model, string transactionType, string cardId, string userType)
    {
        var agentId = _loggedInUserService.GetLoggedInUser();
        var transaction = new Transaction()
        {
            TransactionType = transactionType,
            Amount = model.Amount,
            CardId = cardId,
            AgentId = agentId.Id,
            Medium = userType
        };

        _transactionRepository.Insert(transaction);
        _transactionRepository.SaveChanges();

        return transaction;
    }
}
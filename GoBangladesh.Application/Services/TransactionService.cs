using System;
using System.Linq;
using System.Net.Http;
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
    private readonly IRepository<User> _userRepository;
    private readonly ILoggedInUserService _loggedInUserService;
    private readonly IRepository<Trip> _tripRepository;
    private readonly IRepository<Session> _sessionRepository;
    private readonly DistanceMatrixApiSettings _distanceMatrixApiSettings;
    private readonly IRepository<Card> _cardRepository;

    public TransactionService(IRepository<Transaction> transactionRepository,
        IRepository<User> userRepository,
        ILoggedInUserService loggedInUserService, 
        IRepository<Trip> tripRepository,
        IRepository<Session> sessionRepository, 
        IOptions<DistanceMatrixApiSettings> distanceMatrixApiSettings, 
        IRepository<Card> cardRepository)
    {
        _transactionRepository = transactionRepository;
        _userRepository = userRepository;
        _loggedInUserService = loggedInUserService;
        _tripRepository = tripRepository;
        _sessionRepository = sessionRepository;
        _cardRepository = cardRepository;
        _distanceMatrixApiSettings = distanceMatrixApiSettings.Value;
    }

    public PayloadResponse Recharge(RechargeRequest model)
    {
        UpdateCardDatabase(model);

        var passenger = _userRepository
            .GetConditional(p => p.CardNumber == model.CardNumber);

        if (passenger == null)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                Message = "Passenger with the card number not found!"
            };
        }

        Transaction transaction;

        try
        {
            transaction = AddRechargeTransaction(model, TransactionType.Recharge, passenger.Id);
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
            UpdateCardAmount(passenger, model.Amount, TransactionOperation.Add);

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

    private void UpdateCardDatabase(RechargeRequest model)
    {
        var card = _cardRepository
            .GetConditional(c => c.CardNumber == model.CardNumber);

        if (card == null) { return; }

        card.Balance += model.Amount;

        _cardRepository.Update(card);
        _cardRepository.SaveChanges();
    }

    public PayloadResponse Tap(TapRequest tapRequest)
    {
        var passenger = _userRepository
            .GetAll()
            .Where(p => p.CardNumber == tapRequest.CardNumber)
            .Include(u => u.Organization)
            .FirstOrDefault();

        if (passenger == null)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                PayloadType = "Tap",
                Message = "Passenger with this card number is not found!"
            };
        }

        var session = _sessionRepository
            .GetAll()
            .Where(s => s.Id == tapRequest.SessionId)
            .Include(s => s.User)
            .Include(s => s.Bus)
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

        var minimumBalanceCheck = IsMinimumBalanceAvailable(passenger, session.Bus.Route.MinimumBalance);

        if (!minimumBalanceCheck.IsSuccess) return minimumBalanceCheck;

        if (passenger.OrganizationId != session.User.OrganizationId)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                PayloadType = "Tap",
                Message = "Organization is not same!"
            };
        }

        var cardSessionVerification = IfCardIsOnAnyOngoingTripOnAnotherSession(passenger.Id, tapRequest.SessionId);

        if (!cardSessionVerification.IsSuccess) return cardSessionVerification;

        var trip = _tripRepository
            .GetAll()
            .OrderByDescending(t => t.CreateTime)
            .FirstOrDefault(t => t.PassengerId == passenger.Id && t.SessionId == tapRequest.SessionId);

        if (trip is null)
        {
            try
            {
                AddTrip(tapRequest, passenger.Id);

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
                AddTrip(tapRequest, passenger.Id);

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

        Transaction transaction;

        try
        {
            transaction = AddBusFareTransaction(TransactionType.BusFare, passenger.Id, trip);
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
            UpdateCardAmount(passenger, trip.Amount, TransactionOperation.Subtract);

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
                Message = $"Recharge has been failed because {ex.Message}!"
            };
        }
    }

    public PayloadResponse ForceTripStop(ForceStopTripDto forceStop)
    {
        var passenger = _userRepository.GetConditional(p => p.Id == forceStop.PassengerId);

        if (passenger == null)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                PayloadType = "Trip",
                Message = "Passenger not found!"
            };
        }
        
        var trip = _tripRepository.GetConditional(t => t.Id == forceStop.TripId);

        if (trip == null)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                PayloadType = "Trip",
                Message = "Trip not found!"
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

        Transaction transaction;

        try
        {
            transaction = AddBusFareTransaction(TransactionType.BusFare, forceStop.PassengerId, trip);
        }
        catch (Exception ex)
        {
            RollBackTrip(trip);
            return new PayloadResponse()
            {
                IsSuccess = false,
                PayloadType = "Trip",
                Message = $"Force stop failed because {ex.Message}"
            };
        }

        try
        {
            UpdateCardAmount(passenger, trip.Amount, TransactionOperation.Subtract);

            return new PayloadResponse()
            {
                IsSuccess = true,
                PayloadType = "Trip",
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
                PayloadType = "Trip",
                Message = $"Force stop failed because {ex.Message}!"
            };
        }
    }

    private PayloadResponse IfCardIsOnAnyOngoingTripOnAnotherSession(string passengerId, string sessionId)
    {
        var trip = _tripRepository
            .GetAll()
            .Where(t => t.PassengerId == passengerId && t.SessionId != sessionId && t.IsRunning)
            .Include(t => t.Session)
            .Include(t => t.Session.Bus)
            .FirstOrDefault();

        if (trip == null) return new PayloadResponse()
        {
            IsSuccess = true
        };

        return new PayloadResponse()
        {
            IsSuccess = false,
            PayloadType = "Tap",
            Message = $"Passenger has already ongoing trip on Bus Name: {trip.Session.Bus.BusName}, Bus Number: {trip.Session.Bus.BusNumber}"
        };
    }

    private TripFareDistanceDto GetTripFareAndDistance(Trip trip, Route route)
    {
        var distance = GetDistance(trip);
        var fare = GetCalculatedAmount(distance, route);

        return new TripFareDistanceDto()
        {
            Distance = distance,
            Fare = fare
        };
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

    public PayloadResponse IsMinimumBalanceAvailable(User passenger, decimal minimumBalance)
    {
        try
        {
            if (passenger == null) 
            {
                return new PayloadResponse()
                {
                    IsSuccess = false,
                    Content = false,
                    PayloadType = "Transaction",
                    Message = "Passenger with this card number not found"
                };
            }

            if (passenger.Balance < minimumBalance)
            {
                return new PayloadResponse()
                {
                    IsSuccess = false,
                    Content = false,
                    PayloadType = "Transaction",
                    Message = $"Passenger balance is only {passenger.Balance}"
                };
            }

            return new PayloadResponse()
            {
                IsSuccess = true,
                Content = true,
                PayloadType = "Transaction",
                Message = $"Passenger balance is upper than the limit"
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

    private void AddTrip(TapRequest tapRequest, string passengerId)
    {
        _tripRepository.Insert(new Trip()
        {
            PassengerId = passengerId,
            SessionId = tapRequest.SessionId,
            StartingLatitude = tapRequest.Latitude,
            StartingLongitude = tapRequest.Longitude,
            TripStartTime = DateTime.UtcNow
        });

        _tripRepository.SaveChanges();
    }

    private Transaction AddBusFareTransaction(string transactionType,
        string passengerId,
        Trip trip)
    {
        var transaction = new Transaction()
        {
            TransactionType = transactionType,
            Amount = trip.Amount,
            PassengerId = passengerId,
            TripId = trip.Id
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

    private void UpdateCardAmount(User passenger, decimal amount, string operation)
    {
        if (operation == TransactionOperation.Add)
        {
            passenger.Balance += amount;
        }
        else
        {
            passenger.Balance -= amount;
        }

        _userRepository.Update(passenger);
        _userRepository.SaveChanges();
    }

    private Transaction AddRechargeTransaction(RechargeRequest model, string transactionType, string passengerId)
    {
        var agentId = _loggedInUserService.GetLoggedInUser();
        var transaction = new Transaction()
        {
            TransactionType = transactionType,
            Amount = model.Amount,
            PassengerId = passengerId,
            AgentId = agentId.Id
        };

        _transactionRepository.Insert(transaction);
        _transactionRepository.SaveChanges();

        return transaction;
    }
}
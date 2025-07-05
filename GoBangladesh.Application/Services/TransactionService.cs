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

namespace GoBangladesh.Application.Services;

public class TransactionService : ITransactionService
{
    private readonly IRepository<Transaction> _transactionRepository;
    private readonly IRepository<User> _userRepository;
    private readonly ILoggedInUserService _loggedInUserService;
    private readonly IRepository<Trip> _tripRepository;
    private readonly IRepository<Session> _sessionRepository;
    private readonly DistanceMatrixApiSettings _distanceMatrixApiSettings;

    public TransactionService(IRepository<Transaction> transactionRepository,
        IRepository<User> userRepository,
        ILoggedInUserService loggedInUserService, 
        IRepository<Trip> tripRepository,
        IRepository<Session> sessionRepository, 
        IOptions<DistanceMatrixApiSettings> distanceMatrixApiSettings)
    {
        _transactionRepository = transactionRepository;
        _userRepository = userRepository;
        _loggedInUserService = loggedInUserService;
        _tripRepository = tripRepository;
        _sessionRepository = sessionRepository;
        _distanceMatrixApiSettings = distanceMatrixApiSettings.Value;
    }

    public PayloadResponse Recharge(RechargeRequest model)
    {
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

        var minimumBalanceCheck = IsMinimumBalanceAvailable(passenger);

        if (!minimumBalanceCheck.IsSuccess) return minimumBalanceCheck;

        var session = _sessionRepository
            .GetAll()
            .Where(s => s.Id == tapRequest.SessionId)
            .Include(s => s.User)
            .Include(s => s.Bus)
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

        if (passenger.OrganizationId != session.User.OrganizationId)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                PayloadType = "Tap",
                Message = "Organization is not same!"
            };
        }

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

            var tripFare = GetTripFareAndDistance(trip, passenger);

            trip.TripEndTime = DateTime.Now;
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

    private TripFareDistanceDto GetTripFareAndDistance(Trip trip, User passenger)
    {
        var distance = GetDistance(trip);
        var fare = GetCalculatedAmount(distance, passenger.Organization);

        return new TripFareDistanceDto()
        {
            Distance = distance,
            Fare = fare
        };
    }

    private decimal GetCalculatedAmount(decimal distance, Organization organization)
    {
        var fare = distance * organization.PerKmFare;

        if (fare < organization.BaseFare) return organization.BaseFare;

        return fare;
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
        var timeDifference = (DateTime.Now - tripStartTime).TotalSeconds;

        return Math.Abs(timeDifference) < 60;
    }

    public PayloadResponse IsMinimumBalanceAvailable(User passenger)
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

            if (passenger.Balance < 20)
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
            TripStartTime = DateTime.Now
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
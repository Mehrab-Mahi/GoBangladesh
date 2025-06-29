using System;
using System.Data;
using System.Linq;
using GoBangladesh.Application.DTOs.Transaction;
using GoBangladesh.Application.Interfaces;
using GoBangladesh.Application.Util;
using GoBangladesh.Application.ViewModels;
using GoBangladesh.Domain.Entities;
using GoBangladesh.Domain.Interfaces;

namespace GoBangladesh.Application.Services;

public class TransactionService : ITransactionService
{
    private readonly IRepository<Transaction> _transactionRepository;
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<StaffBusMapping> _staffBusMappingRepository;
    private readonly ILoggedInUserService _loggedInUserService;
    private readonly IRepository<Trip> _tripRepository;

    public TransactionService(IRepository<Transaction> transactionRepository,
        IRepository<User> userRepository,
        IRepository<StaffBusMapping> staffBusMappingRepository, 
        ILoggedInUserService loggedInUserService, 
        IRepository<Trip> tripRepository)
    {
        _transactionRepository = transactionRepository;
        _userRepository = userRepository;
        _staffBusMappingRepository = staffBusMappingRepository;
        _loggedInUserService = loggedInUserService;
        _tripRepository = tripRepository;
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
            UpdateCardAmount(model.CardNumber, model.Amount, TransactionOperation.Add);

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

    public PayloadResponse BusFare(BusFareRequest model)
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

        var staff = _userRepository
            .GetConditional(s => s.Id == model.StaffUserId);

        if (staff == null)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                Message = "Staff not found!"
            };
        }

        var bus = _staffBusMappingRepository
            .GetConditional(s => s.UserId == model.StaffUserId);

        if (bus == null)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                Message = "Bus not found!"
            };
        }

        Transaction transaction;
        Trip trip;

        try
        {
            trip = AddTrip(model, passenger.Id, bus.Id);
        }
        catch (Exception ex)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                PayloadType = "Bus Fare",
                Message = $"Bus fare transaction has been failed because {ex.Message}"
            };
        }

        try
        {
            transaction = AddBusFareTransaction(TransactionType.BusFare, passenger.Id, trip);
        }
        catch (Exception ex)
        {
            DeleteTrip(trip);
            return new PayloadResponse()
            {
                IsSuccess = false,
                PayloadType = "Bus Fare",
                Message = $"Bus fare transaction has been failed because {ex.Message}"
            };
        }

        try
        {
            UpdateCardAmount(model.CardNumber, trip.Amount, TransactionOperation.Subtract);

            return new PayloadResponse()
            {
                IsSuccess = true,
                PayloadType = "Bus Fare",
                Message = "Bus fare deduction has been successful!"
            };
        }
        catch (Exception ex)
        {
            DeleteTransaction(transaction);
            DeleteTrip(trip);

            return new PayloadResponse()
            {
                IsSuccess = false,
                PayloadType = "Recharge",
                Message = $"Recharge has been failed because {ex.Message}!"
            };
        }
    }

    private void DeleteTrip(Trip trip)
    {
        _tripRepository.Delete(trip);
        _tripRepository.SaveChanges();
    }

    private Trip AddTrip(BusFareRequest model, string passengerId, string busId)
    {
        var amount = GetCalculatedAmount();

        var trip = new Trip()
        {
            PassengerId = passengerId,
            BusId = busId,
            TripStartPoint = model.TripStartPoint,
            TripEndPoint = model.TripEndPoint,
            TripStartTime = model.TripStartTime,
            TripEndTime = model.TripEndTime,
            Amount = amount
        };

        _tripRepository.Insert(trip);
        _tripRepository.SaveChanges();

        return trip;
    }

    private decimal GetCalculatedAmount()
    {
        return 51.25m;
    }

    private Transaction AddBusFareTransaction(string transactionType,
        string passengerId,
        Trip trip)
    {
        if (trip.Bus == null) throw new DataException("Bus not found!");

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

    private void UpdateCardAmount(string cardNumber, decimal amount, string operation)
    {
        var passenger = _userRepository
            .GetConditional(u => u.CardNumber == cardNumber);

        if (passenger == null)
        {
            throw new DataException("Passenger with this card number is not found");
        }

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
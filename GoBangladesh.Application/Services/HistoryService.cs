using GoBangladesh.Application.Interfaces;
using GoBangladesh.Application.ViewModels;
using GoBangladesh.Domain.Entities;
using GoBangladesh.Domain.Interfaces;
using System.Linq;
using System;
using GoBangladesh.Application.Util;
using Microsoft.EntityFrameworkCore;

namespace GoBangladesh.Application.Services;

public class HistoryService : IHistoryService
{
    private readonly IRepository<Transaction> _transactionRepository;
    private readonly IRepository<Trip> _tripRepository;

    public HistoryService(IRepository<Transaction> transactionRepository, 
        IRepository<Trip> tripRepository)
    {
        _transactionRepository = transactionRepository;
        _tripRepository = tripRepository;
    }

    public PayloadResponse PassengerHistory(string id, int pageNo, int pageSize)
    {
        try
        {
            var transactionHistory = _transactionRepository
                .GetAll()
                .Where(p => p.PassengerId == id)
                .OrderByDescending(t => t.CreateTime)
                .Skip((pageNo - 1) * pageSize)
                .Take(pageSize)
                .Include(t => t.Agent)
                .Include(t => t.Agent.Organization)
                .Include(t => t.Trip)
                .Include(t => t.Trip.Session)
                .Include(t => t.Trip.Session.Bus)
                .ToList();

            return new PayloadResponse()
            {
                IsSuccess = true,
                PayloadType = "Passenger transaction history",
                Content = transactionHistory
            };
        }
        catch (Exception ex)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                PayloadType = "Passenger transaction history",
                Message = $"Transaction history fetching failed because {ex.Message}"
            };
        }
    }

    public PayloadResponse AgentHistory(string id, int pageNo, int pageSize)
    {
        try
        {
            var transactionHistory = _transactionRepository
                .GetAll()
                .Where(p => p.CreatedBy == id && p.TransactionType == TransactionType.Recharge)
                .OrderByDescending(t => t.CreateTime)
                .Skip((pageNo - 1) * pageSize)
                .Take(pageSize)
                .Include(t => t.Passenger)
                .ToList();

            return new PayloadResponse()
            {
                IsSuccess = true,
                PayloadType = "Agent transaction history",
                Content = transactionHistory
            };
        }
        catch (Exception ex)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                PayloadType = "Agent transaction history",
                Message = $"Transaction history fetching failed because {ex.Message}"
            };
        }
    }

    public PayloadResponse SessionHistory(string id, int pageNo, int pageSize)
    {
        try
        {
            var tripHistory = _tripRepository
                .GetAll()
                .Where(t => t.SessionId == id)
                .OrderByDescending(t => t.CreateTime)
                .Skip((pageNo - 1) * pageSize)
                .Take(pageSize)
                .Include(t => t.Passenger)
                .ToList();

            return new PayloadResponse()
            {
                IsSuccess = true,
                PayloadType = "Session transaction history",
                Content = tripHistory,
                Message = "Session transaction history"
            };
        }
        catch (Exception ex)
        {
            return new PayloadResponse()
            {
                IsSuccess = true,
                PayloadType = "Session transaction history",
                Message = $"Session transaction history fetching failed because {ex.Message}"
            };
        }
    }
}
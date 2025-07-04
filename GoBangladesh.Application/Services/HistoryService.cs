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

    public HistoryService(IRepository<Transaction> transactionRepository)
    {
        _transactionRepository = transactionRepository;
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
                .Include(t => t.Trip)
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
}
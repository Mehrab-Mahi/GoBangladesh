using GoBangladesh.Application.DTOs.TicketChecker;
using GoBangladesh.Application.Interfaces;
using GoBangladesh.Application.Util;
using GoBangladesh.Application.ViewModels;
using GoBangladesh.Domain.Entities;
using GoBangladesh.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GoBangladesh.Application.Services;

public class HistoryService : IHistoryService
{
    private readonly IRepository<Transaction> _transactionRepository;
    private readonly IRepository<Trip> _tripRepository;
    private readonly ICommonService _commonService;

    public HistoryService(IRepository<Transaction> transactionRepository, 
        IRepository<Trip> tripRepository,
        ICommonService commonService)
    {
        _transactionRepository = transactionRepository;
        _tripRepository = tripRepository;
        _commonService = commonService;
    }

    public PayloadResponse PassengerHistory(string id, int pageNo, int pageSize)
    {
        try
        {
            var cardIds = _commonService.GetCardIdsFromPassengerId(id);

            var transactionHistory = _transactionRepository
                .GetAll()
                .Where(p => cardIds.Contains(p.CardId))
                .OrderByDescending(t => t.CreateTime)
                .Include(t => t.Agent)
                .Include(t => t.Agent.Organization)
                .Include(t => t.Trip)
                .Include(t => t.Trip.Session)
                .Include(t => t.Trip.Session.Bus)
                .Skip((pageNo - 1) * pageSize)
                .Take(pageSize)
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
    
    public PayloadResponse PassengerRechargeHistory(string id, int pageNo, int pageSize)
    {
        try
        {
            var cardIds = _commonService.GetCardIdsFromPassengerId(id);

            var transactionHistory = _transactionRepository
                .GetAll()
                .Where(p => cardIds.Contains(p.CardId) && (p.TransactionType == TransactionType.Recharge || p.TransactionType == TransactionType.Return))
                .Include(t => t.Agent)
                .Include(t => t.Agent.Organization)
                .OrderByDescending(t => t.CreateTime);

            var rowCount = transactionHistory.Count();

            var data = transactionHistory
                .Skip((pageNo - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return new PayloadResponse()
            {
                IsSuccess = true,
                PayloadType = "Passenger recharge transaction history",
                Content = new 
                {
                    data,
                    rowCount
                }
            };
        }
        catch (Exception ex)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                PayloadType = "Passenger recharge transaction history",
                Message = $"Transaction history fetching failed because {ex.Message}"
            };
        }
    }

    public PayloadResponse PassengerTripHistory(string id, int pageNo, int pageSize)
    {
        try
        {
            var cardIds = _commonService.GetCardIdsFromPassengerId(id);

            var transactionHistory = _transactionRepository
                .GetAll()
                .Where(p => cardIds.Contains(p.CardId) && p.TransactionType == TransactionType.BusFare)
                .Include(t => t.Trip)
                .Include(t => t.Trip.Session)
                .Include(t => t.Trip.Session.Bus)
                .Include(t => t.Trip.Session.Bus.Route)
                .Include(t => t.Trip.Session.Bus.Route.Organization)
                .OrderByDescending(t => t.CreateTime);

            var rowCount = transactionHistory.Count();

            var data = transactionHistory
                .Skip((pageNo - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return new PayloadResponse()
            {
                IsSuccess = true,
                PayloadType = "Passenger recharge transaction history",
                Content = new
                {
                    data,
                    rowCount
                }
            };
        }
        catch (Exception ex)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                PayloadType = "Passenger recharge transaction history",
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
                .Where(p => p.CreatedBy == id && (p.TransactionType == TransactionType.Recharge || p.TransactionType == TransactionType.Return))
                .Include(t => t.Card)
                .OrderByDescending(t => t.CreateTime)
                .Skip((pageNo - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var userList = _commonService.GetUserListByCardIds(transactionHistory.Select(t => t.CardId).ToList());

            foreach (var history in transactionHistory)
            {
                var passenger = userList.FirstOrDefault(u => u.CardId == history.CardId);
                history.Passenger = _commonService.GetPassengerDataFromMappingDto(passenger);
                history.PassengerId = history.Passenger?.Id;
            }

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
                .Include(t => t.Card)
                .OrderByDescending(t => t.IsRunning)
                .ThenByDescending(t => t.CreateTime)
                .Skip((pageNo - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var userList = _commonService.GetUserListByCardIds(tripHistory.Select(t => t.CardId).ToList());

            foreach (var history in tripHistory)
            {
                var passenger = userList.FirstOrDefault(u => u.CardId == history.CardId);
                history.Passenger = _commonService.GetPassengerDataFromMappingDto(passenger);
                history.PassengerId = history.Passenger?.Id;
            }

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

    public PayloadResponse TicketExaminerRechargeHistory(string id, int pageNo, int pageSize)
    {
        try
        {
            var transactionHistory = _transactionRepository
                .GetAll()
                .Where(p => p.CreatedBy == id && p.TransactionType == TransactionType.Recharge)
                .Include(t => t.Card)
                .OrderByDescending(t => t.CreateTime)
                .Skip((pageNo - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var userList = _commonService.GetUserListByCardIds(transactionHistory.Select(t => t.CardId).ToList());

            var data = new List<TicketExaminerRechargeHistoryDto>();

            foreach (var history in transactionHistory)
            {
                var passenger = userList.FirstOrDefault(u => u.CardId == history.CardId);
                
                data.Add(new TicketExaminerRechargeHistoryDto()
                {
                    PassengerName = passenger != null? passenger.Name : string.Empty,
                    CardNumber = history.Card.CardNumber,
                    Amount = history.Amount,
                    TransactionId = history.TransactionId,
                    TransactionTime = history.CreateTime
                });
            }

            return new PayloadResponse()
            {
                IsSuccess = true,
                PayloadType = "TicketExaminer transaction history",
                Content = data
            };
        }
        catch (Exception ex)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                PayloadType = "TicketExaminer transaction history",
                Message = $"TicketExaminer transaction history fetching failed because {ex.Message}"
            };
        }
    }

    public PayloadResponse TicketExaminerTripHistory(string id, int pageNo, int pageSize)
    {
        try
        {
            var tripHistory = _tripRepository
                .GetAll()
                .Where(t => t.CreatedBy == id)
                .Include(t => t.Card)
                .Include(t => t.Session)
                .Include(t => t.Session.Bus)
                .OrderByDescending(t => t.CreateTime)
                .Skip((pageNo - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var transactionIdList = _transactionRepository.GetAll()
                .Where(tr => tripHistory.Select(t => t.Id).Contains(tr.TripId))
                .ToList();

            var userList = _commonService.GetUserListByCardIds(tripHistory.Select(t => t.CardId).ToList());

            var data = new List<TicketExaminerTripHistoryDto>();

            foreach (var history in tripHistory)
            {
                var passenger = userList.FirstOrDefault(u => u.CardId == history.CardId);
                var transaction = transactionIdList.FirstOrDefault(tr => tr.TripId == history.Id);

                data.Add(new TicketExaminerTripHistoryDto()
                {
                    PassengerName = passenger != null? passenger.Name : string.Empty,
                    CardNumber = history.Card.CardNumber,
                    BusNumber = history.Session.Bus.BusNumber,
                    Amount = history.Amount,
                    IsRunning = history.IsRunning,
                    TransactionId = transaction != null ? transaction.TransactionId : string.Empty,
                    TripStartTime = history.TripStartTime,
                });
            }

            return new PayloadResponse()
            {
                IsSuccess = true,
                PayloadType = "TicketExaminer trip history",
                Content = data
            };
        }
        catch (Exception ex)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                PayloadType = "TicketExaminer trip history",
                Message = $"TicketExaminer trip history fetching failed because {ex.Message}"
            };
        }
    }
}
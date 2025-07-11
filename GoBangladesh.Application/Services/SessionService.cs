using GoBangladesh.Application.DTOs.Session;
using GoBangladesh.Application.Interfaces;
using GoBangladesh.Application.Util;
using GoBangladesh.Application.ViewModels;
using GoBangladesh.Domain.Entities;
using GoBangladesh.Domain.Interfaces;
using System;
using System.Linq;
using GoBangladesh.Application.DTOs.Transaction;
using Microsoft.EntityFrameworkCore;

namespace GoBangladesh.Application.Services;

public class SessionService : ISessionService
{
    private readonly ILoggedInUserService _loggedInUserService;
    private readonly IRepository<Session> _sessionRepository;
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<Trip> _tripRepository;
    private readonly IBaseRepository _baseRepository;
    private readonly ITransactionService _transactionService;

    public SessionService(ILoggedInUserService loggedInUserService,
        IRepository<Session> sessionRepository, 
        IRepository<User> userRepository,
        IRepository<Trip> tripRepository,
        IBaseRepository baseRepository,
        ITransactionService transactionService)
    {
        _loggedInUserService = loggedInUserService;
        _sessionRepository = sessionRepository;
        _userRepository = userRepository;
        _tripRepository = tripRepository;
        _baseRepository = baseRepository;
        _transactionService = transactionService;
    }

    public PayloadResponse StartSession(SessionStartDto sessionStartDto)
    {
        try
        {
            var currentUser = _loggedInUserService
                .GetLoggedInUser();

            if (currentUser is null)
            {
                return new PayloadResponse()
                {
                    IsSuccess = false,
                    PayloadType = "Staff",
                    Message = "Current User not found!"
                };
            }

            var verifyIfAnySessionRunningOnBus = VerifyIfAnySessionRunningOnBus(sessionStartDto);

            if (!verifyIfAnySessionRunningOnBus.IsSuccess)
            {
                return new PayloadResponse()
                {
                    IsSuccess = false,
                    Message = verifyIfAnySessionRunningOnBus.Message
                };
            }

            var serialNumber = GetSerialNumberForSession();

            var session = new Session()
            {
                BusId = sessionStartDto.BusId,
                UserId = sessionStartDto.UserId,
                StartTime = DateTime.UtcNow,
                Serial = serialNumber,
                SessionCode = $"SSN-{serialNumber:D6}"
            };

            _sessionRepository.Insert(session);
            _sessionRepository.SaveChanges();

            return new PayloadResponse()
            {
                IsSuccess = true,
                Content = session,
                PayloadType = "Session",
                Message = "Session started!"
            };
        }
        catch (Exception ex)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                PayloadType = "Staff Bus Mapping",
                Message = $"Staff bus mapping failed because {ex.Message}"
            };
        }
    }

    public PayloadResponse StopSession(SessionStopDto sessionStopDto)
    {
        try
        {
            var currentUser = _loggedInUserService
                .GetLoggedInUser();

            if (currentUser is null)
            {
                return new PayloadResponse()
                {
                    IsSuccess = false,
                    PayloadType = "Staff",
                    Message = "Current User not found!"
                };
            }

            var session = _sessionRepository.GetAll()
                .Where(s => s.Id == sessionStopDto.SessionId)
                .Include(s => s.Bus)
                .FirstOrDefault();

            if (session == null)
            {
                return new PayloadResponse()
                {
                    IsSuccess = false,
                    Message = "Wrong session id!"
                };
            }

            if (!session.IsRunning)
            {
                return new PayloadResponse()
                {
                    IsSuccess = false,
                    Message = "Session is already stopped!"
                };
            }

            session.IsRunning = false;
            session.EndTime = DateTime.UtcNow;

            _sessionRepository.Update(session);
            _sessionRepository.SaveChanges();

            ForceStopTripsLinkedWithSession(session);

            return new PayloadResponse()
            {
                IsSuccess = true,
                Message = "Session stopped successfully!"
            };
        }
        catch (Exception ex)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                Message = $"Session stop failed because {ex.Message}"
            };
        }
    }

    private void ForceStopTripsLinkedWithSession(Session session)
    {
        var trips = _tripRepository
            .GetAll()
            .Where(t => t.SessionId == session.Id && t.IsRunning)
            .Include(p => p.Passenger)
            .ToList();

        foreach (var trip in trips)
        {
            var tapRequest = new TapRequest()
            {
                CardNumber = trip.Passenger.CardNumber,
                SessionId = trip.SessionId,
                Latitude = session.Bus.PresentLatitude,
                Longitude = session.Bus.PresentLongitude
            };
            _transactionService.Tap(tapRequest);
        }
    }

    public PayloadResponse GetSessionStatistics(string sessionId)
    {
        try
        {
            var session = _sessionRepository.GetConditional(s => s.Id == sessionId);

            if (session == null)
            {
                return new PayloadResponse()
                {
                    IsSuccess = false,
                    PayloadType = "Session",
                    Message = "Session not found!"
                };
            }

            var tripList = _tripRepository
                .GetAll()
                .Where(s => s.SessionId == sessionId);

            var sessionStat = new SessionStatisticsDto()
            {
                TapIn = tripList.Count(),
                TapOut = tripList.Count(t => !t.IsRunning),
                TotalFare = tripList.Sum(t => t.Amount)
            };

            return new PayloadResponse()
            {
                IsSuccess = true,
                PayloadType = "Session",
                Content = sessionStat,
                Message = "Session stat returned successfully!"
            };
        }
        catch (Exception ex)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                PayloadType = "Session",
                Message = $"Session stat fetching failed because {ex.Message}!"
            };
        }
    }

    public PayloadResponse CheckIfSessionRunning(string sessionId)
    {
        var session = _sessionRepository.GetConditional(s => s.Id == sessionId && s.IsRunning);

        if (session == null)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                PayloadType = "Session",
                Message = "No running session found!"
            };
        }

        return new PayloadResponse()
        {
            IsSuccess = true,
            PayloadType = "Session",
            Message = "Session is running!"
        };
    }

    public PayloadResponse GetStatistics(string sessionId)
    {
        var currentUser = _loggedInUserService.GetLoggedInUser();

        if (currentUser is not { UserType: UserTypes.Staff })
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                PayloadType = "Staff",
                Message = currentUser == null ?
                    "No user found" :
                    "User is not a staff!"
            };
        }

        var query = $@"select count(distinct t.PassengerId)                       TotalPassenger,
                           count(t.Id)                                      as TotalTapIn,
                           sum(case when t.IsRunning = 0 then 1 else 0 end) as TotalTapOut,
                           sum(t.Amount)                                    as TotalRevenue
                    from Sessions s
                             left join Trips t on s.Id = t.SessionId
                    where s.Id = '{sessionId}'";

        var statData = _baseRepository
            .Query<SessionStatistics>(query);

        return new PayloadResponse()
        {
            IsSuccess = true,
            PayloadType = "Agent",
            Content = statData,
            Message = "Agent stat found"
        };
    }

    public PayloadResponse CheckIfSessionRunningForLoggedInUser(string userId)
    {
        var session = _sessionRepository
            .GetAll()
            .Where(s => s.UserId == userId && s.IsRunning)
            .Include(s => s.Bus)
            .FirstOrDefault();

        if (session == null)
        {
            return new PayloadResponse()
            {
                IsSuccess = true,
                Message = "No running session for the user!"
            };
        }

        return new PayloadResponse()
        {
            IsSuccess = false,
            Content = session,
            Message = "Running session found for the user!"
        };
    }

    private int GetSerialNumberForSession()
    {
        try
        {
            var maxSerial = _sessionRepository.GetAll().Max(s => s.Serial);
            return maxSerial + 1;
        }
        catch
        {
            return 1;
        }
    }

    private PayloadResponse VerifyIfAnySessionRunningOnBus(SessionStartDto sessionStartDto)
    {
        var session = _sessionRepository
            .GetConditional(s => s.BusId == sessionStartDto.BusId && s.IsRunning);

        if (session == null)
        {
            return new PayloadResponse()
            {
                IsSuccess = true
            };
        }

        var user = _userRepository.GetConditional(u => u.Id == session.UserId);

        return new PayloadResponse()
        {
            IsSuccess = false,
            Message = $"A session is running on your selected bus by {user.Name}, Mobile : {user.MobileNumber}"
        };
    }
}
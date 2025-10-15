using GoBangladesh.Application.DTOs.Bus;
using GoBangladesh.Application.Interfaces;
using GoBangladesh.Application.ViewModels;
using GoBangladesh.Domain.Entities;
using GoBangladesh.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using GoBangladesh.Application.DTOs;
using Microsoft.EntityFrameworkCore;

namespace GoBangladesh.Application.Services;

public class BusService : IBusService
{
    private readonly IRepository<Bus> _busRepository;
    private readonly IRepository<Route> _routeRepository;
    private readonly ILoggedInUserService _loggedInUserService;
    private readonly ICommonService _commonService;
    private readonly IRepository<Session> _sessionRepository;
    private readonly IBaseRepository _baseRepository;
    private readonly ISessionService _sessionService;

    public BusService(IRepository<Bus> busRepository,
        ILoggedInUserService loggedInUserService,
        ICommonService commonService,
        IRepository<Session> sessionRepository,
        IBaseRepository baseRepository,
        IRepository<Route> routeRepository,
        ISessionService sessionService)
    {
        _busRepository = busRepository;
        _loggedInUserService = loggedInUserService;
        _commonService = commonService;
        _sessionRepository = sessionRepository;
        _baseRepository = baseRepository;
        _routeRepository = routeRepository;
        _sessionService = sessionService;
    }

    public PayloadResponse BusInsert(BusCreateRequest model)
    {
        try
        {
            if (IfDuplicateBusNumber(model.BusNumber))
            {
                return new PayloadResponse()
                {
                    IsSuccess = false,
                    PayloadType = "Bus",
                    Message = "Duplicate number found for bus"
                };
            }

            var bus = new Bus()
            {
                BusNumber = model.BusNumber,
                BusName = model.BusName,
                RouteId = model.RouteId,
                OrganizationId = model.OrganizationId,
            };

            _busRepository.Insert(bus);
            _busRepository.SaveChanges();

            return new PayloadResponse()
            {
                IsSuccess = true,
                PayloadType = "Bus",
                Message = "Bus data has been inserted successfully!"
            };
        }
        catch (Exception ex)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                PayloadType = "Bus",
                Message = $"Bus creation is failed because {ex.Message}!"
            };
        }
    }

    public PayloadResponse BusUpdate(BusUpdateRequest model)
    {
        try
        {
            var bus = _busRepository
                .GetConditional(b => b.Id == model.Id);

            if (bus == null)
            {
                return new PayloadResponse()
                {
                    IsSuccess = false,
                    PayloadType = "Bus",
                    Message = "Bus not found!"
                };
            }

            if (bus.BusNumber != model.BusNumber)
            {
                if (IfDuplicateBusNumber(model.BusNumber))
                {
                    return new PayloadResponse()
                    {
                        IsSuccess = false,
                        PayloadType = "Bus",
                        Message = "Duplicate bus number found"
                    };
                }
            }

            bus.BusName = model.BusName;
            bus.BusNumber = model.BusNumber;
            bus.RouteId = model.RouteId;
            bus.OrganizationId = model.OrganizationId;

            _busRepository.Update(bus);
            _busRepository.SaveChanges();

            return new PayloadResponse()
            {
                IsSuccess = true,
                PayloadType = "Bus",
                Message = "Bus has been updated successfully!"
            };
        }
        catch (Exception ex)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                PayloadType = "Bus",
                Message = $"Bus update is failed because {ex.Message}!"
            };
        }
    }

    public PayloadResponse GetById(string id)
    {
        try
        {
            var bus = _busRepository.GetAll()
                .Where(o => o.Id == id)
                .Include(o => o.Organization)
                .Include(o => o.Route)
                .FirstOrDefault();

            var busDataQuery = $@"
                                select b.*,
                                       count(distinct s.Id) as TotalSession,
                                       count(distinct u.Id) as TotalPassenger,
                                       sum(t.Amount)        as TotalRevenue,
                                       s.IsRunning          as IsSessionRunning
                                from Buses b
                                         left join Sessions s on b.id = s.BusId
                                         left join Trips t on s.Id = t.SessionId
                                         left join Cards c on t.CardId = c.Id
                                         left join PassengerCardHistory pch on c.Id = pch.CardId
                                         left join PassengerCardMappings pcm on c.Id = pcm.CardId
                                         left join Users u on pch.UserId = u.Id or pcm.UserId = u.Id
                                where b.Id = '{id}'
                                group by b.Id, b.BusNumber, b.BusName, b.OrganizationId, b.CreateTime, b.LastModifiedTime, b.CreatedBy,
                                         b.LastModifiedBy, b.IsDeleted, b.PresentLatitude, b.PresentLongitude, b.RouteId, b.IsActive, s.IsRunning";

            var busData = _baseRepository.Query<BusDataDto>(busDataQuery).FirstOrDefault();

            if (busData != null && bus != null)
            {
                busData.Route = bus.Route;
                busData.Organization = bus.Organization;
            }

            if (bus == null)
            {
                return new PayloadResponse()
                {
                    IsSuccess = false,
                    PayloadType = "Bus",
                    Message = "Bus not found"
                };
            }

            return new PayloadResponse()
            {
                IsSuccess = true,
                PayloadType = "Bus",
                Content = busData,
                Message = "Bus has been found"
            };
        }
        catch (Exception ex)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                PayloadType = "Bus",
                Message = $"Bus fetching has been failed because {ex.Message}"
            };
        }
    }

    public PayloadResponse GetAll(BusDataFilter filter)
    {
        try
        {
            var currentUser = _loggedInUserService
                .GetLoggedInUser();

            if (currentUser == null)
            {
                return new PayloadResponse()
                {
                    IsSuccess = false,
                    PayloadType = "Bus",
                    Message = "Bus not found"
                };
            }

            var condition = new List<string>();
            var extraCondition = $@"ORDER BY CreateTime desc
                                    OFFSET ({filter.PageNo} - 1) * {filter.PageSize} ROWS
                                    FETCH NEXT {filter.PageSize} ROWS ONLY";

            if (!currentUser.IsSuperAdmin)
            {
                if (string.IsNullOrEmpty(currentUser.OrganizationId))
                {
                    return new PayloadResponse()
                    {
                        IsSuccess = false,
                        PayloadType = "Bus",
                        Message = "Current User is not associated with any organization!"
                    };
                }

                filter.OrganizationId = currentUser.OrganizationId;
            }

            if (!string.IsNullOrEmpty(filter.SearchQuery))
            {
                condition.Add($" (BusNumber like '%{filter.SearchQuery}%' or BusName like '%{filter.SearchQuery}%') ");
            }

            if (!string.IsNullOrEmpty(filter.OrganizationId))
            {
                condition.Add($" OrganizationId = '{filter.OrganizationId}'");
            }

            var whereCondition = _commonService.GenerateWhereConditionFromConditionList(condition);

            var rowCount = _commonService.GetRowCountForData("Buses", whereCondition);

            var finalQueryData = _commonService.GetFinalData<Bus>("Buses", whereCondition, extraCondition);

            var busIds = finalQueryData.Select(q => q.Id).ToList();

            var busData = _busRepository.GetAll()
                .Where(u => busIds.Contains(u.Id))
                .Include(u => u.Organization)
                .Include(u => u.Route)
                .ToList();

            var processedData = GetProcessedData(busData);

            return new PayloadResponse()
            {
                IsSuccess = true,
                PayloadType = "Bus",
                Content = new { data = processedData, rowCount },
                Message = "Bus data fetch is successful"
            };
        }
        catch (Exception ex)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                PayloadType = "Bus",
                Message = $"Bus fetching is failed because {ex.Message}!"
            };
        }
    }

    private List<AllBusDataDto> GetProcessedData(List<Bus> busData)
    {
        var runningBuses = _sessionRepository.GetAll()
            .Where(s => s.IsRunning && busData.Select(b => b.Id).Contains(s.BusId))
            .Select(s => s.BusId)
            .ToList();

        var finalData = busData
            .Select(b => new AllBusDataDto()
            {
                Id = b.Id,
                CreateTime = b.CreateTime,
                LastModifiedTime = b.LastModifiedTime,
                CreatedBy = b.CreatedBy,
                LastModifiedBy = b.LastModifiedBy,
                IsDeleted = b.IsDeleted,
                BusNumber = b.BusNumber,
                BusName = b.BusName,
                RouteId = b.RouteId,
                Route = b.Route,
                OrganizationId = b.OrganizationId,
                Organization = b.Organization,
                PresentLatitude = b.PresentLatitude,
                PresentLongitude = b.PresentLongitude,
                IsActive = b.IsActive,
                IsSessionRunning = runningBuses.Contains(b.Id)
            })
            .ToList();

        return finalData;
    }

    public PayloadResponse Delete(string id)
    {
        try
        {
            var bus = _busRepository
                .GetConditional(o => o.Id == id);

            if (bus == null)
            {
                return new PayloadResponse()
                {
                    IsSuccess = false,
                    PayloadType = "Bus",
                    Message = "Bus not found"
                };
            }

            _busRepository.Delete(bus);
            _busRepository.SaveChanges();

            return new PayloadResponse()
            {
                IsSuccess = true,
                PayloadType = "Bus",
                Message = "Bus has been deleted successfully"
            };
        }
        catch (Exception ex)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                PayloadType = "Bus",
                Message = $"Bus deletion is failed! because {ex.Message}"
            };
        }
    }

    public PayloadResponse UpdateLocation(LocationUpdateDto locationData)
    {
        try
        {
            var bus = _busRepository.GetConditional(b => b.Id == locationData.BusId);

            if (bus == null)
            {
                return new PayloadResponse()
                {
                    IsSuccess = false,
                    PayloadType = "Bus",
                    Message = "Bus not found"
                };
            }

            _sessionService.UpdateSessionDistance(locationData, bus.PresentLatitude, bus.PresentLongitude);

            bus.PresentLatitude = locationData.Latitude;
            bus.PresentLongitude = locationData.Longitude;

            _busRepository.Update(bus);
            _busRepository.SaveChanges();

            return new PayloadResponse()
            {
                IsSuccess = true,
                PayloadType = "Bus",
                Message = "Bus location updated!"
            };
        }
        catch (Exception ex)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                PayloadType = "Bus",
                Message = $"Bus location update failed because {ex.Message}!"
            };
        }
    }

    public PayloadResponse GetAllForDropDown(string organizationId, string routeId)
    {
        try
        {
            var currentUser = _loggedInUserService.GetLoggedInUser();

            if (currentUser == null)
            {
                return new PayloadResponse()
                {
                    IsSuccess = false,
                    PayloadType = "Bus",
                    Message = "Current user not found"
                };
            }

            var allBus = _sessionRepository
                .GetAll()
                .Where(s => s.IsRunning)
                .Include(s => s.Bus)
                .Select(s => s.Bus);

            if (!string.IsNullOrEmpty(routeId)) 
            {
                allBus = allBus.Where(b => b.RouteId == routeId);
            }

            if (currentUser.IsSuperAdmin)
            {
                if (!string.IsNullOrEmpty(organizationId))
                {
                    allBus = allBus.Where(b => b.OrganizationId == organizationId);
                }

                var busData = allBus.Select(b => new ValueLabel()
                    {
                        Value = b.Id,
                        Label = b.BusNumber
                    })
                    .ToList();

                return new PayloadResponse()
                {
                    IsSuccess = true,
                    PayloadType = "Bus",
                    Content = busData,
                    Message = "Bus data for dropdown has been fetched successfully!"
                };
            }

            var data = allBus
                .Where(b => b.OrganizationId == currentUser.OrganizationId)
                .Select(b => new ValueLabel()
                {
                    Value = b.Id,
                    Label = b.BusNumber
                })
                .ToList();

            return new PayloadResponse()
            {
                IsSuccess = true,
                PayloadType = "Bus",
                Content = data,
                Message = "Bus data for dropdown has been fetched successfully!"
            };
        }
        catch (Exception ex)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                PayloadType = "Bus",
                Message = $"Bus data for dropdown fetch has been failed because {ex.Message}!"
            };
        }
    }

    public PayloadResponse GetAllBusMapData(string organizationId, string busId, string routId)
    {
        try
        {
            var currentUser = _loggedInUserService.GetLoggedInUser();

            if (currentUser == null)
            {
                return new PayloadResponse()
                {
                    IsSuccess = false,
                    PayloadType = "Bus",
                    Message = "Current user not found"
                };
            }

            var allBus = _sessionRepository
                .GetAll()
                .Where(s => s.IsRunning)
                .Include(s => s.Bus)
                .Include(s => s.Bus.Organization)
                .Include(s => s.Bus.Route)
                .Select(b => b.Bus);

            if (!string.IsNullOrEmpty(routId))
            {
                allBus = allBus.Where(b => b.RouteId == routId);
            }

            if (!string.IsNullOrEmpty(busId))
            {
                allBus = allBus.Where(b => b.Id == busId);
            }

            if (currentUser.IsSuperAdmin)
            {
                if (!string.IsNullOrEmpty(organizationId))
                {
                    allBus = allBus.Where(b => b.OrganizationId == organizationId);
                }

                var busData = allBus
                    .Select(b => new BusMapDataDto()
                    {
                        Id = b.Id,
                        BusNumber = b.BusNumber,
                        BusName = b.BusName,
                        OrganizationName = b.Organization.Name,
                        PresentLatitude = b.PresentLatitude,
                        PresentLongitude = b.PresentLongitude,
                        Route = $"{b.Route.TripStartPlace} - {b.Route.TripEndPlace}"
                    })
                    .ToList();

                foreach (var bus in busData)
                {
                    bus.RunningTrips = GetRunningTripsCountOnBus(bus.Id);
                }
                
                return new PayloadResponse()
                {
                    IsSuccess = true,
                    PayloadType = "Bus",
                    Content = busData,
                    Message = "Bus map data has been fetched successfully!"
                };
            }

            List<BusMapDataDto> data;

            if (!string.IsNullOrEmpty(organizationId))
            {
                data = allBus
                    .Where(b => b.OrganizationId == organizationId)
                    .Select(b => new BusMapDataDto()
                    {
                        Id = b.Id,
                        BusNumber = b.BusNumber,
                        BusName = b.BusName,
                        PresentLatitude = b.PresentLatitude,
                        PresentLongitude = b.PresentLongitude
                    })
                    .ToList();
            }
            else
            {
                data = allBus
                    .Where(b => b.OrganizationId == currentUser.OrganizationId)
                    .Select(b => new BusMapDataDto()
                    {
                        Id = b.Id,
                        BusNumber = b.BusNumber,
                        BusName = b.BusName,
                        PresentLatitude = b.PresentLatitude,
                        PresentLongitude = b.PresentLongitude
                    })
                    .ToList();
            }

            foreach (var bus in data)
            {
                bus.RunningTrips = GetRunningTripsCountOnBus(bus.Id);
            }

            return new PayloadResponse()
            {
                IsSuccess = true,
                PayloadType = "Bus",
                Content = data,
                Message = "Bus map data has been fetched successfully!"
            };
        }
        catch (Exception ex)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                PayloadType = "Bus",
                Message = $"Bus map data fetch has been failed because {ex.Message}!"
            };
        }
    }

    private int GetRunningTripsCountOnBus(string busId)
    {
        var query = $@"
                    select count(distinct case when t.IsRunning = 1 then t.Id end)
                    from Sessions s
                             left join Buses b on s.BusId = b.Id
                             left join Trips t on s.Id = t.SessionId
                    where b.Id = '{busId}'
                      and s.IsRunning = 1";

        var runningTripsCount = _baseRepository.Query<int>(query).FirstOrDefault();

        return runningTripsCount;
    }

    public PayloadResponse GetAllRunningBus()
    {
        var currentUser = _loggedInUserService.GetLoggedInUser();

        if (currentUser == null)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                PayloadType = "Bus",
                Message = "Current user not found"
            };
        }

        var allRunningBus = _sessionRepository
            .GetAll()
            .Include(s => s.Bus)
            .Include(s => s.Bus.Route)
            .Where(s => s.IsRunning && s.Bus.OrganizationId == currentUser.OrganizationId)
            .Select(s => s)
            .Distinct()
            .ToList();

        return new PayloadResponse()
        {
            IsSuccess = true,
            PayloadType = "Bus",
            Content = allRunningBus,
            Message = "All running buses"
        };
    }

    public PayloadResponse ActivateBus(BusActivationDto busActivation)
    {
        var bus = _busRepository
            .GetConditional(b => b.Id == busActivation.BusId);

        if (bus == null)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                PayloadType = "Bus",
                Message = "Bus not found"
            };
        }

        if(bus.IsActive)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                PayloadType = "Bus",
                Message = "This bus is already active."
            };
        }

        if (IfBusRouteInactive(bus.RouteId))
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                PayloadType = "Bus",
                Message = "This bus route is inactive. Please activate the route first."
            };
        }

        bus.IsActive = true;

        _busRepository.Update(bus);
        _busRepository.SaveChanges();

        return new PayloadResponse()
        {
            IsSuccess = true,
            PayloadType = "Bus",
            Message = "Bus has been activated successfully!"
        };
    }

    private bool IfBusRouteInactive(string routeId)
    {
        var route = _routeRepository
            .GetConditional(r => r.Id == routeId);

        if (route is null)
        {
            return true;
        }

        return !route.IsActive;
    }

    public PayloadResponse DeactivateBus(BusActivationDto busActivation)
    {
        var bus = _busRepository
            .GetConditional(b => b.Id == busActivation.BusId);

        if (bus == null)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                PayloadType = "Bus",
                Message = "Bus not found"
            };
        }
        
        var session = _sessionRepository
            .GetConditional(s => s.BusId == busActivation.BusId && s.IsRunning);

        if (session != null)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                PayloadType = "Bus",
                Message = "This bus is currently running. Please stop the session first."
            };
        }

        if (!bus.IsActive)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                PayloadType = "Bus",
                Message = "This bus is already deactivated."
            };
        }

        bus.IsActive = false;

        _busRepository.Update(bus);
        _busRepository.SaveChanges();

        return new PayloadResponse()
        {
            IsSuccess = true,
            PayloadType = "Bus",
            Message = "Bus has been deactivated successfully!"
        };
    }

    public PayloadResponse GetAllActiveBuses(BusDataFilter filter)
    {
        try
        {
            var currentUser = _loggedInUserService
                .GetLoggedInUser();

            if (currentUser == null)
            {
                return new PayloadResponse()
                {
                    IsSuccess = false,
                    PayloadType = "Bus",
                    Message = "Bus not found"
                };
            }

            var condition = new List<string> { " IsActive = 1 " };

            var extraCondition = $@"ORDER BY CreateTime desc
                                    OFFSET ({filter.PageNo} - 1) * {filter.PageSize} ROWS
                                    FETCH NEXT {filter.PageSize} ROWS ONLY";

            if (!currentUser.IsSuperAdmin)
            {
                if (string.IsNullOrEmpty(currentUser.OrganizationId))
                {
                    return new PayloadResponse()
                    {
                        IsSuccess = false,
                        PayloadType = "Bus",
                        Message = "Current User is not associated with any organization!"
                    };
                }

                filter.OrganizationId = currentUser.OrganizationId;
            }

            if (!string.IsNullOrEmpty(filter.SearchQuery))
            {
                condition.Add($" (BusNumber like '%{filter.SearchQuery}%' or BusName like '%{filter.SearchQuery}%') ");
            }

            if (!string.IsNullOrEmpty(filter.OrganizationId))
            {
                condition.Add($" OrganizationId = '{filter.OrganizationId}'");
            }

            var whereCondition = _commonService.GenerateWhereConditionFromConditionList(condition);

            var rowCount = _commonService.GetRowCountForData("Buses", whereCondition);

            var finalQueryData = _commonService.GetFinalData<Bus>("Buses", whereCondition, extraCondition);

            var busIds = finalQueryData.Select(q => q.Id).ToList();

            var busData = _busRepository.GetAll()
                .Where(u => busIds.Contains(u.Id))
                .Include(u => u.Organization)
                .Include(u => u.Route)
                .ToList();

            var processedData = GetProcessedData(busData);

            return new PayloadResponse()
            {
                IsSuccess = true,
                PayloadType = "Bus",
                Content = new { data = processedData, rowCount },
                Message = "Bus data fetch is successful"
            };
        }
        catch (Exception ex)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                PayloadType = "Bus",
                Message = $"Bus fetching is failed because {ex.Message}!"
            };
        }
    }

    private bool IfDuplicateBusNumber(string busNumber)
    {
        var bus = _busRepository.GetConditional(b => b.BusName == busNumber);

        return bus is not null;
    }
}
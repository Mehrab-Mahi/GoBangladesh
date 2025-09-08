using GoBangladesh.Application.DTOs.Route;
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

public class RouteService : IRouteService
{
    private readonly IRepository<Route> _routeRepository;
    private readonly IRepository<Bus> _busRepository;
    private readonly IRepository<Session> _sessionRepository;
    private readonly ILoggedInUserService _loggedInUserService;
    private readonly ICommonService _commonService;
    private readonly IBaseRepository _baseRepository;

    public RouteService(IRepository<Route> routeRepository,
        ILoggedInUserService loggedInUserService,
        ICommonService commonService, 
        IRepository<Bus> busRepository, 
        IRepository<Session> sessionRepository,
        IBaseRepository baseRepository)
    {
        _routeRepository = routeRepository;
        _loggedInUserService = loggedInUserService;
        _commonService = commonService;
        _busRepository = busRepository;
        _sessionRepository = sessionRepository;
        _baseRepository = baseRepository;
    }

    public PayloadResponse RouteInsert(RouteCreateRequest model)
    {
        try
        {
            var currentUser = _loggedInUserService.GetLoggedInUser();

            if (currentUser == null)
            {
                return new PayloadResponse()
                {
                    IsSuccess = false,
                    PayloadType = "Route",
                    Message = "User not found!"
                };
            }

            var route = new Route()
            {
                TripStartPlace = model.TripStartPlace,
                TripEndPlace = model.TripEndPlace,
                OrganizationId = string.IsNullOrEmpty(model.OrganizationId) ? currentUser.OrganizationId : model.OrganizationId,
                PerKmFare = model.PerKmFare,
                BaseFare = model.BaseFare,
                MinimumBalance = model.MinimumBalance,
                PenaltyAmount = model.PenaltyAmount
            };

            _routeRepository.Insert(route);
            _routeRepository.SaveChanges();

            return new PayloadResponse()
            {
                IsSuccess = true,
                PayloadType = "Route",
                Message = "Route has been inserted successfully!"
            };
        }
        catch (Exception ex)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                PayloadType = "Route",
                Message = $"Route creation is failed because {ex.Message}!"
            };
        }
    }

    public PayloadResponse RouteUpdate(RouteUpdateRequest model)
    {
        try{
            var route = _routeRepository
                .GetConditional(b => b.Id == model.Id);

            if (route == null)
            {
                return new PayloadResponse()
                {
                    IsSuccess = false,
                    PayloadType = "Route",
                    Message = "Route not found!"
                };
            }

            route.TripStartPlace = model.TripStartPlace;
            route.TripEndPlace = model.TripEndPlace;
            route.PerKmFare = model.PerKmFare;
            route.BaseFare = model.BaseFare;
            route.MinimumBalance = model.MinimumBalance;
            route.PenaltyAmount = model.PenaltyAmount;
            route.OrganizationId = model.OrganizationId;

            _routeRepository.Update(route);
            _routeRepository.SaveChanges();

            return new PayloadResponse()
            {
                IsSuccess = true,
                PayloadType = "Route",
                Message = "Route has been updated successfully!"
            };
        }
        catch (Exception ex)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                PayloadType = "Route",
                Message = $"Route update is failed because {ex.Message}!"
            };
        }
    }

    public PayloadResponse GetById(string id)
    {
        try
        {
            var route = _routeRepository.GetAll()
                .Where(r => r.Id == id)
                .Include(r => r.Organization)
                .FirstOrDefault();

            if (route == null)
            {
                return new PayloadResponse()
                {
                    IsSuccess = false,
                    PayloadType = "Route",
                    Message = "Route not found"
                };
            }

            return new PayloadResponse()
            {
                IsSuccess = true,
                PayloadType = "Route",
                Content = route,
                Message = "Route has been found"
            };
        }
        catch (Exception ex)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                PayloadType = "Route",
                Message = $"Route fetching has been failed because {ex.Message}"
            };
        }
    }

    public PayloadResponse GetAll(RouteDataFilter filter)
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
                    PayloadType = "Route",
                    Message = "Route not found"
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
                        PayloadType = "Route",
                        Message = "Current User is not associated with any organization!"
                    };
                }

                filter.OrganizationId = currentUser.OrganizationId;
            }

            if (!string.IsNullOrEmpty(filter.SearchQuery))
            {
                condition.Add($" (TripStartPlace like '%{filter.SearchQuery}%' or TripEndPlace like '%{filter.SearchQuery}%') ");
            }

            if (!string.IsNullOrEmpty(filter.OrganizationId))
            {
                condition.Add($" OrganizationId = '{filter.OrganizationId}'");
            }

            var whereCondition = _commonService.GenerateWhereConditionFromConditionList(condition);

            var rowCount = _commonService.GetRowCountForData("Routes", whereCondition);

            var finalQueryData = _commonService.GetFinalData<Bus>("Routes", whereCondition, extraCondition);

            var routeIds = finalQueryData.Select(q => q.Id).ToList();

            var routeData = _routeRepository.GetAll()
                .Where(u => routeIds.Contains(u.Id))
                .Include(r => r.Organization)
                .ToList();

            return new PayloadResponse()
            {
                IsSuccess = true,
                PayloadType = "Route",
                Content = new { data = routeData, rowCount },
                Message = "Route data fetch is successful"
            };
        }
        catch (Exception ex)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                PayloadType = "Route",
                Message = $"Route fetching is failed because {ex.Message}!"
            };
        }
    }

    public PayloadResponse Delete(string id)
    {
        try
        {
            var route = _routeRepository
                .GetConditional(o => o.Id == id);

            if (route == null)
            {
                return new PayloadResponse()
                {
                    IsSuccess = false,
                    PayloadType = "Route",
                    Message = "Route not found"
                };
            }

            _routeRepository.Delete(route);
            _routeRepository.SaveChanges();

            return new PayloadResponse()
            {
                IsSuccess = true,
                PayloadType = "Route",
                Message = "Route has been deleted successfully"
            };
        }
        catch (Exception ex)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                PayloadType = "Route",
                Message = $"Route deletion is failed! because {ex.Message}"
            };
        }
    }

    public PayloadResponse GetRouteDropdown(string organizationId)
    {
        try
        {
            var currentUser = _loggedInUserService
                .GetLoggedInUser();

            var allRoute = _routeRepository.GetAll().Where(r => r.IsActive);

            if (currentUser.IsSuperAdmin)
            {
                if (!string.IsNullOrEmpty(organizationId))
                {
                    allRoute = allRoute.Where(r => r.OrganizationId == organizationId);
                }
            }
            else
            {
                allRoute = allRoute.Where(r => r.OrganizationId == currentUser.OrganizationId);
            }

            var routeData = allRoute.Select(r => new ValueLabel()
            {
                Value = r.Id,
                Label = $"{r.TripStartPlace} - {r.TripEndPlace}"
            }).ToList();

            return new PayloadResponse()
            {
                IsSuccess = true,
                Content = routeData,
                PayloadType = "Route",
                Message = "Route data has been fetching successfully!"
            };
        }
        catch (Exception ex)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                PayloadType = "Route",
                Message = $"Route data fetching has been failed because {ex.Message}!"
            };
        }
    }

    public PayloadResponse RouteDropdownForMobile(string organizationId)
    {
        try
        {
            if (string.IsNullOrEmpty(organizationId))
            {
                return new PayloadResponse()
                {
                    IsSuccess = false,
                    PayloadType = "Route",
                    Message = "Need organization!"
                };
            }

            var runningBusRoutes = _baseRepository.Query<string>($@"
                                select distinct b.RouteId
                                from Sessions s
                                         left join Buses b on s.BusId = b.Id
                                where s.IsRunning = 1 and b.OrganizationId = '{organizationId}'");

            var allRoute = _routeRepository
                .GetAll()
                .Where(r => r.OrganizationId == organizationId &&
                            r.IsActive &&
                            runningBusRoutes.Contains(r.Id));

            var routeData = allRoute.Select(r => new ValueLabel()
            {
                Value = r.Id,
                Label = $"{r.TripStartPlace} - {r.TripEndPlace}"
            }).ToList();

            return new PayloadResponse()
            {
                IsSuccess = true,
                Content = routeData,
                PayloadType = "Route",
                Message = "Route data has been fetching successfully!"
            };
        }
        catch (Exception ex)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                PayloadType = "Route",
                Message = $"Route data fetching has been failed because {ex.Message}!"
            };
        }
    }

    public PayloadResponse ActivateRoute(RouteActivationDto routeActivation)
    {
        var route = _routeRepository
            .GetConditional(r => r.Id == routeActivation.RouteId);

        if (route == null)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                PayloadType = "Route",
                Message = "Route not found!"
            };
        }

        if (route.IsActive)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                PayloadType = "Route",
                Message = "Route is already active!"
            };
        }

        route.IsActive = true;

        _routeRepository.Update(route);
        _routeRepository.SaveChanges();

        return new PayloadResponse()
        {
            IsSuccess = true,
            PayloadType = "Route",
            Message = "Route has been activated successfully!"
        };
    }

    public PayloadResponse DeactivateRoute(RouteActivationDto routeActivation)
    {
        var route = _routeRepository
            .GetConditional(r => r.Id == routeActivation.RouteId);

        if (route == null)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                PayloadType = "Route",
                Message = "Route not found!"
            };
        }

        if(!route.IsActive)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                PayloadType = "Route",
                Message = "Route is already deactivated!"
            };
        }

        if (IfSessionRunningOnThisRoute(routeActivation.RouteId))
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                PayloadType = "Route",
                Message = "Route cannot be deactivated because there are active sessions on this route!"
            };
        }

        route.IsActive = false;
        _routeRepository.Update(route);
        _routeRepository.SaveChanges();

        DeactivateBusesInThisRoute(routeActivation.RouteId);

        return new PayloadResponse()
        {
            IsSuccess = true,
            PayloadType = "Route",
            Message = "Route has been deactivated successfully!"
        };
    }

    private void DeactivateBusesInThisRoute(string routeId)
    {
        var busList = _busRepository.GetAll()
            .Where(b => b.RouteId == routeId && b.IsActive)
            .ToList();

        foreach (var bus in busList)
        {
            bus.IsActive = false;
            _busRepository.Update(bus);
        }
        _busRepository.SaveChanges();
    }

    private bool IfSessionRunningOnThisRoute(string routeId)
    {
        var busIdList = _busRepository.GetAll()
            .Where(b => b.RouteId == routeId && b.IsActive)
            .Select(b => b.Id)
            .ToList();

        var runningSession = _sessionRepository.GetAll()
            .Where(s => s.IsRunning && busIdList.Contains(s.BusId))
            .ToList();

        return runningSession.Any();
    }
}
using GoBangladesh.Application.DTOs;
using GoBangladesh.Application.DTOs.Route;
using GoBangladesh.Application.Interfaces;
using GoBangladesh.Application.ViewModels;
using GoBangladesh.Application.ViewModels.Transaction;
using GoBangladesh.Domain.Entities;
using GoBangladesh.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;

namespace GoBangladesh.Application.Services;

public class RouteService : IRouteService
{
    private readonly IRepository<Route> _routeRepository;
    private readonly IRepository<Bus> _busRepository;
    private readonly IRepository<Session> _sessionRepository;
    private readonly ILoggedInUserService _loggedInUserService;
    private readonly ICommonService _commonService;
    private readonly IBaseRepository _baseRepository;
    private readonly IRepository<Stoppage> _stoppageRepository;
    private readonly HttpClient _httpClient;
    private readonly DistanceMatrixApiSettings _distanceMatrixApiSettings;

    public RouteService(IRepository<Route> routeRepository,
        ILoggedInUserService loggedInUserService,
        ICommonService commonService, 
        IRepository<Bus> busRepository, 
        IRepository<Session> sessionRepository,
        IBaseRepository baseRepository,
        IRepository<Stoppage> stoppageRepository,
        HttpClient httpClient,
        IOptions<DistanceMatrixApiSettings> distanceMatrixApiSettings)
    {
        _routeRepository = routeRepository;
        _loggedInUserService = loggedInUserService;
        _commonService = commonService;
        _busRepository = busRepository;
        _sessionRepository = sessionRepository;
        _baseRepository = baseRepository;
        _stoppageRepository = stoppageRepository;
        _httpClient = httpClient;
        _distanceMatrixApiSettings = distanceMatrixApiSettings.Value;
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
                TripStartLatitude = model.TripStartLatitude,
                TripStartLongitude = model.TripStartLongitude,
                TripEndPlace = model.TripEndPlace,
                TripEndLatitude = model.TripEndLatitude,
                TripEndLongitude = model.TripEndLongitude,
                OrganizationId = string.IsNullOrEmpty(model.OrganizationId) ? currentUser.OrganizationId : model.OrganizationId,
                PerKmFare = model.PerKmFare,
                BaseFare = model.BaseFare,
                MinimumBalance = model.MinimumBalance,
                PenaltyAmount = model.PenaltyAmount,
                RoutePath = GetRoutePath(model.StoppageList)
            };

            _routeRepository.Insert(route);
            _routeRepository.SaveChanges();

            if (model.StoppageList.Any())
            {
                UpdateRouteStoppageList(route.Id, model.StoppageList);
            }

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

    private LineString GetRoutePath(List<StoppageDto> modelStoppageList)
    {
        var coordinates = string.Join(";", modelStoppageList.Select(p => $"{p.Longitude},{p.Latitude}"));

        var url = $"{_distanceMatrixApiSettings.BaseUrl}{coordinates}?overview=full&geometries=geojson";

        var response = _httpClient.GetAsync(url).Result;
        var json = response.Content.ReadAsStringAsync().Result;
        using var doc = JsonDocument.Parse(json);

        var coords = doc.RootElement
            .GetProperty("routes")[0]
            .GetProperty("geometry")
            .GetProperty("coordinates")
            .EnumerateArray()
            .Select(c => new Coordinate(c[0].GetDouble(), c[1].GetDouble()))
            .ToArray();

        var geometryFactory = NetTopologySuite.NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);

        var lineString = geometryFactory.CreateLineString(coords);

        return lineString;
    }

    private void UpdateRouteStoppageList(string routeId, List<StoppageDto> stoppageList)
    {
        var existingStoppageList = _stoppageRepository.GetAll()
            .Where(s => s.RouteId == routeId)
            .ToList();

        _stoppageRepository.Delete(existingStoppageList);

        foreach (var stoppage in stoppageList)
        {
            _stoppageRepository.Insert(new Stoppage()
            {
                RouteId = routeId,
                Name = stoppage.Name,
                SortOrder = stoppage.SortOrder,
                Latitude = stoppage.Latitude,
                Longitude = stoppage.Longitude
            });
        }

        _stoppageRepository.SaveChanges();
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
            route.TripStartLatitude = model.TripStartLatitude;
            route.TripStartLongitude = model.TripStartLongitude;
            route.TripEndPlace = model.TripEndPlace;
            route.TripEndLatitude = model.TripEndLatitude;
            route.TripEndLongitude = model.TripEndLongitude;
            route.PerKmFare = model.PerKmFare;
            route.BaseFare = model.BaseFare;
            route.MinimumBalance = model.MinimumBalance;
            route.PenaltyAmount = model.PenaltyAmount;
            route.OrganizationId = model.OrganizationId;
            route.RoutePath = GetRoutePath(model.StoppageList);

            _routeRepository.Update(route);
            _routeRepository.SaveChanges();

            UpdateRouteStoppageList(route.Id, model.StoppageList);

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

            route.RoutePath = null;

            var stoppageList = _stoppageRepository.GetAll()
                .Where(s => s.RouteId == route.Id)
                .OrderBy(s => s.SortOrder)
                .ToList();

            return new PayloadResponse()
            {
                IsSuccess = true,
                PayloadType = "Route",
                Content = new { route, stoppageList},
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

            foreach (var route in routeData)
            {
                route.RoutePath = null;
            }

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
            List<string> runningBusRoutes;

            var allRoute = _routeRepository
                .GetAll()
                .Where(r => r.IsActive);

            if (string.IsNullOrEmpty(organizationId))
            {
                runningBusRoutes = _baseRepository.Query<string>($@"
                                select distinct b.RouteId
                                from Sessions s
                                         left join Buses b on s.BusId = b.Id
                                where s.IsRunning = 1");

                allRoute = allRoute.Where(r => runningBusRoutes.Contains(r.Id));
            }
            else
            {
                runningBusRoutes = _baseRepository.Query<string>($@"
                                select distinct b.RouteId
                                from Sessions s
                                         left join Buses b on s.BusId = b.Id
                                where s.IsRunning = 1 and b.OrganizationId = '{organizationId}'");

                allRoute = allRoute
                    .Where(r => r.OrganizationId == organizationId &&
                                               runningBusRoutes.Contains(r.Id));
            }

            var routeData = allRoute
                .Select(r => new ValueLabel()
                {
                    Value = r.Id,
                    Label = $"{r.TripStartPlace} - {r.TripEndPlace}"
                })
                .ToList();

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

    public PayloadResponse GetStoppagesByRouteId(string routeId)
    {
        try
        {
            var route = _routeRepository
                .GetAll()
                .FirstOrDefault(r => r.Id == routeId);

            if (route == null)
            {
                return new PayloadResponse()
                {
                    IsSuccess = false,
                    PayloadType = "Route",
                    Message = "Route not found!"
                };
            }

            var stoppageList = _stoppageRepository.GetAll()
                .Where(s => s.RouteId == routeId)
                .OrderBy(s => s.SortOrder)
                .ToList();

            if (!stoppageList.Any())
            {
                return new PayloadResponse()
                {
                    IsSuccess = false,
                    PayloadType = "Route",
                    Message = "No stoppage found for this route!"
                };
            }

            var stoppage = string.Join("-", stoppageList.Select(s => s.Name));

            return new PayloadResponse()
            {
                IsSuccess = true,
                PayloadType = "Route",
                Content = stoppage,
                Message = "Stoppage list has been found"
            };
        }
        catch (Exception ex)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                PayloadType = "Route",
                Message = $"Stoppage list fetching has been failed because {ex.Message}"
            };
        }
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
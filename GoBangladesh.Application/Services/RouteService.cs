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
    private readonly ILoggedInUserService _loggedInUserService;
    private readonly ICommonService _commonService;

    public RouteService(IRepository<Route> routeRepository,
        ILoggedInUserService loggedInUserService,
        ICommonService commonService)
    {
        _routeRepository = routeRepository;
        _loggedInUserService = loggedInUserService;
        _commonService = commonService;
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

            var allRoute = _routeRepository.GetAll();

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
}
using GoBangladesh.Application.DTOs.Route;
using GoBangladesh.Application.ViewModels;

namespace GoBangladesh.Application.Interfaces;

public interface IRouteService
{
    PayloadResponse RouteInsert(RouteCreateRequest model);
    PayloadResponse RouteUpdate(RouteUpdateRequest model);
    PayloadResponse GetById(string id);
    PayloadResponse GetAll(RouteDataFilter filter);
    PayloadResponse Delete(string id);
    PayloadResponse GetRouteDropdown(string organizationId);
}
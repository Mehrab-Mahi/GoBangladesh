using System.Collections.Generic;
using GoBangladesh.Application.ViewModels;
using GoBangladesh.Domain.Entities;

namespace GoBangladesh.Application.Interfaces
{
    public interface ILocationService
    {
        List<Location> GetLocationByParentId(string parentId);
        PayloadResponse Create(LocationVm locationData);
        PayloadResponse Update(LocationVm locationData);
        PayloadResponse Delete(string id);
        Location Get(string id);
    }
}
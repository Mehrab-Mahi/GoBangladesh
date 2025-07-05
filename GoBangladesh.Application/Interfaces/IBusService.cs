using GoBangladesh.Application.DTOs.Bus;
using GoBangladesh.Application.ViewModels;

namespace GoBangladesh.Application.Interfaces;

public interface IBusService
{
    PayloadResponse BusInsert(BusCreateRequest model);
    PayloadResponse BusUpdate(BusUpdateRequest model);
    PayloadResponse GetById(string id);
    PayloadResponse GetAll(BusDataFilter filter);
    PayloadResponse Delete(string id);
}
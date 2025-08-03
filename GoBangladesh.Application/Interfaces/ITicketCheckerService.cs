using GoBangladesh.Application.DTOs.TicketChecker;
using GoBangladesh.Application.ViewModels;

namespace GoBangladesh.Application.Interfaces;

public interface ITicketCheckerService
{
    PayloadResponse TicketCheckerCreate(TicketCheckerCreateRequest model);
    PayloadResponse TicketCheckerUpdate(TicketCheckerUpdateRequest model);
    PayloadResponse GetById(string id);
    PayloadResponse GetAll(TicketCheckerDataFilter filter);
    PayloadResponse Delete(string id);
}
using GoBangladesh.Application.DTOs.TicketChecker;
using GoBangladesh.Application.ViewModels;

namespace GoBangladesh.Application.Interfaces;

public interface ITicketExaminerService
{
    PayloadResponse TicketCheckerCreate(TicketCheckerCreateRequest model);
    PayloadResponse TicketCheckerUpdate(TicketCheckerUpdateRequest model);
    PayloadResponse GetById(string id);
    PayloadResponse GetAll(TicketCheckerDataFilter filter);
    PayloadResponse Delete(string id);
    PayloadResponse GetTripInfoByCard(string sessionId, string cardNumber);
    PayloadResponse StartPenaltyTrip(TicketExaminerPenaltyTripRequest tapRequest);
}
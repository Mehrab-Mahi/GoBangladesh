using GoBangladesh.Application.DTOs.Transaction;
using GoBangladesh.Application.ViewModels;
using GoBangladesh.Domain.Entities;

namespace GoBangladesh.Application.Interfaces;

public interface ITransactionService
{
    PayloadResponse Recharge(RechargeRequest model);
    PayloadResponse Tap(TapRequest tap);
    PayloadResponse ForceTripStop(ForceStopTripDto forceStop);
    void ForceTripStopLinkedWIthSession(Trip trip, Domain.Entities.Route route, string latitude, string longitude, string tapOutStatus);
}
using GoBangladesh.Application.DTOs.Passenger;
using GoBangladesh.Application.ViewModels;

namespace GoBangladesh.Application.Interfaces;

public interface IPassengerService
{
    PayloadResponse PassengerInsert(PassengerCreateRequest model);
    PayloadResponse UpdatePassenger(PassengerUpdateRequest model);
    PayloadResponse GetPassengerById(string id);
    PayloadResponse UpdateCardNumber(CardNumberUpdateRequest model);
    PayloadResponse GetAll(int pageNo, int pageSize);
    PayloadResponse Delete(string id);
}
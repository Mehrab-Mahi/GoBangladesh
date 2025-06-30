using GoBangladesh.Application.DTOs.Transaction;
using GoBangladesh.Application.ViewModels;

namespace GoBangladesh.Application.Interfaces;

public interface ITransactionService
{
    PayloadResponse Recharge(RechargeRequest model);
    PayloadResponse BusFare(BusFareRequest model);
    PayloadResponse IsMinimumBalanceAvailable(string cardNumber);
}
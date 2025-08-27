using System;
using System.Threading.Tasks;

namespace GoBangladesh.Application.Interfaces;

public interface IInvoiceService
{
    void GenerateWeeklyInvoices(DateTimeOffset toDate, DateTimeOffset localTime);
}
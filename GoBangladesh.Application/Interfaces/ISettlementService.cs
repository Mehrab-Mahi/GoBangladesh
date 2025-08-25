using GoBangladesh.Application.DTOs.Settlement;
using GoBangladesh.Application.ViewModels;

namespace GoBangladesh.Application.Interfaces;

public interface ISettlementService
{
    PayloadResponse GetSettlementPayableSummaryData(SettlementDataFilter filter);
    PayloadResponse GetSettlementReceivableSummaryData(SettlementDataFilter filter);
    PayloadResponse GetSettlementDetailData(SettlementDataFilter filter);
    PayloadResponse GetPayableUnsettledInvoices(string organizationId, int pageNo, int pageSize);
    PayloadResponse GetPayableSettledInvoices(string organizationId, int pageNo, int pageSize);
    PayloadResponse GetInvoiceWiseTransactions(string invoiceNumber, int pageNo, int pageSize);
    PayloadResponse GetReceivableUnsettledInvoices(string organizationId, int pageNo, int pageSize);
    PayloadResponse GetReceivableSettledInvoices(string organizationId, int pageNo, int pageSize);
}
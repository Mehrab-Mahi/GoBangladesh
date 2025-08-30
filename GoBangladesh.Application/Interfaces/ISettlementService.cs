using GoBangladesh.Application.DTOs.Settlement;
using GoBangladesh.Application.ViewModels;

namespace GoBangladesh.Application.Interfaces;

public interface ISettlementService
{
    PayloadResponse GetSettlementPayableSummaryData(SettlementDataFilter filter);
    PayloadResponse GetSettlementReceivableSummaryData(SettlementDataFilter filter);
    PayloadResponse GetSettlementDetailData(SettlementDataFilter filter);
    PayloadResponse GetPayableUnsettledInvoices(SettlementFilter filter);
    PayloadResponse GetPayableInReviewInvoices(SettlementFilter filter);
    PayloadResponse GetPayableSettledInvoices(SettlementFilter filter);
    PayloadResponse GetReceivableUnsettledInvoices(SettlementFilter filter);
    PayloadResponse GetReceivableInReviewInvoices(SettlementFilter filter);
    PayloadResponse GetReceivableSettledInvoices(SettlementFilter filter);
    PayloadResponse GetInvoiceWiseTransactions(string invoiceNumber, int pageNo, int pageSize);
    PayloadResponse Payment(InvoiceWisePaymentDto payment);
    PayloadResponse VerifyPayment(PaymentVerificationDto verification);
    PayloadResponse GetInvoiceWisePayments(string invoiceNumber);
}
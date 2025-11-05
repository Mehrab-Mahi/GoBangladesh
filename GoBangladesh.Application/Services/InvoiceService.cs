using GoBangladesh.Application.DTOs.Settlement;
using GoBangladesh.Application.Interfaces;
using GoBangladesh.Application.Util;
using GoBangladesh.Domain.Entities;
using GoBangladesh.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using GoBangladesh.Application.DTOs.Notification;

namespace GoBangladesh.Application.Services;

public class InvoiceService : IInvoiceService
{
    private readonly IRepository<Organization> _organizationRepository;
    private readonly IRepository<Invoice> _invoiceRepository;
    private readonly IBaseRepository _baseRepository;
    private readonly IRepository<User> _userRepository; 
    private readonly INotificationService _notificationService;

    public InvoiceService(IRepository<Organization> organizationRepository,
        IRepository<Invoice> invoiceRepository,
        IBaseRepository baseRepository,
        IRepository<User> userRepository, 
        INotificationService notificationService)
    {
        _organizationRepository = organizationRepository;
        _invoiceRepository = invoiceRepository;
        _baseRepository = baseRepository;
        _userRepository = userRepository;
        _notificationService = notificationService;
    }

    public void GenerateWeeklyInvoices(DateTimeOffset date, DateTimeOffset localTime)
    {
        var allSettlementData = GetAllOrganizationEligibleForInvoice(date);

        var allOrganizationList = _organizationRepository.GetAll().ToList();

        foreach (var settlement in allSettlementData)
        {
            var senderOrganization = allOrganizationList.FirstOrDefault(o => o.Id == settlement.SenderOrganizationId);
            var receiverOrganization = allOrganizationList.FirstOrDefault(o => o.Id == settlement.ReceiverOrganizationId);

            var invoiceNumber = GetNewInvoiceNumber(senderOrganization, date);

            _invoiceRepository.Insert(new Invoice()
            {
                InvoiceNumber = invoiceNumber,
                FromOrganizationId = settlement.SenderOrganizationId,
                ToOrganizationId = settlement.ReceiverOrganizationId,
                FromDate = localTime.AddDays(-6).Date,
                ToDate = localTime.Date,
                Amount = settlement.TotalAmount,
                Status = InvoiceStatus.Unsettled
            });

            UpdateSettlementTransactionDataForNewlyGeneratedInvoice(invoiceNumber,
                settlement.SenderOrganizationId,
                settlement.ReceiverOrganizationId,
                date);

            SendNotificationToTheOrganizationAdmins(invoiceNumber, settlement.TotalAmount, senderOrganization, receiverOrganization);
        }

        _invoiceRepository.SaveChanges();
    }

    private void SendNotificationToTheOrganizationAdmins(string invoiceNumber, decimal totalAmount, Organization senderOrganization, Organization receiverOrganization)
    {
        var senderAdmins = _userRepository
            .GetAll()
            .Where(u => u.OrganizationId == senderOrganization.Id && u.UserType == UserTypes.Admin)
            .ToList();
        var receiverAdmins = _userRepository
            .GetAll()
            .Where(u => u.OrganizationId == receiverOrganization.Id && u.UserType == UserTypes.Admin)
            .ToList();
        
        foreach (var admin in senderAdmins)
        {
            _notificationService.InsertEventNotification(new EventNotificationCreateRequest()
            {
                UserId = admin.Id,
                Title = "New Payable Invoice Generated",
                Message = $"A new payable invoice - {invoiceNumber} of amount {totalAmount} has been generated which needs to pay to {receiverOrganization.Name}"
            });
        }
        foreach (var admin in receiverAdmins)
        {
            _notificationService.InsertEventNotification(new EventNotificationCreateRequest()
            {
                UserId = admin.Id,
                Title = "New Receivable Invoice Generated",
                Message = $"A new receivable invoice - {invoiceNumber} of amount {totalAmount} has been generated which will be received from {senderOrganization.Name}"
            });
        }
    }

    private void UpdateSettlementTransactionDataForNewlyGeneratedInvoice(string invoiceNumber, string fromOrganizationId, string toOrganizationId, DateTimeOffset date)
    {
        var query = $@"UPDATE OrganizationSettlement
                    SET InvoiceNumber = '{invoiceNumber}',
                        Status = '{SettlementStatus.Unsettled}'
                    where FromOrganizationId = '{fromOrganizationId}'
                      and ToOrganizationId = '{toOrganizationId}'
                      and CreateTime <= '{date}'
                      and InvoiceNumber is null;";
        
        _baseRepository.ExecuteQuery(query);
    }

    private string GetNewInvoiceNumber(Organization organization, DateTimeOffset date)
    {
        var invoiceCount = _invoiceRepository
            .GetAll()
            .Count(i => i.FromOrganizationId == organization.Id 
                        && i.CreateTime.Year == date.Year 
                        && i.CreateTime.Month == date.Month);
        return $"{organization.Code}-{date:yyyy-MM}-{invoiceCount + 1:D3}";
    }

    private List<SettlementSummaryData> GetAllOrganizationEligibleForInvoice(DateTimeOffset date)
    {
        var query = $@"SELECT os.ToOrganizationId                                                  as ReceiverOrganizationId,
                           oo.Name                                                                 AS ReceiverOrganization,
                           os.FromOrganizationId                                                   as SenderOrganizationId,
                           o.Name                                                                  AS SenderOrganization,
                           SUM(CASE WHEN os.TransactionType = 'BusFare' THEN os.Amount ELSE 0 END) AS BusFareAmount,
                           SUM(CASE WHEN os.TransactionType = 'Return' THEN os.Amount ELSE 0 END)  AS ReturnAmount,
                           SUM(CASE WHEN os.TransactionType = 'Due' THEN os.Amount ELSE 0 END)     AS DueAmount,
                           SUM(CASE WHEN os.Status = 'In Review' THEN os.Amount ELSE 0 END)        AS InReviewAmount,
                           SUM(Amount)                                                             AS TotalAmount
                    FROM OrganizationSettlement os
                             left join Organizations o on os.FromOrganizationId = o.Id
                             left join Organizations oo on os.ToOrganizationId = oo.Id
                    where os.CreateTime <= '{date}'
                      and os.InvoiceNumber is null
                    GROUP BY o.Name, oo.Name, os.ToOrganizationId, os.FromOrganizationId order by TotalAmount";

        var data = _baseRepository.Query<SettlementSummaryData>(query);

        return data;
    }
}
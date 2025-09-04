using GoBangladesh.Application.DTOs.Settlement;
using GoBangladesh.Application.Interfaces;
using GoBangladesh.Application.Util;
using GoBangladesh.Domain.Entities;
using GoBangladesh.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GoBangladesh.Application.Services;

public class InvoiceService : IInvoiceService
{
    private readonly IRepository<Organization> _organizationRepository;
    private readonly IRepository<Invoice> _invoiceRepository;
    private readonly IBaseRepository _baseRepository;

    public InvoiceService(IRepository<Organization> organizationRepository,
        IRepository<Invoice> invoiceRepository,
        IBaseRepository baseRepository)
    {
        _organizationRepository = organizationRepository;
        _invoiceRepository = invoiceRepository;
        _baseRepository = baseRepository;
    }

    public void GenerateWeeklyInvoices(DateTimeOffset date, DateTimeOffset localTime)
    {
        var allSettlementData = GetAllOrganizationEligibleForInvoice(date);

        var allOrganizationList = _organizationRepository.GetAll().ToList();

        foreach (var settlement in allSettlementData)
        {
            var organization = allOrganizationList.FirstOrDefault(o => o.Id == settlement.SenderOrganizationId);

            var invoiceNumber = GetNewInvoiceNumber(organization, date);

            _invoiceRepository.Insert(new Invoice()
            {
                InvoiceNumber = invoiceNumber,
                FromOrganizationId = settlement.SenderOrganizationId,
                ToOrganizationId = settlement.ReceiverOrganizationId,
                FromDate = localTime.AddDays(-6).Date,
                ToDate = localTime.Date,
                Amount = Math.Ceiling(settlement.TotalAmount),
                Status = InvoiceStatus.Unsettled
            });

            UpdateSettlementTransactionDataForNewlyGeneratedInvoice(invoiceNumber,
                settlement.SenderOrganizationId,
                settlement.ReceiverOrganizationId,
                date);
        }

        _invoiceRepository.SaveChanges();
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
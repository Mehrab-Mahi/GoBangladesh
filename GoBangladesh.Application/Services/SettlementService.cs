using System;
using System.Collections.Generic;
using System.Linq;
using GoBangladesh.Application.DTOs;
using GoBangladesh.Application.DTOs.Settlement;
using GoBangladesh.Application.Interfaces;
using GoBangladesh.Application.Util;
using GoBangladesh.Application.ViewModels;
using GoBangladesh.Domain.Entities;
using GoBangladesh.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GoBangladesh.Application.Services;

public class SettlementService : ISettlementService
{
    private readonly IBaseRepository _baseRepository;
    private readonly ILoggedInUserService _loggedInUserService;
    private readonly ICommonService _commonService;
    private readonly IRepository<Invoice> _invoiceRepository;
    private readonly IRepository<InvoicePayment> _invoicePaymentRepository;

    public SettlementService(IBaseRepository baseRepository,
        ILoggedInUserService loggedInUserService,
        ICommonService commonService,
        IRepository<Invoice> invoiceRepository,
        IRepository<InvoicePayment> invoicePaymentRepository)
    {
        _baseRepository = baseRepository;
        _loggedInUserService = loggedInUserService;
        _commonService = commonService;
        _invoiceRepository = invoiceRepository;
        _invoicePaymentRepository = invoicePaymentRepository;
    }

    public PayloadResponse GetSettlementPayableSummaryData(SettlementDataFilter filter)
    {
        try
        {
            var currentUser = _loggedInUserService.GetLoggedInUser();

            var groupByCondition = "GROUP BY o.Name, oo.Name, os.ToOrganizationId, os.FromOrganizationId";

            var extraCondition = $@"order by TotalAmount desc
                                OFFSET ({filter.PageNo} - 1) * {filter.PageSize} ROWS
                                FETCH NEXT {filter.PageSize} ROWS ONLY";

            var condition = new List<string>();

            if (string.IsNullOrEmpty(filter.FromOrganizationId))
            {
                if (!currentUser.IsSuperAdmin)
                {
                    condition.Add($" FromOrganizationId = '{currentUser.OrganizationId}' ");
                }
            }
            else
            {
                condition.Add($" FromOrganizationId = '{filter.FromOrganizationId}' ");
            }

            if (!string.IsNullOrEmpty(filter.ToOrganizationId))
            {
                condition.Add($" ToOrganizationId = '{filter.ToOrganizationId}' ");
            }

            if (filter.StartDate != null || filter.EndDate != null)
            {
                var dateTimeFilter = _commonService.GetDateTimeFilterData(filter.StartDate, filter.EndDate);

                condition.Add($" (os.CreateTime between '{dateTimeFilter.StartDate}' and '{dateTimeFilter.EndDate}') ");
            }

            var whereCondition = _commonService.GenerateWhereConditionFromConditionList(condition);

            var query = GetSummaryDataQuery();

            var rowCount = GetSummaryRowCountData(query, groupByCondition, whereCondition);

            var data = GetSummaryData(query, whereCondition, groupByCondition,
                extraCondition);

            var dropDownData = GetDropDownDataForPayableSummaryData(whereCondition, groupByCondition);

            return new PayloadResponse()
            {
                IsSuccess = true,
                Content = new { dropDownData, data, rowCount },
                Message = "Data fetched successfully"
            };
        }
        catch (Exception ex)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                Content = null,
                Message = ex.Message
            };
        }
    }

    private List<ValueLabel> GetDropDownDataForPayableSummaryData(string whereCondition, string groupByCondition)
    {
        var query =
            $@"SELECT distinct os.ToOrganizationId                       as Value,
                       oo.Name                                           AS Label
                FROM OrganizationSettlement os
                         left join Organizations o on os.FromOrganizationId = o.Id
                         left join Organizations oo on os.ToOrganizationId = oo.Id
                {whereCondition} {groupByCondition}";

        var data = _baseRepository.Query<ValueLabel>(query);

        return data;
    }

    public PayloadResponse GetSettlementReceivableSummaryData(SettlementDataFilter filter)
    {
        try
        {
            var currentUser = _loggedInUserService.GetLoggedInUser();

            var groupByCondition = "GROUP BY o.Name, oo.Name, os.ToOrganizationId, os.FromOrganizationId";

            var extraCondition = $@"order by TotalAmount desc
                                OFFSET ({filter.PageNo} - 1) * {filter.PageSize} ROWS
                                FETCH NEXT {filter.PageSize} ROWS ONLY";

            var condition = new List<string>();

            if (string.IsNullOrEmpty(filter.ToOrganizationId))
            {
                if (!currentUser.IsSuperAdmin)
                {
                    condition.Add($" ToOrganizationId = '{currentUser.OrganizationId}' ");
                }
            }
            else
            {
                condition.Add($" ToOrganizationId = '{filter.ToOrganizationId}' ");
            }

            if (!string.IsNullOrEmpty(filter.FromOrganizationId))
            {
                condition.Add($" FromOrganizationId = '{filter.FromOrganizationId}' ");
            }

            if (filter.StartDate != null || filter.EndDate != null)
            {
                var dateTimeFilter = _commonService.GetDateTimeFilterData(filter.StartDate, filter.EndDate);

                condition.Add($" (os.CreateTime between '{dateTimeFilter.StartDate}' and '{dateTimeFilter.EndDate}') ");
            }

            var whereCondition = _commonService.GenerateWhereConditionFromConditionList(condition);

            var query = GetSummaryDataQuery();

            var rowCount = GetSummaryRowCountData(query, groupByCondition, whereCondition);

            var data = GetSummaryData(query, whereCondition, groupByCondition,
                extraCondition);

            var dropDownData = GetDropDownDataForReceivableSummaryData(whereCondition, groupByCondition);

            return new PayloadResponse()
            {
                IsSuccess = true,
                Content = new { dropDownData, data, rowCount },
                Message = "Data fetched successfully"
            };
        }
        catch (Exception ex)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                Content = null,
                Message = ex.Message
            };
        }
    }

    private object GetDropDownDataForReceivableSummaryData(string whereCondition, string groupByCondition)
    {
        var query =
            $@"SELECT distinct os.FromOrganizationId                    as Value,
                       o.Name                                           aS Label
                FROM OrganizationSettlement os
                         left join Organizations o on os.FromOrganizationId = o.Id
                         left join Organizations oo on os.ToOrganizationId = oo.Id
                {whereCondition} {groupByCondition}";

        var data = _baseRepository.Query<ValueLabel>(query);

        return data;
    }

    public PayloadResponse GetSettlementDetailData(SettlementDataFilter filter)
    {
        try
        {
            if (string.IsNullOrEmpty(filter.FromOrganizationId) || string.IsNullOrEmpty(filter.ToOrganizationId))
            {
                return new PayloadResponse()
                {
                    IsSuccess = false,
                    Content = null,
                    Message = "FromOrganizationId and ToOrganizationId are required"
                };
            }

            var extraCondition = $@"order by os.CreateTime desc
                                OFFSET ({filter.PageNo} - 1) * {filter.PageSize} ROWS
                                FETCH NEXT {filter.PageSize} ROWS ONLY";

            var condition = new List<string>
            {
                $" os.FromOrganizationId = '{filter.FromOrganizationId}' ",
                $" os.ToOrganizationId = '{filter.ToOrganizationId}' "
            };

            if (filter.StartDate != null || filter.EndDate != null)
            {
                var dateTimeFilter = _commonService.GetDateTimeFilterData(filter.StartDate, filter.EndDate);

                condition.Add($" (os.CreateTime between '{dateTimeFilter.StartDate}' and '{dateTimeFilter.EndDate}') ");
            }

            var whereCondition = _commonService.GenerateWhereConditionFromConditionList(condition);

            var query = GetDetailDataQuery();

            var rowCount = GetDetailRowCountData(query, whereCondition);

            var data = GetDetailData(query, whereCondition, extraCondition);

            return new PayloadResponse()
            {
                IsSuccess = true,
                Content = new { data, rowCount },
                Message = "Data fetched successfully"
            };
        }
        catch (Exception ex)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                Content = null,
                Message = ex.Message
            };
        }
    }

    public PayloadResponse GetPayableUnsettledInvoices(string organizationId, int pageNo, int pageSize)
    {
        try
        {
            var currentUser = _loggedInUserService.GetLoggedInUser();

            if (currentUser == null)
            {
                return new PayloadResponse()
                {
                    IsSuccess = false,
                    Content = null,
                    Message = "User not logged in"
                };
            }

            var invoiceData = _invoiceRepository
                .GetAll()
                .Where(i => i.Status == InvoiceStatus.Unsettled || i.Status == InvoiceStatus.Partial);

            if (string.IsNullOrEmpty(organizationId))
            {
                if (!currentUser.IsSuperAdmin)
                {
                    invoiceData = invoiceData
                        .Where(i => i.FromOrganizationId == currentUser.OrganizationId);
                }
            }
            else
            {
                invoiceData = invoiceData
                    .Where(i => i.FromOrganizationId == organizationId);
            }

            var rowCount = invoiceData.Count();

            var finalInvoiceList = invoiceData
                .Include(i => i.FromOrganization)
                .Include(i => i.ToOrganization)
                .OrderByDescending(i => i.CreateTime)
                .Skip((pageNo - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return new PayloadResponse()
            {
                IsSuccess = true,
                Content = new { data = finalInvoiceList, rowCount },
                Message = "Data fetched successfully"
            };
        }
        catch (Exception ex)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                Message = $"Data fetching failed because {ex.Message}"
            };
        }
    }

    public PayloadResponse GetPayableInReviewInvoices(string organizationId, int pageNo, int pageSize)
    {
        try
        {
            var currentUser = _loggedInUserService.GetLoggedInUser();

            if (currentUser == null)
            {
                return new PayloadResponse()
                {
                    IsSuccess = false,
                    Content = null,
                    Message = "User not logged in"
                };
            }

            var invoiceData = _invoiceRepository
                .GetAll()
                .Where(i => i.Status == InvoiceStatus.InReview);

            if (string.IsNullOrEmpty(organizationId))
            {
                if (!currentUser.IsSuperAdmin)
                {
                    invoiceData = invoiceData
                        .Where(i => i.FromOrganizationId == currentUser.OrganizationId);
                }
            }
            else
            {
                invoiceData = invoiceData
                    .Where(i => i.FromOrganizationId == organizationId);
            }

            var rowCount = invoiceData.Count();

            var finalInvoiceList = invoiceData
                .Include(i => i.FromOrganization)
                .Include(i => i.ToOrganization)
                .OrderByDescending(i => i.CreateTime)
                .Skip((pageNo - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return new PayloadResponse()
            {
                IsSuccess = true,
                Content = new { data = finalInvoiceList, rowCount },
                Message = "Data fetched successfully"
            };
        }
        catch (Exception ex)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                Message = $"Data fetching failed because {ex.Message}"
            };
        }
    }

    public PayloadResponse GetPayableSettledInvoices(string organizationId, int pageNo, int pageSize)
    {
        try
        {
            var currentUser = _loggedInUserService.GetLoggedInUser();

            if (currentUser == null)
            {
                return new PayloadResponse()
                {
                    IsSuccess = false,
                    Content = null,
                    Message = "User not logged in"
                };
            }

            var invoiceData = _invoiceRepository
                .GetAll()
                .Where(i => i.Status == InvoiceStatus.Settled);

            if (string.IsNullOrEmpty(organizationId))
            {
                if (!currentUser.IsSuperAdmin)
                {
                    invoiceData = invoiceData
                        .Where(i => i.FromOrganizationId == currentUser.OrganizationId);
                }
            }
            else
            {
                invoiceData = invoiceData
                    .Where(i => i.FromOrganizationId == organizationId);
            }

            var rowCount = invoiceData.Count();

            var finalInvoiceList = invoiceData
                .Include(i => i.FromOrganization)
                .Include(i => i.ToOrganization)
                .OrderByDescending(i => i.CreateTime)
                .Skip((pageNo - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return new PayloadResponse()
            {
                IsSuccess = true,
                Content = new { data = finalInvoiceList, rowCount },
                Message = "Data fetched successfully"
            };
        }
        catch (Exception ex)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                Message = $"Data fetching failed because {ex.Message}"
            };
        }
    }

    public PayloadResponse GetInvoiceWiseTransactions(string invoiceNumber, int pageNo, int pageSize)
    {
        try
        {
            var extraCondition = $@"order by os.CreateTime desc
                                OFFSET ({pageNo} - 1) * {pageSize} ROWS
                                FETCH NEXT {pageSize} ROWS ONLY";

            var condition = new List<string>
            {
                $" os.InvoiceNumber = '{invoiceNumber}' "
            };

            var whereCondition = _commonService.GenerateWhereConditionFromConditionList(condition);

            var query = GetDetailDataQuery();

            var rowCount = GetDetailRowCountData(query, whereCondition);

            var data = GetDetailData(query, whereCondition, extraCondition);

            return new PayloadResponse()
            {
                IsSuccess = true,
                Content = new { data, rowCount },
                Message = "Data fetched successfully"
            };
        }
        catch (Exception ex)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                Content = null,
                Message = $"Data fetching failed because {ex.Message}"
            };
        }
    }

    public PayloadResponse GetReceivableUnsettledInvoices(string organizationId, int pageNo, int pageSize)
    {
        try
        {
            var currentUser = _loggedInUserService.GetLoggedInUser();

            if (currentUser == null)
            {
                return new PayloadResponse()
                {
                    IsSuccess = false,
                    Content = null,
                    Message = "User not logged in"
                };
            }

            var invoiceData = _invoiceRepository
                .GetAll()
                .Where(i => i.Status == InvoiceStatus.Unsettled || i.Status == InvoiceStatus.Partial);

            if (string.IsNullOrEmpty(organizationId))
            {
                if (!currentUser.IsSuperAdmin)
                {
                    invoiceData = invoiceData
                        .Where(i => i.ToOrganizationId == currentUser.OrganizationId);
                }
            }
            else
            {
                invoiceData = invoiceData
                    .Where(i => i.ToOrganizationId == organizationId);
            }

            var rowCount = invoiceData.Count();

            var finalInvoiceList = invoiceData
                .Include(i => i.FromOrganization)
                .Include(i => i.ToOrganization)
                .OrderByDescending(i => i.CreateTime)
                .Skip((pageNo - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return new PayloadResponse()
            {
                IsSuccess = true,
                Content = new { data = finalInvoiceList, rowCount },
                Message = "Data fetched successfully"
            };
        }
        catch (Exception ex)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                Message = $"Data fetching failed because {ex.Message}"
            };
        }
    }

    public PayloadResponse GetReceivableInReviewInvoices(string organizationId, int pageNo, int pageSize)
    {
        try
        {
            var currentUser = _loggedInUserService.GetLoggedInUser();

            if (currentUser == null)
            {
                return new PayloadResponse()
                {
                    IsSuccess = false,
                    Content = null,
                    Message = "User not logged in"
                };
            }

            var invoiceData = _invoiceRepository
                .GetAll()
                .Where(i => i.Status == InvoiceStatus.InReview);

            if (string.IsNullOrEmpty(organizationId))
            {
                if (!currentUser.IsSuperAdmin)
                {
                    invoiceData = invoiceData
                        .Where(i => i.ToOrganizationId == currentUser.OrganizationId);
                }
            }
            else
            {
                invoiceData = invoiceData
                    .Where(i => i.ToOrganizationId == organizationId);
            }

            var rowCount = invoiceData.Count();

            var finalInvoiceList = invoiceData
                .Include(i => i.FromOrganization)
                .Include(i => i.ToOrganization)
                .OrderByDescending(i => i.CreateTime)
                .Skip((pageNo - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return new PayloadResponse()
            {
                IsSuccess = true,
                Content = new { data = finalInvoiceList, rowCount },
                Message = "Data fetched successfully"
            };
        }
        catch (Exception ex)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                Message = $"Data fetching failed because {ex.Message}"
            };
        }
    }

    public PayloadResponse GetReceivableSettledInvoices(string organizationId, int pageNo, int pageSize)
    {
        try
        {
            var currentUser = _loggedInUserService.GetLoggedInUser();

            if (currentUser == null)
            {
                return new PayloadResponse()
                {
                    IsSuccess = false,
                    Content = null,
                    Message = "User not logged in"
                };
            }

            var invoiceData = _invoiceRepository
                .GetAll()
                .Where(i => i.Status == InvoiceStatus.Settled);

            if (string.IsNullOrEmpty(organizationId))
            {
                if (!currentUser.IsSuperAdmin)
                {
                    invoiceData = invoiceData
                        .Where(i => i.ToOrganizationId == currentUser.OrganizationId);
                }
            }
            else
            {
                invoiceData = invoiceData
                    .Where(i => i.ToOrganizationId == organizationId);
            }

            var rowCount = invoiceData.Count();

            var finalInvoiceList = invoiceData
                .Include(i => i.FromOrganization)
                .Include(i => i.ToOrganization)
                .OrderByDescending(i => i.CreateTime)
                .Skip((pageNo - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return new PayloadResponse()
            {
                IsSuccess = true,
                Content = new { data = finalInvoiceList, rowCount },
                Message = "Data fetched successfully"
            };
        }
        catch (Exception ex)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                Message = $"Data fetching failed because {ex.Message}"
            };
        }
    }

    public PayloadResponse Payment(InvoiceWisePaymentDto payment)
    {
        try
        {
            var currentUser = _loggedInUserService.GetLoggedInUser();
            var invoice = _invoiceRepository.GetAll().FirstOrDefault(i => i.Id == payment.InvoiceNumber);
            var isConditionsAreSatisfied = CheckIfConditionsAreSatisfiedForPaymentProof(payment, currentUser, invoice);

            if (!isConditionsAreSatisfied.IsSuccess) return isConditionsAreSatisfied;

            var existingInvoicePayment = _invoicePaymentRepository
                .GetAll()
                .FirstOrDefault(ip => ip.InvoiceNumber == payment.InvoiceNumber);

            _invoicePaymentRepository.Insert(new InvoicePayment()
            {
                InvoiceNumber = payment.InvoiceNumber,
                PaymentProof = _commonService
                    .UploadMultipleFilesAndGetCommaSeparatedUrl(payment.PaymentProof, "InvoicePaymentProof"),
                PaymentBy = currentUser.Id,
                PaymentTime = DateTime.UtcNow,
                SenderAccountId = payment.SenderAccountId,
                ReceiverAccountId = payment.ReceiverAccountId,
                Status = InvoicePaymentStatus.Pending
            });

            _invoicePaymentRepository.SaveChanges();

            if (invoice!.Status != InvoiceStatus.InReview)
            {
                invoice.Status = InvoiceStatus.InReview;
                _invoiceRepository.Update(invoice);
                _invoiceRepository.SaveChanges();
            }

            if (existingInvoicePayment is null)
            {
                UpdateInvoiceWiseSettlementStatus(invoice!.InvoiceNumber, SettlementStatus.InReview);
            }

            return new PayloadResponse()
            {
                IsSuccess = true,
                Content = null,
                Message = "Payment proof submitted successfully"
            };
        }
        catch(Exception ex)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                Content = null,
                Message = $"Payment failed because {ex.Message}"
            };
        }
    }

    public PayloadResponse VerifyPayment(PaymentVerificationDto verification)
    {
        var currentUser = _loggedInUserService.GetLoggedInUser();

        var invoicePayment = _invoicePaymentRepository.GetAll()
            .FirstOrDefault(ip => ip.Id == verification.InvoicePaymentId);

        if (invoicePayment == null)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                Content = null,
                Message = "Invoice payment record not found"
            };
        }

        if(invoicePayment.Status != InvoicePaymentStatus.Pending)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                Content = null,
                Message = "Invoice payment is not in pending status"
            };
        }

        var invoice = _invoiceRepository.GetAll().FirstOrDefault(i => i.Id == invoicePayment.InvoiceNumber);

        if (invoice == null)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                Content = null,
                Message = "Invoice not found"
            };
        }

        if (invoice.Status != InvoiceStatus.InReview && invoice.Status != InvoiceStatus.Partial )
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                Content = null,
                Message = "Invoice is not in review or partial payment state"
            };
        }

        if (!currentUser.IsSuperAdmin)
        {
            if (currentUser.OrganizationId != invoice.ToOrganizationId)
            {
                return new PayloadResponse()
                {
                    IsSuccess = false,
                    Content = null,
                    Message = "You are not authorized to verify payment for this invoice"
                };
            }
        }

        if(verification.IsVerified)
        {
            invoicePayment.Status = InvoicePaymentStatus.Settled;
            invoicePayment.PaymentReceivedBy = currentUser.Id;
            invoicePayment.PaymentReceivedTime = DateTime.UtcNow;
            _invoicePaymentRepository.Update(invoicePayment);
            _invoicePaymentRepository.SaveChanges();

            var existingInvoicePaymentsInReview = _invoicePaymentRepository.GetAll()
                .Where(ip => ip.InvoiceNumber == invoicePayment.InvoiceNumber && ip.Status == InvoicePaymentStatus.Settled)
                .ToList();

            var totalSettled = existingInvoicePaymentsInReview.Sum(ip => ip.Amount);

            if (totalSettled >= invoice.Amount)
            {
                invoice.Status = InvoiceStatus.Settled;
                _invoiceRepository.Update(invoice);
                _invoiceRepository.SaveChanges();

                UpdateInvoiceWiseSettlementStatus(invoice.InvoiceNumber, SettlementStatus.Settled);
            }
            else
            {
                if (invoice.Status != InvoiceStatus.Partial)
                {
                    invoice.Status = InvoiceStatus.Partial;
                    _invoiceRepository.Update(invoice);
                    _invoiceRepository.SaveChanges();

                    UpdateInvoiceWiseSettlementStatus(invoice.InvoiceNumber, SettlementStatus.Partial);
                }
            }
        }
        else
        {
            invoicePayment.Status = InvoicePaymentStatus.Rejected;
            invoicePayment.PaymentReceivedBy = currentUser.Id;
            invoicePayment.PaymentReceivedTime = DateTime.UtcNow;
            _invoicePaymentRepository.Update(invoicePayment);
            _invoicePaymentRepository.SaveChanges();

            if(invoice.Status != InvoiceStatus.Settled)
            {
                invoice.Status = InvoiceStatus.Partial;
                _invoiceRepository.Update(invoice);
                _invoiceRepository.SaveChanges();

                UpdateInvoiceWiseSettlementStatus(invoice.InvoiceNumber, SettlementStatus.Partial);
            }
        }

        return new PayloadResponse()
        {
            IsSuccess = true,
            Content = null,
            Message = "Payment verification status updated successfully"
        };
    }

    public PayloadResponse GetInvoiceWisePayments(string invoiceNumber)
    {
        var invoicePayments = _invoicePaymentRepository.GetAll()
            .Where(ip => ip.InvoiceNumber == invoiceNumber)
            .Include(ip => ip.PaymentByUser)
            .Include(ip => ip.PaymentReceivedByUser)
            .Include(ip => ip.SenderAccount)
            .Include(ip => ip.ReceiverAccount)
            .OrderBy(ip => ip.CreateTime)
            .ToList();

        return new PayloadResponse()
        {
            IsSuccess = true,
            Content = invoicePayments,
            Message = "Data fetched successfully"
        };
    }

    private void UpdateInvoiceWiseSettlementStatus(string invoiceNumber, string settlementStatus)
    {
        var query = $@"UPDATE OrganizationSettlement
                    SET Status = '{settlementStatus}'
                    where InvoiceNumber = '{invoiceNumber}'";

        _baseRepository.ExecuteQuery(query);
    }

    private PayloadResponse CheckIfConditionsAreSatisfiedForPaymentProof(InvoiceWisePaymentDto payment, User currentUser, Invoice invoice)
    {
        if (!payment.PaymentProof.Any())
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                Content = null,
                Message = "Payment proof is required"
            };
        }

        if (currentUser == null)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                Content = null,
                Message = "User not logged in"
            };
        }

        if (invoice == null)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                Content = null,
                Message = "Invoice not found"
            };
        }

        if (invoice.Status == InvoiceStatus.Settled)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                Content = null,
                Message = "Invoice already settled"
            };
        }

        if (!currentUser.IsSuperAdmin)
        {
            if (currentUser.OrganizationId != invoice.FromOrganizationId)
            {
                return new PayloadResponse()
                {
                    IsSuccess = false,
                    Content = null,
                    Message = "You are not authorized to make payment for this invoice"
                };
            }
        }

        return new PayloadResponse()
        {
            IsSuccess = true
        };
    }

    private List<SettlementDetailData> GetDetailData(string query, string whereCondition, string extraCondition)
    {
        var finalQuery = $@"
                            {query}
                            {whereCondition}
                            {extraCondition}";

        var data = _baseRepository.Query<SettlementDetailData>(finalQuery);

        return data;
    }

    private int GetDetailRowCountData(string query, string whereCondition)
    {
        var finalQuery = $@"
                            with data as (
                                {query}
                                {whereCondition}
                            )
                            select count(*) as count from data";

        var countData = _baseRepository.Query<int>(finalQuery).FirstOrDefault();

        return countData;
    }

    private string GetDetailDataQuery()
    {
        return @"SELECT os.CreateTime          as TransactionTime,
                       c.CardNumber,
                       os.TransactionId,
                       os.TransactionType,
                       os.Amount,
                       os.InvoiceNumber,     
                       os.Status     
                FROM OrganizationSettlement os
                         left join Transactions t on os.TransactionId = t.TransactionId
                         left join Cards c on t.CardId = c.Id";
    }

    private List<SettlementSummaryData> GetSummaryData(string query, string whereCondition, string groupByCondition, string extraCondition)
    {
        var finalQuery = $@"
                            {query}
                            {whereCondition}
                            {groupByCondition}
                            {extraCondition}";

        var data = _baseRepository.Query<SettlementSummaryData>(finalQuery);

        return data;
    }

    private int GetSummaryRowCountData(string query, string groupByCondition, string whereCondition)
    {
        var finalQuery = $@"
                            with data as (
                                {query}
                                {whereCondition}
                                {groupByCondition}
                            )
                            select count(*) as count from data";

        var countData = _baseRepository.Query<int>(finalQuery).FirstOrDefault();

        return countData;
    }

    private string GetSummaryDataQuery()
    {
        return @"SELECT os.ToOrganizationId                                                     as ReceiverOrganizationId,
                       oo.Name                                                                 AS ReceiverOrganization,
                       os.FromOrganizationId                                                   as SenderOrganizationId,
                       o.Name                                                                  AS SenderOrganization,
                       SUM(CASE WHEN os.TransactionType = 'BusFare' THEN os.Amount ELSE 0 END) AS BusFareAmount,
                       SUM(CASE WHEN os.TransactionType = 'Return' THEN os.Amount ELSE 0 END)  AS ReturnAmount,
                       SUM(CASE WHEN os.TransactionType = 'Due' THEN os.Amount ELSE 0 END)     AS DueAmount,
                       SUM(CASE WHEN os.Status = 'In Review' THEN os.Amount ELSE 0 END)        AS InReviewAmount,
                       SUM(CASE WHEN os.Status = 'Settled' THEN os.Amount ELSE 0 END)          AS SettledAmount,
                       SUM(CASE WHEN os.Status = 'Pending' THEN os.Amount ELSE 0 END)          AS PendingAmount,
                       SUM(CASE WHEN os.Status = 'Generated' THEN os.Amount ELSE 0 END)        AS InvoiceAmount,
                       SUM(Amount)                                                             AS TotalAmount
                FROM OrganizationSettlement os
                         left join Organizations o on os.FromOrganizationId = o.Id
                         left join Organizations oo on os.ToOrganizationId = oo.Id";
    }
}
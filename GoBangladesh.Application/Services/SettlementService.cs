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

namespace GoBangladesh.Application.Services;

public class SettlementService : ISettlementService
{
    private readonly IBaseRepository _baseRepository;
    private readonly ILoggedInUserService _loggedInUserService;
    private readonly ICommonService _commonService;
    private readonly IRepository<Invoice> _invoiceRepository;
    private readonly IRepository<Organization> _organizationRepository;

    public SettlementService(IBaseRepository baseRepository,
        ILoggedInUserService loggedInUserService,
        ICommonService commonService,
        IRepository<Invoice> invoiceRepository,
        IRepository<Organization> organizationRepository)
    {
        _baseRepository = baseRepository;
        _loggedInUserService = loggedInUserService;
        _commonService = commonService;
        _invoiceRepository = invoiceRepository;
        _organizationRepository = organizationRepository;
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
                .Where(i => i.Status == InvoiceStatus.Pending || i.Status == InvoiceStatus.InReview);

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

            var finalInvoiceList = (from invoice in invoiceData
                    join fromOrg in _organizationRepository.GetAll() on invoice.FromOrganizationId equals fromOrg.Id
                    join toOrg in _organizationRepository.GetAll() on invoice.ToOrganizationId equals toOrg.Id
                    orderby invoice.CreateTime descending
                    select new InvoiceData()
                    {
                        Id = invoice.Id,
                        InvoiceNumber = invoice.InvoiceNumber,
                        FromOrganizationId = invoice.FromOrganizationId,
                        ToOrganizationId = invoice.ToOrganizationId,
                        FromOrganization = fromOrg.Name,
                        ToOrganization = toOrg.Name,
                        FromDate = invoice.FromDate,
                        ToDate = invoice.ToDate,
                        Amount = invoice.Amount,
                        Status = invoice.Status
                    })
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

            var finalInvoiceList = (from invoice in invoiceData
                                    join fromOrg in _organizationRepository.GetAll() on invoice.FromOrganizationId equals fromOrg.Id
                                    join toOrg in _organizationRepository.GetAll() on invoice.ToOrganizationId equals toOrg.Id
                                    orderby invoice.CreateTime descending
                                    select new InvoiceData()
                                    {
                                        Id = invoice.Id,
                                        InvoiceNumber = invoice.InvoiceNumber,
                                        FromOrganizationId = invoice.FromOrganizationId,
                                        ToOrganizationId = invoice.ToOrganizationId,
                                        FromOrganization = fromOrg.Name,
                                        ToOrganization = toOrg.Name,
                                        FromDate = invoice.FromDate,
                                        ToDate = invoice.ToDate,
                                        Amount = invoice.Amount,
                                        Status = invoice.Status
                                    })
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
                .Where(i => i.Status == InvoiceStatus.Pending || i.Status == InvoiceStatus.InReview);

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

            var finalInvoiceList = (from invoice in invoiceData
                                    join fromOrg in _organizationRepository.GetAll() on invoice.FromOrganizationId equals fromOrg.Id
                                    join toOrg in _organizationRepository.GetAll() on invoice.ToOrganizationId equals toOrg.Id
                                    orderby invoice.CreateTime descending
                                    select new InvoiceData()
                                    {
                                        Id = invoice.Id,
                                        InvoiceNumber = invoice.InvoiceNumber,
                                        FromOrganizationId = invoice.FromOrganizationId,
                                        ToOrganizationId = invoice.ToOrganizationId,
                                        FromOrganization = fromOrg.Name,
                                        ToOrganization = toOrg.Name,
                                        FromDate = invoice.FromDate,
                                        ToDate = invoice.ToDate,
                                        Amount = invoice.Amount,
                                        Status = invoice.Status
                                    })
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

            var finalInvoiceList = (from invoice in invoiceData
                                    join fromOrg in _organizationRepository.GetAll() on invoice.FromOrganizationId equals fromOrg.Id
                                    join toOrg in _organizationRepository.GetAll() on invoice.ToOrganizationId equals toOrg.Id
                                    orderby invoice.CreateTime descending
                                    select new InvoiceData()
                                    {
                                        Id = invoice.Id,
                                        InvoiceNumber = invoice.InvoiceNumber,
                                        FromOrganizationId = invoice.FromOrganizationId,
                                        ToOrganizationId = invoice.ToOrganizationId,
                                        FromOrganization = fromOrg.Name,
                                        ToOrganization = toOrg.Name,
                                        FromDate = invoice.FromDate,
                                        ToDate = invoice.ToDate,
                                        Amount = invoice.Amount,
                                        Status = invoice.Status
                                    })
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
                       SUM(Amount)                                                             AS TotalAmount
                FROM OrganizationSettlement os
                         left join Organizations o on os.FromOrganizationId = o.Id
                         left join Organizations oo on os.ToOrganizationId = oo.Id";
    }
}
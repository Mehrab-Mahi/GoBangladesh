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
    private readonly IRepository<Organization> _organizationRepository;

    public SettlementService(IBaseRepository baseRepository,
        ILoggedInUserService loggedInUserService,
        ICommonService commonService,
        IRepository<Invoice> invoiceRepository,
        IRepository<InvoicePayment> invoicePaymentRepository,
        IRepository<Organization> organizationRepository)
    {
        _baseRepository = baseRepository;
        _loggedInUserService = loggedInUserService;
        _commonService = commonService;
        _invoiceRepository = invoiceRepository;
        _invoicePaymentRepository = invoicePaymentRepository;
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
                    condition.Add($" os.FromOrganizationId = '{currentUser.OrganizationId}' ");
                }
            }
            else
            {
                condition.Add($" os.FromOrganizationId = '{filter.FromOrganizationId}' ");
            }

            if (!string.IsNullOrEmpty(filter.ToOrganizationId))
            {
                condition.Add($" os.ToOrganizationId = '{filter.ToOrganizationId}' ");
            }

            if (filter.StartDate != null || filter.EndDate != null)
            {
                var dateTimeFilter = _commonService.GetDateTimeFilterData(filter.StartDate, filter.EndDate);

                condition.Add($" (os.CreateTime between '{dateTimeFilter.StartDate}' and '{dateTimeFilter.EndDate}') ");
            }

            var whereCondition = _commonService.GenerateWhereConditionFromConditionList(condition);

            var query = GetSummaryDataQuery();

            var rowCount = GetSummaryRowCountData(groupByCondition, whereCondition);

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

            var rowCount = GetSummaryRowCountData(groupByCondition, whereCondition);

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

    public PayloadResponse GetPayableUnsettledInvoices(SettlementFilter filter) 
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
            var condition = new List<string>
                { $" (i.Status = '{InvoiceStatus.Unsettled}' or i.Status = '{InvoiceStatus.Partial}') " };

            if (string.IsNullOrEmpty(filter.OrganizationId))
            {
                if (!currentUser.IsSuperAdmin)
                {
                    condition.Add($" i.FromOrganizationId = '{currentUser.OrganizationId}' ");
                }
            }
            else
            {
                condition.Add($" i.FromOrganizationId = '{filter.OrganizationId}' ");
            }

            if (!string.IsNullOrEmpty(filter.SearchQuery))
            {
                condition.Add($" i.InvoiceNumber like '%{filter.SearchQuery}%' ");
            }

            var whereCondition = _commonService.GenerateWhereConditionFromConditionList(condition);
            var dataQuery = GetInvoiceDataQuery();
            var extraCondition = GetInvoiceExtraCondition(filter.PageNo, filter.PageSize);

            var rowCountQuery = GetInvoiceRowCountQuery();
            var rowCount = _baseRepository.Query<int>($"{rowCountQuery} {whereCondition}").FirstOrDefault();

            var finalInvoiceList = _baseRepository.Query<InvoiceData>($"{dataQuery} {whereCondition} {extraCondition}");

            var organizationList = _organizationRepository.GetAll().ToList();

            var invoiceWiseSettlementData = GetInvoiceWiseSettlementData(finalInvoiceList.Select(i => i.InvoiceNumber).ToList());

            foreach (var invoice in finalInvoiceList)
            {
                invoice.FromOrganization = organizationList.FirstOrDefault(o => o.Id == invoice.FromOrganizationId);
                invoice.ToOrganization = organizationList.FirstOrDefault(o => o.Id == invoice.ToOrganizationId);

                var settlementData = invoiceWiseSettlementData
                    .FirstOrDefault(iws => iws.InvoiceNumber == invoice.InvoiceNumber);

                if (settlementData != null)
                {
                    invoice.InvoiceTripAmount = settlementData.InvoiceTripAmount;
                    invoice.InvoiceReturnAmount = settlementData.InvoiceReturnAmount;
                    invoice.InvoiceDueAmount = settlementData.InvoiceDueAmount;
                }
            }

            var dropDownQuery = GetDropDownDataQueryForPayableInvoiceData();
            var dropDownData = _baseRepository.Query<ValueLabel>($"{dropDownQuery} {whereCondition}");

            return new PayloadResponse()
            {
                IsSuccess = true,
                Content = new { data = finalInvoiceList, rowCount, dropDownData },
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

    private List<InvoiceWiseSettlementData> GetInvoiceWiseSettlementData(List<string> invoiceNumberList)
    {
        if (!invoiceNumberList.Any())
        {
            return new List<InvoiceWiseSettlementData>();
        }

        var query = $@"select InvoiceNumber,
                           coalesce(sum(case when TransactionType = 'BusFare' then Amount end), 0) as InvoiceTripAmount,
                           coalesce(sum(case when TransactionType = 'Return' then Amount end), 0)  as InvoiceReturnAmount,
                           coalesce(sum(case when TransactionType = 'Due' then Amount end), 0)     as InvoiceDueAmount
                    from OrganizationSettlement
                    where InvoiceNumber in ('{string.Join("','", invoiceNumberList)}')
                    group by InvoiceNumber";

        return _baseRepository
            .Query<InvoiceWiseSettlementData>(query)
            .ToList();
    }

    public PayloadResponse GetPayableInReviewInvoices(SettlementFilter filter)
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
            var condition = new List<string>
                { $" (i.Status = '{InvoiceStatus.InReview}') " };

            if (string.IsNullOrEmpty(filter.OrganizationId))
            {
                if (!currentUser.IsSuperAdmin)
                {
                    condition.Add($" i.FromOrganizationId = '{currentUser.OrganizationId}' ");
                }
            }
            else
            {
                condition.Add($" i.FromOrganizationId = '{filter.OrganizationId}' ");
            }

            if (!string.IsNullOrEmpty(filter.SearchQuery))
            {
                condition.Add($" i.InvoiceNumber like '%{filter.SearchQuery}%' ");
            }

            var whereCondition = _commonService.GenerateWhereConditionFromConditionList(condition);
            var dataQuery = GetInvoiceDataQuery();
            var extraCondition = GetInvoiceExtraCondition(filter.PageNo, filter.PageSize);

            var rowCountQuery = GetInvoiceRowCountQuery();
            var rowCount = _baseRepository.Query<int>($"{rowCountQuery} {whereCondition}").FirstOrDefault();

            var finalInvoiceList = _baseRepository.Query<InvoiceData>($"{dataQuery} {whereCondition} {extraCondition}");

            var organizationList = _organizationRepository.GetAll().ToList();

            var invoiceWiseSettlementData = GetInvoiceWiseSettlementData(finalInvoiceList.Select(i => i.InvoiceNumber).ToList());

            foreach (var invoice in finalInvoiceList)
            {
                invoice.FromOrganization = organizationList.FirstOrDefault(o => o.Id == invoice.FromOrganizationId);
                invoice.ToOrganization = organizationList.FirstOrDefault(o => o.Id == invoice.ToOrganizationId);

                var settlementData = invoiceWiseSettlementData
                    .FirstOrDefault(iws => iws.InvoiceNumber == invoice.InvoiceNumber);

                if (settlementData != null)
                {
                    invoice.InvoiceTripAmount = settlementData.InvoiceTripAmount;
                    invoice.InvoiceReturnAmount = settlementData.InvoiceReturnAmount;
                    invoice.InvoiceDueAmount = settlementData.InvoiceDueAmount;
                }
            }

            var dropDownQuery = GetDropDownDataQueryForPayableInvoiceData();
            var dropDownData = _baseRepository.Query<ValueLabel>($"{dropDownQuery} {whereCondition}");

            return new PayloadResponse()
            {
                IsSuccess = true,
                Content = new { data = finalInvoiceList, rowCount, dropDownData },
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

    public PayloadResponse GetPayableSettledInvoices(SettlementFilter filter)
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
            var condition = new List<string>
                { $" (i.Status = '{InvoiceStatus.Settled}') " };

            if (string.IsNullOrEmpty(filter.OrganizationId))
            {
                if (!currentUser.IsSuperAdmin)
                {
                    condition.Add($" i.FromOrganizationId = '{currentUser.OrganizationId}' ");
                }
            }
            else
            {
                condition.Add($" i.FromOrganizationId = '{filter.OrganizationId}' ");
            }

            if (!string.IsNullOrEmpty(filter.SearchQuery))
            {
                condition.Add($" i.InvoiceNumber like '%{filter.SearchQuery}%' ");
            }

            var whereCondition = _commonService.GenerateWhereConditionFromConditionList(condition);
            var dataQuery = GetInvoiceDataQuery();
            var extraCondition = GetInvoiceExtraCondition(filter.PageNo, filter.PageSize);

            var rowCountQuery = GetInvoiceRowCountQuery();
            var rowCount = _baseRepository.Query<int>($"{rowCountQuery} {whereCondition}").FirstOrDefault();

            var finalInvoiceList = _baseRepository.Query<InvoiceData>($"{dataQuery} {whereCondition} {extraCondition}");

            var organizationList = _organizationRepository.GetAll().ToList();

            var invoiceWiseSettlementData = GetInvoiceWiseSettlementData(finalInvoiceList.Select(i => i.InvoiceNumber).ToList());

            foreach (var invoice in finalInvoiceList)
            {
                invoice.FromOrganization = organizationList.FirstOrDefault(o => o.Id == invoice.FromOrganizationId);
                invoice.ToOrganization = organizationList.FirstOrDefault(o => o.Id == invoice.ToOrganizationId);

                var settlementData = invoiceWiseSettlementData
                    .FirstOrDefault(iws => iws.InvoiceNumber == invoice.InvoiceNumber);

                if (settlementData != null)
                {
                    invoice.InvoiceTripAmount = settlementData.InvoiceTripAmount;
                    invoice.InvoiceReturnAmount = settlementData.InvoiceReturnAmount;
                    invoice.InvoiceDueAmount = settlementData.InvoiceDueAmount;
                }
            }

            var dropDownQuery = GetDropDownDataQueryForPayableInvoiceData();
            var dropDownData = _baseRepository.Query<ValueLabel>($"{dropDownQuery} {whereCondition}");

            return new PayloadResponse()
            {
                IsSuccess = true,
                Content = new { data = finalInvoiceList, rowCount, dropDownData },
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

    public PayloadResponse GetReceivableUnsettledInvoices(SettlementFilter filter)
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
            var condition = new List<string>
                { $" (i.Status = '{InvoiceStatus.Unsettled}' or i.Status = '{InvoiceStatus.Partial}') " };

            if (string.IsNullOrEmpty(filter.OrganizationId))
            {
                if (!currentUser.IsSuperAdmin)
                {
                    condition.Add($" i.ToOrganizationId = '{currentUser.OrganizationId}' ");
                }
            }
            else
            {
                condition.Add($" i.ToOrganizationId = '{filter.OrganizationId}' ");
            }

            if (!string.IsNullOrEmpty(filter.SearchQuery))
            {
                condition.Add($" i.InvoiceNumber like '%{filter.SearchQuery}%' ");
            }

            var whereCondition = _commonService.GenerateWhereConditionFromConditionList(condition);
            var dataQuery = GetInvoiceDataQuery();
            var extraCondition = GetInvoiceExtraCondition(filter.PageNo, filter.PageSize);

            var rowCountQuery = GetInvoiceRowCountQuery();
            var rowCount = _baseRepository.Query<int>($"{rowCountQuery} {whereCondition}").FirstOrDefault();

            var finalInvoiceList = _baseRepository.Query<InvoiceData>($"{dataQuery} {whereCondition} {extraCondition}");

            var organizationList = _organizationRepository.GetAll().ToList();

            var invoiceWiseSettlementData = GetInvoiceWiseSettlementData(finalInvoiceList.Select(i => i.InvoiceNumber).ToList());

            foreach (var invoice in finalInvoiceList)
            {
                invoice.FromOrganization = organizationList.FirstOrDefault(o => o.Id == invoice.FromOrganizationId);
                invoice.ToOrganization = organizationList.FirstOrDefault(o => o.Id == invoice.ToOrganizationId);

                var settlementData = invoiceWiseSettlementData
                    .FirstOrDefault(iws => iws.InvoiceNumber == invoice.InvoiceNumber);

                if (settlementData != null)
                {
                    invoice.InvoiceTripAmount = settlementData.InvoiceTripAmount;
                    invoice.InvoiceReturnAmount = settlementData.InvoiceReturnAmount;
                    invoice.InvoiceDueAmount = settlementData.InvoiceDueAmount;
                }
            }

            var dropDownQuery = GetDropDownDataQueryForReceivableInvoiceData();
            var dropDownData = _baseRepository.Query<ValueLabel>($"{dropDownQuery} {whereCondition}");

            return new PayloadResponse()
            {
                IsSuccess = true,
                Content = new { data = finalInvoiceList, rowCount, dropDownData },
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

    public PayloadResponse GetReceivableInReviewInvoices(SettlementFilter filter)
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
            var condition = new List<string>
                { $" (i.Status = '{InvoiceStatus.InReview}') " };

            if (string.IsNullOrEmpty(filter.OrganizationId))
            {
                if (!currentUser.IsSuperAdmin)
                {
                    condition.Add($" i.ToOrganizationId = '{currentUser.OrganizationId}' ");
                }
            }
            else
            {
                condition.Add($" i.ToOrganizationId = '{filter.OrganizationId}' ");
            }

            if (!string.IsNullOrEmpty(filter.SearchQuery))
            {
                condition.Add($" i.InvoiceNumber like '%{filter.SearchQuery}%' ");
            }

            var whereCondition = _commonService.GenerateWhereConditionFromConditionList(condition);
            var dataQuery = GetInvoiceDataQuery();
            var extraCondition = GetInvoiceExtraCondition(filter.PageNo, filter.PageSize);

            var rowCountQuery = GetInvoiceRowCountQuery();
            var rowCount = _baseRepository.Query<int>($"{rowCountQuery} {whereCondition}").FirstOrDefault();

            var finalInvoiceList = _baseRepository.Query<InvoiceData>($"{dataQuery} {whereCondition} {extraCondition}");

            var organizationList = _organizationRepository.GetAll().ToList();

            var invoiceWiseSettlementData = GetInvoiceWiseSettlementData(finalInvoiceList.Select(i => i.InvoiceNumber).ToList());

            foreach (var invoice in finalInvoiceList)
            {
                invoice.FromOrganization = organizationList.FirstOrDefault(o => o.Id == invoice.FromOrganizationId);
                invoice.ToOrganization = organizationList.FirstOrDefault(o => o.Id == invoice.ToOrganizationId);

                var settlementData = invoiceWiseSettlementData
                    .FirstOrDefault(iws => iws.InvoiceNumber == invoice.InvoiceNumber);

                if (settlementData != null)
                {
                    invoice.InvoiceTripAmount = settlementData.InvoiceTripAmount;
                    invoice.InvoiceReturnAmount = settlementData.InvoiceReturnAmount;
                    invoice.InvoiceDueAmount = settlementData.InvoiceDueAmount;
                }
            }

            var dropDownQuery = GetDropDownDataQueryForReceivableInvoiceData();
            var dropDownData = _baseRepository.Query<ValueLabel>($"{dropDownQuery} {whereCondition}");

            return new PayloadResponse()
            {
                IsSuccess = true,
                Content = new { data = finalInvoiceList, rowCount, dropDownData },
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

    public PayloadResponse GetReceivableSettledInvoices(SettlementFilter filter)
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
            var condition = new List<string>
                { $" (i.Status = '{InvoiceStatus.Settled}') " };

            if (string.IsNullOrEmpty(filter.OrganizationId))
            {
                if (!currentUser.IsSuperAdmin)
                {
                    condition.Add($" i.ToOrganizationId = '{currentUser.OrganizationId}' ");
                }
            }
            else
            {
                condition.Add($" i.ToOrganizationId = '{filter.OrganizationId}' ");
            }

            if (!string.IsNullOrEmpty(filter.SearchQuery))
            {
                condition.Add($" i.InvoiceNumber like '%{filter.SearchQuery}%' ");
            }

            var whereCondition = _commonService.GenerateWhereConditionFromConditionList(condition);
            var dataQuery = GetInvoiceDataQuery();
            var extraCondition = GetInvoiceExtraCondition(filter.PageNo, filter.PageSize);

            var rowCountQuery = GetInvoiceRowCountQuery();
            var rowCount = _baseRepository.Query<int>($"{rowCountQuery} {whereCondition}").FirstOrDefault();

            var finalInvoiceList = _baseRepository.Query<InvoiceData>($"{dataQuery} {whereCondition} {extraCondition}");

            var organizationList = _organizationRepository.GetAll().ToList();

            var invoiceWiseSettlementData = GetInvoiceWiseSettlementData(finalInvoiceList.Select(i => i.InvoiceNumber).ToList());

            foreach (var invoice in finalInvoiceList)
            {
                invoice.FromOrganization = organizationList.FirstOrDefault(o => o.Id == invoice.FromOrganizationId);
                invoice.ToOrganization = organizationList.FirstOrDefault(o => o.Id == invoice.ToOrganizationId);

                var settlementData = invoiceWiseSettlementData
                    .FirstOrDefault(iws => iws.InvoiceNumber == invoice.InvoiceNumber);

                if (settlementData != null)
                {
                    invoice.InvoiceTripAmount = settlementData.InvoiceTripAmount;
                    invoice.InvoiceReturnAmount = settlementData.InvoiceReturnAmount;
                    invoice.InvoiceDueAmount = settlementData.InvoiceDueAmount;
                }
            }

            var dropDownQuery = GetDropDownDataQueryForReceivableInvoiceData();
            var dropDownData = _baseRepository.Query<ValueLabel>($"{dropDownQuery} {whereCondition}");

            return new PayloadResponse()
            {
                IsSuccess = true,
                Content = new { data = finalInvoiceList, rowCount, dropDownData },
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

    public PayloadResponse Payment(InvoiceWisePaymentDto payment)
    {
        try
        {
            if (payment.PaymentData == null || !payment.PaymentData.Any())
            {
                return new PayloadResponse()
                {
                    IsSuccess = false,
                    Content = null,
                    Message = "Payment data is required"
                };
            }

            var currentUser = _loggedInUserService.GetLoggedInUser();
            var invoiceNumber = payment.PaymentData.FirstOrDefault()!.InvoiceNumber;
            var invoice = _invoiceRepository.GetAll().FirstOrDefault(i => i.InvoiceNumber == invoiceNumber);
            var isConditionsAreSatisfied = CheckIfConditionsAreSatisfiedForPaymentProof(currentUser, invoice);

            if (!isConditionsAreSatisfied.IsSuccess) return isConditionsAreSatisfied;

            var existingInvoicePayment = _invoicePaymentRepository
                .GetAll()
                .FirstOrDefault(ip => ip.InvoiceNumber == invoiceNumber);

            foreach (var paymentData in payment.PaymentData)
            {
                _invoicePaymentRepository.Insert(new InvoicePayment()
                {
                    InvoiceNumber = paymentData.InvoiceNumber,
                    PaymentProof = _commonService
                        .UploadMultipleFilesAndGetCommaSeparatedUrl(paymentData.PaymentProof, "InvoicePaymentProof"),
                    PaymentBy = currentUser.Id,
                    PaymentTime = DateTime.UtcNow,
                    SenderAccountId = paymentData.SenderAccountId,
                    ReceiverAccountId = paymentData.ReceiverAccountId,
                    Status = InvoicePaymentStatus.Pending,
                    Amount = paymentData.Amount
                });
            }

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

        var invoice = _invoiceRepository.GetAll().FirstOrDefault(i => i.InvoiceNumber == invoicePayment.InvoiceNumber);

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
                var pendingPayments = _invoicePaymentRepository
                    .GetAll()
                    .Count(ip => ip.InvoiceNumber == invoicePayment.InvoiceNumber && ip.Status == InvoicePaymentStatus.Pending);

                if (pendingPayments == 0)
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

            var pendingPayments = _invoicePaymentRepository
                .GetAll()
                .Count(ip => ip.InvoiceNumber == invoicePayment.InvoiceNumber && ip.Status == InvoicePaymentStatus.Pending);

            if (pendingPayments == 0)
            {
                var existingInvoicePaymentsInReview = _invoicePaymentRepository.GetAll()
                    .Where(ip => ip.InvoiceNumber == invoicePayment.InvoiceNumber && ip.Status == InvoicePaymentStatus.Settled)
                    .ToList();
                var totalSettled = existingInvoicePaymentsInReview.Sum(ip => ip.Amount);

                if(totalSettled >= invoice.Amount)
                {
                    invoice.Status = InvoiceStatus.Settled;
                    _invoiceRepository.Update(invoice);
                    _invoiceRepository.SaveChanges();
                    UpdateInvoiceWiseSettlementStatus(invoice.InvoiceNumber, SettlementStatus.Settled);
                }
                else
                {
                    invoice.Status = InvoiceStatus.Partial;
                    _invoiceRepository.Update(invoice);
                    _invoiceRepository.SaveChanges();

                    UpdateInvoiceWiseSettlementStatus(invoice.InvoiceNumber, SettlementStatus.Partial);
                }
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
            .Include(ip => ip.SenderAccount.Organization)
            .Include(ip => ip.ReceiverAccount)
            .Include(ip => ip.ReceiverAccount.Organization)
            .OrderBy(ip => ip.CreateTime)
            .ToList();

        return new PayloadResponse()
        {
            IsSuccess = true,
            Content = invoicePayments,
            Message = "Data fetched successfully"
        };
    }

    private string GetDropDownDataQueryForPayableInvoiceData()
    {
        return @"select distinct o.Id as Value, o.Name as Label
                from Invoices i
                         left join Organizations o on i.ToOrganizationId = o.Id";
    }

    private string GetDropDownDataQueryForReceivableInvoiceData()
    {
        return @"select distinct o.Id as Value, o.Name as Label
                from Invoices i
                         left join Organizations o on i.FromOrganizationId = o.Id";
    }

    private void UpdateInvoiceWiseSettlementStatus(string invoiceNumber, string settlementStatus)
    {
        var query = $@"UPDATE OrganizationSettlement
                    SET Status = '{settlementStatus}'
                    where InvoiceNumber = '{invoiceNumber}'";

        _baseRepository.ExecuteQuery(query);
    }

    private PayloadResponse CheckIfConditionsAreSatisfiedForPaymentProof(User currentUser, Invoice invoice)
    {
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

    private string GetInvoiceExtraCondition(int pageNo, int pageSize)
    {
        return $@"
                group by i.Id, i.InvoiceNumber, FromOrganizationId, ToOrganizationId, FromDate, ToDate, i.Amount, i.Status,
                         i.CreateTime, i.LastModifiedTime, i.CreatedBy, i.LastModifiedBy, i.IsDeleted, InvoiceFilePath
                order by i.CreateTime desc
                offset {(pageNo - 1) * pageSize} rows fetch next {pageSize} rows only";
    }

    private string GetInvoiceDataQuery()
    {
        return @"select i.*,
                       coalesce(SUM(case when ip.Status = 'Settled' then ip.Amount end),0)            as PaidAmount,
                       i.Amount - coalesce(SUM(case when ip.Status = 'Settled' then ip.Amount end),0) as DueAmount,
                       coalesce(SUM(case when ip.Status = 'Pending' then ip.Amount end),0)     as    PendingAmount,
                       case when (coalesce(SUM(case when ip.Status = 'Settled' then ip.Amount end),0) +
                                  coalesce(SUM(case when ip.Status = 'Pending' then ip.Amount end),0)) < i.Amount
                       then 1
                       else 0 end                                                     as IsPaymentButtonAvailable
                from Invoices i
                         left join InvoicePayment ip on i.InvoiceNumber = ip.InvoiceNumber";
    }

    private string GetInvoiceRowCountQuery()
    {
        return "select count(*) as count from Invoices i";
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

    private int GetSummaryRowCountData(string groupByCondition, string whereCondition)
    {
        var finalQuery = $@"WITH PaymentSummary AS (
                            SELECT InvoiceNumber,
                                   SUM(CASE WHEN Status = 'Pending' THEN Amount ELSE 0 END) AS InReviewAmount,
                                   SUM(CASE WHEN Status = 'Settled' THEN Amount ELSE 0 END) AS SettledAmount
                            FROM InvoicePayment
                            GROUP BY InvoiceNumber
                        ),
                        data AS (
                            SELECT os.ToOrganizationId                                                     AS ReceiverOrganizationId,
                                   oo.Name                                                                 AS ReceiverOrganization,
                                   os.FromOrganizationId                                                   AS SenderOrganizationId,
                                   o.Name                                                                  AS SenderOrganization,
                                   SUM(CASE WHEN os.TransactionType = 'BusFare' THEN os.Amount ELSE 0 END) AS BusFareAmount,
                                   SUM(CASE WHEN os.TransactionType = 'Return' THEN os.Amount ELSE 0 END)  AS ReturnAmount,
                                   SUM(CASE WHEN os.TransactionType = 'Due' THEN os.Amount ELSE 0 END)     AS DueAmount,
                                   SUM(ps.InReviewAmount)                                                  AS InReviewAmount,
                                   SUM(ps.SettledAmount)                                                   AS SettledAmount,
                                   SUM(CASE WHEN os.InvoiceNumber IS NULL THEN os.Amount ELSE 0 END)       AS PendingAmount,
                                   SUM(CASE WHEN os.InvoiceNumber IS NOT NULL THEN os.Amount ELSE 0 END)   AS InvoiceAmount,
                                   SUM(os.Amount)                                                          AS TotalAmount
                            FROM OrganizationSettlement os
                                     LEFT JOIN Invoices i ON os.InvoiceNumber = i.InvoiceNumber
                                     LEFT JOIN PaymentSummary ps ON i.InvoiceNumber = ps.InvoiceNumber
                                     LEFT JOIN Organizations o ON os.FromOrganizationId = o.Id
                                     LEFT JOIN Organizations oo ON os.ToOrganizationId = oo.Id
                            {whereCondition}
                            {groupByCondition}
                        )
                        SELECT COUNT(*) as count
                        FROM data";

        var countData = _baseRepository.Query<int>(finalQuery).FirstOrDefault();

        return countData;
    }

    private string GetSummaryDataQuery()
    {
        return @"WITH PaymentSummary AS (
                    SELECT
                        InvoiceNumber,
                        SUM(CASE WHEN Status = 'Pending' THEN Amount ELSE 0 END) AS InReviewAmount,
                        SUM(CASE WHEN Status = 'Settled' THEN Amount ELSE 0 END) AS SettledAmount
                    FROM InvoicePayment
                    GROUP BY InvoiceNumber
                )
                SELECT
                    os.ToOrganizationId                                                     AS ReceiverOrganizationId,
                    oo.Name                                                                 AS ReceiverOrganization,
                    os.FromOrganizationId                                                   AS SenderOrganizationId,
                    o.Name                                                                  AS SenderOrganization,
                    SUM(CASE WHEN os.TransactionType = 'BusFare' THEN os.Amount ELSE 0 END) AS BusFareAmount,
                    SUM(CASE WHEN os.TransactionType = 'Return' THEN os.Amount ELSE 0 END)  AS ReturnAmount,
                    SUM(CASE WHEN os.TransactionType = 'Due' THEN os.Amount ELSE 0 END)     AS DueAmount,
                    SUM(ps.InReviewAmount)                                                  AS InReviewAmount,
                    SUM(ps.SettledAmount)                                                   AS SettledAmount,
                    SUM(CASE WHEN os.InvoiceNumber IS NULL THEN os.Amount ELSE 0 END)       AS PendingAmount,
                    SUM(CASE WHEN os.InvoiceNumber IS NOT NULL THEN os.Amount ELSE 0 END)   AS InvoiceAmount,
                    SUM(os.Amount)                                                          AS TotalAmount
                FROM OrganizationSettlement os
                LEFT JOIN Invoices i ON os.InvoiceNumber = i.InvoiceNumber
                LEFT JOIN PaymentSummary ps ON i.InvoiceNumber = ps.InvoiceNumber
                LEFT JOIN Organizations o ON os.FromOrganizationId = o.Id
                LEFT JOIN Organizations oo ON os.ToOrganizationId = oo.Id";
    }
}
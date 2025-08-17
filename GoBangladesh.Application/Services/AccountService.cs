using System;
using System.Collections.Generic;
using System.Linq;
using GoBangladesh.Application.DTOs.Account;
using GoBangladesh.Application.Interfaces;
using GoBangladesh.Application.Util;
using GoBangladesh.Application.ViewModels;
using GoBangladesh.Domain.Entities;
using GoBangladesh.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GoBangladesh.Application.Services;

public class AccountService : IAccountService
{
    private readonly IRepository<Account> _accountRepository;
    private readonly ILoggedInUserService _loggedInUserService;
    private readonly ICommonService _commonService;

    public AccountService(IRepository<Account> accountRepository,
        ILoggedInUserService loggedInUserService,
        ICommonService commonService)
    {
        _accountRepository = accountRepository;
        _loggedInUserService = loggedInUserService;
        _commonService = commonService;
    }

    public PayloadResponse Insert(AccountCreateRequest model)
    {
        try
        {
            var currentUser = _loggedInUserService.GetLoggedInUser();

            if (currentUser == null)
            {
                return new PayloadResponse()
                {
                    IsSuccess = false,
                    PayloadType = "Account",
                    Message = "User not found"
                };
            }

            if (string.IsNullOrEmpty(currentUser.OrganizationId))
            {
                return new PayloadResponse()
                {
                    IsSuccess = false,
                    PayloadType = "Account",
                    Message = "User is not assigned with any organization!"
                };
            }

            if (string.IsNullOrEmpty(model.Type) || string.IsNullOrEmpty(model.AccountOrganizationName) || string.IsNullOrEmpty(model.AccountNumber))
            {
                return new PayloadResponse()
                {
                    IsSuccess = false,
                    PayloadType = "Account",
                    Message = "Account type, organization name and number can't be empty!"
                };
            }

            var account = new Account
            {
                Type = model.Type,
                AccountOrganizationName = model.AccountOrganizationName,
                AccountType = model.AccountType,
                AccountName = model.AccountName,
                AccountNumber = model.AccountNumber,
                BranchName = model.BranchName,
                BranchCode = model.BranchCode,
                RoutingNumber = model.RoutingNumber,
                District = model.District,
                OrganizationId = string.IsNullOrEmpty(model.OrganizationId) ?
                    currentUser.OrganizationId : model.OrganizationId
            };

            _accountRepository.Insert(account);
            _accountRepository.SaveChanges();

            return new PayloadResponse()
            {
                IsSuccess = true,
                PayloadType = "Account",
                Message = "Account created successfully"
            };
        }
        catch (Exception ex)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                PayloadType = "Account",
                Message = $"Account creation has been failed because {ex.Message}"
            };
        }
    }

    public PayloadResponse Update(AccountUpdateRequest model)
    {
        try
        {
            var currentUser = _loggedInUserService.GetLoggedInUser();

            if (currentUser == null)
            {
                return new PayloadResponse()
                {
                    IsSuccess = false,
                    PayloadType = "Account",
                    Message = "User not found"
                };
            }

            if (string.IsNullOrEmpty(currentUser.OrganizationId))
            {
                return new PayloadResponse()
                {
                    IsSuccess = false,
                    PayloadType = "Account",
                    Message = "User is not assigned with any organization!"
                };
            }

            if (string.IsNullOrEmpty(model.Type) || string.IsNullOrEmpty(model.AccountOrganizationName) || string.IsNullOrEmpty(model.AccountNumber))
            {
                return new PayloadResponse()
                {
                    IsSuccess = false,
                    PayloadType = "Account",
                    Message = "Account type, organization name and number can't be empty!"
                };
            }

            var account = _accountRepository
                .GetConditional(a => a.Id == model.Id);

            if (account == null)
            {
                return new PayloadResponse()
                {
                    IsSuccess = false,
                    PayloadType = "Account",
                    Message = "Account not found"
                };
            }

            account.Type = model.Type;
            account.AccountOrganizationName = model.AccountOrganizationName;
            account.AccountType = model.AccountType;
            account.AccountName = model.AccountName;
            account.AccountNumber = model.AccountNumber;
            account.BranchName = model.BranchName;
            account.BranchCode = model.BranchCode;
            account.RoutingNumber = model.RoutingNumber;
            account.District = model.District;

            _accountRepository.Update(account);
            _accountRepository.SaveChanges();

            return new PayloadResponse()
            {
                IsSuccess = true,
                PayloadType = "Account",
                Message = "Account updated successfully"
            };
        }
        catch (Exception ex)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                PayloadType = "Account",
                Message = $"Account update has been failed because {ex.Message}"
            };
        }
    }

    public PayloadResponse GetById(string id)
    {
        var currentUser = _loggedInUserService.GetLoggedInUser();

        if (currentUser == null)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                PayloadType = "Account",
                Message = "User not found"
            };
        }

        if (string.IsNullOrEmpty(currentUser.OrganizationId))
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                PayloadType = "Account",
                Message = "User is not assigned with any organization!"
            };
        }

        var account = _accountRepository
            .GetAll()
            .Where(a => a.Id == id)
            .Include(a => a.Organization)
            .FirstOrDefault();

        if (account == null)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                PayloadType = "Account",
                Message = "Account not found"
            };
        }

        return new PayloadResponse()
        {
            IsSuccess = true,
            PayloadType = "Account",
            Message = "Account retrieved successfully",
            Content = account
        };
    }

    public PayloadResponse GetAll(AccountDataFilter filter)
    {
        try
        {
            var currentUser = _loggedInUserService
                .GetLoggedInUser();

            if (currentUser is null)
            {
                return new PayloadResponse()
                {
                    IsSuccess = false,
                    PayloadType = "Account",
                    Message = "Current User not found!"
                };
            }

            var condition = new List<string>();
            var extraCondition = $@"ORDER BY CreateTime desc
                                    OFFSET ({filter.PageNo} - 1) * {filter.PageSize} ROWS
                                    FETCH NEXT {filter.PageSize} ROWS ONLY";

            if (!currentUser.IsSuperAdmin)
            {
                if (string.IsNullOrEmpty(currentUser.OrganizationId))
                {
                    return new PayloadResponse()
                    {
                        IsSuccess = false,
                        PayloadType = "Account",
                        Message = "Current User is not associated with any organization!"
                    };
                }

                filter.OrganizationId = currentUser.OrganizationId;
            }

            if (!string.IsNullOrEmpty(filter.SearchQuery))
            {
                condition.Add($" (Type like '%{filter.SearchQuery}%' or AccountOrganizationName like '%{filter.SearchQuery}%' or AccountType like '%{filter.SearchQuery}%' or AccountName like '%{filter.SearchQuery}%' or AccountNumber like '%{filter.SearchQuery}%' or BranchName like '%{filter.SearchQuery}%' or BranchCode like '%{filter.SearchQuery}%' or RoutingNumber like '%{filter.SearchQuery}%' or District like '%{filter.SearchQuery}%') ");
            }

            if (!string.IsNullOrEmpty(filter.OrganizationId))
            {
                condition.Add($" OrganizationId = '{filter.OrganizationId}'");
            }

            var whereCondition = _commonService.GenerateWhereConditionFromConditionList(condition);

            var rowCount = _commonService.GetRowCountForData("Account", whereCondition);

            var finalQueryData = _commonService.GetFinalData<Account>("Account", whereCondition, extraCondition);

            var accountIds = finalQueryData.Select(q => q.Id).ToList();

            var accountData = _accountRepository.GetAll()
                .Where(u => accountIds.Contains(u.Id))
                .Include(u => u.Organization)
                .ToList();

            return new PayloadResponse()
            {
                IsSuccess = true,
                PayloadType = "Account",
                Content = new { data = accountData, rowCount },
                Message = "Account data fetch is successful"
            };
        }
        catch (Exception ex)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                PayloadType = "Account",
                Message = $"Account fetching is failed because {ex.Message}!"
            };
        }
    }

    public PayloadResponse Delete(string id)
    {
        try
        {
            var account = _accountRepository
                .GetConditional(a => a.Id == id);

            if (account == null)
            {
                return new PayloadResponse()
                {
                    IsSuccess = false,
                    PayloadType = "Account",
                    Message = "Account not found"
                };
            }

            _accountRepository.Delete(account);
            _accountRepository.SaveChanges();

            return new PayloadResponse()
            {
                IsSuccess = true,
                PayloadType = "Account",
                Message = "Account has been deleted successfully"
            };
        }
        catch (Exception ex)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                PayloadType = "Account",
                Message = $"Account deletion is failed! because {ex.Message}"
            };
        }
    }

    public PayloadResponse Activate(AccountActivationDto accountActivation)
    {
        var currentUser = _loggedInUserService
            .GetLoggedInUser();

        if (currentUser is null)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                PayloadType = "Account",
                Message = "Current User not found!"
            };
        }

        if (currentUser.UserType != UserTypes.Admin && currentUser.UserType != UserTypes.SuperAdmin)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                PayloadType = "Account",
                Message = "Current User is not authorized to activate accounts!"
            };
        }

        var account = _accountRepository
            .GetConditional(a => a.Id == accountActivation.AccountId);

        if (account == null)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                PayloadType = "Account",
                Message = "Account not found"
            };
        }

        if (account.IsActive)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                PayloadType = "Account",
                Message = "Account is already active"
            };
        }

        account.IsActive = true;

        _accountRepository.Update(account);
        _accountRepository.SaveChanges();

        return new PayloadResponse()
        {
            IsSuccess = true,
            PayloadType = "Account",
            Message = "Account activated successfully"
        };
    }

    public PayloadResponse Deactivate(AccountActivationDto accountActivation)
    {
        var currentUser = _loggedInUserService
            .GetLoggedInUser();

        if (currentUser is null)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                PayloadType = "Account",
                Message = "Current User not found!"
            };
        }

        if (currentUser.UserType != UserTypes.Admin && currentUser.UserType != UserTypes.SuperAdmin)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                PayloadType = "Account",
                Message = "Current User is not authorized to deactivate accounts!"
            };
        }

        var account = _accountRepository
            .GetConditional(a => a.Id == accountActivation.AccountId);

        if (account == null)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                PayloadType = "Account",
                Message = "Account not found"
            };
        }

        if (!account.IsActive)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                PayloadType = "Account",
                Message = "Account is already inactive"
            };
        }

        account.IsActive = false;

        _accountRepository.Update(account);
        _accountRepository.SaveChanges();

        return new PayloadResponse()
        {
            IsSuccess = true,
            PayloadType = "Account",
            Message = "Account deactivated successfully"
        };
    }
}
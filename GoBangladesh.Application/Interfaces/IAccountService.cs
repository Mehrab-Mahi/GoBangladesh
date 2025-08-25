using GoBangladesh.Application.DTOs.Account;
using GoBangladesh.Application.ViewModels;

namespace GoBangladesh.Application.Interfaces;

public interface IAccountService
{
    PayloadResponse Insert(AccountCreateRequest model);
    PayloadResponse Update(AccountUpdateRequest model);
    PayloadResponse GetById(string id);
    PayloadResponse GetAll(AccountDataFilter filter);
    PayloadResponse Delete(string id);
    PayloadResponse Activate(AccountActivationDto accountActivation);
    PayloadResponse Deactivate(AccountActivationDto accountActivation);
    PayloadResponse GetByOrganizationId(string organizationId);
}
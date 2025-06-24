using System.Collections.Generic;
using GoBangladesh.Application.ViewModels;
using GoBangladesh.Domain.Entities;

namespace GoBangladesh.Application.Interfaces
{
    public interface IContactService
    {
        PayloadResponse Create(Contact contactData);
        object GetAll(string contactType, int pageNo, int pageSize);
        PayloadResponse ReadContact(string id);
        ContactVm Get(string id);
    }
}
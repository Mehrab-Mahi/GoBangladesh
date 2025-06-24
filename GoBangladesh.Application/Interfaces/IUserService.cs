using GoBangladesh.Application.ViewModels;
using GoBangladesh.Domain.Entities;
using System.Collections.Generic;

namespace GoBangladesh.Application.Interfaces
{
    public interface IUserService
    {
        User Get(AuthRequest model);
        UserCreationVm GetById(string id);
        PayloadResponse Update(UserCreationVm User);
        object GetAll();
        PayloadResponse Insert(UserCreationVm model);
        public bool Delete(string id, string table);
        PayloadResponse DeleteUser(string id);
        PayloadResponse ChangePassword(ChangePassword changePassword);
    }
}
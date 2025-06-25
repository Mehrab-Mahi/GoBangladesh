using GoBangladesh.Application.DTOs.Passenger;
using GoBangladesh.Application.ViewModels;
using GoBangladesh.Domain.Entities;
using System.Collections.Generic;

namespace GoBangladesh.Application.Interfaces
{
    public interface IUserService
    {
        User Get(AuthRequest model);
        UserCreationVm GetById(string id);
        object GetAll();
        public bool Delete(string id, string table);
        PayloadResponse DeleteUser(string id);
        PayloadResponse ChangePassword(ChangePassword changePassword);
    }
}
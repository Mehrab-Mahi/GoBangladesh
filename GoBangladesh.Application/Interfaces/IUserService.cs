using GoBangladesh.Application.DTOs;
using GoBangladesh.Application.DTOs.Passenger;
using GoBangladesh.Application.ViewModels;
using GoBangladesh.Domain.Entities;
using System.Collections.Generic;

namespace GoBangladesh.Application.Interfaces
{
    public interface IUserService
    {
        List<User> Get(AuthRequest model);
        UserCreationVm GetById(string id);
        object GetAll();
        public bool Delete(string id, string table);
        PayloadResponse DeleteUser(string id);
        PayloadResponse ChangePassword(ChangePassword changePassword);
        PayloadResponse ForgotPassword(ForgotPassword forgotPassword);
        PayloadResponse DeleteUserImage(DeleteFileByUrl fileUrl);
        PayloadResponse DeactivateAccount(UserAccountActivationDto model);
        void Update(User user);
        void UpdateUserCardToInUse(string userId);
        PayloadResponse ActivateAccount(UserAccountActivationDto model);
        bool CheckIfAnUserIsActivated(string userId);
    }
}
using GoBangladesh.Application.Interfaces;
using GoBangladesh.Application.ViewModels;
using GoBangladesh.Domain.Entities;
using GoBangladesh.Domain.Interfaces;
using System;
using System.Linq;

namespace GoBangladesh.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IRepository<User> _userRepo;
        private readonly ILoggedInUserService _loggedInUserService;
        public UserService(IRepository<User> userRepo,
            ILoggedInUserService loggedInUserService)
        {
            _userRepo = userRepo;
            _loggedInUserService = loggedInUserService;
        }

        public User Get(AuthRequest model)
        {
            var user = _userRepo.GetConditional(u => u.MobileNumber == model.MobileNumber || u.EmailAddress == model.Email); 

            return user;
        }

        public object GetAll()
        {
            return _userRepo.GetAll().ToList();
        }

        public UserCreationVm GetById(string id)
        {
            var user = _userRepo.GetConditional(u => u.Id == id);

            return new UserCreationVm();
        }

        private string GeneratePassword(string password)
        {
            var defaultPass = Guid.NewGuid().ToString("N");
            if (!string.IsNullOrEmpty(password))
            {
                defaultPass = password;
            }
            return BCrypt.Net.BCrypt.HashPassword(defaultPass, workFactor: 12);
        }

        public bool Delete(string id, string table)
        {
            try
            {
                _userRepo.Delete(id);
                _userRepo.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public PayloadResponse DeleteUser(string id)
        {
            var model = _userRepo.GetConditional(u => u.Id == id);

            if (model is null)
            {
                return new PayloadResponse()
                {
                    IsSuccess = false,
                    Message = "User not found!"
                };
            }

            _userRepo.Delete(model);
            _userRepo.SaveChanges();

            return new PayloadResponse()
            {
                IsSuccess = true,
                Message = "User has been deleted successfully!"
            };
        }

        public PayloadResponse ChangePassword(ChangePassword changePassword)
        {
            try
            {
                var currentUser = _loggedInUserService.GetLoggedInUser();

                if (!OldPasswordIsCorrect(changePassword.OldPassword, currentUser))
                {
                    return new PayloadResponse()
                    {
                        IsSuccess = false,
                        Message = "Old password is not correct!"
                    };
                }

                if (IfNewPasswordNotSame(changePassword.NewPassword, changePassword.ConfirmNewPassword))
                {
                    return new PayloadResponse()
                    {
                        IsSuccess = false,
                        Message = "New password and Confirm New Password is not matched!"
                    };
                }

                var newPasswordHash = GeneratePassword(changePassword.NewPassword);

                currentUser.PasswordHash = newPasswordHash;

                _userRepo.Update(currentUser);
                _userRepo.SaveChanges();

                return new PayloadResponse()
                {
                    IsSuccess = true,
                    Message = "Password changed successfully!"
                };
            }
            catch (Exception ex)
            {
                return new PayloadResponse()
                {
                    IsSuccess = false,
                    Message = $"Exception {ex.Message}"
                };
            }
        }

        private bool IfNewPasswordNotSame(string newPassword, string confirmNewPassword)
        {
            if (newPassword != confirmNewPassword) return true;
            return false;
        }

        private bool OldPasswordIsCorrect(string oldPassword, User currentUser)
        {
            return BCrypt.Net.BCrypt.Verify(oldPassword, currentUser.PasswordHash);
        }
    }
}
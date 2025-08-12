using GoBangladesh.Application.Interfaces;
using GoBangladesh.Application.ViewModels;
using GoBangladesh.Domain.Entities;
using GoBangladesh.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using GoBangladesh.Application.DTOs;
using GoBangladesh.Application.Util;

namespace GoBangladesh.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IRepository<User> _userRepo;
        private readonly ILoggedInUserService _loggedInUserService;
        private readonly ICommonService _commonService;
        private readonly ICardService _cardService;
        private readonly ITripService _tripService;
        private readonly ISessionService _sessionService;
        public UserService(IRepository<User> userRepo,
            ILoggedInUserService loggedInUserService,
            ICommonService commonService, ICardService cardService,
            ITripService tripService,
            ISessionService sessionService)
        {
            _userRepo = userRepo;
            _loggedInUserService = loggedInUserService;
            _commonService = commonService;
            _cardService = cardService;
            _tripService = tripService;
            _sessionService = sessionService;
        }

        public List<User> Get(AuthRequest model)
        {
            var users = new List<User>();

            if (!string.IsNullOrEmpty(model.MobileNumber))
            {
                users = _userRepo
                    .GetAll()
                    .Where(u => u.MobileNumber == model.MobileNumber)
                    .Include(u => u.Organization)
                    .ToList();
            }

            else if (!string.IsNullOrEmpty(model.Email))
            {
                users = _userRepo
                    .GetAll()
                    .Where(u => u.EmailAddress == model.Email)
                    .Include(u => u.Organization)
                    .ToList();
            }

            return users;
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

        public PayloadResponse ForgotPassword(ForgotPassword forgotPassword)
        {
            try
            {
                var user = _userRepo.GetConditional(u => u.MobileNumber == forgotPassword.MobileNumber);

                if (user == null)
                {
                    return new PayloadResponse()
                    {
                        IsSuccess = false,
                        Message = "User with this mobile number is not found!"
                    };
                }

                if (forgotPassword.NewPassword != forgotPassword.ConfirmNewPassword)
                {
                    return new PayloadResponse()
                    {
                        IsSuccess = false,
                        Message = "Password not matched!"
                    };
                }

                var newPasswordHash = GeneratePassword(forgotPassword.NewPassword);

                user.PasswordHash = newPasswordHash;

                _userRepo.Update(user);
                _userRepo.SaveChanges();

                return new PayloadResponse()
                {
                    IsSuccess = true,
                    Message = "Password has been changed!"
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

        public PayloadResponse DeleteUserImage(DeleteFileByUrl fileUrl)
        {
            try
            {
                var user = _userRepo.GetConditional(u => u.Id == fileUrl.UserId);

                if(user == null)
                {
                    return new PayloadResponse()
                    {
                        IsSuccess = false,
                        Message = "User not found!"
                    };
                }
                user.ImageUrl = null;

                _userRepo.Update(user);
                _userRepo.SaveChanges();

                if(!string.IsNullOrEmpty(fileUrl.Url))
                {
                    _commonService.DeleteFile(fileUrl.Url);
                }

                return new PayloadResponse() 
                { 
                    IsSuccess = true,
                    Message = "File removed successfully!"
                };
            }
            catch(Exception ex)
            {
                return new PayloadResponse()
                {
                    IsSuccess = false,
                    Message = $"File delete failed because {ex.Message}!"
                };
            }
        }

        public PayloadResponse DeactivateAccount(UserAccountActivationDto model)
        {
            var user = _userRepo.GetConditional(u => u.Id == model.UserId);

            if (user == null)
            {
                return new PayloadResponse()
                {
                    IsSuccess = false,
                    Message = "User not found!"
                };
            }

            var runningStatus = CheckIfAnyTripOrSessionRunning(user);

            if (!runningStatus.IsSuccess)
            {
                return runningStatus;
            }

            user.IsActive = false;

            _userRepo.Update(user);
            _userRepo.SaveChanges();

            if (user.UserType is UserTypes.Public or UserTypes.Private)
            {
                var card = _cardService.GetPassengerCardDetailByPassengerId(model.UserId);

                if (card != null)
                {
                    _cardService.UpdateCardStatus(card.CardNumber, CardStatus.Paused);
                }
            }

            return new PayloadResponse()
            {
                IsSuccess = true,
                Message = "User account has been deactivated successfully!"
            };
        }

        private PayloadResponse CheckIfAnyTripOrSessionRunning(User user)
        {
            if (user.UserType is UserTypes.Private or UserTypes.Public)
            {
                var card = _cardService.GetPassengerCardDetailByPassengerId(user.Id);

                if(card != null)
                {
                    var trip = _tripService.GetRunningTripByCardNumber(card.Id);

                    if (trip != null)
                    {
                        return new PayloadResponse()
                        {
                            IsSuccess = false,
                            Message = "You have an ongoing trip. Please end this trip before deleting the account."
                        };
                    }

                    return new PayloadResponse()
                    {
                        IsSuccess = true
                    };
                }
                return new PayloadResponse()
                {
                    IsSuccess = true
                };
            }

            if (user.UserType == UserTypes.Staff)
            {
                var session = _sessionService.CheckIfSessionRunningForLoggedInUser(user.Id);

                if (!session.IsSuccess)
                {
                    return new PayloadResponse()
                    {
                        IsSuccess = false,
                        Message = "The staff has an ongoing session. Please allow them to end the session before deactivating the account."
                    };
                }

                return new PayloadResponse()
                {
                    IsSuccess = true
                };
            }

            return new PayloadResponse()
            {
                IsSuccess = true
            };
        }

        public void Update(User user)
        {
            _userRepo.Update(user);
            _userRepo.SaveChanges();
        }

        public void UpdateUserCardToInUse(string userId)
        {
            var card = _cardService.GetPassengerCardDetailByPassengerId(userId);

            if (card != null)
            {
                _cardService.UpdateCardStatus(card.CardNumber, CardStatus.InUse);
            }
        }

        public PayloadResponse ActivateAccount(UserAccountActivationDto model)
        {
            var user = _userRepo.GetConditional(u => u.Id == model.UserId);

            if (user == null)
            {
                return new PayloadResponse()
                {
                    IsSuccess = false,
                    Message = "User not found!"
                };
            }

            if (user.IsActive)
            {
                return new PayloadResponse()
                {
                    IsSuccess = false,
                    Message = "User is already active!"
                };
            }

            if (user.UserType is UserTypes.Public or UserTypes.Private && user.Id == user.LastModifiedBy)
            {
                return new PayloadResponse()
                {
                    IsSuccess = false,
                    Message = "User can't be activated!"
                };
            }

            user.IsActive = true;

            _userRepo.Update(user);
            _userRepo.SaveChanges();

            if (user.UserType is UserTypes.Public or UserTypes.Private)
            {
                var card = _cardService.GetPassengerCardDetailByPassengerId(model.UserId);

                if (card != null)
                {
                    _cardService.UpdateCardStatus(card.CardNumber, CardStatus.InUse);
                }
            }

            return new PayloadResponse()
            {
                IsSuccess = true,
                Message = "User account has been activated successfully!"
            };
        }

        public bool CheckIfAnUserIsActivated(string userId)
        {
            var user = _userRepo
                .GetAll()
                .Include(u => u.Organization)
                .FirstOrDefault(u => u.Id == userId);

            if (user == null)
            {
                return false;
            }

            return user.IsActive && user.Organization.IsActive;
        }
    }
}
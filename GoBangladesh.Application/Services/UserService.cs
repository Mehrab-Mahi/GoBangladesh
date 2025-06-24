using GoBangladesh.Application.Interfaces;
using GoBangladesh.Application.ViewModels;
using GoBangladesh.Domain.Entities;
using GoBangladesh.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace GoBangladesh.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IRepository<User> _userRepo;
        private readonly IFileService _fileService;
        private readonly ILoggedInUserService _loggedInUserService;
        public UserService(IRepository<User> userRepo,
            IFileService fileService, 
            ILoggedInUserService loggedInUserService)
        {
            _userRepo = userRepo;
            _fileService = fileService;
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

            return new UserCreationVm() { };
        }

        public PayloadResponse Insert(UserCreationVm user)
        {
            if (IfDuplicateUser(user.MobileNumber))
            {
                return new PayloadResponse
                {
                    IsSuccess = false,
                    PayloadType = "User Creation",
                    Content = null,
                    Message = "User with this mobile number already exists!"
                };
            }

            try
            {
                var model = new User()
                {
                    DateOfBirth = user.DateOfBirth,
                    MobileNumber = user.MobileNumber,
                    Address = user.Address,
                    Gender = user.Gender,
                    UserType = user.UserType,
                    ImageUrl = user.ImageUrl,
                    PassengerId = user.PassengerId
                };

                var currentUser = _loggedInUserService.GetLoggedInUser();

                model.PasswordHash = GeneratePassword(user.Password);
                model.ImageUrl = UploadAndGetImageUrl(user.ProfilePicture);
                model.CreatedBy = currentUser is null ? "" : currentUser.Id;
                model.LastModifiedBy = currentUser is null ? "" : currentUser.Id;

                _userRepo.InsertWithUserData(model);
                _userRepo.SaveChanges();

                return new PayloadResponse
                {
                    IsSuccess = true,
                    PayloadType = "User Creation",
                    Content = null,
                    Message = "User Creation has been successful"
                };
            }
            catch (Exception ex)
            {
                return new PayloadResponse
                {
                    IsSuccess = false,
                    PayloadType = "User Creation",
                    Content = null,
                    Message = $"User Creation become unsuccessful because {ex.Message}"
                };
            }
        }

        private string UploadNidData(List<IFormFile> userNid)
        {
            if (userNid is null || userNid.Count == 0) return string.Empty;

            var nidUrlList = new List<string>();

            foreach (var nid in userNid)
            {
                var fileName = GetFileName(nid.FileName);

                var filePath = UploadFile(fileName, "ProfilePicture", nid);

                nidUrlList.Add(filePath);
            }

            return string.Join(",", nidUrlList);
        }

        private bool IfDuplicateUser(string mobileNumber)
        {
            var user = _userRepo.GetAll().FirstOrDefault(u => u.MobileNumber == mobileNumber);

            return user is not null;
        }

        private string UploadAndGetImageUrl(IFormFile userProfilePicture)
        {
            if (userProfilePicture is null) return string.Empty;

            var fileName = GetFileName(userProfilePicture.FileName);

            return UploadFile(fileName, "ProfilePicture", userProfilePicture);
        }

        private string UploadFile(string fileName, string fileSavePath, IFormFile file)
        {
            var path = Path.Combine(_fileService.GetRootPath(), fileSavePath);
            _fileService.CreateDirectoryIfNotExists(path);
            var filePath = Path.Combine(path, fileName);
            _fileService.SaveFile(filePath, file); 
            return Path.Combine(fileSavePath, fileName);
        }

        private string GetFileName(string fileName)
        {
            return Guid.NewGuid().ToString("N") + "-" + fileName;
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

        public PayloadResponse Update(UserCreationVm user)
        {
            var model = _userRepo.GetConditional(u => u.Id == user.Id);
            try
            {
                if (user.MobileNumber != model.MobileNumber)
                {
                    if (IfDuplicateUser(user.MobileNumber))
                    {
                        return new PayloadResponse
                        {
                            IsSuccess = false,
                            PayloadType = "User Update",
                            Content = null,
                            Message = "User with the mobile number already exists!"
                        };
                    }
                }

                model.DateOfBirth = user.DateOfBirth;
                model.MobileNumber = user.MobileNumber;
                model.Address = user.Address;
                model.Gender = user.Gender;
                model.UserType = user.UserType;

                if (user.ProfilePicture is { Length: > 0 })
                {
                    _fileService.DeleteFile(model.ImageUrl);
                    model.ImageUrl = UploadAndGetImageUrl(user.ProfilePicture);
                }

                _userRepo.Update(model);
                _userRepo.SaveChanges();

                return new PayloadResponse
                {
                    IsSuccess = true,
                    PayloadType = "User Update",
                    Content = null,
                    Message = "User Update successful"
                };
            }
            catch (Exception)
            {
                return new PayloadResponse
                {
                    IsSuccess = false,
                    PayloadType = "User Update",
                    Content = null,
                    Message = "User Update become failed"
                };
            }
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

        public PayloadResponse ApproveUser(string id)
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

            model.IsApproved = true;

            _userRepo.Update(model);
            _userRepo.SaveChanges();

            return new PayloadResponse()
            {
                IsSuccess = true,
                Message = "User has been approved successfully!"
            };
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

            model.IsApproved = false;

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
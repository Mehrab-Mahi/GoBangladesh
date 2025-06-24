using GoBangladesh.Application.Interfaces;
using GoBangladesh.Application.ViewModels;
using GoBangladesh.Domain.Entities;
using GoBangladesh.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using GoBangladesh.Application.Util;
using Microsoft.AspNetCore.Http;
using System.IO;
using Microsoft.AspNetCore.Identity;

namespace GoBangladesh.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IRepository<User> _userRepo;
        private readonly IRepository<Location> _locationRepository;
        private readonly IRepository<Campaign> _campaignRepository;
        private readonly IFileService _fileService;
        private readonly ILoggedInUserService _loggedInUserService;
        public UserService(IRepository<User> userRepo,
            IFileService fileService, 
            IRepository<Location> locationRepository,
            ILoggedInUserService loggedInUserService,
            IRepository<Campaign> campaignRepository)
        {
            _userRepo = userRepo;
            _fileService = fileService;
            _locationRepository = locationRepository;
            _loggedInUserService = loggedInUserService;
            _campaignRepository = campaignRepository;
        }

        public User Get(AuthRequest model)
        {
            var user = _userRepo.GetConditional(u => u.MobileNumber == model.MobileNumber && u.DateOfBirth == model.DateOfBirth); 

            return user;
        }

        public object GetAll(UserFilter filter)
        {
            var allUser = _userRepo
                .GetAll().Where(u => !u.IsSuperAdmin);

            if (!string.IsNullOrEmpty(filter.BloodGroup))
            {
                allUser = FilterByBloodGroup(allUser, filter.BloodGroup);
            }

            if (!string.IsNullOrEmpty(filter.Upazila))
            {
                allUser = FilterByUpazila(allUser, filter.Upazila);
            }

            if (!string.IsNullOrEmpty(filter.Union))
            {
                allUser = FilterByUnion(allUser, filter.Union);
            }
            
            if (!string.IsNullOrEmpty(filter.GoBangladeshStatus))
            {
                allUser = FilterByGoBangladeshStatus(allUser, filter.GoBangladeshStatus);
            }

            if (!string.IsNullOrEmpty(filter.UserType))
            {
                allUser = FilterByUserType(allUser, filter.UserType);
            }
            
            if (filter.IsApproved != null)
            {
                allUser = FilterByApproval(allUser, filter.IsApproved.Value);
            }

            if (!string.IsNullOrEmpty(filter.Gender))
            {
                allUser = FilterByGender(allUser, filter.Gender);
            }

            if (filter.StartAge is null || filter.EndAge is null)
            {
                filter.StartAge = 0;
                filter.EndAge = 100;
            }

            var startDob = GetDateDifference(filter.StartAge.Value);
            var endDob = GetDateDifference(filter.EndAge.Value);

            allUser = FilterByDate(allUser, startDob, endDob);

            var totalRowCount = allUser.Count();

            if (filter.PageNo is null || filter.PageSize is null)
            {
                filter.PageNo = 0;
                filter.PageSize = 10;
            }

            allUser = allUser
                .OrderByDescending(u => u.CreateTime)
                .Skip((filter.PageNo.Value - 1) * filter.PageSize.Value)
                .Take(filter.PageSize.Value);

            var userData = (from user in allUser
                join district in _locationRepository.GetAll() on user.District equals district.Id
                join upazila in _locationRepository.GetAll() on user.Upazila equals upazila.Id
                join union in _locationRepository.GetAll() on user.Union equals union.Id
                select new UserCreationVm()
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    BloodGroup = user.BloodGroup,
                    DateOfBirth = user.DateOfBirth,
                    MobileNumber = user.MobileNumber,
                    District = user.District,
                    DistrictName = district.Name,
                    Upazila = user.Upazila,
                    UpazilaName = upazila.Name,
                    Union = user.Union,
                    UnionName = union.Name,
                    Address = user.Address,
                    FatherName = user.FatherName,
                    MotherName = user.MotherName,
                    BloodDonationStatus = user.BloodDonationStatus,
                    Gender = user.Gender,
                    UserType = user.UserType,
                    LastDonationTime = user.LastDonationTime,
                    ImageUrl = user.ImageUrl,
                    BloodDonationCount = user.BloodDonationCount,
                    IsApproved = user.IsApproved,
                    PhysicalComplexity = user.PhysicalComplexity,
                    NidUrls = GetNidUrlsFromCommaSeparatedString(user.NidUrls),
                    Code = user.Code
                })
                .ToList();

            return new
            {
                data = userData,
                rowCount = totalRowCount
            };
        }

        private IQueryable<User> FilterByGender(IQueryable<User> allUser, string gender)
        {
            return allUser.Where(u => u.Gender == gender);
        }

        private IQueryable<User> FilterByApproval(IQueryable<User> allUser, bool isApproved)
        {
            return allUser.Where(u => u.IsApproved == isApproved);
        }

        private static List<string> GetNidUrlsFromCommaSeparatedString(string nidUrls)
        {
            if(string.IsNullOrEmpty(nidUrls)) return new List<string>();

            return nidUrls
                .Split(",")
                .ToList();
        }

        private IQueryable<User> FilterByGoBangladeshStatus(IQueryable<User> allUser, string bloodDonationStatus)
        {
            return allUser.Where(u => u.BloodDonationStatus == bloodDonationStatus);
        }

        private static IQueryable<User> FilterByUserType(IQueryable<User> allUser, string userType)
        {
            return allUser.Where(u => u.UserType == userType);
        }

        private static DateTime GetDateDifference(int ageToReduce)
        {
            var date = DateTime.Now.AddYears((0 - ageToReduce));

            return date;
        }

        private IQueryable<User> FilterByDate(IQueryable<User> user, DateTime startDob, DateTime endDob)
        {
            return user.Where(u => u.Dob <= startDob && u.Dob >= endDob);
        }

        private IQueryable<User> FilterByUnion(IQueryable<User> user, string union)
        {
            return user.Where(u => u.Union == union);
        }

        private IQueryable<User> FilterByUpazila(IQueryable<User> user, string upazila)
        {
            return user.Where(u => u.Upazila == upazila);
        }

        private IQueryable<User> FilterByBloodGroup(IQueryable<User> user, string bloodGroup)
        {
            return user.Where(u => u.BloodGroup == bloodGroup);
        }

        public UserCreationVm GetById(string id)
        {
            var user = _userRepo.GetConditional(u => u.Id == id);

            if (user == null) return new UserCreationVm();

            var userFakeList = new List<User> { user };

            var mappedData = GetMappedData(userFakeList);

            return mappedData.FirstOrDefault();
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
                var serial = GetSerialNumber();

                var model = new User()
                {
                    FullName = user.FullName,
                    BloodGroup = user.BloodGroup,
                    DateOfBirth = user.DateOfBirth,
                    MobileNumber = user.MobileNumber,
                    District = user.District,
                    Upazila = user.Upazila,
                    Union = user.Union,
                    Address = user.Address,
                    FatherName = user.FatherName,
                    MotherName = user.MotherName,
                    BloodDonationStatus = user.BloodDonationStatus,
                    Gender = user.Gender,
                    UserType = user.UserType,
                    LastDonationTime = user.LastDonationTime,
                    ImageUrl = user.ImageUrl,
                    IsSuperAdmin = user.IsSuperAdmin,
                    BloodDonationCount = user.BloodDonationCount,
                    Dob = DateTime.Parse(user.DateOfBirth),
                    Serial = serial,
                    Code = serial.ToString("D6"),
                    InstituteName = user.InstituteName,
                    LeaderType = user.LeaderType,
                    CampaignId = user.CampaignId,
                    Designation = user.Designation
                };

                if (model.UserType != UserTypes.Admin)
                {
                    model.IsApproved = true;
                    user.Password = "123";
                }

                if ((model.UserType == UserTypes.Volunteer) || IsSelfRegistration())
                {
                    model.IsApproved = false;
                }

                if (model.UserType == UserTypes.Admin)
                {
                    model.IsApproved = true;
                    user.Password = "12345678";
                }

                var currentUser = _loggedInUserService.GetLoggedInUser();

                model.PasswordHash = GeneratePassword(user.Password);
                model.ImageUrl = UploadAndGetImageUrl(user.ProfilePicture);
                model.NidUrls = UploadNidData(user.Nid);
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

        private int GetSerialNumber()
        {
            var maxSerial = _userRepo.GetAll().Max(s => s.Serial);
            return maxSerial + 1;
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

        private bool IsSelfRegistration()
        {
            var user = _loggedInUserService.GetLoggedInUser();

            if (user is null) return true;

            return false;
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

                model.FullName = user.FullName;
                model.BloodGroup = user.BloodGroup;
                model.DateOfBirth = user.DateOfBirth;
                model.MobileNumber = user.MobileNumber;
                model.District = user.District;
                model.Upazila = user.Upazila;
                model.Union = user.Union;
                model.Address = user.Address;
                model.FatherName = user.FatherName;
                model.MotherName = user.MotherName;
                model.BloodDonationStatus = user.BloodDonationStatus;
                model.Gender = user.Gender;
                model.UserType = user.UserType;
                model.BloodDonationCount = user.BloodDonationCount;
                model.LastDonationTime = user.LastDonationTime;
                model.PhysicalComplexity = user.PhysicalComplexity;
                model.Dob = DateTime.Parse(user.DateOfBirth);
                model.InstituteName = user.InstituteName;
                model.LeaderType = user.LeaderType;
                model.Designation = user.Designation;

                if (user.ProfilePicture is { Length: > 0 })
                {
                    _fileService.DeleteFile(model.ImageUrl);
                    model.ImageUrl = UploadAndGetImageUrl(user.ProfilePicture);
                }

                if (user.Nid is not null && user.Nid.Count > 0)
                {
                    var previousUrls = model.NidUrls;

                    if (!string.IsNullOrEmpty(previousUrls))
                    {
                        var previousUrlList = previousUrls.Split(",").ToList();

                        foreach (var url in previousUrlList)
                        {
                            _fileService.DeleteFile(url);
                        }

                        model.NidUrls = UploadNidData(user.Nid);
                    }
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

        public UserTypeResponse GetUserTypeByPhoneNumberAndDob(AuthRequest model)
        {
            var data = _userRepo.GetConditional(u =>
                u.MobileNumber == model.MobileNumber && u.DateOfBirth == model.DateOfBirth);

            if (data is null)
            {
                return new UserTypeResponse()
                {
                    IsSuccess = false,
                    Message = "Mobile number and date of birth doesn't match!"
                };
            }

            return new UserTypeResponse()
            {
                UserType = data.UserType,
                IsSuccess = true,
                Message = "Mobile number and date of birth has been matched!"
            };
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

        public object GetUnapprovedVolunteer(int pageNo, int pageSize)
        {
            var unapprovedData = _userRepo
                .GetAll()
                .Where(u => u.IsApproved == false && u.UserType == UserTypes.Volunteer)
                .OrderByDescending(u => u.CreateTime);

            var totalRowCount = unapprovedData
                .Count();
            
            var mappedData = GetMappedData(unapprovedData.Skip((pageNo-1)*pageSize).Take(pageSize).ToList());

            return new
            {
                data = mappedData,
                rowCount = totalRowCount
            };
        }

        public object GetApprovedVolunteer(int pageNo, int pageSize)
        {
            var userData = _userRepo
                .GetAll()
                .Where(u => u.UserType == UserTypes.Volunteer && u.IsApproved == true)
                .OrderByDescending(u => u.LastModifiedTime);

            var totalRowCount = userData.Count();

            var mappedData = GetMappedData(userData
                .Skip((pageNo - 1) * pageSize)
                .Take(pageSize)
                .ToList());

            return new
            {
                data = mappedData,
                rowCount = totalRowCount
            };
        }

        public PayloadResponse DisapproveUser(string id)
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

            _userRepo.Update(model);
            _userRepo.SaveChanges();

            return new PayloadResponse()
            {
                IsSuccess = true,
                Message = "User has been disapproved successfully!"
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

        public object GetApprovedDonor(DonorFilter filter)
        {
            var allUser = _userRepo
            .GetAll().Where(u => u.UserType == "Donor" && u.IsApproved);

            return GetDonorData(allUser, filter);
        }

        public object GetUnapprovedDonor(DonorFilter filter)
        {
            var allUser = _userRepo
                .GetAll().Where(u => u.UserType == "Donor" && !u.IsApproved);

            return GetDonorData(allUser, filter);
        }

        public object GetAllAdmin(int pageNo, int pageSize)
        {
            var adminData = _userRepo
                .GetAll()
                .Where(u => u.UserType == "Admin" && !u.IsSuperAdmin);

            var totalRowCount = adminData.Count();
            var paginatedData = adminData
                .Skip((pageNo - 1) * pageSize)
                .Take(pageSize);

            var adminUserData = (from user in paginatedData
                                 join district in _locationRepository.GetAll() on user.District equals district.Id
                    join upazila in _locationRepository.GetAll() on user.Upazila equals upazila.Id
                    join union in _locationRepository.GetAll() on user.Union equals union.Id
                    select new UserCreationVm()
                    {
                        Id = user.Id,
                        FullName = user.FullName,
                        BloodGroup = user.BloodGroup,
                        DateOfBirth = user.DateOfBirth,
                        MobileNumber = user.MobileNumber,
                        District = user.District,
                        DistrictName = district.Name,
                        Upazila = user.Upazila,
                        UpazilaName = upazila.Name,
                        Union = user.Union,
                        UnionName = union.Name,
                        Address = user.Address,
                        FatherName = user.FatherName,
                        MotherName = user.MotherName,
                        BloodDonationStatus = user.BloodDonationStatus,
                        Gender = user.Gender,
                        UserType = user.UserType,
                        LastDonationTime = user.LastDonationTime,
                        ImageUrl = user.ImageUrl,
                        BloodDonationCount = user.BloodDonationCount,
                        IsApproved = user.IsApproved,
                        PhysicalComplexity = user.PhysicalComplexity,
                        NidUrls = GetNidUrlsFromCommaSeparatedString(user.NidUrls),
                        Code = user.Code,
                        LeaderType = user.LeaderType,
                        InstituteName = user.InstituteName
                    })
                .ToList();

            return new
            {
                data = adminUserData,
                rowCount = totalRowCount
            };
        }

        public object GetPermittedDonors(int pageNo, int pageSize)
        {
            var currentUser = _loggedInUserService.GetLoggedInUser();

            var donorData = _userRepo
                .GetAll()
                .Where(u => u.UserType == "Donor" && u.CreatedBy == currentUser.Id);

            var totalRowCount = donorData.Count();
            var paginatedData = donorData
                .Skip((pageNo - 1) * pageSize)
                .Take(pageSize);

            var donorUserData = (from user in paginatedData
                    join district in _locationRepository.GetAll() on user.District equals district.Id
                    join upazila in _locationRepository.GetAll() on user.Upazila equals upazila.Id
                    join union in _locationRepository.GetAll() on user.Union equals union.Id
                    join campaign in _campaignRepository.GetAll() on user.CampaignId equals campaign.Id
                    select new UserCreationVm()
                    {
                        Id = user.Id,
                        FullName = user.FullName,
                        BloodGroup = user.BloodGroup,
                        DateOfBirth = user.DateOfBirth,
                        MobileNumber = user.MobileNumber,
                        District = user.District,
                        DistrictName = district.Name,
                        Upazila = user.Upazila,
                        UpazilaName = upazila.Name,
                        Union = user.Union,
                        UnionName = union.Name,
                        Address = user.Address,
                        FatherName = user.FatherName,
                        MotherName = user.MotherName,
                        BloodDonationStatus = user.BloodDonationStatus,
                        Gender = user.Gender,
                        UserType = user.UserType,
                        LastDonationTime = user.LastDonationTime,
                        ImageUrl = user.ImageUrl,
                        BloodDonationCount = user.BloodDonationCount,
                        IsApproved = user.IsApproved,
                        PhysicalComplexity = user.PhysicalComplexity,
                        NidUrls = GetNidUrlsFromCommaSeparatedString(user.NidUrls),
                        Code = user.Code,
                        CampaignName = campaign.Name
                    })
                .ToList();

            return new
            {
                data = donorUserData,
                rowCount = totalRowCount
            };
        }

        public OfficialLeaderDto GetOfficialLeaders()
        {
            var dcOfficeLeaders = _userRepo
                .GetConditionalList(u => u.LeaderType == "Deputy Commissioner Official")
                .OrderBy(u => u.CreateTime)
                .Select(l => new LeaderDataDto()
                {
                        Id = l.Id,
                        FullName = l.FullName,
                        Gender = l.Gender,
                        InstituteName = l.InstituteName,
                        LeaderType = l.LeaderType,
                        ImageUrl = l.ImageUrl
                })
                .ToList();

            var civilOfficeLeaders = _userRepo
                .GetConditionalList(u => u.LeaderType == "Civil Surgeon Official")
                .OrderBy(u => u.CreateTime)
                .Select(l => new LeaderDataDto()
                {
                    Id = l.Id,
                    FullName = l.FullName,
                    Gender = l.Gender,
                    InstituteName = l.InstituteName,
                    LeaderType = l.LeaderType,
                    ImageUrl = l.ImageUrl
                })
                .ToList();

            return new OfficialLeaderDto()
            {
                DcOfficeLeaders = dcOfficeLeaders,
                CivilOfficeLeaders = civilOfficeLeaders
            };
        }

        public object GetScoutLeaders(int pageNo, int pageSize)
        {
            var allScoutLeaders = _userRepo
                .GetConditionalList(u => u.LeaderType == "Volunteer (Scout)")
                .OrderByDescending(u => u.CreateTime);

            var scoutLeaders = allScoutLeaders.Skip((pageNo - 1) * pageSize)
                .Take(pageSize)
                .Select(l => new LeaderDataDto()
                {
                    Id = l.Id,
                    FullName = l.FullName,
                    Gender = l.Gender,
                    InstituteName = l.InstituteName,
                    LeaderType = l.LeaderType,
                    ImageUrl = l.ImageUrl
                })
                .ToList();

            return new
            {
                data = scoutLeaders,
                rowCount = allScoutLeaders.Count()
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

        private object GetDonorData(IQueryable<User> allUser, DonorFilter filter)
        {
            if (!string.IsNullOrEmpty(filter.BloodGroup))
            {
                allUser = FilterByBloodGroup(allUser, filter.BloodGroup);
            }

            if (!string.IsNullOrEmpty(filter.Upazila))
            {
                allUser = FilterByUpazila(allUser, filter.Upazila);
            }

            if (!string.IsNullOrEmpty(filter.Union))
            {
                allUser = FilterByUnion(allUser, filter.Union);
            }

            if (!string.IsNullOrEmpty(filter.GoBangladeshStatus))
            {
                allUser = FilterByGoBangladeshStatus(allUser, filter.GoBangladeshStatus);
            }

            if (!string.IsNullOrEmpty(filter.Gender))
            {
                allUser = FilterByGender(allUser, filter.Gender);
            }

            if (filter.StartAge is null || filter.EndAge is null)
            {
                filter.StartAge = 0;
                filter.EndAge = 100;
            }

            var startDob = GetDateDifference(filter.StartAge.Value);
            var endDob = GetDateDifference(filter.EndAge.Value);

            allUser = FilterByDate(allUser, startDob, endDob);

            var totalRowCount = allUser.Count();

            if (filter.PageNo is null || filter.PageSize is null)
            {
                filter.PageNo = 0;
                filter.PageSize = 10;
            }

            allUser = allUser
            .OrderByDescending(u => u.CreateTime)
            .Skip((filter.PageNo.Value - 1) * filter.PageSize.Value)
                .Take(filter.PageSize.Value);

            var userData = (from user in allUser
                            join district in _locationRepository.GetAll() on user.District equals district.Id
                            join upazila in _locationRepository.GetAll() on user.Upazila equals upazila.Id
                            join union in _locationRepository.GetAll() on user.Union equals union.Id
                            select new UserCreationVm()
                            {
                                Id = user.Id,
                                FullName = user.FullName,
                                BloodGroup = user.BloodGroup,
                                DateOfBirth = user.DateOfBirth,
                                MobileNumber = user.MobileNumber,
                                District = user.District,
                                DistrictName = district.Name,
                                Upazila = user.Upazila,
                                UpazilaName = upazila.Name,
                                Union = user.Union,
                                UnionName = union.Name,
                                Address = user.Address,
                                FatherName = user.FatherName,
                                MotherName = user.MotherName,
                                BloodDonationStatus = user.BloodDonationStatus,
                                Gender = user.Gender,
                                UserType = user.UserType,
                                LastDonationTime = user.LastDonationTime,
                                ImageUrl = user.ImageUrl,
                                BloodDonationCount = user.BloodDonationCount,
                                IsApproved = user.IsApproved,
                                PhysicalComplexity = user.PhysicalComplexity,
                                NidUrls = GetNidUrlsFromCommaSeparatedString(user.NidUrls),
                                Code = user.Code
                            })
                .ToList();

            return new
            {
                data = userData,
                rowCount = totalRowCount
            };
        }

        private List<UserCreationVm> GetMappedData(List<User> userData)
        {
            var mappedUserData = (from user in userData
                join district in _locationRepository.GetAll() on user.District equals district.Id
                join upazila in _locationRepository.GetAll() on user.Upazila equals upazila.Id
                join union in _locationRepository.GetAll() on user.Union equals union.Id
                select new UserCreationVm()
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    BloodGroup = user.BloodGroup,
                    DateOfBirth = user.DateOfBirth,
                    MobileNumber = user.MobileNumber,
                    District = user.District,
                    DistrictName = district.Name,
                    Upazila = user.Upazila,
                    UpazilaName = upazila.Name,
                    Union = user.Union,
                    UnionName = union.Name,
                    Address = user.Address,
                    FatherName = user.FatherName,
                    MotherName = user.MotherName,
                    BloodDonationStatus = user.BloodDonationStatus,
                    Gender = user.Gender,
                    UserType = user.UserType,
                    LastDonationTime = user.LastDonationTime,
                    ImageUrl = user.ImageUrl,
                    IsSuperAdmin = user.IsSuperAdmin,
                    IsApproved = user.IsApproved,
                    PhysicalComplexity = user.PhysicalComplexity,
                    NidUrls = !string.IsNullOrEmpty(user.NidUrls) ? user
                        .NidUrls
                        .Split(",")
                        .ToList() : new List<string>(),
                    Serial = user.Serial,
                    Code = user.Code,
                    LeaderType = user.LeaderType,
                    InstituteName = user.InstituteName,
                    BloodDonationCount = user.BloodDonationCount,
                    Designation = user.Designation,
                    CreateTime = user.CreateTime,
                    CampaignId = user.CampaignId
                }).ToList();

            return mappedUserData;
        }
    }
}
using GoBangladesh.Application.Helper;
using GoBangladesh.Application.Interfaces;
using GoBangladesh.Application.ViewModels;
using GoBangladesh.Domain.Entities;
using GoBangladesh.Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace GoBangladesh.Application.Services
{
    public class AuthService : IAuthService
    {
        private IUserService _userService;
        private readonly AppSettings _appSettings;
        private IHttpContextAccessor _httpContextAccessor;
        private readonly IBaseRepository _repo;
        private readonly IRepository<AccessControl> _accessRepo;
        private readonly IOtpService _otpService;

        public AuthService(IUserService userService,
            IOptions<AppSettings> appSettings,
            IHttpContextAccessor httpContextAccessor,
            IBaseRepository repo,
            IRepository<AccessControl> accessRepo,
            IOtpService otpService)
        {
            _userService = userService;
            _appSettings = appSettings.Value;
            _httpContextAccessor = httpContextAccessor;
            _repo = repo;
            _accessRepo = accessRepo;
            _otpService = otpService;
        }

        public PayloadResponse Authenticate(AuthRequest model)
        {
            var user = _userService.Get(model);
            if (user == null)
            {
                return new PayloadResponse
                {
                    IsSuccess = false,
                    PayloadType = "authentication",
                    Content = null,
                    Message = "User not found!"
                };
            }
            if (!user.IsApproved)
            {
                return new PayloadResponse
                {
                    IsSuccess = false,
                    PayloadType = "authentication",
                    Content = null,
                    Message = "User not Approved!"
                };
            }

            var verification = (!string.IsNullOrEmpty(model.MobileNumber) && !string.IsNullOrEmpty(model.Otp)) ?
                _otpService.VerifyOtp(model.MobileNumber, model.Otp) :
                VerifyPassword(model.Password, user.PasswordHash);

            if (!verification.IsSuccess)
            {
                return new PayloadResponse
                {
                    IsSuccess = false,
                    PayloadType = "authentication",
                    Content = null,
                    Message = verification.Message
                };
            }
            var token = GenerateJwtToken(user);

            return new PayloadResponse
            {
                IsSuccess = true,
                PayloadType = "authentication",
                Content = new AuthResponse(token),
                Message = "Authentication successful"
            };
        }

        private string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
            var credentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new(ClaimTypes.Name, (user.Name)),
                    new(type: "UserId", user.Id),
                    new(type: "IsSuperAdmin", user.IsSuperAdmin.ToString()),
                    new(type: "Name", user.Name),
                    new(type: "UserType", user.UserType),
                    new(type: "OrganizationId", user.OrganizationId),
                    new(type: "OrganizationName", user.Organization.Name),
                    new(type: "OrganizationType", user.Organization.OrganizationType)
                }),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = credentials
            };
            var tokenValue = tokenHandler.CreateToken(tokenDescriptor);
            var token = tokenHandler.WriteToken(tokenValue);
            _httpContextAccessor.HttpContext.Session.SetString("token", token);
            _httpContextAccessor.HttpContext.Session.SetString("userType", user.UserType);
            return token;
        }

        public UserAuthVm ValidateToken(string authToken)
        {
            try
            {
                var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
                var tokenHandler = new JwtSecurityTokenHandler();
                var validationParameters = JwtAuthentication.GetValidatorParameters(key);

                SecurityToken validatedToken;
                var principal = tokenHandler.ValidateToken(authToken, validationParameters, out validatedToken);
                return FillAuthClaims(principal);
            }
            catch (Exception)
            {
                return new UserAuthVm { IsAuthenticate = false };
            }
        }

        private UserAuthVm FillAuthClaims(ClaimsPrincipal principal)
        {
            if (principal.Identity.IsAuthenticated)
            {
                var name = principal.Claims.FirstOrDefault(_ => _.Type == ClaimTypes.Name).Value;
                var id = principal.Claims.FirstOrDefault(_ => _.Type == "UserId").Value;
                var isSuperAdmin = principal.Claims.FirstOrDefault(_ => _.Type == "IsSuperAdmin").Value;
                var userType = principal.Claims.FirstOrDefault(_ => _.Type == "UserType").Value;
                return new UserAuthVm
                {
                    IsAuthenticate = true,
                    Name = name,
                    Id = id,
                    IsSuperAdmin = Convert.ToBoolean(isSuperAdmin),
                    UserType = userType
                };
            }
            else
            {
                return new UserAuthVm { IsAuthenticate = false };
            }
        }

        private PayloadResponse VerifyPassword(string password, string passwordHash)
        {
            var isVerified = BCrypt.Net.BCrypt.Verify(password, passwordHash);

            if (isVerified)
            {
                return new PayloadResponse()
                {
                    IsSuccess = true,
                    Message = "Password has been matched"
                };
            }

            return new PayloadResponse()
            {
                IsSuccess = false,
                Message = "Password doesn't match!"
            };
        }

        public UserAuthVm GetCurrentUser()
        {
            return _httpContextAccessor.HttpContext.Session.GetObject<UserAuthVm>("Auth");
        }

        public List<AccessControlVm> GetUserMenu(string roleId)
        {
            var query = string.Empty;
            if (GetCurrentUser().IsSuperAdmin)
            {
                query = query + @"select ac.Id,ac.Name,ac.ParentId,ac.Type,ac.Url,ac.MenuId,ac.Icon
                               from AccessControls ac";
            }
            else
            {
                query = query + @$"select ac.Id,ac.Name,ac.ParentId,ac.Type,ac.Url,ac.MenuId,ac.Icon,ac.SortOrder
                        from MenuCruds mc
                        join AccessControls ac on mc.AccessControlId = ac.Id where mc.RoleId = '{roleId}'; ";
            }
            return BuildMenuTree(_repo.Query<AccessControlVm>(query));
        }

        private List<AccessControlVm> BuildMenuTree(List<AccessControlVm> accessControlVms)
        {
            var list = new List<AccessControlVm>();
            foreach (var item in accessControlVms)
            {
                if (item.ParentId != "#")
                {
                    var parent = accessControlVms.Where(_ => _.Id == item.ParentId).FirstOrDefault();
                    if (parent == null)
                    {
                        var innerParent = _accessRepo.Find(item.ParentId);

                        parent = new AccessControlVm
                        {
                            Id = innerParent.Id,
                            ParentId = innerParent.ParentId,
                            Child = new List<AccessControlVm>(),
                            Name = innerParent.Name,
                            Type = innerParent.Type,
                            Url = innerParent.Url,
                            Icon = innerParent.Icon,
                            MenuId = innerParent.MenuId,
                            SortOrder = innerParent.SortOrder
                        };

                        parent.Child.Add(item);
                        list.Add(parent);
                    }
                    else
                    {
                        var child = parent.Child.Where(_ => _.Id == item.Id).FirstOrDefault();
                        if (child == null)
                        {
                            parent.Child.Add(item);
                            var chkParent = list.Where(_ => _.Id == parent.Id).FirstOrDefault();
                            if (chkParent == null)
                            {
                                list.Add(parent);
                            }
                        }
                        else
                        {
                        }
                    }
                }
                else
                {
                    var childList = accessControlVms.Where(_ => _.ParentId == item.Id).ToList();
                    foreach (var childItem in childList)
                    {
                        if (!item.Child.Contains(childItem))
                        {
                            item.Child.Add(childItem);
                        }
                    }
                    var chkParent = list.Where(_ => _.Id == item.Id).FirstOrDefault();
                    if (chkParent == null)
                    {
                        list.Add(item);
                    }
                }
            }
            return list.GroupBy(x => x.Id).Select(y => y.First()).ToList();
        }
    }
}
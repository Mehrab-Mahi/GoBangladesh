using GoBangladesh.Application.Helper;
using GoBangladesh.Application.Interfaces;
using GoBangladesh.Application.ViewModels;
using GoBangladesh.Domain.Entities;
using GoBangladesh.Domain.Interfaces;
using Microsoft.AspNetCore.Http;

namespace GoBangladesh.Application.Services
{
    public class LoggedInUserService : ILoggedInUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IRepository<User> _userRepository;

            public LoggedInUserService(IHttpContextAccessor httpContextAccessor,
                IRepository<User> userRepository)
            {
                _httpContextAccessor = httpContextAccessor;
                _userRepository = userRepository;
            }

        public User GetLoggedInUser()
        {
            var authorization = _httpContextAccessor.HttpContext!.Request.Headers["Authorization"].ToString();

            if(string.IsNullOrEmpty(authorization)) return null;

            var bytes = new byte[1024];
            _httpContextAccessor.HttpContext.Session.TryGetValue("userId", out bytes);

            if(bytes is null) return null;

            var loggedInUserId = System.Text.Encoding.UTF8.GetString(bytes).Trim('"');

            return _userRepository.GetConditional(u => u.Id == loggedInUserId);
        }
    }
}

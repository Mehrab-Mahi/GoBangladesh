using GoBangladesh.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace GoBangladesh.Application.Helper;

public class GoBangladeshAuthorizeFilter : IAuthorizationFilter
{
    private readonly IAuthService _authService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    public GoBangladeshAuthorizeFilter(IAuthService authService, IHttpContextAccessor httpContextAccessor)
    {
        _authService = authService;
        _httpContextAccessor = httpContextAccessor;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var request = context.HttpContext.Request;
        var authorization = request.Headers["Authorization"];

        if (!string.IsNullOrEmpty(authorization.ToString()))
        {
            var arr = authorization.ToString().Split(" ");
            if (arr[0].ToLower() == "bearer")
            {
                var auth = _authService.ValidateToken(arr[1]);
                if (!auth.IsAuthenticate)
                {
                    context.Result = new UnauthorizedResult();
                }
                else
                {
                    _httpContextAccessor.HttpContext!.Session.SetObject("userId", auth.Id);
                }
            }
            else
            {
                context.Result = new UnauthorizedResult();
                context.HttpContext.Response.StatusCode = 401;
            }
        }
        else
        {
            context.Result = new UnauthorizedResult();
            context.HttpContext.Response.StatusCode = 401;
        }
    }
}
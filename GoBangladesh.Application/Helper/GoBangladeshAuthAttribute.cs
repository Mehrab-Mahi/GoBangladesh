using Microsoft.AspNetCore.Mvc;

namespace GoBangladesh.Application.Helper;

public class GoBangladeshAuthAttribute : TypeFilterAttribute
{
    public GoBangladeshAuthAttribute() : base(typeof(GoBangladeshAuthorizeFilter))
    {
        Arguments = new object[] { };
    }
}
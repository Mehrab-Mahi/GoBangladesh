using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;

namespace GoBangladesh.Application.Helper
{
    public class GoBangladeshAuthAttribute : TypeFilterAttribute
    {
        public GoBangladeshAuthAttribute() : base(typeof(GoBangladeshAuthorizeFilter))
        {
            Arguments = new object[] { };
        }
    }
}

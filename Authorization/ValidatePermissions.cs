using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace taesa_aprovador_api.Authorization
{
    public class ValidatePermissions
    {
        public static bool Validate(HttpContext context, params string[] type)
        {
            if(type.Contains(context.User.Claims.FirstOrDefault(c => c.Type.Equals("Type")).Value)){
                return true;
            }
            return false;
        }
    }

    public class PermissionsAttribute : TypeFilterAttribute
    {
        public PermissionsAttribute(params string[] claimValue) : base (typeof(AuthorizationFilter))
        {
            Arguments = new object[] {claimValue};
        }
    }

    public class AuthorizationFilter : IAuthorizationFilter
    {
        private readonly string[] _claim;

        public AuthorizationFilter(string[] claim)
        {
            _claim = claim;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            if(!ValidatePermissions.Validate(context.HttpContext, _claim)){
                context.Result = new StatusCodeResult(403);
            }
        }
    }
}
using Microsoft.AspNetCore.Authorization;

namespace CarbonWise.API.Configuration.Authorization
{
    public class HasPermissionAttribute : AuthorizeAttribute
    {
        public HasPermissionAttribute(string permission) : base(permission)
        {
        }
    }
}
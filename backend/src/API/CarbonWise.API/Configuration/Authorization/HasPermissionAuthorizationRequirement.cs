using Microsoft.AspNetCore.Authorization;

namespace CarbonWise.API.Configuration.Authorization
{
    public class HasPermissionAuthorizationRequirement : IAuthorizationRequirement
    {
        public HasPermissionAuthorizationRequirement(string permission)
        {
            Permission = permission;
        }

        public string Permission { get; }
    }
}
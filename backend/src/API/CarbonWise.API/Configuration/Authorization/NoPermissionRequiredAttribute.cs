using Microsoft.AspNetCore.Authorization;

namespace CarbonWise.API.Configuration.Authorization
{
    public class NoPermissionRequiredAttribute : AllowAnonymousAttribute
    {
    }
}
using Microsoft.AspNetCore.Authorization;

namespace CarbonWise.API.Configuration.Authorization
{
    public class HasPermissionAuthorizationHandler : AuthorizationHandler<HasPermissionAuthorizationRequirement>
    {
        private readonly IAuthorizationChecker _authorizationChecker;

        public HasPermissionAuthorizationHandler(IAuthorizationChecker authorizationChecker)
        {
            _authorizationChecker = authorizationChecker;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            HasPermissionAuthorizationRequirement requirement)
        {
            var permissions = await _authorizationChecker.GetUserPermissionsAsync(context.User);

            if (permissions.Contains(requirement.Permission))
            {
                context.Succeed(requirement);
            }
        }
    }
}
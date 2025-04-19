using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Reflection;

namespace CarbonWise.API.Configuration.Authorization
{
    public class AttributeAuthorizationHandler : IAuthorizationHandler
    {
        private readonly IAuthorizationChecker _authorizationChecker;

        public AttributeAuthorizationHandler(IAuthorizationChecker authorizationChecker)
        {
            _authorizationChecker = authorizationChecker;
        }

        public async Task HandleAsync(AuthorizationHandlerContext context)
        {
            if (context.Resource is AuthorizationFilterContext authorizationFilterContext)
            {
                if (authorizationFilterContext.ActionDescriptor is ControllerActionDescriptor controllerActionDescriptor)
                {
                    var hasPermissionAttributes = controllerActionDescriptor.MethodInfo
                        .GetCustomAttributes<HasPermissionAttribute>()
                        .ToList();

                    if (!hasPermissionAttributes.Any())
                    {
                        hasPermissionAttributes = controllerActionDescriptor.ControllerTypeInfo
                            .GetCustomAttributes<HasPermissionAttribute>()
                            .ToList();
                    }

                    if (hasPermissionAttributes.Any())
                    {
                        var permissions = await _authorizationChecker.GetUserPermissionsAsync(context.User);

                        foreach (var requirement in context.PendingRequirements.ToList())
                        {
                            if (requirement is HasPermissionAuthorizationRequirement hasPermissionRequirement)
                            {
                                if (permissions.Contains(hasPermissionRequirement.Permission))
                                {
                                    context.Succeed(requirement);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
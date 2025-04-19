using System.Security.Claims;

namespace CarbonWise.API.Configuration.Authorization
{
    public interface IAuthorizationChecker
    {
        Task<IEnumerable<string>> GetUserPermissionsAsync(ClaimsPrincipal user);
    }

    public class AuthorizationChecker : IAuthorizationChecker
    {
        public Task<IEnumerable<string>> GetUserPermissionsAsync(ClaimsPrincipal user)
        {
            var permissions = new List<string> { "read", "write" };
            return Task.FromResult<IEnumerable<string>>(permissions);
        }
    }
}
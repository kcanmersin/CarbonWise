using CarbonWise.BuildingBlocks.Domain.Users;
using System.Security.Claims;

namespace CarbonWise.BuildingBlocks.Infrastructure.Security
{
    public interface IJwtTokenGenerator
    {
        string GenerateToken(User user);
        ClaimsPrincipal? ValidateToken(string token);
        UserInfo? GetUserFromToken(string token);
        bool IsTokenValid(string token);
    }
}
using CarbonWise.BuildingBlocks.Domain.Users;

namespace CarbonWise.BuildingBlocks.Infrastructure.Security
{
    public interface IJwtTokenGenerator
    {
        string GenerateToken(User user);
    }
}
using CarbonWise.BuildingBlocks.Domain.Users;

namespace CarbonWise.BuildingBlocks.Application.Security
{
    public interface IJwtTokenGenerator
    {
        string GenerateToken(User user);
    }
}
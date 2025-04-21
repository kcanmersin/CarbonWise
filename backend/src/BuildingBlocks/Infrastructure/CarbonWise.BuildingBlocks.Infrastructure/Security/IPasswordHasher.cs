namespace CarbonWise.BuildingBlocks.Infrastructure.Security
{
    public interface IPasswordHasher
    {
        string HashPassword(string password);
        bool VerifyPassword(string password, string passwordHash);
    }
}
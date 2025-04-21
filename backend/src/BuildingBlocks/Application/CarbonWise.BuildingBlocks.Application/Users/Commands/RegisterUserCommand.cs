using CarbonWise.BuildingBlocks.Domain.Users;

namespace CarbonWise.BuildingBlocks.Application.Users.Commands
{
    public class RegisterUserCommand
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public UserRole Role { get; set; } = UserRole.User; // Default role
    }
}
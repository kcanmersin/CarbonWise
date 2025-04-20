using System.Threading.Tasks;
using CarbonWise.BuildingBlocks.Domain.Users;

namespace CarbonWise.BuildingBlocks.Application.Users.GetUser
{
    public class GetUserQueryHandler
    {
        private readonly IUserRepository _userRepository;

        public GetUserQueryHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<UserDto> Handle(GetUserQuery query)
        {
            var user = await _userRepository.GetByIdAsync(new UserId(query.UserId));

            if (user == null)
            {
                return null;
            }

            return new UserDto
            {
                Id = user.Id.Value,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role.ToString(),
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt
            };
        }
    }
}
using CarbonWise.BuildingBlocks.Domain.Users;
using CarbonWise.BuildingBlocks.Application.Security; 
using CarbonWise.BuildingBlocks.Infrastructure;

namespace CarbonWise.BuildingBlocks.Application.Users.RegisterUser
{
    public class RegisterUserCommandHandler
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IUnitOfWork _unitOfWork;

        public RegisterUserCommandHandler(
            IUserRepository userRepository,
            IPasswordHasher passwordHasher,
            IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _unitOfWork = unitOfWork;
        }

        public async Task<UserDto> Handle(RegisterUserCommand command)
        {
            if (await _userRepository.ExistsAsync(command.Username, command.Email))
            {
                throw new UserAlreadyExistsException("User with this username or email already exists");
            }

            string passwordHash = _passwordHasher.HashPassword(command.Password);

            var user = User.Create(
                command.Username,
                command.Email,
                passwordHash,
                command.Role);

            await _userRepository.AddAsync(user);
            await _unitOfWork.CommitAsync();

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
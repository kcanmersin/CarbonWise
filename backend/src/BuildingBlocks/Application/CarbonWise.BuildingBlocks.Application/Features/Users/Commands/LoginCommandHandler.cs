using CarbonWise.BuildingBlocks.Domain.Users;
using CarbonWise.BuildingBlocks.Infrastructure;
using CarbonWise.BuildingBlocks.Application.Users.Login;
using CarbonWise.BuildingBlocks.Infrastructure.Security;

namespace CarbonWise.BuildingBlocks.Application.Users.Commands
{
    public class LoginCommandHandler
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;
        private readonly IUnitOfWork _unitOfWork;

        public LoginCommandHandler(
            IUserRepository userRepository,
            IPasswordHasher passwordHasher,
            IJwtTokenGenerator jwtTokenGenerator,
            IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _jwtTokenGenerator = jwtTokenGenerator;
            _unitOfWork = unitOfWork;
        }

        public async Task<AuthenticationResult> Handle(LoginCommand command)
        {
            var user = await _userRepository.GetByUsernameAsync(command.Username);
            if (user == null)
            {
                return AuthenticationResult.FailureResult("Invalid username or password");
            }

            if (!_passwordHasher.VerifyPassword(command.Password, user.PasswordHash))
            {
                return AuthenticationResult.FailureResult("Invalid username or password");
            }

            user.UpdateLastLogin();
            await _userRepository.UpdateAsync(user);
            await _unitOfWork.CommitAsync();

            var token = _jwtTokenGenerator.GenerateToken(user);
            var userDto = new UserDto
            {
                Id = user.Id.Value,
                Username = user.Username,
                Name = user.Name,
                Surname = user.Surname,
                Email = user.Email,
                Gender = user.Gender,
                IsInInstitution = user.IsInInstitution,
                IsStudent = user.IsStudent,
                IsAcademicPersonal = user.IsAcademicPersonal,
                IsAdministrativeStaff = user.IsAdministrativeStaff,
                UniqueId = user.UniqueId,
                SustainabilityPoint = user.SustainabilityPoint,
                Role = user.Role.ToString(),
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt
            };

            return AuthenticationResult.SuccessResult(userDto, token);
        }
    }
}
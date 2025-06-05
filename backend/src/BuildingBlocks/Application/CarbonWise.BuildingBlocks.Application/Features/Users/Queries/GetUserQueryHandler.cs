using System.Threading.Tasks;
using CarbonWise.BuildingBlocks.Domain.Users;

namespace CarbonWise.BuildingBlocks.Application.Users.Queries
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
        }
    }
}
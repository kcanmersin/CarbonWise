using CarbonWise.BuildingBlocks.Domain.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarbonWise.BuildingBlocks.Application.Services
{
    public interface IUserManagementService
    {
        Task<UserManagementResult> PromoteToAdminAsync(Guid userId, Guid promotingUserId);
        Task<UserManagementResult> DemoteFromAdminAsync(Guid userId, Guid demotingUserId);
        Task<UserManagementResult> ChangeUserRoleAsync(Guid userId, UserRole newRole, Guid changingUserId);
        Task<List<UserDto>> GetAllUsersAsync();
        Task<List<UserDto>> GetUsersByRoleAsync(UserRole role);
        Task<UserDto> GetUserByIdAsync(Guid userId);
    }

    public class UserManagementResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public UserDto User { get; set; }

        public static UserManagementResult CreateSuccess(UserDto user, string message = "Operation completed successfully")
        {
            return new UserManagementResult
            {
                Success = true,
                Message = message,
                User = user
            };
        }

        public static UserManagementResult CreateFailure(string message)
        {
            return new UserManagementResult
            {
                Success = false,
                Message = message
            };
        }
    }

    public class UserDto
    {
        public Guid Id { get; set; }
        public string Username { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Email { get; set; }
        public string Gender { get; set; }
        public bool IsInInstitution { get; set; }
        public bool IsStudent { get; set; }
        public bool IsAcademicPersonal { get; set; }
        public bool IsAdministrativeStaff { get; set; }
        public string UniqueId { get; set; }
        public int? SustainabilityPoint { get; set; }
        public UserRole Role { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
    }
}

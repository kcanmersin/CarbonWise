using CarbonWise.BuildingBlocks.Domain.Users;
using CarbonWise.BuildingBlocks.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarbonWise.BuildingBlocks.Application.Services
{
    public class UserManagementService : IUserManagementService
    {
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;

        public UserManagementService(IUserRepository userRepository, IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<UserManagementResult> PromoteToAdminAsync(Guid userId, Guid promotingUserId)
        {
            var promotingUser = await _userRepository.GetByIdAsync(new UserId(promotingUserId));
            if (promotingUser == null)
                return UserManagementResult.CreateFailure("Promoting user not found");

            if (promotingUser.Role != UserRole.SuperUser)
                return UserManagementResult.CreateFailure("Only SuperUsers can promote users to Admin");

            var targetUser = await _userRepository.GetByIdAsync(new UserId(userId));
            if (targetUser == null)
                return UserManagementResult.CreateFailure("Target user not found");

            if (targetUser.Role == UserRole.Admin)
                return UserManagementResult.CreateFailure("User is already an Admin");

            if (targetUser.Role == UserRole.SuperUser)
                return UserManagementResult.CreateFailure("Cannot change SuperUser role");

            targetUser.ChangeRole(UserRole.Admin);
            await _userRepository.UpdateAsync(targetUser);
            await _unitOfWork.CommitAsync();

            return UserManagementResult.CreateSuccess(MapToDto(targetUser), "User successfully promoted to Admin");
        }

        public async Task<UserManagementResult> DemoteFromAdminAsync(Guid userId, Guid demotingUserId)
        {
            var demotingUser = await _userRepository.GetByIdAsync(new UserId(demotingUserId));
            if (demotingUser == null)
                return UserManagementResult.CreateFailure("Demoting user not found");

            if (demotingUser.Role != UserRole.SuperUser)
                return UserManagementResult.CreateFailure("Only SuperUsers can demote Admins");

            var targetUser = await _userRepository.GetByIdAsync(new UserId(userId));
            if (targetUser == null)
                return UserManagementResult.CreateFailure("Target user not found");

            if (targetUser.Role != UserRole.Admin)
                return UserManagementResult.CreateFailure("User is not an Admin");

            targetUser.ChangeRole(UserRole.User);
            await _userRepository.UpdateAsync(targetUser);
            await _unitOfWork.CommitAsync();

            return UserManagementResult.CreateSuccess(MapToDto(targetUser), "Admin successfully demoted to User");
        }

        public async Task<UserManagementResult> ChangeUserRoleAsync(Guid userId, UserRole newRole, Guid changingUserId)
        {
            var changingUser = await _userRepository.GetByIdAsync(new UserId(changingUserId));
            if (changingUser == null)
                return UserManagementResult.CreateFailure("Changing user not found");

            if (changingUser.Role != UserRole.SuperUser)
                return UserManagementResult.CreateFailure("Only SuperUsers can change user roles");

            var targetUser = await _userRepository.GetByIdAsync(new UserId(userId));
            if (targetUser == null)
                return UserManagementResult.CreateFailure("Target user not found");

            if (targetUser.Role == UserRole.SuperUser && changingUser.Id.Value != targetUser.Id.Value)
                return UserManagementResult.CreateFailure("Cannot change another SuperUser's role");

            if (targetUser.Role == newRole)
                return UserManagementResult.CreateFailure($"User already has {newRole} role");

            targetUser.ChangeRole(newRole);
            await _userRepository.UpdateAsync(targetUser);
            await _unitOfWork.CommitAsync();

            return UserManagementResult.CreateSuccess(MapToDto(targetUser), $"User role successfully changed to {newRole}");
        }

        public async Task<List<UserDto>> GetAllUsersAsync()
        {
            var users = await _userRepository.GetAllAsync();
            return users.Select(MapToDto).OrderBy(u => u.Username).ToList();
        }

        public async Task<List<UserDto>> GetUsersByRoleAsync(UserRole role)
        {
            var users = await _userRepository.GetByRoleAsync(role);
            return users.Select(MapToDto).OrderBy(u => u.Username).ToList();
        }

        public async Task<UserDto> GetUserByIdAsync(Guid userId)
        {
            var user = await _userRepository.GetByIdAsync(new UserId(userId));
            return user != null ? MapToDto(user) : null;
        }

        private static UserDto MapToDto(User user)
        {
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
                Role = user.Role,
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt
            };
        }
    }
}

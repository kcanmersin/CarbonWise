using CarbonWise.BuildingBlocks.Application.Services;
using CarbonWise.BuildingBlocks.Domain.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
namespace CarbonWise.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "SuperUser")]
    public class AdminController : ControllerBase
    {
        private readonly IUserManagementService _userManagementService;

        public AdminController(IUserManagementService userManagementService)
        {
            _userManagementService = userManagementService;
        }

        [HttpPost("promote-user")]
        public async Task<IActionResult> PromoteUser([FromBody] PromoteUserRequest request)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == null)
                return Unauthorized("Unable to determine current user");

            var result = await _userManagementService.PromoteToAdminAsync(request.UserId, currentUserId.Value);

            if (result.Success)
                return Ok(result);

            return BadRequest(new { error = result.Message });
        }

        [HttpPost("demote-user")]
        public async Task<IActionResult> DemoteUser([FromBody] DemoteUserRequest request)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == null)
                return Unauthorized("Unable to determine current user");

            var result = await _userManagementService.DemoteFromAdminAsync(request.UserId, currentUserId.Value);

            if (result.Success)
                return Ok(result);

            return BadRequest(new { error = result.Message });
        }

        [HttpPost("change-role")]
        public async Task<IActionResult> ChangeUserRole([FromBody] ChangeRoleRequest request)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == null)
                return Unauthorized("Unable to determine current user");

            var result = await _userManagementService.ChangeUserRoleAsync(request.UserId, request.NewRole, currentUserId.Value);

            if (result.Success)
                return Ok(result);

            return BadRequest(new { error = result.Message });
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userManagementService.GetAllUsersAsync();
            return Ok(users);
        }

        [HttpGet("users/role/{role}")]
        public async Task<IActionResult> GetUsersByRole(UserRole role)
        {
            var users = await _userManagementService.GetUsersByRoleAsync(role);
            return Ok(users);
        }

        [HttpGet("users/{id}")]
        public async Task<IActionResult> GetUser(Guid id)
        {
            var user = await _userManagementService.GetUserByIdAsync(id);

            if (user == null)
                return NotFound("User not found");

            return Ok(user);
        }

        [HttpGet("dashboard/stats")]
        public async Task<IActionResult> GetDashboardStats()
        {
            var allUsers = await _userManagementService.GetAllUsersAsync();

            var stats = new
            {
                TotalUsers = allUsers.Count,
                SuperUsers = allUsers.Count(u => u.Role == UserRole.SuperUser),
                Admins = allUsers.Count(u => u.Role == UserRole.Admin),
                RegularUsers = allUsers.Count(u => u.Role == UserRole.User),
                RecentlyJoined = allUsers.Where(u => u.CreatedAt >= DateTime.UtcNow.AddDays(-30)).Count(),
                ActiveUsers = allUsers.Where(u => u.LastLoginAt >= DateTime.UtcNow.AddDays(-7)).Count()
            };

            return Ok(stats);
        }

        private Guid? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                           ?? User.FindFirst("sub")?.Value;

            if (Guid.TryParse(userIdClaim, out var userId))
                return userId;

            return null;
        }
    }

    public class PromoteUserRequest
    {
        public Guid UserId { get; set; }
    }

    public class DemoteUserRequest
    {
        public Guid UserId { get; set; }
    }

    public class ChangeRoleRequest
    {
        public Guid UserId { get; set; }
        public UserRole NewRole { get; set; }
    }
}
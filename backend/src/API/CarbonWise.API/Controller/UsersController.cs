using System;
using System.Threading.Tasks;
using CarbonWise.BuildingBlocks.Application.Users.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;

namespace CarbonWise.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly GetUserQueryHandler _getUserQueryHandler;

        public UsersController(GetUserQueryHandler getUserQueryHandler)
        {
            _getUserQueryHandler = getUserQueryHandler;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(Guid id)
        {
            var result = await _getUserQueryHandler.Handle(new GetUserQuery { UserId = id });

            if (result == null)
            {
                return NotFound();
            }

            return Ok(result);
        }

        [HttpGet("me")]
        [Authorize]
        public IActionResult GetCurrentUser()
        {
            var profile = new
            {
                Id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                Email = User.FindFirst(ClaimTypes.Email)?.Value,
                Username = User.FindFirst("username")?.Value,
                IsInInstitution = bool.TryParse(User.FindFirst("isInInstitution")?.Value, out var isInInst) && isInInst,
                IsStudent = bool.TryParse(User.FindFirst("isStudent")?.Value, out var isStudent) && isStudent,
                IsAcademicPersonal = bool.TryParse(User.FindFirst("isAcademicPersonal")?.Value, out var isAcademic) && isAcademic,
                IsAdministrativeStaff = bool.TryParse(User.FindFirst("isAdministrativeStaff")?.Value, out var isAdmin) && isAdmin,
                UniqueId = User.FindFirst("uniqueId")?.Value,
                SustainabilityPoint = int.TryParse(User.FindFirst("sustainabilityPoint")?.Value, out var points) ? points : (int?)null,
                Role = User.FindFirst(ClaimTypes.Role)?.Value
            };

            return Ok(profile);
        }

        [HttpGet("me/profile")]
        [Authorize]
        public IActionResult GetCurrentUserProfile()
        {
            var profile = new
            {
                Id = User.FindFirst("sub")?.Value,
                Email = User.FindFirst(ClaimTypes.Email)?.Value,
                Username = User.FindFirst("username")?.Value,
                IsInInstitution = bool.TryParse(User.FindFirst("isInInstitution")?.Value, out var isInInst) && isInInst,
                IsStudent = bool.TryParse(User.FindFirst("isStudent")?.Value, out var isStudent) && isStudent,
                IsAcademicPersonal = bool.TryParse(User.FindFirst("isAcademicPersonal")?.Value, out var isAcademic) && isAcademic,
                IsAdministrativeStaff = bool.TryParse(User.FindFirst("isAdministrativeStaff")?.Value, out var isAdmin) && isAdmin,
                UniqueId = User.FindFirst("uniqueId")?.Value,
                SustainabilityPoint = int.TryParse(User.FindFirst("sustainabilityPoint")?.Value, out var points) ? points : (int?)null,
                Role = User.FindFirst(ClaimTypes.Role)?.Value,
                TokenIssuedAt = User.FindFirst("iat")?.Value
            };

            return Ok(profile);
        }

        [HttpGet("me/claims")]
        [Authorize]
        public IActionResult GetCurrentUserClaims()
        {
            var claims = User.Claims.Select(c => new
            {
                Type = c.Type,
                Value = c.Value
            }).ToList();

            return Ok(claims);
        }
    }
}
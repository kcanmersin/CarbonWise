using System;
using System.Threading.Tasks;
using CarbonWise.BuildingBlocks.Application.Users.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarbonWise.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize]
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
        public async Task<IActionResult> GetCurrentUser()
        {
            if (!Guid.TryParse(User.FindFirst("sub")?.Value, out Guid userId))
            {
                return Unauthorized();
            }

            var result = await _getUserQueryHandler.Handle(new GetUserQuery { UserId = userId });

            if (result == null)
            {
                return NotFound();
            }

            return Ok(result);
        }
    }
}
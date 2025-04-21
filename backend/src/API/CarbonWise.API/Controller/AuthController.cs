using System.Threading.Tasks;
using CarbonWise.BuildingBlocks.Application.Users;
using CarbonWise.BuildingBlocks.Application.Users.Commands;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarbonWise.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly RegisterUserCommandHandler _registerUserCommandHandler;
        private readonly LoginCommandHandler _loginCommandHandler;

        public AuthController(
            RegisterUserCommandHandler registerUserCommandHandler,
            LoginCommandHandler loginCommandHandler)
        {
            _registerUserCommandHandler = registerUserCommandHandler;
            _loginCommandHandler = loginCommandHandler;
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterUserCommand command)
        {
            try
            {
                var result = await _registerUserCommandHandler.Handle(command);
                return Ok(result);
            }
            catch (UserAlreadyExistsException e)
            {
                return Conflict(new { error = e.Message });
            }
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginCommand command)
        {
            var result = await _loginCommandHandler.Handle(command);

            if (!result.Success)
            {
                return Unauthorized(new { error = result.ErrorMessage });
            }

            return Ok(result);
        }
    }
}
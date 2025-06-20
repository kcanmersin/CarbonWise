
using System.Threading.Tasks;
using CarbonWise.API.Services;
using CarbonWise.BuildingBlocks.Application.Users;
using CarbonWise.BuildingBlocks.Application.Users.Commands;
using CarbonWise.BuildingBlocks.Application.Users.Login;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace CarbonWise.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly RegisterUserCommandHandler _registerUserCommandHandler;
        private readonly LoginCommandHandler _loginCommandHandler;
        private readonly IOAuthService _oauthService;
        private readonly IMemoryCache _memoryCache;

        public AuthController(
            RegisterUserCommandHandler registerUserCommandHandler,
            LoginCommandHandler loginCommandHandler,
            IOAuthService oauthService,
            IMemoryCache memoryCache)
        {
            _registerUserCommandHandler = registerUserCommandHandler;
            _loginCommandHandler = loginCommandHandler;
            _oauthService = oauthService;
            _memoryCache = memoryCache;
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

        [HttpGet("oauth/login-url")]
        [AllowAnonymous]
        public IActionResult GetOAuthLoginUrl()
        {
            try
            {
                var loginUrl = _oauthService.GenerateLoginUrl(_memoryCache);
                return Ok(new { loginUrl = loginUrl });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("oauth/callback")]
        [AllowAnonymous]
        public async Task<IActionResult> OAuthCallback([FromBody] OAuthCallbackRequest request)
        {
            if (string.IsNullOrEmpty(request.Code) || string.IsNullOrEmpty(request.State))
            {
                return BadRequest(new { error = "Code and state are required" });
            }

            var result = await _oauthService.HandleOAuthRedirect(request.State, request.Code, _memoryCache);

            if (!result.Success)
            {
                return StatusCode(result.StatusCode, new { error = result.ErrorMessage });
            }

            var userDto = new UserDto
            {
                Id = result.User.Id.Value,
                Username = result.User.Username,
                Email = result.User.Email,
                Role = result.User.Role.ToString(),
                CreatedAt = result.User.CreatedAt,
                LastLoginAt = result.User.LastLoginAt
            };

            var authResult = new AuthenticationResult
            {
                Success = true,
                User = userDto,
                Token = result.Token
            };

            return Ok(authResult);
        }
    }

    public class OAuthCallbackRequest
    {
        public string Code { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
    }
}
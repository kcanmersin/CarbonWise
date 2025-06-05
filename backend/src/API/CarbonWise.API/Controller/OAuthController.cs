using CarbonWise.API.Services;
using CarbonWise.BuildingBlocks.Domain.Users;
using CarbonWise.BuildingBlocks.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace CarbonWise.API.Controllers
{
    [Route("")]
    [ApiController]
    public class OAuthController : ControllerBase
    {
        private readonly IOAuthService _oAuthService;
        private readonly IMemoryCache _cache;

        public OAuthController(IOAuthService oAuthService, IMemoryCache cache)
        {
            _oAuthService = oAuthService;
            _cache = cache;
        }

        [HttpGet("login")]
        public IActionResult Login()
        {
            var result = _oAuthService.GenerateLoginUrl(_cache);
            return Ok(new { authorizationUrl = result });
        }

        [HttpGet("auth")]
        public async Task<IActionResult> OAuthRedirect([FromQuery] string state, [FromQuery] string code)
        {
            var result = await _oAuthService.HandleOAuthRedirect(state, code, _cache);
            if (result.Success)
                return Ok(result.User);
            return StatusCode(result.StatusCode, result.ErrorMessage);
        }
    }
}
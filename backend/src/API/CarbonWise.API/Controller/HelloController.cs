using Microsoft.AspNetCore.Mvc;

namespace CarbonWise.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HelloController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok("Hello from CarbonWise API!");
        }

        [HttpGet("{name}")]
        public IActionResult Get(string name)
        {
            return Ok($"Hello, {name}! Welcome to CarbonWise API.");
        }
    }
}
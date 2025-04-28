using CarbonWise.BuildingBlocks.Application.Services.ExternalAPIs;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace CarbonWise.API.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class ExternalAPIsController : ControllerBase
    {
        private readonly IExternalAPIsService _externalAPIsService;

        public ExternalAPIsController(IExternalAPIsService externalAPIsService)
        {
            _externalAPIsService = externalAPIsService;
        }

        [HttpGet("airquality/city/{cityName}")]
        public async Task<IActionResult> GetCityAirQuality(string cityName)
        {
            var result = await _externalAPIsService.GetAirQualityDataAsync(cityName);

            if (result.Status != "ok")
            {
                return StatusCode(500, new { error = result.ErrorMessage ?? "Unknown error occurred" });
            }

            if (result.Data == null)
            {
                return NotFound(new { error = $"No air quality data found for {cityName}" });
            }

            return Ok(result);
        }

        [HttpGet("airquality/geo")]
        public async Task<IActionResult> GetGeoAirQuality([FromQuery] double lat, [FromQuery] double lng)
        {
            var result = await _externalAPIsService.GetAirQualityByGeoLocationAsync(lat, lng);

            if (result.Status != "ok")
            {
                return StatusCode(500, new { error = result.ErrorMessage ?? "Unknown error occurred" });
            }

            if (result.Data == null)
            {
                return NotFound(new { error = $"No air quality data found for coordinates ({lat}, {lng})" });
            }

            return Ok(result);
        }
    }
}
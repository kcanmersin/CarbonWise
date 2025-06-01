using CarbonWise.BuildingBlocks.Application.Services.ExternalAPIs;
using CarbonWise.BuildingBlocks.Application.Features.AirQuality.Queries;
using CarbonWise.BuildingBlocks.Application.Services.AirQuality;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace CarbonWise.API.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class ExternalAPIsController : ControllerBase
    {
        private readonly IExternalAPIsService _externalAPIsService;
        private readonly IMediator _mediator;
        private readonly IAirQualityBackgroundService _airQualityService;

        public ExternalAPIsController(
            IExternalAPIsService externalAPIsService,
            IMediator mediator,
            IAirQualityBackgroundService airQualityService)
        {
            _externalAPIsService = externalAPIsService;
            _mediator = mediator;
            _airQualityService = airQualityService;
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

        // Database'den hava kalitesi verilerini getiren endpoint'ler
        [HttpGet("airquality/database/city/{cityName}")]
        public async Task<IActionResult> GetStoredAirQualityByCity(string cityName)
        {
            try
            {
                var query = new GetAirQualityByCityQuery { City = cityName };
                var result = await _mediator.Send(query);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("airquality/database/last30days")]
        public async Task<IActionResult> GetLast30DaysStoredAirQuality([FromQuery] string city = null)
        {
            try
            {
                var query = new GetLast30DaysAirQualityQuery { City = city };
                var result = await _mediator.Send(query);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("airquality/fetch-manual")]
        public async Task<IActionResult> FetchAirQualityDataManually()
        {
            try
            {
                await _airQualityService.FetchAndStoreAirQualityDataAsync();
                return Ok(new { message = "Air quality data fetched and stored successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
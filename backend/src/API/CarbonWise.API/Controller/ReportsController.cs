using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using CarbonWise.BuildingBlocks.Application.Services.Reports;
using Microsoft.AspNetCore.Mvc;

namespace CarbonWise.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportsController : ControllerBase
    {
        private readonly IReportService _reportService;

        public ReportsController(IReportService reportService)
        {
            _reportService = reportService;
        }

        [HttpGet("consumption-types")]
        public async Task<IActionResult> GetConsumptionTypes()
        {
            var consumptionTypes = await _reportService.GetConsumptionTypesAsync();
            return Ok(consumptionTypes);
        }

        [HttpPost("carbon-footprint")]
        public async Task<IActionResult> GenerateCarbonFootprintReport([FromBody] DateRangeRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var report = await _reportService.GenerateCarbonFootprintReportAsync(
                    request.StartDate,
                    request.EndDate);

                return Ok(report);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("consumption")]
        public async Task<IActionResult> GenerateConsumptionReport([FromBody] ConsumptionReportRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var report = await _reportService.GenerateConsumptionReportAsync(
                    request.ConsumptionType,
                    request.BuildingId,
                    request.StartDate,
                    request.EndDate);

                return Ok(report);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

    public class DateRangeRequest
    {
        [Required]
        [DefaultValue("2020-04-28T20:40:11.760Z")]
        public DateTime StartDate { get; set; }

        [Required]
        [DefaultValue("2025-04-28T20:40:11.760Z")]

        public DateTime EndDate { get; set; }
    }

    public class ConsumptionReportRequest
    {
        [Required]
        [RegularExpression("^(Electric|NaturalGas|Water|Paper)$", ErrorMessage = "ConsumptionType must be one of: Electric, NaturalGas, Water, Paper")]
        public string ConsumptionType { get; set; }

        public Guid? BuildingId { get; set; }
        [Required]
        [DefaultValue("2020-04-28T20:40:11.760Z")]
        public DateTime StartDate { get; set; }

        [Required]
        [DefaultValue("2025-04-28T20:40:11.760Z")]

        public DateTime EndDate { get; set; }
    }
}
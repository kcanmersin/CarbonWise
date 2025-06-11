using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CarbonWise.API.Controller.CarbonFootprints;
using CarbonWise.BuildingBlocks.Application.Services.CarbonFootprints;
using Microsoft.AspNetCore.Mvc;

namespace CarbonWise.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CarbonFootprintsController : ControllerBase
    {
        private readonly ICarbonFootprintService _carbonFootprintService;

        public CarbonFootprintsController(ICarbonFootprintService carbonFootprintService)
        {
            _carbonFootprintService = carbonFootprintService;
        }

        [HttpGet("year/{year}")]
        public async Task<IActionResult> GetByYear([FromQuery] YearCarbonFootprintRequest request)
        {
            try
            {
                var carbonFootprint = await _carbonFootprintService.CalculateForYearAsync(
                    request.Year,
                    request.ElectricityFactor,
                    request.ShuttleBusFactor,
                    request.CarFactor,
                    request.MotorcycleFactor);

                var result = new CarbonFootprintDto
                {
                    Year = carbonFootprint.Year,
                    ElectricityEmission = carbonFootprint.ElectricityEmission,
                    ShuttleBusEmission = carbonFootprint.ShuttleBusEmission,
                    CarEmission = carbonFootprint.CarEmission,
                    MotorcycleEmission = carbonFootprint.MotorcycleEmission,
                    TotalEmission = carbonFootprint.TotalEmission
                };

                return Ok(result);
            }
            catch (ApplicationException ex)
            {
                return NotFound(new { error = ex.Message });
            }
        }

        [HttpGet("period")]
        public async Task<IActionResult> GetByPeriod([FromQuery] PeriodCarbonFootprintRequest request)
        {
            if (request.EndDate < request.StartDate)
            {
                return BadRequest(new { error = "End date must be after start date" });
            }

            try
            {
                var carbonFootprints = await _carbonFootprintService.CalculateForPeriodAsync(
                    request.StartDate,
                    request.EndDate,
                    request.ElectricityFactor,
                    request.ShuttleBusFactor,
                    request.CarFactor,
                    request.MotorcycleFactor);

                var results = carbonFootprints.Select(cf => new CarbonFootprintDto
                {
                    Year = cf.Year,
                    ElectricityEmission = cf.ElectricityEmission,
                    ShuttleBusEmission = cf.ShuttleBusEmission,
                    CarEmission = cf.CarEmission,
                    MotorcycleEmission = cf.MotorcycleEmission,
                    TotalEmission = cf.TotalEmission
                }).ToList();

                return Ok(results);
            }
            catch (ApplicationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("comparison")]
        public async Task<IActionResult> GetYearComparison([FromQuery] ComparisonRequest request)
        {
            try
            {
                var comparison = await _carbonFootprintService.GetYearComparisonAsync(
                    request.ElectricityFactor,
                    request.ShuttleBusFactor,
                    request.CarFactor,
                    request.MotorcycleFactor);

                return Ok(comparison);
            }
            catch (ApplicationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboardData([FromQuery] ComparisonRequest request)
        {
            try
            {
                var comparison = await _carbonFootprintService.GetYearComparisonAsync(
                    request.ElectricityFactor,
                    request.ShuttleBusFactor,
                    request.CarFactor,
                    request.MotorcycleFactor);

                var dashboardData = new
                {
                    Title = "Carbon Footprint Dashboard",
                    LastUpdated = DateTime.Now,
                    YearComparison = comparison,
                    Summary = new
                    {
                        CurrentYearTotal = comparison.CurrentYearData?.TotalEmission ?? 0,
                        PreviousYearTotal = comparison.PreviousYearData?.TotalEmission ?? 0,
                        HasImproved = comparison.Comparison?.IsImprovement ?? false,
                        ChangePercentage = comparison.Comparison?.TotalEmissionChangePercentage ?? 0
                    }
                };

                return Ok(dashboardData);
            }
            catch (ApplicationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }

    public class ComparisonRequest
    {
        public decimal? ElectricityFactor { get; set; }
        public decimal? ShuttleBusFactor { get; set; }
        public decimal? CarFactor { get; set; }
        public decimal? MotorcycleFactor { get; set; }
    }
}
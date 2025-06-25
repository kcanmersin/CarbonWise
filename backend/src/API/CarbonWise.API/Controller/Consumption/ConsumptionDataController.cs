using CarbonWise.API.Models.Consumption;
using CarbonWise.BuildingBlocks.Application.Services.Consumption;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace CarbonWise.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ConsumptionDataController : ControllerBase
    {
        private readonly IConsumptionDataService _consumptionDataService;

        public ConsumptionDataController(IConsumptionDataService consumptionDataService)
        {
            _consumptionDataService = consumptionDataService;
        }

        [HttpGet("{consumptionType}")]
        public async Task<IActionResult> GetConsumptionData(
            [FromRoute] string consumptionType,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var result = await _consumptionDataService.GetConsumptionDataAsync(consumptionType, startDate, endDate);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (ApplicationException ex)
            {
                return NotFound(new { error = ex.Message });
            }
        }

        [HttpGet("export")]
        public async Task<IActionResult> ExportConsumptionData([FromQuery] ExportConsumptionDataRequest request)
        {
            try
            {
                var excelBytes = await _consumptionDataService.GenerateConsumptionExcelAsync(
                    request.ConsumptionType,
                    request.StartDate,
                    request.EndDate,
                    request.IncludeGraphs,
                    request.BuildingId);

                var buildingInfo = request.BuildingId.HasValue ? $"_Building_{request.BuildingId}" : "";
                var fileName = $"{request.ConsumptionType}_ConsumptionData{buildingInfo}_{DateTime.Now:yyyyMMddHHmmss}.xlsx";

                return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (ApplicationException ex)
            {
                return NotFound(new { error = ex.Message });
            }
        }
    }
}
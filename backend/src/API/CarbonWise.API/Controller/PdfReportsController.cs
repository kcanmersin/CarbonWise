using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using CarbonWise.BuildingBlocks.Application.Services.Reports;
using Microsoft.AspNetCore.Mvc;

namespace CarbonWise.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PdfReportsController : ControllerBase
    {
        private readonly IPdfReportService _pdfReportService;

        public PdfReportsController(IPdfReportService pdfReportService)
        {
            _pdfReportService = pdfReportService;
        }

        [HttpGet("carbon-footprint")]
        public async Task<IActionResult> GetCarbonFootprintPdfReport([FromQuery] DateRangeRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var pdfBytes = await _pdfReportService.GenerateCarbonFootprintPdfReportAsync(
                    request.StartDate,
                    request.EndDate);

                string fileName = $"CarbonFootprintReport_{request.StartDate:yyyyMMdd}-{request.EndDate:yyyyMMdd}.pdf";
                return File(pdfBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("consumption")]
        public async Task<IActionResult> GetConsumptionPdfReport([FromQuery] ConsumptionReportRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var pdfBytes = await _pdfReportService.GenerateConsumptionPdfReportAsync(
                    request.ConsumptionType,
                    request.BuildingId,
                    request.StartDate,
                    request.EndDate);

                string buildingInfo = request.BuildingId.HasValue ? $"_{request.BuildingId}" : "";
                string fileName = $"{request.ConsumptionType}ConsumptionReport{buildingInfo}_{request.StartDate:yyyyMMdd}-{request.EndDate:yyyyMMdd}.pdf";
                return File(pdfBytes, "application/pdf", fileName);
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

   
}
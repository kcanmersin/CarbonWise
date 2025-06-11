using CarbonWise.BuildingBlocks.Application.Services.CarbonFootPrintTest;
using CarbonWise.BuildingBlocks.Domain.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace CarbonWise.API.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class CarbonFootprintTestController : ControllerBase
    {
        private readonly ICarbonFootprintTestService _testService;

        public CarbonFootprintTestController(ICarbonFootprintTestService testService)
        {
            _testService = testService;
        }

        [HttpGet("questions")]
        public async Task<IActionResult> GetQuestions()
        {
            var questions = await _testService.GetAllQuestionsAsync();
            return Ok(questions);
        }

        [HttpPost("start")]
        [Authorize]
        public async Task<IActionResult> StartTest()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid userId))
            {
                return Unauthorized("Unable to determine user identity from token");
            }

            var test = await _testService.StartNewTestAsync(userId);
            return Ok(test);
        }

        [HttpPost("{testId}/response")]
        [Authorize]
        public async Task<IActionResult> SaveResponse(Guid testId, [FromBody] SaveResponseRequest request)
        {
            try
            {
                var test = await _testService.SaveResponseAsync(testId, request.QuestionId, request.OptionId);
                return Ok(test);
            }
            catch (ApplicationException ex)
            {
                return NotFound(new { error = ex.Message });
            }
        }

        [HttpPost("{testId}/complete")]
        [Authorize]
        public async Task<IActionResult> CompleteTest(Guid testId)
        {
            try
            {
                var result = await _testService.CompleteTestAsync(testId);
                return Ok(result);
            }
            catch (ApplicationException ex)
            {
                return NotFound(new { error = ex.Message });
            }
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetSustainabilityStats()
        {
            var stats = await _testService.GetSustainabilityStatsAsync();
            return Ok(stats);
        }
    }

    public class SaveResponseRequest
    {
        [Required]
        public Guid QuestionId { get; set; }

        [Required]
        public Guid OptionId { get; set; }
    }
}
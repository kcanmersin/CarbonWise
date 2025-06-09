using CarbonWise.BuildingBlocks.Application.Services.AI;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace CarbonWise.API.Controller
{

    [ApiController]
    [Route("api/[controller]")]
    public class AIController : ControllerBase
    {
        private readonly IAIService _aiService;

    public AIController(IAIService aiService)
    {
        _aiService = aiService;
    }

    /// <summary>
    /// Train AI models for energy consumption prediction
    /// </summary>
    [HttpPost("train")]
    public async Task<IActionResult> TrainModel([FromBody] TrainModelRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var aiRequest = new AITrainRequest
            {
                ResourceType = request.ResourceType.ToLower(),
                BuildingId = request.BuildingId ?? "0",
                ModelTypes = request.ModelTypes ?? new System.Collections.Generic.List<string> { "rf", "xgb", "gb" },
                EnsembleTypes = request.EnsembleTypes ?? new System.Collections.Generic.List<string> { "rf_gb", "rf_xgb", "gb_xgb", "rf_gb_xgb" }
            };

            var result = await _aiService.TrainModelAsync(aiRequest);

            if (!result.Success)
            {
                return BadRequest(new { error = result.Message });
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = $"An error occurred: {ex.Message}" });
        }
    }

    /// <summary>
    /// Predict future energy consumption using trained models
    /// </summary>
    [HttpPost("predict")]
    public async Task<IActionResult> Predict([FromBody] PredictRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var aiRequest = new AIPredictRequest
            {
                ResourceType = request.ResourceType.ToLower(),
                BuildingId = request.BuildingId ?? "0",
                ModelType = request.ModelType.ToLower(),
                MonthsAhead = request.MonthsAhead
            };

            var result = await _aiService.PredictAsync(aiRequest);

            if (!result.Success)
            {
                return BadRequest(new { error = "Prediction failed", predictions = result.Predictions });
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = $"An error occurred: {ex.Message}" });
        }
    }

    /// <summary>
    /// Get all trained models
    /// </summary>
    [HttpGet("models")]
    public async Task<IActionResult> GetModels()
    {
        try
        {
            var models = await _aiService.GetModelsAsync();
            return Ok(models);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = $"An error occurred: {ex.Message}" });
        }
    }

    /// <summary>
    /// Check AI service health status
    /// </summary>
    [HttpGet("health")]
    public async Task<IActionResult> GetHealth()
    {
        try
        {
            var health = await _aiService.GetHealthAsync();

            if (health.Status == "healthy")
            {
                return Ok(health);
            }
            else
            {
                return StatusCode(503, health);
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = $"An error occurred: {ex.Message}" });
        }
    }
}

// Request DTOs
public class TrainModelRequest
{
    [Required]
    [RegularExpression("^(electricity|water|naturalgas|paper)$",
        ErrorMessage = "ResourceType must be one of: electricity, water, naturalgas, paper")]
    public string ResourceType { get; set; }

    public string BuildingId { get; set; } = "0";

    public System.Collections.Generic.List<string> ModelTypes { get; set; }

    public System.Collections.Generic.List<string> EnsembleTypes { get; set; }
}

public class PredictRequest
{
    [Required]
    [RegularExpression("^(electricity|water|naturalgas|paper)$",
        ErrorMessage = "ResourceType must be one of: electricity, water, naturalgas, paper")]
    public string ResourceType { get; set; }

    public string BuildingId { get; set; } = "0";

    [Required]
    public string ModelType { get; set; }

    [Range(1, 60, ErrorMessage = "MonthsAhead must be between 1 and 60")]
    public int MonthsAhead { get; set; } = 12;
}
}

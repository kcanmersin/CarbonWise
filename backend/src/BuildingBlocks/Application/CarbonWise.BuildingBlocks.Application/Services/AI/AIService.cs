using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CarbonWise.BuildingBlocks.Application.Services.AI
{
    public class AIService : IAIService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<AIService> _logger;
        private readonly string _fastApiBaseUrl;
        private readonly JsonSerializerOptions _jsonOptions;

        public AIService(HttpClient httpClient, IConfiguration configuration, ILogger<AIService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _fastApiBaseUrl = configuration["AIService:BaseUrl"] ?? "http://localhost:8000";

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true
            };

            // Configure HttpClient
            _httpClient.BaseAddress = new Uri(_fastApiBaseUrl);
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
            _httpClient.Timeout = TimeSpan.FromMinutes(10); // Long timeout for training
        }

        public async Task<AITrainResponse> TrainModelAsync(AITrainRequest request)
        {
            try
            {
                _logger.LogInformation("Starting model training for {ResourceType}, Building: {BuildingId}",
                    request.ResourceType, request.BuildingId);

                var requestBody = new
                {
                    resource_type = request.ResourceType,
                    building_id = request.BuildingId,
                    model_types = request.ModelTypes,
                    ensemble_types = request.EnsembleTypes
                };

                var json = JsonSerializer.Serialize(requestBody, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("/train", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Training failed with status {StatusCode}: {Content}",
                        response.StatusCode, responseContent);

                    return new AITrainResponse
                    {
                        Success = false,
                        Message = $"Training failed: {response.StatusCode} - {responseContent}"
                    };
                }

                var fastApiResponse = JsonSerializer.Deserialize<FastApiTrainResponse>(responseContent, _jsonOptions);

                var result = new AITrainResponse
                {
                    Success = fastApiResponse.Success,
                    Message = fastApiResponse.Message,
                    ModelsTrained = fastApiResponse.ModelsTrained ?? new List<string>(),
                    Metrics = ConvertMetrics(fastApiResponse.Metrics),
                    DataInfo = ConvertDataInfo(fastApiResponse.DataInfo)
                };

                _logger.LogInformation("Training completed successfully. Models trained: {Count}",
                    result.ModelsTrained.Count);

                return result;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error during model training");
                return new AITrainResponse
                {
                    Success = false,
                    Message = $"Network error: {ex.Message}"
                };
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "Timeout during model training");
                return new AITrainResponse
                {
                    Success = false,
                    Message = "Training request timed out"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during model training");
                return new AITrainResponse
                {
                    Success = false,
                    Message = $"Unexpected error: {ex.Message}"
                };
            }
        }

        public async Task<AIPredictResponse> PredictAsync(AIPredictRequest request)
        {
            try
            {
                _logger.LogInformation("Starting prediction for {ResourceType}, Building: {BuildingId}, Model: {ModelType}",
                    request.ResourceType, request.BuildingId, request.ModelType);

                var requestBody = new
                {
                    resource_type = request.ResourceType,
                    building_id = request.BuildingId,
                    model_type = request.ModelType,
                    months_ahead = request.MonthsAhead
                };

                var json = JsonSerializer.Serialize(requestBody, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("/predict", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Prediction failed with status {StatusCode}: {Content}",
                        response.StatusCode, responseContent);

                    return new AIPredictResponse
                    {
                        Success = false,
                        Predictions = new List<AIPrediction>()
                    };
                }

                var fastApiResponse = JsonSerializer.Deserialize<FastApiPredictResponse>(responseContent, _jsonOptions);

                var result = new AIPredictResponse
                {
                    Success = fastApiResponse.Success,
                    Predictions = ConvertPredictions(fastApiResponse.Predictions),
                    ModelInfo = ConvertModelInfo(fastApiResponse.ModelInfo)
                };

                _logger.LogInformation("Prediction completed successfully. Predictions: {Count}",
                    result.Predictions.Count);

                return result;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error during prediction");
                return new AIPredictResponse
                {
                    Success = false,
                    Predictions = new List<AIPrediction>()
                };
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "Timeout during prediction");
                return new AIPredictResponse
                {
                    Success = false,
                    Predictions = new List<AIPrediction>()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during prediction");
                return new AIPredictResponse
                {
                    Success = false,
                    Predictions = new List<AIPrediction>()
                };
            }
        }

        public async Task<List<AIModelInfo>> GetModelsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("/models");
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Get models failed with status {StatusCode}: {Content}",
                        response.StatusCode, responseContent);
                    return new List<AIModelInfo>();
                }

                var fastApiModels = JsonSerializer.Deserialize<List<FastApiModelInfo>>(responseContent, _jsonOptions);
                return ConvertModelInfoList(fastApiModels);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting models list");
                return new List<AIModelInfo>();
            }
        }

        public async Task<AIHealthResponse> GetHealthAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("/health");
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return new AIHealthResponse
                    {
                        Status = "unhealthy",
                        Database = "unknown"
                    };
                }

                var fastApiHealth = JsonSerializer.Deserialize<FastApiHealthResponse>(responseContent, _jsonOptions);

                return new AIHealthResponse
                {
                    Status = fastApiHealth.Status,
                    Database = fastApiHealth.Database,
                    Environment = fastApiHealth.Environment,
                    Debug = fastApiHealth.Debug,
                    DatabaseHost = fastApiHealth.DatabaseHost,
                    DatabaseName = fastApiHealth.DatabaseName,
                    ModelsDirectory = fastApiHealth.ModelsDirectory,
                    Timestamp = fastApiHealth.Timestamp
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking AI service health");
                return new AIHealthResponse
                {
                    Status = "unhealthy",
                    Database = "error"
                };
            }
        }

        // Private helper methods for conversion
        private Dictionary<string, AIMetrics> ConvertMetrics(Dictionary<string, object> fastApiMetrics)
        {
            var result = new Dictionary<string, AIMetrics>();

            if (fastApiMetrics == null) return result;

            foreach (var kvp in fastApiMetrics)
            {
                try
                {
                    var metricsJson = JsonSerializer.Serialize(kvp.Value);
                    var metrics = JsonSerializer.Deserialize<FastApiMetrics>(metricsJson, _jsonOptions);

                    result[kvp.Key] = new AIMetrics
                    {
                        MSE = metrics.MSE,
                        RMSE = metrics.RMSE,
                        MAE = metrics.MAE,
                        MAPE = metrics.MAPE,
                        R2 = metrics.R2
                    };
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error converting metrics for model {ModelType}", kvp.Key);
                }
            }

            return result;
        }

        private AIDataInfo ConvertDataInfo(object fastApiDataInfo)
        {
            if (fastApiDataInfo == null) return new AIDataInfo();

            try
            {
                var dataInfoJson = JsonSerializer.Serialize(fastApiDataInfo);
                var dataInfo = JsonSerializer.Deserialize<FastApiDataInfo>(dataInfoJson, _jsonOptions);

                return new AIDataInfo
                {
                    TotalRecords = dataInfo.TotalRecords,
                    TrainingRecords = dataInfo.TrainingRecords,
                    TestRecords = dataInfo.TestRecords,
                    FeaturesCount = dataInfo.FeaturesCount,
                    DateRange = dataInfo.DateRange
                };
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error converting data info");
                return new AIDataInfo();
            }
        }

        private List<AIPrediction> ConvertPredictions(List<FastApiPrediction> fastApiPredictions)
        {
            var result = new List<AIPrediction>();

            if (fastApiPredictions == null) return result;

            foreach (var pred in fastApiPredictions)
            {
                result.Add(new AIPrediction
                {
                    Date = pred.Date,
                    PredictedUsage = pred.PredictedUsage,
                    Month = pred.Month,
                    Year = pred.Year
                });
            }

            return result;
        }

        private AIModelInfo ConvertModelInfo(object fastApiModelInfo)
        {
            if (fastApiModelInfo == null) return new AIModelInfo();

            try
            {
                var modelInfoJson = JsonSerializer.Serialize(fastApiModelInfo);
                var modelInfo = JsonSerializer.Deserialize<FastApiModelInfo>(modelInfoJson, _jsonOptions);

                return new AIModelInfo
                {
                    ResourceType = modelInfo.ResourceType,
                    BuildingId = modelInfo.BuildingId,
                    ModelType = modelInfo.ModelType,
                    TrainedAt = modelInfo.TrainedAt,
                    Metrics = modelInfo.Metrics != null ? new AIMetrics
                    {
                        MSE = modelInfo.Metrics.MSE,
                        RMSE = modelInfo.Metrics.RMSE,
                        MAE = modelInfo.Metrics.MAE,
                        MAPE = modelInfo.Metrics.MAPE,
                        R2 = modelInfo.Metrics.R2
                    } : new AIMetrics(),
                    DataPoints = modelInfo.DataPoints,
                    MonthsPredicted = modelInfo.MonthsPredicted,
                    FeatureCount = modelInfo.FeatureCount,
                    FeaturesUsed = modelInfo.FeaturesUsed ?? new List<string>()
                };
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error converting model info");
                return new AIModelInfo();
            }
        }

        private List<AIModelInfo> ConvertModelInfoList(List<FastApiModelInfo> fastApiModels)
        {
            var result = new List<AIModelInfo>();

            if (fastApiModels == null) return result;

            foreach (var model in fastApiModels)
            {
                result.Add(new AIModelInfo
                {
                    ResourceType = model.ResourceType,
                    BuildingId = model.BuildingId,
                    ModelType = model.ModelType,
                    TrainedAt = model.TrainedAt,
                    Metrics = model.Metrics != null ? new AIMetrics
                    {
                        MSE = model.Metrics.MSE,
                        RMSE = model.Metrics.RMSE,
                        MAE = model.Metrics.MAE,
                        MAPE = model.Metrics.MAPE,
                        R2 = model.Metrics.R2
                    } : new AIMetrics(),
                    DataPoints = model.DataPoints
                });
            }

            return result;
        }
    }

    // FastAPI Response DTOs (for deserialization)
    internal class FastApiTrainResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public List<string> ModelsTrained { get; set; }
        public Dictionary<string, object> Metrics { get; set; }
        public object DataInfo { get; set; }
    }

    internal class FastApiPredictResponse
    {
        public bool Success { get; set; }
        public List<FastApiPrediction> Predictions { get; set; }
        public object ModelInfo { get; set; }
    }

    internal class FastApiPrediction
    {
        public string Date { get; set; }
        public double PredictedUsage { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
    }

    internal class FastApiModelInfo
    {
        public string ResourceType { get; set; }
        public string BuildingId { get; set; }
        public string ModelType { get; set; }
        public string TrainedAt { get; set; }
        public FastApiMetrics Metrics { get; set; }
        public int DataPoints { get; set; }
        public double MonthsPredicted { get; set; }
        public double FeatureCount { get; set; }
        public List<string> FeaturesUsed { get; set; }
    }

    internal class FastApiMetrics
    {
        public double MSE { get; set; }
        public double RMSE { get; set; }
        public double MAE { get; set; }
        public double MAPE { get; set; }
        public double R2 { get; set; }
    }

    internal class FastApiDataInfo
    {
        public double TotalRecords { get; set; }
        public double TrainingRecords { get; set; }
        public double TestRecords { get; set; }
        public double FeaturesCount { get; set; }
        public string DateRange { get; set; }
    }

    internal class FastApiHealthResponse
    {
        public string Status { get; set; }
        public string Database { get; set; }
        public string Environment { get; set; }
        public bool Debug { get; set; }
        public string DatabaseHost { get; set; }
        public string DatabaseName { get; set; }
        public string ModelsDirectory { get; set; }
        public string Timestamp { get; set; }
    }
}

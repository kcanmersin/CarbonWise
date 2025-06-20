using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarbonWise.BuildingBlocks.Application.Services.AI
{
    public interface IAIService
    {
        Task<AITrainResponse> TrainModelAsync(AITrainRequest request);
        Task<AIPredictResponse> PredictAsync(AIPredictRequest request);
        Task<List<AIModelInfo>> GetModelsAsync();
        Task<AIHealthResponse> GetHealthAsync();
    }
    public class AITrainRequest
    {
        public string ResourceType { get; set; }
        public string BuildingId { get; set; }
        public List<string> ModelTypes { get; set; } = new List<string> { "rf", "xgb", "gb" };
        public List<string> EnsembleTypes { get; set; } = new List<string> { "rf_gb", "rf_xgb", "gb_xgb", "rf_gb_xgb" };
    }

    public class AITrainResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public List<string> ModelsTrained { get; set; } = new List<string>();
        public Dictionary<string, AIMetrics> Metrics { get; set; } = new Dictionary<string, AIMetrics>();
        public AIDataInfo DataInfo { get; set; }
    }

    public class AIPredictRequest
    {
        public string ResourceType { get; set; }
        public string BuildingId { get; set; } = "0";
        public string ModelType { get; set; }
        public int MonthsAhead { get; set; } = 12;
    }

    public class AIPredictResponse
    {
        public bool Success { get; set; }
        public List<AIPrediction> Predictions { get; set; } = new List<AIPrediction>();
        public AIModelInfo ModelInfo { get; set; }
    }

    public class AIMetrics
    {
        public double MSE { get; set; }
        public double RMSE { get; set; }
        public double MAE { get; set; }
        public double MAPE { get; set; }
        public double R2 { get; set; }
    }

    public class AIDataInfo
    {
        public double TotalRecords { get; set; }
        public double TrainingRecords { get; set; }
        public double TestRecords { get; set; }
        public double FeaturesCount { get; set; }
        public string DateRange { get; set; }
    }

    public class AIPrediction
    {
        public string Date { get; set; }
        public double PredictedUsage { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
    }

    public class AIModelInfo
    {
        public string ResourceType { get; set; }
        public string BuildingId { get; set; }
        public string ModelType { get; set; }
        public string TrainedAt { get; set; }
        public AIMetrics Metrics { get; set; }
        public int DataPoints { get; set; }
        public double MonthsPredicted { get; set; }
        public double FeatureCount { get; set; }
        public List<string> FeaturesUsed { get; set; } = new List<string>();
    }

    public class AIHealthResponse
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

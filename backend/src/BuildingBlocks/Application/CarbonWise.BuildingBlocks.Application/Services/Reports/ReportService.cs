using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using CarbonWise.BuildingBlocks.Application.Services.CarbonFootprints;
using CarbonWise.BuildingBlocks.Application.Services.Consumption;
using CarbonWise.BuildingBlocks.Application.Services.LLMService;
using CarbonWise.BuildingBlocks.Domain.Buildings;
using CarbonWise.BuildingBlocks.Domain.Electrics;
using CarbonWise.BuildingBlocks.Domain.NaturalGases;

namespace CarbonWise.BuildingBlocks.Application.Services.Reports
{
    public class ReportService : IReportService
    {
        private readonly ICarbonFootprintService _carbonFootprintService;
        private readonly IConsumptionDataService _consumptionDataService;
        private readonly IBuildingRepository _buildingRepository;
        private readonly ILlmService _llmService;
        private readonly IElectricRepository _electricRepository;
        private readonly INaturalGasRepository _naturalGasRepository;

        private static readonly List<string> _consumptionTypes = new List<string>
        {
            "Electric",
            "NaturalGas",
            "Water",
            "Paper"
        };

        public ReportService(
            ICarbonFootprintService carbonFootprintService,
            IConsumptionDataService consumptionDataService,
            IBuildingRepository buildingRepository,
            ILlmService llmService,
            IElectricRepository electricRepository,
            INaturalGasRepository naturalGasRepository)
        {
            _carbonFootprintService = carbonFootprintService;
            _consumptionDataService = consumptionDataService;
            _buildingRepository = buildingRepository;
            _llmService = llmService;
            _electricRepository = electricRepository;
            _naturalGasRepository = naturalGasRepository;
        }

        public async Task<ReportDto> GenerateCarbonFootprintReportAsync(DateTime startDate, DateTime endDate)
        {
            // Get carbon footprint data
            var carbonFootprints = await _carbonFootprintService.CalculateForPeriodAsync(startDate, endDate);

            // Generate analysis using LLM
            var prompt = CreateCarbonFootprintAnalysisPrompt(carbonFootprints, startDate, endDate);
            var analysis = await GetLlmAnalysisAsync(prompt, carbonFootprints);

            // Create the report
            return new ReportDto
            {
                Id = Guid.NewGuid(),
                Title = $"Carbon Footprint Report ({startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd})",
                ReportType = "CarbonFootprint",
                StartDate = startDate,
                EndDate = endDate,
                GeneratedAt = DateTime.UtcNow,
                Analysis = analysis,
                Data = carbonFootprints
            };
        }

        public async Task<ReportDto> GenerateConsumptionReportAsync(
            string consumptionType,
            Guid? buildingId,
            DateTime startDate,
            DateTime endDate)
        {
            // Validate consumption type
            if (!_consumptionTypes.Contains(consumptionType))
            {
                throw new ArgumentException($"Invalid consumption type: {consumptionType}");
            }

            // Get building information if specified
            string buildingName = null;
            if (buildingId.HasValue)
            {
                var building = await _buildingRepository.GetByIdAsync(new BuildingId(buildingId.Value));
                if (building == null)
                {
                    throw new ArgumentException($"Building with ID {buildingId} not found");
                }
                buildingName = building.Name;
            }

            // Declare variable to hold the data for analysis
            object dataForAnalysis;
            IEnumerable<ConsumptionDataDto> rawConsumptionData = null;

            // Different handling based on consumption type
            if (consumptionType == "Electric")
            {
                if (buildingId.HasValue)
                {
                    // For specific building, get regular consumption data
                    var consumptionData = await _consumptionDataService.GetConsumptionDataAsync(
                        consumptionType,
                        startDate,
                        endDate);

                    rawConsumptionData = consumptionData.Where(d => d.BuildingId == buildingId.Value).ToList();
                    dataForAnalysis = rawConsumptionData;
                }
                else
                {
                    // Get monthly totals for organization-wide reports
                    var monthlyTotals = await _electricRepository.GetMonthlyTotalsAsync(startDate, endDate);
                    dataForAnalysis = monthlyTotals;
                }
            }
            else if (consumptionType == "NaturalGas")
            {
                if (buildingId.HasValue)
                {
                    // For specific building, get regular consumption data
                    var consumptionData = await _consumptionDataService.GetConsumptionDataAsync(
                        consumptionType,
                        startDate,
                        endDate);

                    rawConsumptionData = consumptionData.Where(d => d.BuildingId == buildingId.Value).ToList();
                    dataForAnalysis = rawConsumptionData;
                }
                else
                {
                    // Get monthly totals for organization-wide reports
                    var monthlyTotals = await _naturalGasRepository.GetMonthlyTotalsAsync(startDate, endDate);
                    dataForAnalysis = monthlyTotals;
                }
            }
            else
            {
                // For Water and Paper, use the existing approach
                var consumptionData = await _consumptionDataService.GetConsumptionDataAsync(
                    consumptionType,
                    startDate,
                    endDate);

                if (buildingId.HasValue)
                {
                    rawConsumptionData = consumptionData.Where(d => d.BuildingId == buildingId.Value).ToList();
                    dataForAnalysis = rawConsumptionData;
                }
                else
                {
                    rawConsumptionData = consumptionData;
                    dataForAnalysis = CreateMonthlyAggregateData(consumptionType, consumptionData);
                }
            }

            // Generate analysis using LLM
            string prompt;

            if (buildingId.HasValue && rawConsumptionData != null)
            {
                // For specific building with raw data
                prompt = CreateConsumptionAnalysisPrompt(consumptionType, buildingName, rawConsumptionData, startDate, endDate, true);
            }
            else if (consumptionType == "Electric")
            {
                // For electric monthly totals
                prompt = CreateElectricMonthlyTotalsPrompt(dataForAnalysis, startDate, endDate);
            }
            else if (consumptionType == "NaturalGas")
            {
                // For natural gas monthly totals
                prompt = CreateNaturalGasMonthlyTotalsPrompt(dataForAnalysis, startDate, endDate);
            }
            else
            {
                // For water and paper aggregates
                prompt = CreateConsumptionAnalysisPrompt(consumptionType, buildingName, rawConsumptionData, startDate, endDate, false);
            }

            var analysis = await GetLlmAnalysisAsync(prompt, dataForAnalysis);

            // Create report title
            string title = buildingId.HasValue
                ? $"{consumptionType} Consumption Report for {buildingName} ({startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd})"
                : $"{consumptionType} Consumption Report ({startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd})";

            // Create the report
            return new ReportDto
            {
                Id = Guid.NewGuid(),
                Title = title,
                ReportType = "Consumption",
                ConsumptionType = consumptionType,
                BuildingId = buildingId,
                BuildingName = buildingName,
                StartDate = startDate,
                EndDate = endDate,
                GeneratedAt = DateTime.UtcNow,
                Analysis = analysis,
                Data = dataForAnalysis
            };
        }

        public Task<List<string>> GetConsumptionTypesAsync()
        {
            return Task.FromResult(_consumptionTypes);
        }

        private string CreateCarbonFootprintAnalysisPrompt(
            List<Domain.CarbonFootprints.CarbonFootprint> carbonFootprints,
            DateTime startDate,
            DateTime endDate)
        {
            var totalEmission = carbonFootprints.Sum(cf => cf.TotalEmission);
            var avgEmission = carbonFootprints.Count > 0 ? totalEmission / carbonFootprints.Count : 0;

            var footprintData = JsonSerializer.Serialize(carbonFootprints, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            return $@"Analyze the following carbon footprint data for the period from {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}.

Total emission for the period: {totalEmission} kg CO2e
Average emission per year/period: {avgEmission} kg CO2e

Raw data:
{footprintData}

Please provide a comprehensive analysis of this carbon footprint data, including:
1. Overall trends in carbon emissions over the period
2. Main contributors to the carbon footprint
3. Areas where emissions have increased or decreased significantly
4. Recommendations for reducing carbon emissions
5. Any other insights or patterns in the data

Keep your analysis concise but informative.";
        }

        private object CreateMonthlyAggregateData(string consumptionType, IEnumerable<ConsumptionDataDto> consumptionData)
        {
            var monthlyData = consumptionData
                .GroupBy(d => new { Year = d.Date.Year, Month = d.Date.Month })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    YearMonth = $"{g.Key.Year}-{g.Key.Month:D2}",
                    TotalUsage = g.Sum(d => d.Usage),
                    AverageUsage = g.Average(d => d.Usage),
                    RecordCount = g.Count(),

                    // Type-specific values
                    TotalKWHValue = consumptionType == "Electric"
                        ? g.Sum(d => d.KWHValue ?? 0)
                        : (decimal?)null,

                    TotalSM3Value = consumptionType == "NaturalGas"
                        ? g.Sum(d => d.SM3Value ?? 0)
                        : (decimal?)null,

                    // Building-level aggregations
                    BuildingBreakdown = consumptionType == "Electric" || consumptionType == "NaturalGas"
                        ? g.GroupBy(d => new { BuildingId = d.BuildingId, BuildingName = d.BuildingName })
                          .Where(bg => bg.Key.BuildingId.HasValue) // Filter out nulls
                          .Select(bg => new {
                              BuildingId = bg.Key.BuildingId,
                              BuildingName = bg.Key.BuildingName,
                              TotalUsage = bg.Sum(d => d.Usage),
                              Percentage = bg.Sum(d => d.Usage) / g.Sum(d => d.Usage) * 100
                          })
                          .OrderByDescending(b => b.TotalUsage)
                          .ToList()
                        : null
                })
                .OrderBy(d => d.Year)
                .ThenBy(d => d.Month)
                .ToList();

            return new
            {
                MonthlyAggregates = monthlyData,
                TotalUsage = monthlyData.Sum(m => m.TotalUsage),
                AverageMonthlyUsage = monthlyData.Average(m => m.TotalUsage),
                MonthCount = monthlyData.Count,

                // Type-specific totals
                TotalKWHValue = consumptionType == "Electric"
                    ? monthlyData.Sum(m => m.TotalKWHValue ?? 0)
                    : (decimal?)null,

                TotalSM3Value = consumptionType == "NaturalGas"
                    ? monthlyData.Sum(m => m.TotalSM3Value ?? 0)
                    : (decimal?)null
            };
        }

        private string CreateConsumptionAnalysisPrompt(
            string consumptionType,
            string buildingName,
            IEnumerable<ConsumptionDataDto> consumptionData,
            DateTime startDate,
            DateTime endDate,
            bool isForSpecificBuilding)
        {
            var data = consumptionData.ToList();
            var totalUsage = data.Sum(d => d.Usage);
            var avgUsage = data.Count > 0 ? totalUsage / data.Count : 0;

            string buildingInfo = buildingName != null
                ? $" for building '{buildingName}'"
                : " across all buildings";

            string promptIntro = $"Analyze the following {consumptionType.ToLower()} consumption data{buildingInfo} for the period from {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}.";

            // Different prompt sections based on whether this is for a specific building or for monthly aggregates
            if (isForSpecificBuilding)
            {
                var monthlyAggregation = data
                    .GroupBy(d => new { Year = d.Date.Year, Month = d.Date.Month })
                    .Select(g => new
                    {
                        YearMonth = $"{g.Key.Year}-{g.Key.Month:D2}",
                        TotalUsage = g.Sum(d => d.Usage),
                        Count = g.Count()
                    })
                    .OrderBy(g => g.YearMonth)
                    .ToList();

                var consumptionDetails = JsonSerializer.Serialize(new
                {
                    SummaryData = new
                    {
                        ConsumptionType = consumptionType,
                        BuildingName = buildingName,
                        StartDate = startDate,
                        EndDate = endDate,
                        TotalUsage = totalUsage,
                        AverageUsage = avgUsage,
                        RecordCount = data.Count
                    },
                    MonthlyTrends = monthlyAggregation
                }, new JsonSerializerOptions { WriteIndented = true });

                return $@"{promptIntro}

Consumption data details:
{consumptionDetails}

Please provide a comprehensive analysis of this building's consumption data, including:
1. Overall trends in {consumptionType.ToLower()} consumption over the period
2. Monthly or seasonal patterns in the data
3. Unusual spikes or drops in consumption
4. Efficiency recommendations based on the consumption patterns
5. Any other insights or patterns in the data

Keep your analysis concise but informative, focusing on actionable insights.";
            }
            else
            {
                // For monthly aggregate data
                return $@"{promptIntro}

The data provided is monthly aggregate data across all buildings.

Please provide a comprehensive analysis of this consumption data, including:
1. Overall trends in {consumptionType.ToLower()} consumption over the period
2. Monthly or seasonal patterns across the organization
3. Year-over-year comparison if data spans multiple years
4. Building-level insights (which buildings consume the most and why)
5. Recommendations for reducing {consumptionType.ToLower()} consumption organization-wide
6. Any other noteworthy insights from the data

Keep your analysis concise but informative, focusing on organization-wide trends and actionable insights.";
            }
        }

        private string CreateElectricMonthlyTotalsPrompt(object monthlyTotals, DateTime startDate, DateTime endDate)
        {
            return $@"Analyze the following electricity consumption monthly totals data for the period from {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}.

This data shows the monthly aggregated electricity consumption across the organization.

Please provide a comprehensive analysis of this consumption data, including:
1. Overall trends in electricity consumption over the period
2. Monthly or seasonal patterns in the data
3. Year-over-year comparison if data spans multiple years
4. Months with unusually high or low consumption
5. Recommendations for optimizing electricity usage
6. Any other insights or patterns in the data

Focus on identifying actionable insights that could help reduce electricity consumption and improve efficiency.";
        }

        private string CreateNaturalGasMonthlyTotalsPrompt(object monthlyTotals, DateTime startDate, DateTime endDate)
        {
            return $@"Analyze the following natural gas consumption monthly totals data for the period from {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}.

This data shows the monthly aggregated natural gas consumption across the organization.

Please provide a comprehensive analysis of this consumption data, including:
1. Overall trends in natural gas consumption over the period
2. Seasonal patterns (especially heating seasons)
3. Year-over-year comparison if data spans multiple years
4. Months with unusually high or low consumption
5. Recommendations for optimizing natural gas usage
6. Any other insights or patterns in the data

Focus on identifying actionable insights that could help reduce natural gas consumption and improve efficiency.";
        }

        private async Task<string> GetLlmAnalysisAsync<T>(string prompt, T data)
        {
            // Prepare context for LLM
            var context = new Dictionary<string, object>
            {
                { "data", data }
            };

            // Request analysis from LLM
            var llmRequest = new LlmRequest
            {
                Prompt = prompt,
                Context = context
            };

            var llmResponse = await _llmService.GenerateContentAsync(llmRequest);
            return llmResponse.Content;
        }
    }
}
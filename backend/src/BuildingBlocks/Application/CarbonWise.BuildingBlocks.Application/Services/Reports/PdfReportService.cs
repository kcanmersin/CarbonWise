using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CarbonWise.BuildingBlocks.Application.Services.Consumption;
using CarbonWise.BuildingBlocks.Application.Services.CarbonFootprints;
using CarbonWise.BuildingBlocks.Application.Services.LLMService;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Text;
namespace CarbonWise.BuildingBlocks.Application.Services.Reports
{
    public class PdfReportService : IPdfReportService
    {
        private readonly IReportService _reportService;
        private readonly ILlmService _llmService;

        public PdfReportService(IReportService reportService, ILlmService llmService)
        {
            _reportService = reportService;
            _llmService = llmService;

            QuestPDF.Settings.DocumentLayoutExceptionThreshold = 50000;
        }

        public async Task<byte[]> GenerateCarbonFootprintPdfReportAsync(DateTime startDate, DateTime endDate)
        {
            var report = await _reportService.GenerateCarbonFootprintReportAsync(startDate, endDate);

            var carbonFootprints = report.Data as List<Domain.CarbonFootprints.CarbonFootprint>;
            if (carbonFootprints == null || !carbonFootprints.Any())
            {
                return Document.Create(document =>
                {
                    document.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(2, Unit.Centimetre);
                        page.Content().AlignCenter().Text("No carbon footprint data available for the selected period.")
                            .FontSize(14);
                    });
                }).GeneratePdf();
            }

            if (report.Analysis != null)
            {
                report.Analysis = SanitizeTextForPdf(report.Analysis);
            }

            string actionRecommendations = await GenerateLlmActionRecommendations(carbonFootprints, startDate, endDate);
            actionRecommendations = SanitizeTextForPdf(actionRecommendations);

            return Document.Create(document =>
            {
                document.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.DefaultTextStyle(style => style.FontFamily("Arial"));

                    page.Header().Element(container =>
                    {
                        container.Column(column =>
                        {
                            column.Item().AlignCenter().Text("GEBZE TECHNICAL UNIVERSITY")
                                .FontSize(20)
                                .Bold()
                                .FontColor(Colors.Blue.Darken4);

                            column.Item().BorderBottom(1).BorderColor(Colors.Grey.Medium).PaddingBottom(5);
                        });
                    });

                    page.Content().Column(column =>
                    {
                        column.Item().AlignCenter().Text("Carbon Footprint Report")
                            .FontSize(16)
                            .Bold();

                        column.Item().AlignCenter().Text($"Period: {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}")
                            .FontSize(12);

                        column.Item().Height(20);

                        column.Item().Background(Colors.Grey.Lighten5).Padding(10).Column(dataColumn =>
                        {
                            dataColumn.Item().Text("Carbon Footprint Data")
                                .FontSize(14)
                                .Bold()
                                .FontColor(Colors.Blue.Darken3);

                            dataColumn.Item().Height(10);

                            dataColumn.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.ConstantColumn(70);
                                    columns.ConstantColumn(80);
                                    columns.ConstantColumn(80);
                                    columns.ConstantColumn(70);
                                    columns.ConstantColumn(80);
                                    columns.ConstantColumn(70);
                                });

                                table.Cell().Border(1).Background(Colors.Grey.Lighten3).AlignCenter().Text("Year");
                                table.Cell().Border(1).Background(Colors.Grey.Lighten3).AlignCenter().Text("Electricity");
                                table.Cell().Border(1).Background(Colors.Grey.Lighten3).AlignCenter().Text("Shuttle Bus");
                                table.Cell().Border(1).Background(Colors.Grey.Lighten3).AlignCenter().Text("Car");
                                table.Cell().Border(1).Background(Colors.Grey.Lighten3).AlignCenter().Text("Motorcycle");
                                table.Cell().Border(1).Background(Colors.Grey.Lighten3).AlignCenter().Text("Total");

                                foreach (var footprint in carbonFootprints.OrderBy(f => f.Year))
                                {
                                    table.Cell().Border(1).AlignCenter().Text(footprint.Year.ToString());
                                    table.Cell().Border(1).AlignRight().Text($"{footprint.ElectricityEmission:N1}");
                                    table.Cell().Border(1).AlignRight().Text($"{footprint.ShuttleBusEmission:N1}");
                                    table.Cell().Border(1).AlignRight().Text($"{footprint.CarEmission:N1}");
                                    table.Cell().Border(1).AlignRight().Text($"{footprint.MotorcycleEmission:N1}");
                                    table.Cell().Border(1).AlignRight().Text($"{footprint.TotalEmission:N1}").Bold();
                                }

                                var totalEmission = carbonFootprints.Sum(cf => cf.TotalEmission);
                                table.Cell().Border(1).Background(Colors.Grey.Lighten3).AlignCenter().Text("TOTAL").Bold();
                                table.Cell().Border(1).Background(Colors.Grey.Lighten3).AlignRight().Text($"{carbonFootprints.Sum(f => f.ElectricityEmission):N1}").Bold();
                                table.Cell().Border(1).Background(Colors.Grey.Lighten3).AlignRight().Text($"{carbonFootprints.Sum(f => f.ShuttleBusEmission):N1}").Bold();
                                table.Cell().Border(1).Background(Colors.Grey.Lighten3).AlignRight().Text($"{carbonFootprints.Sum(f => f.CarEmission):N1}").Bold();
                                table.Cell().Border(1).Background(Colors.Grey.Lighten3).AlignRight().Text($"{carbonFootprints.Sum(f => f.MotorcycleEmission):N1}").Bold();
                                table.Cell().Border(1).Background(Colors.Grey.Lighten3).AlignRight().Text($"{totalEmission:N1}").Bold();
                            });

                            dataColumn.Item().Height(15);

                            var totalEmission = carbonFootprints.Sum(cf => cf.TotalEmission);
                            var electricityTotal = carbonFootprints.Sum(f => f.ElectricityEmission);
                            var electricityPercentage = totalEmission > 0 ? (electricityTotal / totalEmission * 100) : 0;
                            var shuttleBusTotal = carbonFootprints.Sum(f => f.ShuttleBusEmission);
                            var shuttleBusPercentage = totalEmission > 0 ? (shuttleBusTotal / totalEmission * 100) : 0;
                            var carTotal = carbonFootprints.Sum(f => f.CarEmission);
                            var carPercentage = totalEmission > 0 ? (carTotal / totalEmission * 100) : 0;
                            var motorcycleTotal = carbonFootprints.Sum(f => f.MotorcycleEmission);
                            var motorcyclePercentage = totalEmission > 0 ? (motorcycleTotal / totalEmission * 100) : 0;

                            dataColumn.Item().Text("Emission Sources Breakdown:")
                                .FontSize(12)
                                .Bold();

                            dataColumn.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(3);
                                    columns.RelativeColumn(2);
                                    columns.RelativeColumn(2);
                                    columns.RelativeColumn(4);
                                });

                                table.Cell().Background(Colors.Grey.Lighten4).Text("Source").Bold();
                                table.Cell().Background(Colors.Grey.Lighten4).Text("Emissions (kg CO2e)").Bold();
                                table.Cell().Background(Colors.Grey.Lighten4).Text("Percentage").Bold();
                                table.Cell().Background(Colors.Grey.Lighten4).Text("Distribution").Bold();

                                // Electricity
                                table.Cell().Text("Electricity");
                                table.Cell().Text($"{electricityTotal:N1}");
                                table.Cell().Text($"{electricityPercentage:N1}%");
                                table.Cell().PaddingVertical(2).PaddingHorizontal(5).Height(20).Background(Colors.Blue.Medium).AlignLeft()
                                    .Width((float)electricityPercentage);

                                // Shuttle Bus
                                table.Cell().Text("Shuttle Bus");
                                table.Cell().Text($"{shuttleBusTotal:N1}");
                                table.Cell().Text($"{shuttleBusPercentage:N1}%");
                                table.Cell().PaddingVertical(2).PaddingHorizontal(5).Height(20).Background(Colors.Green.Medium).AlignLeft()
                                    .Width((float)shuttleBusPercentage);

                                // Car
                                table.Cell().Text("Car");
                                table.Cell().Text($"{carTotal:N1}");
                                table.Cell().Text($"{carPercentage:N1}%");
                                table.Cell().PaddingVertical(2).PaddingHorizontal(5).Height(20).Background(Colors.Orange.Medium).AlignLeft()
                                    .Width((float)carPercentage);

                                // Motorcycle
                                table.Cell().Text("Motorcycle");
                                table.Cell().Text($"{motorcycleTotal:N1}");
                                table.Cell().Text($"{motorcyclePercentage:N1}%");
                                table.Cell().PaddingVertical(2).PaddingHorizontal(5).Height(20).Background(Colors.Red.Medium).AlignLeft()
                                    .Width((float)motorcyclePercentage);

                                // Total
                                table.Cell().Text("Total").Bold();
                                table.Cell().Text($"{totalEmission:N1}").Bold();
                                table.Cell().Text("100%").Bold();
                                table.Cell().Text("");
                            });
                        });

                        column.Item().Height(15);

                        column.Item().Background(Colors.Grey.Lighten5).Padding(10).Column(analysisColumn =>
                        {
                            analysisColumn.Item().Text("Analysis & Insights")
                                .FontSize(14)
                                .Bold()
                                .FontColor(Colors.Blue.Darken3);

                            analysisColumn.Item().Height(5);

                            if (!string.IsNullOrEmpty(report.Analysis))
                            {
                                analysisColumn.Item().Text(report.Analysis)
                                    .FontSize(11);
                            }
                            else
                            {
                                analysisColumn.Item().Text("No analysis available for the selected period.")
                                    .FontSize(11);
                            }
                        });

                        column.Item().Height(15);

                        column.Item().Background(Colors.Grey.Lighten5).Padding(10).Column(recommendationsColumn =>
                        {
                            recommendationsColumn.Item().Text("Action Recommendations")
                                .FontSize(14)
                                .Bold()
                                .FontColor(Colors.Green.Darken3);

                            recommendationsColumn.Item().Height(5);

                            if (!string.IsNullOrEmpty(actionRecommendations))
                            {
                                recommendationsColumn.Item().Text(actionRecommendations)
                                    .FontSize(11);
                            }
                            else
                            {
                                recommendationsColumn.Item().Text("No recommendations available for the selected period.")
                                    .FontSize(11);
                            }
                        });
                    });

                    page.Footer().AlignCenter().Text(text =>
                    {
                        text.Span("Page ");
                        text.CurrentPageNumber();
                        text.Span(" of ");
                        text.TotalPages();
                        text.Span($" | Generated on: {DateTime.Now:yyyy-MM-dd HH:mm}");
                    });
                });
            }).GeneratePdf();
        }

        private string SanitizeBuildingName(string name)
        {
            if (string.IsNullOrEmpty(name))
                return name;

            var replacements = new Dictionary<char, char>
            {
                {'İ', 'I'},
                {'ı', 'i'},
                {'Ş', 'S'},
                {'ş', 's'},
                {'Ğ', 'G'},
                {'ğ', 'g'},
                {'Ü', 'U'},
                {'ü', 'u'},
                {'Ö', 'O'},
                {'ö', 'o'},
                {'Ç', 'C'},
                {'ç', 'c'}
            };

            var result = new StringBuilder(name.Length);
            foreach (char c in name)
            {
                if (replacements.TryGetValue(c, out char replacement))
                    result.Append(replacement);
                else
                    result.Append(c);
            }

            return result.ToString();
        }

        private string SanitizeTextForPdf(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            text = text.Replace("\t", "    ");
            text = text.Replace("\r", "");

            var replacements = new Dictionary<char, char>
            {
                {'İ', 'I'},
                {'ı', 'i'},
                {'Ş', 'S'},
                {'ş', 's'},
                {'Ğ', 'G'},
                {'ğ', 'g'},
                {'Ü', 'U'},
                {'ü', 'u'},
                {'Ö', 'O'},
                {'ö', 'o'},
                {'Ç', 'C'},
                {'ç', 'c'}
            };

            var result = new StringBuilder(text.Length);
            foreach (char c in text)
            {
                if (replacements.TryGetValue(c, out char replacement))
                    result.Append(replacement);
                else
                    result.Append(c);
            }

            return result.ToString();
        }

        public async Task<byte[]> GenerateConsumptionPdfReportAsync(string consumptionType, Guid? buildingId, DateTime startDate, DateTime endDate)
        {
            var report = await _reportService.GenerateConsumptionReportAsync(consumptionType, buildingId, startDate, endDate);

            if (report.BuildingName != null)
            {
                report.BuildingName = SanitizeBuildingName(report.BuildingName);
            }

            if (report.Analysis != null)
            {
                report.Analysis = SanitizeTextForPdf(report.Analysis);
            }

            string efficiencyRecommendations = await GenerateLlmEfficiencyRecommendations(
                consumptionType, report.Data, startDate, endDate, report.BuildingName);
            efficiencyRecommendations = SanitizeTextForPdf(efficiencyRecommendations);

            return GenerateEnhancedConsumptionPdf(report, efficiencyRecommendations);
        }

        private async Task<string> GenerateLlmActionRecommendations(List<Domain.CarbonFootprints.CarbonFootprint> carbonFootprints, DateTime startDate, DateTime endDate)
        {
            try
            {
                var electricityTotal = carbonFootprints.Sum(f => f.ElectricityEmission);
                var shuttleBusTotal = carbonFootprints.Sum(f => f.ShuttleBusEmission);
                var carTotal = carbonFootprints.Sum(f => f.CarEmission);
                var motorcycleTotal = carbonFootprints.Sum(f => f.MotorcycleEmission);
                var totalEmission = carbonFootprints.Sum(f => f.TotalEmission);

                var prompt = $@"You are an expert in environmental science and sustainability specializing in carbon footprint analysis for educational institutions.

Based on the following carbon footprint data from Gebze Technical University for the period {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}:

- Total Carbon Emissions: {totalEmission:N1} kg CO2e
- Electricity Emissions: {electricityTotal:N1} kg CO2e ({(totalEmission > 0 ? electricityTotal / totalEmission * 100 : 0):N1}%)
- Shuttle Bus Emissions: {shuttleBusTotal:N1} kg CO2e ({(totalEmission > 0 ? shuttleBusTotal / totalEmission * 100 : 0):N1}%)
- Car Emissions: {carTotal:N1} kg CO2e ({(totalEmission > 0 ? carTotal / totalEmission * 100 : 0):N1}%)
- Motorcycle Emissions: {motorcycleTotal:N1} kg CO2e ({(totalEmission > 0 ? motorcycleTotal / totalEmission * 100 : 0):N1}%)

Please provide ONLY actionable recommendations for reducing the carbon footprint at the university. Focus on:
1. Practical steps to reduce emissions in the highest contributing categories
2. Specific initiatives the university could implement
3. Potential technology or policy changes that could make a significant impact
4. Measurable targets that could be set

Keep your response concise (maximum 300 words) and focused on practical recommendations that could be implemented at a university setting.";

                var llmRequest = new LlmRequest
                {
                    Prompt = prompt
                };

                var llmResponse = await _llmService.GenerateContentAsync(llmRequest);
                return llmResponse.Content;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error generating LLM action recommendations: {ex.Message}");
                return "Unable to generate action recommendations at this time.";
            }
        }

        private async Task<string> GenerateLlmEfficiencyRecommendations(string consumptionType, object data, DateTime startDate, DateTime endDate, string buildingName)
        {
            try
            {
                string sanitizedBuildingName = SanitizeBuildingName(buildingName);
                string buildingScope = string.IsNullOrEmpty(sanitizedBuildingName) ? "the entire university" : $"the {sanitizedBuildingName} building";

                var prompt = $@"You are an expert in resource efficiency and sustainability for educational institutions.

Based on the {consumptionType.ToLower()} consumption data from Gebze Technical University for {buildingScope} during the period {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}, please provide:

1. Specific efficiency recommendations to reduce {consumptionType.ToLower()} consumption
2. Technology upgrades or system improvements that could be implemented
3. Behavioral changes that could be encouraged among students and staff
4. Practical monitoring and management techniques to track progress
5. Potential cost savings and environmental benefits from implementing these recommendations

Keep your recommendations concise (maximum 300 words), practical and tailored specifically for a university environment.";

                var llmRequest = new LlmRequest
                {
                    Prompt = prompt
                };

                var llmResponse = await _llmService.GenerateContentAsync(llmRequest);
                return llmResponse.Content;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error generating LLM efficiency recommendations: {ex.Message}");
                return "Unable to generate efficiency recommendations at this time.";
            }
        }

        private string CleanTextForPdf(string text)
        {
            return SanitizeTextForPdf(text);
        }

        private byte[] GenerateEnhancedConsumptionPdf(ReportDto report, string efficiencyRecommendations)
        {
            return Document.Create(document =>
            {
                document.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(text => text.FontSize(11).FontFamily("Arial"));

                    page.Header().Element(ComposeHeader);

                    page.Content().Column(column =>
                    {
                        column.Item().PaddingTop(10).Text(report.Title)
                            .FontSize(16)
                            .Bold()
                            .FontColor(Colors.Black);

                        column.Item().PaddingTop(10).Grid(grid =>
                        {
                            grid.Columns(2);
                            grid.Item().Text("Report Type:").Bold();
                            grid.Item().Text(report.ReportType);

                            if (!string.IsNullOrEmpty(report.ConsumptionType))
                            {
                                grid.Item().Text("Consumption Type:").Bold();
                                grid.Item().Text(report.ConsumptionType);
                            }

                            if (!string.IsNullOrEmpty(report.BuildingName))
                            {
                                grid.Item().Text("Building:").Bold();
                                grid.Item().Text(report.BuildingName);
                            }

                            grid.Item().Text("Period:").Bold();
                            grid.Item().Text($"{report.StartDate:yyyy-MM-dd} to {report.EndDate:yyyy-MM-dd}");
                        });

                        column.Item().PaddingTop(20).Background(Colors.Grey.Lighten5).Padding(10).Column(dataColumn =>
                        {
                            dataColumn.Item().Text("Consumption Data")
                                .FontSize(14)
                                .Bold()
                                .FontColor(Colors.Blue.Darken4);

                            dataColumn.Item().PaddingTop(5);

                            if (report.ReportType == "Consumption")
                            {
                                if (report.ConsumptionType == "Electric")
                                {
                                    if (report.Data is List<Domain.Electrics.ElectricMonthlyTotalDto> monthlyTotals)
                                    {
                                        AddElectricMonthlyTotalsSummary(dataColumn, monthlyTotals);
                                    }
                                    else if (report.Data is IEnumerable<ConsumptionDataDto> electricData)
                                    {
                                        AddElectricDataSummary(dataColumn, electricData.ToList(), report.BuildingName);
                                    }
                                    else
                                    {
                                        dataColumn.Item().PaddingTop(5).Text("Electric consumption data format not recognized.");
                                    }
                                }
                                else if (report.ConsumptionType == "NaturalGas")
                                {
                                    if (report.Data is List<Application.Features.NaturalGases.NaturalGasMonthlyTotalDto> monthlyTotals)
                                    {
                                        AddNaturalGasMonthlyTotalsSummary(dataColumn, monthlyTotals);
                                    }
                                    else if (report.Data is IEnumerable<ConsumptionDataDto> gasData)
                                    {
                                        AddNaturalGasDataSummary(dataColumn, gasData.ToList(), report.BuildingName);
                                    }
                                    else
                                    {
                                        dataColumn.Item().PaddingTop(5).Text("Natural Gas consumption data format not recognized.");
                                    }
                                }
                                else if (report.ConsumptionType == "Water")
                                {
                                    if (report.Data is List<Domain.Waters.WaterMonthlyTotalDto> monthlyTotals)
                                    {
                                        AddWaterMonthlyTotalsSummary(dataColumn, monthlyTotals);
                                    }
                                    else if (report.Data is IEnumerable<ConsumptionDataDto> consumptionData)
                                    {
                                        AddGenericConsumptionSummary(dataColumn, consumptionData.ToList(), report.ConsumptionType, report.BuildingName);
                                    }
                                    else
                                    {
                                        dataColumn.Item().PaddingTop(5).Text($"Summary data for {report.ConsumptionType} is not in the expected format.");
                                    }
                                }
                                else if (report.ConsumptionType == "Paper")
                                {
                                    if (report.Data is List<Domain.Papers.PaperMonthlyTotalDto> monthlyTotals)
                                    {
                                        AddPaperMonthlyTotalsSummary(dataColumn, monthlyTotals);
                                    }
                                    else if (report.Data is IEnumerable<ConsumptionDataDto> consumptionData)
                                    {
                                        AddGenericConsumptionSummary(dataColumn, consumptionData.ToList(), report.ConsumptionType, report.BuildingName);
                                    }
                                    else
                                    {
                                        dataColumn.Item().PaddingTop(5).Text($"Summary data for {report.ConsumptionType} is not in the expected format.");
                                    }
                                }
                                else
                                {
                                    dataColumn.Item().PaddingTop(5).Text($"Consumption type '{report.ConsumptionType}' is not supported for summary generation.");
                                }
                            }
                            else
                            {
                                dataColumn.Item().PaddingTop(5).Text("Report type not recognized for summary generation.");
                            }
                        });

                        column.Item().PaddingTop(20).Background(Colors.Grey.Lighten5).Padding(10).Column(analysisColumn =>
                        {
                            analysisColumn.Item().Text("Analysis & Insights")
                                .FontSize(14)
                                .Bold()
                                .FontColor(Colors.Blue.Darken4);

                            analysisColumn.Item().PaddingTop(5);

                            if (!string.IsNullOrEmpty(report.Analysis))
                            {
                                analysisColumn.Item().Text(report.Analysis)
                                    .FontSize(11);
                            }
                            else
                            {
                                analysisColumn.Item().Text("No analysis available for the selected period.")
                                    .FontSize(11);
                            }
                        });

                        column.Item().PaddingTop(20).Background(Colors.Grey.Lighten5).Padding(10).Column(recommendationsColumn =>
                        {
                            recommendationsColumn.Item().Text("Efficiency Recommendations")
                                .FontSize(14)
                                .Bold()
                                .FontColor(Colors.Green.Darken3);

                            recommendationsColumn.Item().PaddingTop(5);

                            if (!string.IsNullOrEmpty(efficiencyRecommendations))
                            {
                                recommendationsColumn.Item().Text(efficiencyRecommendations)
                                    .FontSize(11);
                            }
                            else
                            {
                                recommendationsColumn.Item().Text("No efficiency recommendations available for the selected period.")
                                    .FontSize(11);
                            }
                        });
                    });

                    page.Footer().AlignCenter().Text(text =>
                    {
                        text.Span("Page ");
                        text.CurrentPageNumber();
                        text.Span(" of ");
                        text.TotalPages();
                        text.Span($" | Generated on: {DateTime.Now:yyyy-MM-dd HH:mm}");
                    });
                });
            }).GeneratePdf();
        }

        private void ComposeHeader(IContainer container)
        {
            container.Column(column =>
            {
                column.Item().AlignCenter().Text("GEBZE TECHNICAL UNIVERSITY")
                    .FontSize(20)
                    .Bold()
                    .FontColor(Colors.Blue.Darken4);
                column.Item().BorderBottom(1).BorderColor(Colors.Grey.Medium).PaddingBottom(5);
            });
        }

        private void AddElectricMonthlyTotalsSummary(ColumnDescriptor column, List<Domain.Electrics.ElectricMonthlyTotalDto> monthlyTotals)
        {
            column.Item().PaddingTop(5).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(100);
                    columns.ConstantColumn(120);
                    columns.ConstantColumn(120);
                });

                table.Header(header =>
                {
                    header.Cell().Background(Colors.Grey.Lighten2).Text("Month").Bold();
                    header.Cell().Background(Colors.Grey.Lighten2).Text("Total KWH").Bold();
                    header.Cell().Background(Colors.Grey.Lighten2).Text("Total Usage").Bold();
                });

                foreach (var total in monthlyTotals.OrderBy(t => t.Year).ThenBy(t => t.Month))
                {
                    table.Cell().Text(total.FormattedMonth);
                    table.Cell().Text($"{total.TotalKWHValue:N2}");
                    table.Cell().Text($"{total.TotalUsage:N2}");
                }

                table.Cell().Background(Colors.Grey.Lighten3).Text("TOTAL").Bold();
                table.Cell().Background(Colors.Grey.Lighten3).Text($"{monthlyTotals.Sum(t => t.TotalKWHValue):N2}").Bold();
                table.Cell().Background(Colors.Grey.Lighten3).Text($"{monthlyTotals.Sum(t => t.TotalUsage):N2}").Bold();
            });

            var years = monthlyTotals.Select(t => t.Year).Distinct().ToList();
            if (years.Count > 1)
            {
                column.Item().PaddingTop(20).Text("Yearly Summary")
                    .FontSize(12)
                    .Bold();

                column.Item().PaddingTop(5).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(100);
                        columns.ConstantColumn(120);
                        columns.ConstantColumn(120);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Background(Colors.Grey.Lighten2).Text("Year").Bold();
                        header.Cell().Background(Colors.Grey.Lighten2).Text("Total KWH").Bold();
                        header.Cell().Background(Colors.Grey.Lighten2).Text("Total Usage").Bold();
                    });

                    foreach (var year in years.OrderBy(y => y))
                    {
                        var yearData = monthlyTotals.Where(t => t.Year == year).ToList();
                        table.Cell().Text(year.ToString());
                        table.Cell().Text($"{yearData.Sum(t => t.TotalKWHValue):N2}");
                        table.Cell().Text($"{yearData.Sum(t => t.TotalUsage):N2}");
                    }
                });
            }
        }

        private void AddNaturalGasMonthlyTotalsSummary(ColumnDescriptor column, List<Application.Features.NaturalGases.NaturalGasMonthlyTotalDto> monthlyTotals)
        {
            column.Item().PaddingTop(5).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(100);
                    columns.ConstantColumn(120);
                    columns.ConstantColumn(120);
                });

                table.Header(header =>
                {
                    header.Cell().Background(Colors.Grey.Lighten2).Text("Month").Bold();
                    header.Cell().Background(Colors.Grey.Lighten2).Text("Total SM3").Bold();
                    header.Cell().Background(Colors.Grey.Lighten2).Text("Total Usage").Bold();
                });

                foreach (var total in monthlyTotals.OrderBy(t => t.Year).ThenBy(t => t.Month))
                {
                    table.Cell().Text(total.FormattedMonth);
                    table.Cell().Text($"{total.TotalSM3Value:N2}");
                    table.Cell().Text($"{total.TotalUsage:N2}");
                }

                table.Cell().Background(Colors.Grey.Lighten3).Text("TOTAL").Bold();
                table.Cell().Background(Colors.Grey.Lighten3).Text($"{monthlyTotals.Sum(t => t.TotalSM3Value):N2}").Bold();
                table.Cell().Background(Colors.Grey.Lighten3).Text($"{monthlyTotals.Sum(t => t.TotalUsage):N2}").Bold();
            });

            var years = monthlyTotals.Select(t => t.Year).Distinct().ToList();
            if (years.Count > 1)
            {
                column.Item().PaddingTop(20).Text("Yearly Summary")
                    .FontSize(12)
                    .Bold();

                column.Item().PaddingTop(5).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(100);
                        columns.ConstantColumn(120);
                        columns.ConstantColumn(120);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Background(Colors.Grey.Lighten2).Text("Year").Bold();
                        header.Cell().Background(Colors.Grey.Lighten2).Text("Total SM3").Bold();
                        header.Cell().Background(Colors.Grey.Lighten2).Text("Total Usage").Bold();
                    });

                    foreach (var year in years.OrderBy(y => y))
                    {
                        var yearData = monthlyTotals.Where(t => t.Year == year).ToList();
                        table.Cell().Text(year.ToString());
                        table.Cell().Text($"{yearData.Sum(t => t.TotalSM3Value):N2}");
                        table.Cell().Text($"{yearData.Sum(t => t.TotalUsage):N2}");
                    }
                });
            }
        }

        private void AddElectricDataSummary(ColumnDescriptor column, List<ConsumptionDataDto> data, string buildingName)
        {
            buildingName = SanitizeBuildingName(buildingName);

            var monthlyData = data
                .GroupBy(d => new { Year = d.Date.Year, Month = d.Date.Month })
                .Select(g => new
                {
                    YearMonth = $"{g.Key.Year}-{g.Key.Month:D2}",
                    TotalKWHValue = g.Sum(d => d.KWHValue ?? 0),
                    TotalUsage = g.Sum(d => d.Usage),
                    Count = g.Count()
                })
                .OrderBy(m => m.YearMonth)
                .ToList();

            string buildingInfo = string.IsNullOrEmpty(buildingName) ? "Overall" : $"for {buildingName}";
            column.Item().PaddingTop(5).Text($"Monthly Electric Consumption Summary {buildingInfo}")
                .FontSize(12)
                .Bold();

            column.Item().PaddingTop(5).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(100);
                    columns.ConstantColumn(100);
                    columns.ConstantColumn(100);
                    columns.RelativeColumn();
                });

                table.Header(header =>
                {
                    header.Cell().Background(Colors.Grey.Lighten2).Text("Month").Bold();
                    header.Cell().Background(Colors.Grey.Lighten2).Text("Total KWH").Bold();
                    header.Cell().Background(Colors.Grey.Lighten2).Text("Total Usage").Bold();
                    header.Cell().Background(Colors.Grey.Lighten2).Text("Records").Bold();
                });

                foreach (var month in monthlyData)
                {
                    table.Cell().Text(month.YearMonth);
                    table.Cell().Text($"{month.TotalKWHValue:N2}");
                    table.Cell().Text($"{month.TotalUsage:N2}");
                    table.Cell().Text(month.Count.ToString());
                }

                table.Cell().Background(Colors.Grey.Lighten3).Text("TOTAL").Bold();
                table.Cell().Background(Colors.Grey.Lighten3).Text($"{monthlyData.Sum(m => m.TotalKWHValue):N2}").Bold();
                table.Cell().Background(Colors.Grey.Lighten3).Text($"{monthlyData.Sum(m => m.TotalUsage):N2}").Bold();
                table.Cell().Background(Colors.Grey.Lighten3).Text(data.Count.ToString()).Bold();
            });

            var years = data.Select(d => d.Date.Year).Distinct().ToList();
            if (years.Count > 1)
            {
                column.Item().PaddingTop(20).Text("Yearly Summary")
                    .FontSize(12)
                    .Bold();

                column.Item().PaddingTop(5).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(100);
                        columns.ConstantColumn(100);
                        columns.ConstantColumn(100);
                        columns.RelativeColumn();
                    });

                    table.Header(header =>
                    {
                        header.Cell().Background(Colors.Grey.Lighten2).Text("Year").Bold();
                        header.Cell().Background(Colors.Grey.Lighten2).Text("Total KWH").Bold();
                        header.Cell().Background(Colors.Grey.Lighten2).Text("Total Usage").Bold();
                        header.Cell().Background(Colors.Grey.Lighten2).Text("Records").Bold();
                    });

                    foreach (var year in years.OrderBy(y => y))
                    {
                        var yearData = data.Where(d => d.Date.Year == year).ToList();
                        table.Cell().Text(year.ToString());
                        table.Cell().Text($"{yearData.Sum(d => d.KWHValue ?? 0):N2}");
                        table.Cell().Text($"{yearData.Sum(d => d.Usage):N2}");
                        table.Cell().Text(yearData.Count.ToString());
                    }
                });
            }
        }

        private void AddWaterMonthlyTotalsSummary(ColumnDescriptor column, List<Domain.Waters.WaterMonthlyTotalDto> monthlyTotals)
        {
            column.Item().PaddingTop(5).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(100);
                    columns.ConstantColumn(120);
                });

                table.Header(header =>
                {
                    header.Cell().Background(Colors.Grey.Lighten2).Text("Month").Bold();
                    header.Cell().Background(Colors.Grey.Lighten2).Text("Total Usage").Bold();
                });

                foreach (var total in monthlyTotals.OrderBy(t => t.Year).ThenBy(t => t.Month))
                {
                    table.Cell().Text(total.FormattedMonth);
                    table.Cell().Text($"{total.TotalUsage:N2}");
                }

                table.Cell().Background(Colors.Grey.Lighten3).Text("TOTAL").Bold();
                table.Cell().Background(Colors.Grey.Lighten3).Text($"{monthlyTotals.Sum(t => t.TotalUsage):N2}").Bold();
            });

            var years = monthlyTotals.Select(t => t.Year).Distinct().ToList();
            if (years.Count > 1)
            {
                column.Item().PaddingTop(20).Text("Yearly Summary")
                    .FontSize(12)
                    .Bold();

                column.Item().PaddingTop(5).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(100);
                        columns.ConstantColumn(120);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Background(Colors.Grey.Lighten2).Text("Year").Bold();
                        header.Cell().Background(Colors.Grey.Lighten2).Text("Total Usage").Bold();
                    });

                    foreach (var year in years.OrderBy(y => y))
                    {
                        var yearData = monthlyTotals.Where(t => t.Year == year).ToList();
                        table.Cell().Text(year.ToString());
                        table.Cell().Text($"{yearData.Sum(t => t.TotalUsage):N2}");
                    }
                });
            }
        }

        private void AddPaperMonthlyTotalsSummary(ColumnDescriptor column, List<Domain.Papers.PaperMonthlyTotalDto> monthlyTotals)
        {
            column.Item().PaddingTop(5).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(100);
                    columns.ConstantColumn(120);
                });

                table.Header(header =>
                {
                    header.Cell().Background(Colors.Grey.Lighten2).Text("Month").Bold();
                    header.Cell().Background(Colors.Grey.Lighten2).Text("Total Usage").Bold();
                });

                foreach (var total in monthlyTotals.OrderBy(t => t.Year).ThenBy(t => t.Month))
                {
                    table.Cell().Text(total.FormattedMonth);
                    table.Cell().Text($"{total.TotalUsage:N2}");
                }

                table.Cell().Background(Colors.Grey.Lighten3).Text("TOTAL").Bold();
                table.Cell().Background(Colors.Grey.Lighten3).Text($"{monthlyTotals.Sum(t => t.TotalUsage):N2}").Bold();
            });

            var years = monthlyTotals.Select(t => t.Year).Distinct().ToList();
            if (years.Count > 1)
            {
                column.Item().PaddingTop(20).Text("Yearly Summary")
                    .FontSize(12)
                    .Bold();

                column.Item().PaddingTop(5).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(100);
                        columns.ConstantColumn(120);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Background(Colors.Grey.Lighten2).Text("Year").Bold();
                        header.Cell().Background(Colors.Grey.Lighten2).Text("Total Usage").Bold();
                    });

                    foreach (var year in years.OrderBy(y => y))
                    {
                        var yearData = monthlyTotals.Where(t => t.Year == year).ToList();
                        table.Cell().Text(year.ToString());
                        table.Cell().Text($"{yearData.Sum(t => t.TotalUsage):N2}");
                    }
                });
            }
        }

        private void AddNaturalGasDataSummary(ColumnDescriptor column, List<ConsumptionDataDto> data, string buildingName)
        {
            buildingName = SanitizeBuildingName(buildingName);

            var monthlyData = data
                .GroupBy(d => new { Year = d.Date.Year, Month = d.Date.Month })
                .Select(g => new
                {
                    YearMonth = $"{g.Key.Year}-{g.Key.Month:D2}",
                    TotalSM3Value = g.Sum(d => d.SM3Value ?? 0),
                    TotalUsage = g.Sum(d => d.Usage),
                    Count = g.Count()
                })
                .OrderBy(m => m.YearMonth)
                .ToList();

            string buildingInfo = string.IsNullOrEmpty(buildingName) ? "Overall" : $"for {buildingName}";
            column.Item().PaddingTop(5).Text($"Monthly Natural Gas Consumption Summary {buildingInfo}")
                .FontSize(12)
                .Bold();

            column.Item().PaddingTop(5).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(100);
                    columns.ConstantColumn(100);
                    columns.ConstantColumn(100);
                    columns.RelativeColumn();
                });

                table.Header(header =>
                {
                    header.Cell().Background(Colors.Grey.Lighten2).Text("Month").Bold();
                    header.Cell().Background(Colors.Grey.Lighten2).Text("Total SM3").Bold();
                    header.Cell().Background(Colors.Grey.Lighten2).Text("Total Usage").Bold();
                    header.Cell().Background(Colors.Grey.Lighten2).Text("Records").Bold();
                });

                foreach (var month in monthlyData)
                {
                    table.Cell().Text(month.YearMonth);
                    table.Cell().Text($"{month.TotalSM3Value:N2}");
                    table.Cell().Text($"{month.TotalUsage:N2}");
                    table.Cell().Text(month.Count.ToString());
                }

                table.Cell().Background(Colors.Grey.Lighten3).Text("TOTAL").Bold();
                table.Cell().Background(Colors.Grey.Lighten3).Text($"{monthlyData.Sum(m => m.TotalSM3Value):N2}").Bold();
                table.Cell().Background(Colors.Grey.Lighten3).Text($"{monthlyData.Sum(m => m.TotalUsage):N2}").Bold();
                table.Cell().Background(Colors.Grey.Lighten3).Text(data.Count.ToString()).Bold();
            });

            var years = data.Select(d => d.Date.Year).Distinct().ToList();
            if (years.Count > 1)
            {
                column.Item().PaddingTop(20).Text("Yearly Summary")
                    .FontSize(12)
                    .Bold();

                column.Item().PaddingTop(5).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(100);
                        columns.ConstantColumn(100);
                        columns.ConstantColumn(100);
                        columns.RelativeColumn();
                    });

                    table.Header(header =>
                    {
                        header.Cell().Background(Colors.Grey.Lighten2).Text("Year").Bold();
                        header.Cell().Background(Colors.Grey.Lighten2).Text("Total SM3").Bold();
                        header.Cell().Background(Colors.Grey.Lighten2).Text("Total Usage").Bold();
                        header.Cell().Background(Colors.Grey.Lighten2).Text("Records").Bold();
                    });

                    foreach (var year in years.OrderBy(y => y))
                    {
                        var yearData = data.Where(d => d.Date.Year == year).ToList();
                        table.Cell().Text(year.ToString());
                        table.Cell().Text($"{yearData.Sum(d => d.SM3Value ?? 0):N2}");
                        table.Cell().Text($"{yearData.Sum(d => d.Usage):N2}");
                        table.Cell().Text(yearData.Count.ToString());
                    }
                });
            }
        }

        private void AddGenericConsumptionSummary(ColumnDescriptor column, List<ConsumptionDataDto> data, string consumptionType, string buildingName)
        {
            buildingName = SanitizeBuildingName(buildingName);

            var monthlyData = data
                .GroupBy(d => new { Year = d.Date.Year, Month = d.Date.Month })
                .Select(g => new
                {
                    YearMonth = $"{g.Key.Year}-{g.Key.Month:D2}",
                    TotalUsage = g.Sum(d => d.Usage),
                    Count = g.Count()
                })
                .OrderBy(m => m.YearMonth)
                .ToList();

            string buildingText = !string.IsNullOrEmpty(buildingName)
                ? $" for {buildingName}"
                : "";

            column.Item().PaddingTop(5).Text($"Monthly {consumptionType} Consumption Summary{buildingText}")
                .FontSize(12)
                .Bold();

            column.Item().PaddingTop(5).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(100);
                    columns.ConstantColumn(120);
                    columns.RelativeColumn();
                });

                table.Header(header =>
                {
                    header.Cell().Background(Colors.Grey.Lighten2).Text("Month").Bold();
                    header.Cell().Background(Colors.Grey.Lighten2).Text("Total Usage").Bold();
                    header.Cell().Background(Colors.Grey.Lighten2).Text("Records").Bold();
                });

                foreach (var month in monthlyData)
                {
                    table.Cell().Text(month.YearMonth);
                    table.Cell().Text($"{month.TotalUsage:N2}");
                    table.Cell().Text(month.Count.ToString());
                }

                table.Cell().Background(Colors.Grey.Lighten3).Text("TOTAL").Bold();
                table.Cell().Background(Colors.Grey.Lighten3).Text($"{monthlyData.Sum(m => m.TotalUsage):N2}").Bold();
                table.Cell().Background(Colors.Grey.Lighten3).Text(data.Count.ToString()).Bold();
            });

            var years = data.Select(d => d.Date.Year).Distinct().ToList();
            if (years.Count > 1)
            {
                column.Item().PaddingTop(20).Text("Yearly Summary")
                    .FontSize(12)
                    .Bold();

                column.Item().PaddingTop(5).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(100);
                        columns.ConstantColumn(120);
                        columns.RelativeColumn();
                    });

                    table.Header(header =>
                    {
                        header.Cell().Background(Colors.Grey.Lighten2).Text("Year").Bold();
                        header.Cell().Background(Colors.Grey.Lighten2).Text("Total Usage").Bold();
                        header.Cell().Background(Colors.Grey.Lighten2).Text("Records").Bold();
                    });

                    foreach (var year in years.OrderBy(y => y))
                    {
                        var yearData = data.Where(d => d.Date.Year == year).ToList();
                        table.Cell().Text(year.ToString());
                        table.Cell().Text($"{yearData.Sum(d => d.Usage):N2}");
                        table.Cell().Text(yearData.Count.ToString());
                    }
                });
            }
        }
    }
}
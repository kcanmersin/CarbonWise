using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CarbonWise.BuildingBlocks.Application.Services.Consumption;
using CarbonWise.BuildingBlocks.Application.Services.CarbonFootprints;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace CarbonWise.BuildingBlocks.Application.Services.Reports
{
    public class PdfReportService : IPdfReportService
    {
        private readonly IReportService _reportService;

        public PdfReportService(IReportService reportService)
        {
            _reportService = reportService;
        }

        public async Task<byte[]> GenerateCarbonFootprintPdfReportAsync(DateTime startDate, DateTime endDate)
        {
            var report = await _reportService.GenerateCarbonFootprintReportAsync(startDate, endDate);

            return GeneratePdf(report);
        }

        public async Task<byte[]> GenerateConsumptionPdfReportAsync(string consumptionType, Guid? buildingId, DateTime startDate, DateTime endDate)
        {
            var report = await _reportService.GenerateConsumptionReportAsync(consumptionType, buildingId, startDate, endDate);

            return GeneratePdf(report);
        }

        private byte[] GeneratePdf(ReportDto report)
        {
            return Document.Create(document =>
            {
                document.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(text => text.FontSize(11));

                    // Header
                    page.Header().Element(ComposeHeader);

                    // Content
                    page.Content().Column(column =>
                    {
                        // Title
                        column.Item().PaddingTop(10).Text(report.Title)
                            .FontSize(16)
                            .Bold()
                            .FontColor(Colors.Black);

                        // Metadata
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

                        // Analysis Section
                        column.Item().PaddingTop(20).Text("Analysis")
                            .FontSize(14)
                            .Bold()
                            .FontColor(Colors.Blue.Darken4);

                        column.Item().PaddingTop(5).Text(report.Analysis)
                            .FontSize(11);

                        // Summary Section
                        column.Item().PaddingTop(20).Text("Summary Data")
                            .FontSize(14)
                            .Bold()
                            .FontColor(Colors.Blue.Darken4);

                        // Adding appropriate summary based on report type
                        if (report.ReportType == "CarbonFootprint")
                        {
                            var footprints = report.Data as List<Domain.CarbonFootprints.CarbonFootprint>;
                            if (footprints != null && footprints.Any())
                            {
                                AddCarbonFootprintSummary(column, footprints);
                            }
                            else
                            {
                                column.Item().PaddingTop(5).Text("No carbon footprint data available.");
                            }
                        }
                        else if (report.ReportType == "Consumption")
                        {
                            if (report.ConsumptionType == "Electric")
                            {
                                if (report.Data is List<Domain.Electrics.ElectricMonthlyTotalDto> monthlyTotals)
                                {
                                    AddElectricMonthlyTotalsSummary(column, monthlyTotals);
                                }
                                else if (report.Data is IEnumerable<ConsumptionDataDto> electricData)
                                {
                                    AddElectricDataSummary(column, electricData.ToList(), report.BuildingName);
                                }
                                else
                                {
                                    column.Item().PaddingTop(5).Text("Electric consumption data format not recognized.");
                                }
                            }
                            else if (report.ConsumptionType == "NaturalGas")
                            {
                                if (report.Data is List<Application.Features.NaturalGases.NaturalGasMonthlyTotalDto> monthlyTotals)
                                {
                                    AddNaturalGasMonthlyTotalsSummary(column, monthlyTotals);
                                }
                                else if (report.Data is IEnumerable<ConsumptionDataDto> gasData)
                                {
                                    AddNaturalGasDataSummary(column, gasData.ToList(), report.BuildingName);
                                }
                                else
                                {
                                    column.Item().PaddingTop(5).Text("Natural Gas consumption data format not recognized.");
                                }
                            }
                            else if (report.ConsumptionType == "Water" || report.ConsumptionType == "Paper")
                            {
                                if (report.Data is IEnumerable<ConsumptionDataDto> consumptionData)
                                {
                                    AddGenericConsumptionSummary(column, consumptionData.ToList(), report.ConsumptionType, report.BuildingName);
                                }
                                else
                                {
                                    column.Item().PaddingTop(5).Text($"Summary data for {report.ConsumptionType} is not in the expected format.");
                                }
                            }
                            else
                            {
                                column.Item().PaddingTop(5).Text($"Consumption type '{report.ConsumptionType}' is not supported for summary generation.");
                            }
                        }
                        else
                        {
                            column.Item().PaddingTop(5).Text("Report type not recognized for summary generation.");
                        }
                    });

                    // Footer
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

        private void AddCarbonFootprintSummary(ColumnDescriptor column, List<Domain.CarbonFootprints.CarbonFootprint> footprints)
        {
            var totalEmission = footprints.Sum(cf => cf.TotalEmission);

            // Main table
            column.Item().PaddingTop(5).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(80);   // Year
                    columns.ConstantColumn(100);  // Electricity
                    columns.ConstantColumn(100);  // Shuttle Bus
                    columns.ConstantColumn(80);   // Car
                    columns.ConstantColumn(100);  // Motorcycle
                    columns.ConstantColumn(100);  // Total
                });

                table.Header(header =>
                {
                    header.Cell().Background(Colors.Grey.Lighten2).Text("Year").Bold();
                    header.Cell().Background(Colors.Grey.Lighten2).Text("Electricity").Bold();
                    header.Cell().Background(Colors.Grey.Lighten2).Text("Shuttle Bus").Bold();
                    header.Cell().Background(Colors.Grey.Lighten2).Text("Car").Bold();
                    header.Cell().Background(Colors.Grey.Lighten2).Text("Motorcycle").Bold();
                    header.Cell().Background(Colors.Grey.Lighten2).Text("Total").Bold();
                });

                foreach (var footprint in footprints.OrderBy(f => f.Year))
                {
                    table.Cell().Text(footprint.Year.ToString());
                    table.Cell().Text($"{footprint.ElectricityEmission:N2}");
                    table.Cell().Text($"{footprint.ShuttleBusEmission:N2}");
                    table.Cell().Text($"{footprint.CarEmission:N2}");
                    table.Cell().Text($"{footprint.MotorcycleEmission:N2}");
                    table.Cell().Text($"{footprint.TotalEmission:N2}").Bold();
                }

                table.Cell().Background(Colors.Grey.Lighten3).Text("TOTAL").Bold();
                table.Cell().Background(Colors.Grey.Lighten3).Text($"{footprints.Sum(f => f.ElectricityEmission):N2}").Bold();
                table.Cell().Background(Colors.Grey.Lighten3).Text($"{footprints.Sum(f => f.ShuttleBusEmission):N2}").Bold();
                table.Cell().Background(Colors.Grey.Lighten3).Text($"{footprints.Sum(f => f.CarEmission):N2}").Bold();
                table.Cell().Background(Colors.Grey.Lighten3).Text($"{footprints.Sum(f => f.MotorcycleEmission):N2}").Bold();
                table.Cell().Background(Colors.Grey.Lighten3).Text($"{totalEmission:N2}").Bold();
            });

            // Breakdown
            column.Item().PaddingTop(20).Text("Emission Sources Breakdown")
                .FontSize(12)
                .Bold();

            // Percentages
            column.Item().PaddingTop(5).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(120);  // Source
                    columns.ConstantColumn(100);  // Amount
                    columns.ConstantColumn(100);  // Percentage
                });

                table.Header(header =>
                {
                    header.Cell().Background(Colors.Grey.Lighten2).Text("Source").Bold();
                    header.Cell().Background(Colors.Grey.Lighten2).Text("Amount").Bold();
                    header.Cell().Background(Colors.Grey.Lighten2).Text("Percentage").Bold();
                });

                // Calculate totals
                var electricityTotal = footprints.Sum(f => f.ElectricityEmission);
                var electricityPercentage = totalEmission > 0 ? (electricityTotal / totalEmission * 100) : 0;

                var shuttleBusTotal = footprints.Sum(f => f.ShuttleBusEmission);
                var shuttleBusPercentage = totalEmission > 0 ? (shuttleBusTotal / totalEmission * 100) : 0;

                var carTotal = footprints.Sum(f => f.CarEmission);
                var carPercentage = totalEmission > 0 ? (carTotal / totalEmission * 100) : 0;

                var motorcycleTotal = footprints.Sum(f => f.MotorcycleEmission);
                var motorcyclePercentage = totalEmission > 0 ? (motorcycleTotal / totalEmission * 100) : 0;

                // Add data rows
                table.Cell().Text("Electricity");
                table.Cell().Text($"{electricityTotal:N2}");
                table.Cell().Text($"{electricityPercentage:N2}%");

                table.Cell().Text("Shuttle Bus");
                table.Cell().Text($"{shuttleBusTotal:N2}");
                table.Cell().Text($"{shuttleBusPercentage:N2}%");

                table.Cell().Text("Car");
                table.Cell().Text($"{carTotal:N2}");
                table.Cell().Text($"{carPercentage:N2}%");

                table.Cell().Text("Motorcycle");
                table.Cell().Text($"{motorcycleTotal:N2}");
                table.Cell().Text($"{motorcyclePercentage:N2}%");

                table.Cell().Background(Colors.Grey.Lighten3).Text("TOTAL").Bold();
                table.Cell().Background(Colors.Grey.Lighten3).Text($"{totalEmission:N2}").Bold();
                table.Cell().Background(Colors.Grey.Lighten3).Text("100.00%").Bold();
            });
        }

        private void AddElectricMonthlyTotalsSummary(ColumnDescriptor column, List<Domain.Electrics.ElectricMonthlyTotalDto> monthlyTotals)
        {
            column.Item().PaddingTop(5).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(100);  // Month
                    columns.ConstantColumn(120);  // KWH
                    columns.ConstantColumn(120);  // Usage
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

            // Yearly summary if multiple years
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
                        columns.ConstantColumn(100);  // Year
                        columns.ConstantColumn(120);  // KWH
                        columns.ConstantColumn(120);  // Usage
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
                    columns.ConstantColumn(100);  // Month
                    columns.ConstantColumn(120);  // SM3
                    columns.ConstantColumn(120);  // Usage
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

            // Yearly summary if multiple years
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
                        columns.ConstantColumn(100);  // Year
                        columns.ConstantColumn(120);  // SM3
                        columns.ConstantColumn(120);  // Usage
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
                    columns.ConstantColumn(100);  // Month
                    columns.ConstantColumn(100);  // KWH
                    columns.ConstantColumn(100);  // Usage
                    columns.RelativeColumn();     // Records
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
        }

        private void AddNaturalGasDataSummary(ColumnDescriptor column, List<ConsumptionDataDto> data, string buildingName)
        {
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
                    columns.ConstantColumn(100);  // Month
                    columns.ConstantColumn(100);  // SM3
                    columns.ConstantColumn(100);  // Usage
                    columns.RelativeColumn();     // Records
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
        }

        private void AddGenericConsumptionSummary(ColumnDescriptor column, List<ConsumptionDataDto> data, string consumptionType, string buildingName)
        {
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
                    columns.ConstantColumn(100);  // Month
                    columns.ConstantColumn(120);  // Usage
                    columns.RelativeColumn();     // Records
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
        }
    }
}
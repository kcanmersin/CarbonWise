using CarbonWise.BuildingBlocks.Application.Services.Consumption;
using CarbonWise.BuildingBlocks.Domain.Electrics;
using CarbonWise.BuildingBlocks.Domain.NaturalGases;
using CarbonWise.BuildingBlocks.Domain.Papers;
using CarbonWise.BuildingBlocks.Domain.Waters;
using OfficeOpenXml;
using OfficeOpenXml.Drawing.Chart;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace CarbonWise.BuildingBlocks.Infrastructure.Services.Consumption
{
    public class ConsumptionDataService : IConsumptionDataService
    {
        private readonly IElectricRepository _electricRepository;
        private readonly INaturalGasRepository _naturalGasRepository;
        private readonly IPaperRepository _paperRepository;
        private readonly IWaterRepository _waterRepository;

        static ConsumptionDataService()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        public ConsumptionDataService(
            IElectricRepository electricRepository,
            INaturalGasRepository naturalGasRepository,
            IPaperRepository paperRepository,
            IWaterRepository waterRepository)
        {
            _electricRepository = electricRepository;
            _naturalGasRepository = naturalGasRepository;
            _paperRepository = paperRepository;
            _waterRepository = waterRepository;
        }

        public async Task<IEnumerable<ConsumptionDataDto>> GetConsumptionDataAsync(
            string consumptionType,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            switch (consumptionType.ToLower())
            {
                case "water":
                    return await GetWaterConsumptionDataAsync(startDate, endDate);
                case "electric":
                    return await GetElectricConsumptionDataAsync(startDate, endDate);
                case "naturalgas":
                    return await GetNaturalGasConsumptionDataAsync(startDate, endDate);
                case "paper":
                    return await GetPaperConsumptionDataAsync(startDate, endDate);
                default:
                    throw new ArgumentException($"Unsupported consumption type: {consumptionType}");
            }
        }

        public async Task<byte[]> GenerateConsumptionExcelAsync(
            string consumptionType,
            DateTime? startDate = null,
            DateTime? endDate = null,
            bool includeGraphs = false,
            Guid? buildingId = null)
        {
            IEnumerable<ConsumptionDataDto> data;
            
            if (buildingId.HasValue)
            {
                // Get data for specific building
                data = await GetConsumptionDataForBuildingAsync(consumptionType, buildingId.Value, startDate, endDate);
            }
            else
            {
                // Get aggregated data (existing logic)
                data = await GetConsumptionDataAsync(consumptionType, startDate, endDate);
            }

            if (data == null || !data.Any())
            {
                throw new ApplicationException("No consumption data found.");
            }

            return GenerateExcel(data.ToList(), consumptionType, includeGraphs, buildingId);
        }

        private async Task<IEnumerable<ConsumptionDataDto>> GetConsumptionDataForBuildingAsync(
            string consumptionType, 
            Guid buildingId, 
            DateTime? startDate = null, 
            DateTime? endDate = null)
        {
            var buildingIdDomain = new CarbonWise.BuildingBlocks.Domain.Buildings.BuildingId(buildingId);
            
            switch (consumptionType.ToLower())
            {
                case "water":
                    return await GetWaterConsumptionDataForBuildingAsync(buildingIdDomain, startDate, endDate);
                case "electric":
                    return await GetElectricConsumptionDataForBuildingAsync(buildingIdDomain, startDate, endDate);
                case "naturalgas":
                    return await GetNaturalGasConsumptionDataForBuildingAsync(buildingIdDomain, startDate, endDate);
                case "paper":
                    return await GetPaperConsumptionDataForBuildingAsync(buildingIdDomain, startDate, endDate);
                default:
                    throw new ArgumentException($"Unsupported consumption type: {consumptionType}");
            }
        }

        private async Task<IEnumerable<ConsumptionDataDto>> GetWaterConsumptionDataForBuildingAsync(
            CarbonWise.BuildingBlocks.Domain.Buildings.BuildingId buildingId, 
            DateTime? startDate, 
            DateTime? endDate)
        {
            List<Water> waters;

            if (startDate.HasValue && endDate.HasValue)
            {
                waters = await _waterRepository.GetByBuildingIdAndDateRangeAsync(buildingId, startDate.Value, endDate.Value);
            }
            else
            {
                waters = await _waterRepository.GetByBuildingIdAsync(buildingId);
            }

            return waters.Select(w => new ConsumptionDataDto
            {
                Id = w.Id.Value,
                Date = w.Date,
                InitialMeterValue = w.InitialMeterValue,
                FinalMeterValue = w.FinalMeterValue,
                Usage = w.Usage,
                BuildingId = w.BuildingId.Value,
                BuildingName = w.Building?.Name
            });
        }

        private async Task<IEnumerable<ConsumptionDataDto>> GetElectricConsumptionDataForBuildingAsync(
            CarbonWise.BuildingBlocks.Domain.Buildings.BuildingId buildingId, 
            DateTime? startDate, 
            DateTime? endDate)
        {
            List<Electric> electrics;

            if (startDate.HasValue && endDate.HasValue)
            {
                electrics = await _electricRepository.GetByBuildingIdAndDateRangeAsync(buildingId, startDate.Value, endDate.Value);
            }
            else
            {
                electrics = await _electricRepository.GetByBuildingIdAsync(buildingId);
            }

            return electrics.Select(e => new ConsumptionDataDto
            {
                Id = e.Id.Value,
                Date = e.Date,
                InitialMeterValue = e.InitialMeterValue,
                FinalMeterValue = e.FinalMeterValue,
                Usage = e.Usage,
                KWHValue = e.KWHValue,
                BuildingId = e.BuildingId.Value,
                BuildingName = e.Building?.Name
            });
        }

        private async Task<IEnumerable<ConsumptionDataDto>> GetNaturalGasConsumptionDataForBuildingAsync(
            CarbonWise.BuildingBlocks.Domain.Buildings.BuildingId buildingId, 
            DateTime? startDate, 
            DateTime? endDate)
        {
            List<NaturalGas> naturalGases;

            if (startDate.HasValue && endDate.HasValue)
            {
                naturalGases = await _naturalGasRepository.GetByBuildingIdAndDateRangeAsync(buildingId, startDate.Value, endDate.Value);
            }
            else
            {
                naturalGases = await _naturalGasRepository.GetByBuildingIdAsync(buildingId);
            }

            return naturalGases.Select(n => new ConsumptionDataDto
            {
                Id = n.Id.Value,
                Date = n.Date,
                InitialMeterValue = n.InitialMeterValue,
                FinalMeterValue = n.FinalMeterValue,
                Usage = n.Usage,
                SM3Value = n.SM3Value,
                BuildingId = n.BuildingId.Value,
                BuildingName = n.Building?.Name
            });
        }

        private async Task<IEnumerable<ConsumptionDataDto>> GetPaperConsumptionDataForBuildingAsync(
            CarbonWise.BuildingBlocks.Domain.Buildings.BuildingId buildingId, 
            DateTime? startDate, 
            DateTime? endDate)
        {
            List<Paper> papers;

            if (startDate.HasValue && endDate.HasValue)
            {
                papers = await _paperRepository.GetByBuildingIdAndDateRangeAsync(buildingId, startDate.Value, endDate.Value);
            }
            else
            {
                papers = await _paperRepository.GetByBuildingIdAsync(buildingId);
            }

            return papers.Select(p => new ConsumptionDataDto
            {
                Id = p.Id.Value,
                Date = p.Date,
                Usage = p.Usage,
                InitialMeterValue = 0,
                FinalMeterValue = 0,
                BuildingId = p.BuildingId.Value,
                BuildingName = p.Building?.Name
            });
        }

        private async Task<IEnumerable<ConsumptionDataDto>> GetWaterConsumptionDataAsync(DateTime? startDate, DateTime? endDate)
        {
            List<Water> waters;

            if (startDate.HasValue && endDate.HasValue)
            {
                waters = await _waterRepository.GetByDateRangeAsync(startDate.Value, endDate.Value);
            }
            else
            {
                waters = await _waterRepository.GetAllAsync();
            }

            return waters.Select(w => new ConsumptionDataDto
            {
                Id = w.Id.Value,
                Date = w.Date,
                InitialMeterValue = w.InitialMeterValue,
                FinalMeterValue = w.FinalMeterValue,
                Usage = w.Usage,
                BuildingId = w.BuildingId.Value,
                BuildingName = w.Building?.Name
            });
        }

        private async Task<IEnumerable<ConsumptionDataDto>> GetElectricConsumptionDataAsync(DateTime? startDate, DateTime? endDate)
        {
            List<Electric> electrics;

            if (startDate.HasValue && endDate.HasValue)
            {
                electrics = await _electricRepository.GetByDateRangeAsync(startDate.Value, endDate.Value);
            }
            else
            {
                electrics = await _electricRepository.GetByDateRangeAsync(DateTime.MinValue, DateTime.MaxValue);
            }

            return electrics.Select(e => new ConsumptionDataDto
            {
                Id = e.Id.Value,
                Date = e.Date,
                InitialMeterValue = e.InitialMeterValue,
                FinalMeterValue = e.FinalMeterValue,
                Usage = e.Usage,
                KWHValue = e.KWHValue,
                BuildingId = e.BuildingId.Value,
                BuildingName = e.Building?.Name
            });
        }

        private async Task<IEnumerable<ConsumptionDataDto>> GetNaturalGasConsumptionDataAsync(DateTime? startDate, DateTime? endDate)
        {
            List<NaturalGas> naturalGases;

            if (startDate.HasValue && endDate.HasValue)
            {
                naturalGases = await _naturalGasRepository.GetByDateRangeAsync(startDate.Value, endDate.Value);
            }
            else
            {
                naturalGases = await _naturalGasRepository.GetByDateRangeAsync(DateTime.MinValue, DateTime.MaxValue);
            }

            return naturalGases.Select(n => new ConsumptionDataDto
            {
                Id = n.Id.Value,
                Date = n.Date,
                InitialMeterValue = n.InitialMeterValue,
                FinalMeterValue = n.FinalMeterValue,
                Usage = n.Usage,
                SM3Value = n.SM3Value,
                BuildingId = n.BuildingId.Value,
                BuildingName = n.Building?.Name
            });
        }

        private async Task<IEnumerable<ConsumptionDataDto>> GetPaperConsumptionDataAsync(DateTime? startDate, DateTime? endDate)
        {
            List<Paper> papers;

            if (startDate.HasValue && endDate.HasValue)
            {
                papers = await _paperRepository.GetByDateRangeAsync(startDate.Value, endDate.Value);
            }
            else
            {
                papers = await _paperRepository.GetAllAsync();
            }

            return papers.Select(p => new ConsumptionDataDto
            {
                Id = p.Id.Value,
                Date = p.Date,
                Usage = p.Usage,
                InitialMeterValue = 0,
                FinalMeterValue = 0,
                BuildingId = p.BuildingId.Value,
                BuildingName = p.Building?.Name
            });
        }

        private byte[] GenerateExcel(List<ConsumptionDataDto> data, string consumptionType, bool includeGraphs, Guid? buildingId = null)
        {
            using (var package = new ExcelPackage())
            {
                // If buildingId is specified, generate simple sheet for that building only
                if (buildingId.HasValue)
                {
                    return GenerateBuildingSpecificExcel(package, data, consumptionType, includeGraphs);
                }
                if (consumptionType.Equals("Water", StringComparison.OrdinalIgnoreCase))
                {
                    // Water için building-based gruplandırma
                    var allSheet = package.Workbook.Worksheets.Add("All");

                    allSheet.Cells[1, 1].Value = "ID";
                    allSheet.Cells[1, 2].Value = "Date";
                    allSheet.Cells[1, 3].Value = "Initial Meter Value";
                    allSheet.Cells[1, 4].Value = "Final Meter Value";
                    allSheet.Cells[1, 5].Value = "Usage";
                    allSheet.Cells[1, 6].Value = "Building";

                    var sortedData = data.OrderBy(d => d.Date).ToList();

                    int row = 2;
                    foreach (var item in sortedData)
                    {
                        allSheet.Cells[row, 1].Value = item.Id;
                        allSheet.Cells[row, 2].Value = item.Date.ToString("yyyy-MM-dd");
                        allSheet.Cells[row, 3].Value = item.InitialMeterValue;
                        allSheet.Cells[row, 4].Value = item.FinalMeterValue;
                        allSheet.Cells[row, 5].Value = item.Usage;
                        allSheet.Cells[row, 6].Value = item.BuildingName;
                        row++;
                    }

                    if (includeGraphs && row > 2)
                    {
                        string xRange = $"B2:B{row - 1}";
                        string yRange = $"E2:E{row - 1}";
                        AddChart(allSheet, xRange, yRange, "Usage Over Time");
                    }

                    // Building bazında ayrı sheet'ler oluştur
                    var buildingGroups = data.GroupBy(d => d.BuildingName);

                    foreach (var group in buildingGroups.Where(g => !string.IsNullOrEmpty(g.Key)))
                    {
                        var sanitizedSheetName = CleanSheetName(group.Key);
                        var sheet = package.Workbook.Worksheets.Add(sanitizedSheetName);

                        sheet.Cells[1, 1].Value = "ID";
                        sheet.Cells[1, 2].Value = "Date";
                        sheet.Cells[1, 3].Value = "Initial Meter Value";
                        sheet.Cells[1, 4].Value = "Final Meter Value";
                        sheet.Cells[1, 5].Value = "Usage";

                        var sortedGroupData = group.OrderBy(d => d.Date).ToList();

                        int rowBuilding = 2;
                        foreach (var item in sortedGroupData)
                        {
                            sheet.Cells[rowBuilding, 1].Value = item.Id;
                            sheet.Cells[rowBuilding, 2].Value = item.Date.ToString("yyyy-MM-dd");
                            sheet.Cells[rowBuilding, 3].Value = item.InitialMeterValue;
                            sheet.Cells[rowBuilding, 4].Value = item.FinalMeterValue;
                            sheet.Cells[rowBuilding, 5].Value = item.Usage;
                            rowBuilding++;
                        }

                        if (includeGraphs && rowBuilding > 2)
                        {
                            string xRange = $"B2:B{rowBuilding - 1}";
                            string yRange = $"E2:E{rowBuilding - 1}";
                            string chartTitle = $"Usage Over Time - {group.Key}";
                            AddChart(sheet, xRange, yRange, chartTitle);
                        }
                    }
                }
                else if (consumptionType.Equals("Paper", StringComparison.OrdinalIgnoreCase))
                {
                    // Paper için hem genel özet hem de building bazında ayrım
                    var yearlySheet = package.Workbook.Worksheets.Add("Yearly Summary");

                    yearlySheet.Cells[1, 1].Value = "Year";
                    yearlySheet.Cells[1, 2].Value = "Total Usage";

                    var groupedData = data
                        .GroupBy(d => d.Date.Year)
                        .Select(g => new
                        {
                            Year = g.Key,
                            TotalUsage = g.Sum(d => d.Usage)
                        })
                        .OrderBy(g => g.Year)
                        .ToList();

                    int row = 2;
                    foreach (var item in groupedData)
                    {
                        yearlySheet.Cells[row, 1].Value = item.Year;
                        yearlySheet.Cells[row, 2].Value = item.TotalUsage;
                        row++;
                    }

                    if (includeGraphs && row > 2)
                    {
                        string xRange = $"A2:A{row - 1}";
                        string yRange = $"B2:B{row - 1}";
                        AddChart(yearlySheet, xRange, yRange, "Total Usage Per Year", isYear: true);
                    }

                    // Building bazında sheet'ler oluştur
                    var buildingGroups = data.GroupBy(d => d.BuildingName);

                    foreach (var group in buildingGroups.Where(g => !string.IsNullOrEmpty(g.Key)))
                    {
                        var sanitizedSheetName = CleanSheetName(group.Key);
                        var sheet = package.Workbook.Worksheets.Add(sanitizedSheetName);

                        sheet.Cells[1, 1].Value = "ID";
                        sheet.Cells[1, 2].Value = "Date";
                        sheet.Cells[1, 3].Value = "Usage";

                        var sortedGroupData = group.OrderBy(d => d.Date).ToList();

                        int rowBuilding = 2;
                        foreach (var item in sortedGroupData)
                        {
                            sheet.Cells[rowBuilding, 1].Value = item.Id;
                            sheet.Cells[rowBuilding, 2].Value = item.Date.ToString("yyyy-MM-dd");
                            sheet.Cells[rowBuilding, 3].Value = item.Usage;
                            rowBuilding++;
                        }

                        if (includeGraphs && rowBuilding > 2)
                        {
                            string xRange = $"B2:B{rowBuilding - 1}";
                            string yRange = $"C2:C{rowBuilding - 1}";
                            string chartTitle = $"Usage Over Time - {group.Key}";
                            AddChart(sheet, xRange, yRange, chartTitle);
                        }
                    }
                }
                else
                {
                    var allSheetStandard = package.Workbook.Worksheets.Add("All");
                    var groupedDataStandard = data
                        .GroupBy(d => d.Date.ToString("yyyy-MM"))
                        .Select(g => new
                        {
                            Period = g.Key,
                            TotalUsage = g.Sum(d => d.Usage),
                            TotalKWH = consumptionType.Equals("Electric", StringComparison.OrdinalIgnoreCase) ? g.Sum(d => d.KWHValue ?? 0) : 0,
                            TotalSM3 = consumptionType.Equals("NaturalGas", StringComparison.OrdinalIgnoreCase) ? g.Sum(d => d.SM3Value ?? 0) : 0
                        })
                        .OrderBy(g => g.Period)
                        .ToList();

                    allSheetStandard.Cells[1, 1].Value = "Period";
                    allSheetStandard.Cells[1, 2].Value = "Total Usage";
                    if (consumptionType.Equals("Electric", StringComparison.OrdinalIgnoreCase))
                        allSheetStandard.Cells[1, 3].Value = "Total KWH";
                    if (consumptionType.Equals("NaturalGas", StringComparison.OrdinalIgnoreCase))
                        allSheetStandard.Cells[1, 3].Value = "Total SM3";

                    int rowStandard = 2;
                    foreach (var item in groupedDataStandard)
                    {
                        allSheetStandard.Cells[rowStandard, 1].Value = item.Period;
                        allSheetStandard.Cells[rowStandard, 2].Value = item.TotalUsage;
                        if (consumptionType.Equals("Electric", StringComparison.OrdinalIgnoreCase))
                            allSheetStandard.Cells[rowStandard, 3].Value = item.TotalKWH;
                        if (consumptionType.Equals("NaturalGas", StringComparison.OrdinalIgnoreCase))
                            allSheetStandard.Cells[rowStandard, 3].Value = item.TotalSM3;
                        rowStandard++;
                    }

                    if (includeGraphs && rowStandard > 2)
                    {
                        string xRange = $"A2:A{rowStandard - 1}";
                        string yRange = $"B2:B{rowStandard - 1}";
                        AddChart(allSheetStandard, xRange, yRange, "Total Usage Over Period");
                    }

                    // Building bazında sheet'ler oluştur
                    var buildingGroups = data.GroupBy(d => d.BuildingName);

                    foreach (var group in buildingGroups.Where(g => !string.IsNullOrEmpty(g.Key)))
                    {
                        var sanitizedSheetName = CleanSheetName(group.Key);
                        var sheet = package.Workbook.Worksheets.Add(sanitizedSheetName);

                        sheet.Cells[1, 1].Value = "ID";
                        sheet.Cells[1, 2].Value = "Date";
                        sheet.Cells[1, 3].Value = "Initial Meter Value";
                        sheet.Cells[1, 4].Value = "Final Meter Value";
                        sheet.Cells[1, 5].Value = "Usage";
                        if (consumptionType.Equals("Electric", StringComparison.OrdinalIgnoreCase))
                            sheet.Cells[1, 6].Value = "KWH Value";
                        if (consumptionType.Equals("NaturalGas", StringComparison.OrdinalIgnoreCase))
                            sheet.Cells[1, 6].Value = "SM3 Value";

                        var sortedGroupData = group.OrderBy(d => d.Date).ToList();

                        int rowBuilding = 2;
                        foreach (var item in sortedGroupData)
                        {
                            sheet.Cells[rowBuilding, 1].Value = item.Id;
                            sheet.Cells[rowBuilding, 2].Value = item.Date.ToString("yyyy-MM-dd");
                            sheet.Cells[rowBuilding, 3].Value = item.InitialMeterValue;
                            sheet.Cells[rowBuilding, 4].Value = item.FinalMeterValue;
                            sheet.Cells[rowBuilding, 5].Value = item.Usage;
                            if (consumptionType.Equals("Electric", StringComparison.OrdinalIgnoreCase))
                                sheet.Cells[rowBuilding, 6].Value = item.KWHValue;
                            if (consumptionType.Equals("NaturalGas", StringComparison.OrdinalIgnoreCase))
                                sheet.Cells[rowBuilding, 6].Value = item.SM3Value;
                            rowBuilding++;
                        }

                        if (includeGraphs && rowBuilding > 2)
                        {
                            string xRange = $"B2:B{rowBuilding - 1}";
                            string yRange = $"E2:E{rowBuilding - 1}";
                            string chartTitle = $"Usage Over Time - {group.Key}";
                            AddChart(sheet, xRange, yRange, chartTitle);
                        }
                    }
                }

                return package.GetAsByteArray();
            }
        }

        private void AddChart(ExcelWorksheet sheet, string xRange, string yRange, string chartTitle, bool isYear = false)
        {
            var cells = sheet.Cells[yRange];
            int totalRows = cells.End.Row;
            int totalCols = cells.End.Column;

            var chart = sheet.Drawings.AddChart("UsageChart_" + chartTitle.Replace(" ", ""), eChartType.Line) as ExcelLineChart;
            chart.Title.Text = chartTitle;
            chart.SetPosition(totalRows + 2, 0, 0, 0);
            chart.SetSize(800, 400);

            chart.Series.Add(sheet.Cells[yRange], sheet.Cells[xRange]);
            chart.XAxis.Title.Text = isYear ? "Year" : "Date";
            chart.YAxis.Title.Text = "Usage";

            if (isYear)
            {
                chart.XAxis.Format = "0";
            }
            else
            {
                var xAxis = chart.XAxis as ExcelChartAxisStandard;
                xAxis.Format = "yyyy-MM-dd";
                xAxis.MajorTickMark = eAxisTickMark.None;
                xAxis.MinorTickMark = eAxisTickMark.None;
            }

            chart.Legend.Position = eLegendPosition.Bottom;
        }

        private string CleanSheetName(string sheetName)
        {
            if (string.IsNullOrWhiteSpace(sheetName))
                return "Unnamed";

            var invalidChars = new[] { '\\', '/', '*', '[', ']', ':', '?' };
            foreach (var c in invalidChars)
            {
                sheetName = sheetName.Replace(c, '_');
            }

            sheetName = sheetName.Trim('\'');

            return sheetName.Length > 31 ? sheetName.Substring(0, 31) : sheetName;
        }

        private byte[] GenerateBuildingSpecificExcel(ExcelPackage package, List<ConsumptionDataDto> data, string consumptionType, bool includeGraphs)
        {
            var buildingName = data.FirstOrDefault()?.BuildingName ?? "Unknown Building";
            var sheet = package.Workbook.Worksheets.Add(CleanSheetName(buildingName));

            // Add headers based on consumption type
            if (consumptionType.Equals("Paper", StringComparison.OrdinalIgnoreCase))
            {
                sheet.Cells[1, 1].Value = "ID";
                sheet.Cells[1, 2].Value = "Date";
                sheet.Cells[1, 3].Value = "Usage";

                var sortedData = data.OrderBy(d => d.Date).ToList();
                int row = 2;
                foreach (var item in sortedData)
                {
                    sheet.Cells[row, 1].Value = item.Id;
                    sheet.Cells[row, 2].Value = item.Date.ToString("yyyy-MM-dd");
                    sheet.Cells[row, 3].Value = item.Usage;
                    row++;
                }

                if (includeGraphs && row > 2)
                {
                    string xRange = $"B2:B{row - 1}";
                    string yRange = $"C2:C{row - 1}";
                    AddChart(sheet, xRange, yRange, $"Usage Over Time - {buildingName}");
                }
            }
            else
            {
                sheet.Cells[1, 1].Value = "ID";
                sheet.Cells[1, 2].Value = "Date";
                sheet.Cells[1, 3].Value = "Initial Meter Value";
                sheet.Cells[1, 4].Value = "Final Meter Value";
                sheet.Cells[1, 5].Value = "Usage";
                
                if (consumptionType.Equals("Electric", StringComparison.OrdinalIgnoreCase))
                    sheet.Cells[1, 6].Value = "KWH Value";
                if (consumptionType.Equals("NaturalGas", StringComparison.OrdinalIgnoreCase))
                    sheet.Cells[1, 6].Value = "SM3 Value";

                var sortedData = data.OrderBy(d => d.Date).ToList();
                int row = 2;
                foreach (var item in sortedData)
                {
                    sheet.Cells[row, 1].Value = item.Id;
                    sheet.Cells[row, 2].Value = item.Date.ToString("yyyy-MM-dd");
                    sheet.Cells[row, 3].Value = item.InitialMeterValue;
                    sheet.Cells[row, 4].Value = item.FinalMeterValue;
                    sheet.Cells[row, 5].Value = item.Usage;
                    
                    if (consumptionType.Equals("Electric", StringComparison.OrdinalIgnoreCase))
                        sheet.Cells[row, 6].Value = item.KWHValue;
                    if (consumptionType.Equals("NaturalGas", StringComparison.OrdinalIgnoreCase))
                        sheet.Cells[row, 6].Value = item.SM3Value;
                    row++;
                }

                if (includeGraphs && row > 2)
                {
                    string xRange = $"B2:B{row - 1}";
                    string yRange = $"E2:E{row - 1}";
                    AddChart(sheet, xRange, yRange, $"Usage Over Time - {buildingName}");
                }
            }

            return package.GetAsByteArray();
        }
    }
}
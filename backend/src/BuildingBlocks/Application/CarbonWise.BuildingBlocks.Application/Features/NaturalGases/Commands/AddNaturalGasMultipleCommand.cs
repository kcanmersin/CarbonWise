using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CarbonWise.BuildingBlocks.Domain.Buildings;
using CarbonWise.BuildingBlocks.Domain.NaturalGases;
using CarbonWise.BuildingBlocks.Infrastructure;
using DocumentFormat.OpenXml.Packaging;
using MediatR;
using Microsoft.AspNetCore.Http;
using OpenXmlRow = DocumentFormat.OpenXml.Spreadsheet.Row;
using OpenXmlCell = DocumentFormat.OpenXml.Spreadsheet.Cell;
using OpenXmlSheetData = DocumentFormat.OpenXml.Spreadsheet.SheetData;
using OpenXmlSheet = DocumentFormat.OpenXml.Spreadsheet.Sheet;
using OpenXmlCellValues = DocumentFormat.OpenXml.Spreadsheet.CellValues;

namespace CarbonWise.BuildingBlocks.Application.Features.NaturalGases.Commands.AddNaturalGasMultiple
{
    public class AddNaturalGasMultipleCommand : IRequest<AddNaturalGasMultipleResponse>
    {
        public IFormFile File { get; set; }
    }

    public class AddNaturalGasMultipleCommandHandler : IRequestHandler<AddNaturalGasMultipleCommand, AddNaturalGasMultipleResponse>
    {
        private readonly INaturalGasRepository _naturalGasRepository;
        private readonly IBuildingRepository _buildingRepository;
        private readonly IUnitOfWork _unitOfWork;

        public AddNaturalGasMultipleCommandHandler(
            INaturalGasRepository naturalGasRepository,
            IBuildingRepository buildingRepository,
            IUnitOfWork unitOfWork)
        {
            _naturalGasRepository = naturalGasRepository;
            _buildingRepository = buildingRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<AddNaturalGasMultipleResponse> Handle(AddNaturalGasMultipleCommand request, CancellationToken cancellationToken)
        {
            var file = request.File;
            if (file == null || file.Length == 0)
                throw new ArgumentException("No file uploaded");

            if (!file.FileName.EndsWith(".xlsx") && !file.FileName.EndsWith(".xls"))
                throw new ArgumentException("Please upload an Excel file");

            var response = new AddNaturalGasMultipleResponse();

            try
            {
                using var stream = file.OpenReadStream();
                using var document = SpreadsheetDocument.Open(stream, false);

                var workbookPart = document.WorkbookPart;

                foreach (OpenXmlSheet sheet in workbookPart.Workbook.Sheets)
                {
                    var sheetName = sheet.Name?.Value;
                    if (string.IsNullOrEmpty(sheetName))
                        continue;

                    var building = await _buildingRepository.GetByNameAsync(sheetName);
                    if (building == null)
                    {
                        response.Errors.Add($"Building '{sheetName}' not found");
                        continue;
                    }

                    var worksheetPart = (WorksheetPart)document.WorkbookPart.GetPartById(sheet.Id);
                    var sheetData = worksheetPart.Worksheet.Elements<OpenXmlSheetData>().FirstOrDefault();

                    if (sheetData == null)
                        continue;

                    int rowIndex = 0;
                    foreach (OpenXmlRow row in sheetData.Elements<OpenXmlRow>())
                    {
                        rowIndex++;
                        if (rowIndex == 1) continue;

                        try
                        {
                            var cells = row.Elements<OpenXmlCell>().ToList();

                            if (cells.Count < 4)
                            {
                                bool isEmpty = true;
                                foreach (var cell in cells)
                                {
                                    var value = GetCellValue(document, cell);
                                    if (!string.IsNullOrWhiteSpace(value))
                                    {
                                        isEmpty = false;
                                        break;
                                    }
                                }

                                if (isEmpty)
                                    continue;

                                response.Errors.Add($"Sheet '{sheetName}', Row {rowIndex}: Not enough columns");
                                continue;
                            }

                            var dateValue = GetCellValue(document, cells[0]);
                            var initialValue = GetCellValue(document, cells[1]);
                            var finalValue = GetCellValue(document, cells[2]);
                            var sm3Value = GetCellValue(document, cells[3]);

                            if (string.IsNullOrWhiteSpace(dateValue) &&
                                string.IsNullOrWhiteSpace(initialValue) &&
                                string.IsNullOrWhiteSpace(finalValue) &&
                                string.IsNullOrWhiteSpace(sm3Value))
                            {
                                continue;
                            }

                            if (string.IsNullOrEmpty(dateValue) ||
                                string.IsNullOrEmpty(initialValue) ||
                                string.IsNullOrEmpty(finalValue) ||
                                string.IsNullOrEmpty(sm3Value))
                            {
                                response.Errors.Add($"Sheet '{sheetName}', Row {rowIndex}: Missing required values");
                                continue;
                            }

                            var date = ParseDate(dateValue);
                            
                            var existsForMonth = await _naturalGasRepository.ExistsForMonthAsync(building.Id, date.Year, date.Month);
                            if (existsForMonth)
                            {
                                response.Errors.Add($"Sheet '{sheetName}', Row {rowIndex}: Bu bina için {date:yyyy/MM} tarihinde doğalgaz verisi zaten mevcut. Aynı ay için birden fazla veri girilemez.");
                                continue;
                            }

                            var naturalGas = NaturalGas.Create(
                                date,
                                decimal.Parse(initialValue),
                                decimal.Parse(finalValue),
                                decimal.Parse(sm3Value),
                                building.Id);

                            await _naturalGasRepository.AddAsync(naturalGas);
                            response.SuccessCount++;
                            response.Results.Add(new NaturalGasDto
                            {
                                Id = naturalGas.Id.Value,
                                Date = naturalGas.Date,
                                InitialMeterValue = naturalGas.InitialMeterValue,
                                FinalMeterValue = naturalGas.FinalMeterValue,
                                Usage = naturalGas.Usage,
                                SM3Value = naturalGas.SM3Value,
                                BuildingId = naturalGas.BuildingId.Value,
                                BuildingName = building.Name
                            });
                        }
                        catch (Exception ex)
                        {
                            response.Errors.Add($"Sheet '{sheetName}', Row {rowIndex}: {ex.Message}");
                        }
                    }
                }

                await _unitOfWork.CommitAsync(cancellationToken);
                return response;
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Error processing Excel file", ex);
            }
        }

        private string GetCellValue(SpreadsheetDocument document, OpenXmlCell cell)
        {
            if (cell == null) return null;

            var value = cell.InnerText;
            if (cell.DataType != null && cell.DataType.Value == OpenXmlCellValues.SharedString)
            {
                var sharedStringTablePart = document.WorkbookPart.SharedStringTablePart;
                return sharedStringTablePart.SharedStringTable.ChildElements[int.Parse(value)].InnerText;
            }
            return value;
        }

        private DateTime ParseDate(string dateValue)
        {
            if (double.TryParse(dateValue, out double doubleValue))
            {
                try
                {
                    return DateTime.FromOADate(doubleValue);
                }
                catch { }
            }

            string[] formats = {
                "MM/dd/yyyy",
                "M/d/yyyy",
                "yyyy-MM-dd",
                "dd/MM/yyyy",
                "dd.MM.yyyy",
                "d.M.yyyy"
            };

            foreach (var format in formats)
            {
                if (DateTime.TryParseExact(dateValue, format,
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None,
                    out DateTime result))
                {
                    return result;
                }
            }

            return DateTime.Parse(dateValue);
        }
    }

    public class AddNaturalGasMultipleResponse
    {
        public int SuccessCount { get; set; }
        public List<NaturalGasDto> Results { get; set; } = new List<NaturalGasDto>();
        public List<string> Errors { get; set; } = new List<string>();
    }
}
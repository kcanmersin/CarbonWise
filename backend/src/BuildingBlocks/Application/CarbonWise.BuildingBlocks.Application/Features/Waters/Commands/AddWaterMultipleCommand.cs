using CarbonWise.BuildingBlocks.Domain.Buildings;
using CarbonWise.BuildingBlocks.Domain.Waters;
using CarbonWise.BuildingBlocks.Infrastructure;
using DocumentFormat.OpenXml.Packaging;
using MediatR;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenXmlRow = DocumentFormat.OpenXml.Spreadsheet.Row;
using OpenXmlCell = DocumentFormat.OpenXml.Spreadsheet.Cell;
using OpenXmlSheetData = DocumentFormat.OpenXml.Spreadsheet.SheetData;
using OpenXmlSheet = DocumentFormat.OpenXml.Spreadsheet.Sheet;
using OpenXmlCellValues = DocumentFormat.OpenXml.Spreadsheet.CellValues;
namespace CarbonWise.BuildingBlocks.Application.Features.Waters.Commands
{
    public class AddWaterMultipleCommand : IRequest<AddWaterMultipleResponse>
    {
        public IFormFile File { get; set; }
    }

    public class AddWaterMultipleCommandHandler : IRequestHandler<AddWaterMultipleCommand, AddWaterMultipleResponse>
    {
        private readonly IWaterRepository _waterRepository;
        private readonly IBuildingRepository _buildingRepository;
        private readonly IUnitOfWork _unitOfWork;

        public AddWaterMultipleCommandHandler(
            IWaterRepository waterRepository,
            IBuildingRepository buildingRepository,
            IUnitOfWork unitOfWork)
        {
            _waterRepository = waterRepository;
            _buildingRepository = buildingRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<AddWaterMultipleResponse> Handle(AddWaterMultipleCommand request, CancellationToken cancellationToken)
        {
            var file = request.File;
            if (file == null || file.Length == 0)
                throw new ArgumentException("No file uploaded");

            if (!file.FileName.EndsWith(".xlsx") && !file.FileName.EndsWith(".xls"))
                throw new ArgumentException("Please upload an Excel file");

            var response = new AddWaterMultipleResponse();

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

                    // Binayı bul
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
                        // Skip header row
                        if (rowIndex == 1) continue;

                        try
                        {
                            var cells = row.Elements<OpenXmlCell>().ToList();

                            // Boş satırları atla
                            if (cells.Count < 3)
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

                            // Tüm değerler boşsa bu satırı atla
                            if (string.IsNullOrWhiteSpace(dateValue) &&
                                string.IsNullOrWhiteSpace(initialValue) &&
                                string.IsNullOrWhiteSpace(finalValue))
                            {
                                continue;
                            }

                            if (string.IsNullOrEmpty(dateValue) ||
                                string.IsNullOrEmpty(initialValue) ||
                                string.IsNullOrEmpty(finalValue))
                            {
                                response.Errors.Add($"Sheet '{sheetName}', Row {rowIndex}: Missing required values");
                                continue;
                            }

                            var water = Water.Create(
                                ParseDate(dateValue),
                                decimal.Parse(initialValue),
                                decimal.Parse(finalValue),
                                building.Id);

                            await _waterRepository.AddAsync(water);
                            response.SuccessCount++;
                            response.Results.Add(new WaterDto
                            {
                                Id = water.Id.Value,
                                Date = water.Date,
                                InitialMeterValue = water.InitialMeterValue,
                                FinalMeterValue = water.FinalMeterValue,
                                Usage = water.Usage,
                                BuildingId = water.BuildingId.Value,
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
            // Excel'deki tarih formatını kontrol et
            if (double.TryParse(dateValue, out double doubleValue))
            {
                try
                {
                    return DateTime.FromOADate(doubleValue);
                }
                catch { }
            }

            // Çeşitli tarih formatlarını dene
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

    public class AddWaterMultipleResponse
    {
        public int SuccessCount { get; set; }
        public List<WaterDto> Results { get; set; } = new List<WaterDto>();
        public List<string> Errors { get; set; } = new List<string>();
    }
}

// src/API/CarbonWise.API/Controllers/PapersController.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using CarbonWise.BuildingBlocks.Application.Features.Papers;
using CarbonWise.BuildingBlocks.Application.Features.Papers.Commands.CreatePaper;
using CarbonWise.BuildingBlocks.Application.Features.Papers.Commands.DeletePaper;
using CarbonWise.BuildingBlocks.Application.Features.Papers.Commands.UpdatePaper;
using CarbonWise.BuildingBlocks.Application.Features.Papers.Queries.FilterPapers;
using CarbonWise.BuildingBlocks.Application.Features.Papers.Queries.GetAllPapers;
using CarbonWise.BuildingBlocks.Application.Features.Papers.Queries.GetPaperById;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml;

namespace CarbonWise.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PapersController : ControllerBase
    {
        private readonly IMediator _mediator;

        public PapersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("downloadSampleExcel")]
        public IActionResult DownloadSampleExcel()
        {
            using var memoryStream = new MemoryStream();
            using (var document = SpreadsheetDocument.Create(memoryStream, SpreadsheetDocumentType.Workbook))
            {
                // Workbook oluştur
                var workbookPart = document.AddWorkbookPart();
                workbookPart.Workbook = new Workbook();

                // Worksheet oluştur
                var worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
                worksheetPart.Worksheet = new Worksheet(new SheetData());

                // Sheet'i workbook'a ekle
                var sheets = document.WorkbookPart.Workbook.AppendChild(new Sheets());
                var sheet = new Sheet()
                {
                    Id = document.WorkbookPart.GetIdOfPart(worksheetPart),
                    SheetId = 1,
                    Name = "Paper Data"
                };
                sheets.Append(sheet);

                // Veri ekle
                var sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>();

                // Header satırı
                var headerRow = new Row() { RowIndex = 1 };
                headerRow.Append(
                    CreateCell("Date", CellValues.String, 1),
                    CreateCell("Usage", CellValues.String, 2)
                );
                sheetData.Append(headerRow);

                // Örnek veri satırları
                var row2 = new Row() { RowIndex = 2 };
                row2.Append(
                    CreateCell("01/01/2025", CellValues.String, 1),
                    CreateCell("1000", CellValues.Number, 2)
                );
                sheetData.Append(row2);

                var row3 = new Row() { RowIndex = 3 };
                row3.Append(
                    CreateCell("02/01/2025", CellValues.String, 1),
                    CreateCell("1500", CellValues.Number, 2)
                );
                sheetData.Append(row3);

                workbookPart.Workbook.Save();
            }

            memoryStream.Position = 0;
            return File(
                memoryStream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "Paper_Sample.xlsx"
            );
        }

        private Cell CreateCell(string value, CellValues dataType, uint columnIndex)
        {
            var cell = new Cell()
            {
                DataType = dataType,
                CellReference = GetColumnName(columnIndex) + "1" // Satır numarası
            };

            if (dataType == CellValues.String)
                cell.CellValue = new CellValue(value);
            else
                cell.CellValue = new CellValue(value);

            return cell;
        }

        private string GetColumnName(uint columnIndex)
        {
            string columnName = string.Empty;
            while (columnIndex > 0)
            {
                uint remainder = columnIndex % 26;
                if (remainder == 0)
                {
                    columnName = "Z" + columnName;
                    columnIndex = (columnIndex / 26) - 1;
                }
                else
                {
                    columnName = ((char)(remainder + 64)).ToString() + columnName;
                    columnIndex /= 26;
                }
            }
            return columnName;
        }

        [HttpPost("multiple")]
        public async Task<IActionResult> AddMultiple(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded");

            if (!file.FileName.EndsWith(".xlsx") && !file.FileName.EndsWith(".xls"))
                return BadRequest("Please upload an Excel file");

            try
            {
                using var stream = file.OpenReadStream();
                using var document = SpreadsheetDocument.Open(stream, false);

                var workbookPart = document.WorkbookPart;
                var worksheetPart = workbookPart.WorksheetParts.FirstOrDefault();

                if (worksheetPart == null)
                    return BadRequest("No worksheet found in the Excel file");

                var sheetData = worksheetPart.Worksheet.Elements<SheetData>().FirstOrDefault();

                if (sheetData == null)
                    return BadRequest("No data found in the worksheet");

                var results = new List<PaperDto>();
                var errors = new List<string>();

                int rowIndex = 0;
                foreach (Row row in sheetData.Elements<Row>())
                {
                    rowIndex++;
                    // Skip header row
                    if (rowIndex == 1) continue;

                    try
                    {
                        var cells = row.Elements<Cell>().ToList();

                        // Boş satırları atla
                        if (cells.Count < 2)
                        {
                            // Hiç hücre yoksa veya tüm hücreler boşsa, bu satırı sessizce atla
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

                            errors.Add($"Row {rowIndex}: Not enough columns");
                            continue;
                        }

                        var dateValue = GetCellValue(document, cells[0]);
                        var usageValue = GetCellValue(document, cells[1]);

                        // Tüm değerler boşsa bu satırı atla
                        if (string.IsNullOrWhiteSpace(dateValue) &&
                            string.IsNullOrWhiteSpace(usageValue))
                        {
                            continue;
                        }

                        if (string.IsNullOrEmpty(dateValue) ||
                            string.IsNullOrEmpty(usageValue))
                        {
                            errors.Add($"Row {rowIndex}: Missing required values");
                            continue;
                        }

                        var command = new CreatePaperCommand
                        {
                            Date = ParseDate(dateValue),
                            Usage = decimal.Parse(usageValue)
                        };

                        var result = await _mediator.Send(command);
                        results.Add(result);
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"Row {rowIndex}: {ex.Message}");
                    }
                }

                return Ok(new
                {
                    SuccessCount = results.Count,
                    Results = results,
                    Errors = errors
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        private string GetCellValue(SpreadsheetDocument document, Cell cell)
        {
            if (cell == null) return null;

            var value = cell.InnerText;
            if (cell.DataType != null && cell.DataType.Value == CellValues.SharedString)
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

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var query = new GetAllPapersQuery();
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var query = new GetPaperByIdQuery { Id = id };
            var result = await _mediator.Send(query);

            if (result == null)
            {
                return NotFound();
            }

            return Ok(result);
        }

        [HttpGet("filter")]
        public async Task<IActionResult> Filter([FromQuery] PaperFilterRequest filter)
        {
            try
            {
                var query = new FilterPapersQuery
                {
                    StartDate = filter.StartDate,
                    EndDate = filter.EndDate
                };

                var result = await _mediator.Send(query);
                return Ok(result);
            }
            catch (ApplicationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreatePaperRequest request)
        {
            try
            {
                var command = new CreatePaperCommand
                {
                    Date = request.Date,
                    Usage = request.Usage
                };

                var result = await _mediator.Send(command);
                return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePaperRequest request)
        {
            try
            {
                var command = new UpdatePaperCommand
                {
                    Id = id,
                    Date = request.Date,
                    Usage = request.Usage
                };

                var result = await _mediator.Send(command);
                return Ok(result);
            }
            catch (ApplicationException ex) when (ex.Message.Contains("not found"))
            {
                return NotFound(ex.Message);
            }
            catch (ApplicationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var command = new DeletePaperCommand { Id = id };
            var result = await _mediator.Send(command);

            if (!result)
            {
                return NotFound();
            }

            return NoContent();
        }
    }

    public class PaperFilterRequest
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class CreatePaperRequest
    {
        [Required]
        public DateTime Date { get; set; }

        [Required]
        public decimal Usage { get; set; }
    }

    public class UpdatePaperRequest
    {
        [Required]
        public DateTime Date { get; set; }

        [Required]
        public decimal Usage { get; set; }
    }
}
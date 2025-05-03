using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CarbonWise.BuildingBlocks.Application.Features.NaturalGases.Commands;
using CarbonWise.BuildingBlocks.Application.Features.NaturalGases.Queries;
using MediatR;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml;
using OpenXmlSheet = DocumentFormat.OpenXml.Spreadsheet.Sheet;
using OpenXmlCellValues = DocumentFormat.OpenXml.Spreadsheet.CellValues;
using CarbonWise.BuildingBlocks.Application.Features.NaturalGases.Commands.AddNaturalGasMultiple;

namespace CarbonWise.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NaturalGasController : ControllerBase
    {
        private readonly IMediator _mediator;

        public NaturalGasController(IMediator mediator)
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

                var sheets = document.WorkbookPart.Workbook.AppendChild(new Sheets());
                uint sheetId = 1;

                CreateNaturalGasSheet(document, sheets, "YAPI İŞLERİ TEKNİK DAİRE BAŞKAN", sheetId++);

                CreateNaturalGasSheet(document, sheets, "BİLGİSAYAR MÜHENDİSLİĞİ", sheetId++);

                workbookPart.Workbook.Save();
            }

            memoryStream.Position = 0;
            return File(
                memoryStream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "NaturalGas_Sample.xlsx"
            );
        }

        private void CreateNaturalGasSheet(SpreadsheetDocument document, Sheets sheets, string sheetName, uint sheetId)
        {
            var worksheetPart = document.WorkbookPart.AddNewPart<WorksheetPart>();
            worksheetPart.Worksheet = new Worksheet(new SheetData());

            var sheet = new OpenXmlSheet()
            {
                Id = document.WorkbookPart.GetIdOfPart(worksheetPart),
                SheetId = sheetId,
                Name = sheetName
            };
            sheets.Append(sheet);

            // Veri ekle
            var sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>();

            // Header satırı
            var headerRow = new Row() { RowIndex = 1 };
            headerRow.Append(
                CreateCell("Date", OpenXmlCellValues.String, 1),
                CreateCell("InitialMeterValue", OpenXmlCellValues.String, 2),
                CreateCell("FinalMeterValue", OpenXmlCellValues.String, 3),
                CreateCell("SM3Value", OpenXmlCellValues.String, 4)
            );
            sheetData.Append(headerRow);

            // Örnek veri satırları
            var row2 = new Row() { RowIndex = 2 };
            row2.Append(
                CreateCell("01/01/2025", OpenXmlCellValues.String, 1),
                CreateCell("1000", OpenXmlCellValues.Number, 2),
                CreateCell("1100", OpenXmlCellValues.Number, 3),
                CreateCell("0.01", OpenXmlCellValues.Number, 4)
            );
            sheetData.Append(row2);

            var row3 = new Row() { RowIndex = 3 };
            row3.Append(
                CreateCell("02/01/2025", OpenXmlCellValues.String, 1),
                CreateCell("1100", OpenXmlCellValues.Number, 2),
                CreateCell("1250", OpenXmlCellValues.Number, 3),
                CreateCell("0.01", OpenXmlCellValues.Number, 4)
            );
            sheetData.Append(row3);
        }

        private Cell CreateCell(string value, OpenXmlCellValues dataType, uint columnIndex)
        {
            var cell = new Cell()
            {
                DataType = dataType,
                CellReference = GetColumnName(columnIndex) + "1"
            };

            if (dataType == OpenXmlCellValues.String)
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
            var command = new AddNaturalGasMultipleCommand { File = file };
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var query = new GetNaturalGasByIdQuery { Id = id };
            var result = await _mediator.Send(query);

            if (result == null)
            {
                return NotFound();
            }

            return Ok(result);
        }

        [HttpGet("building/{buildingId}")]
        public async Task<IActionResult> GetByBuilding(Guid buildingId)
        {
            try
            {
                var query = new GetNaturalGasByBuildingQuery { BuildingId = buildingId };
                var result = await _mediator.Send(query);
                return Ok(result);
            }
            catch (ApplicationException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet("filter")]
        public async Task<IActionResult> Filter([FromQuery] NaturalGasFilterRequest filter)
        {
            try
            {
                var query = new FilterNaturalGasQuery
                {
                    BuildingId = filter.BuildingId,
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
        public async Task<IActionResult> Create([FromBody] CreateNaturalGasRequest request)
        {
            try
            {
                var command = new CreateNaturalGasCommand
                {
                    Date = request.Date,
                    InitialMeterValue = request.InitialMeterValue,
                    FinalMeterValue = request.FinalMeterValue,
                    SM3Value = request.SM3Value,
                    BuildingId = request.BuildingId
                };

                var result = await _mediator.Send(command);
                return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
            }
            catch (ApplicationException ex)
            {
                return NotFound(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateNaturalGasRequest request)
        {
            try
            {
                var command = new UpdateNaturalGasCommand
                {
                    Id = id,
                    Date = request.Date,
                    InitialMeterValue = request.InitialMeterValue,
                    FinalMeterValue = request.FinalMeterValue,
                    SM3Value = request.SM3Value
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
        [HttpGet("monthly-totals")]
        public async Task<IActionResult> GetMonthlyTotals([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var query = new GetNaturalGasMonthlyTotalsQuery
                {
                    StartDate = startDate,
                    EndDate = endDate
                };

                var result = await _mediator.Send(query);
                return Ok(result);
            }
            catch (ApplicationException ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var command = new DeleteNaturalGasCommand { Id = id };
            var result = await _mediator.Send(command);

            if (!result)
            {
                return NotFound();
            }

            return NoContent();
        }
    }

    public class NaturalGasFilterRequest
    {
        public Guid? BuildingId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class CreateNaturalGasRequest
    {
        [Required]
        public DateTime Date { get; set; }

        [Required]
        public decimal InitialMeterValue { get; set; }

        [Required]
        public decimal FinalMeterValue { get; set; }

        [Required]
        public decimal SM3Value { get; set; }

        [Required]
        public Guid BuildingId { get; set; }
    }

    public class UpdateNaturalGasRequest
    {
        [Required]
        public DateTime Date { get; set; }

        [Required]
        public decimal InitialMeterValue { get; set; }

        [Required]
        public decimal FinalMeterValue { get; set; }

        [Required]
        public decimal SM3Value { get; set; }
    }
}
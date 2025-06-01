using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using CarbonWise.BuildingBlocks.Application.Features.Papers;
using CarbonWise.BuildingBlocks.Application.Features.Papers.Commands.CreatePaper;
using CarbonWise.BuildingBlocks.Application.Features.Papers.Commands.DeletePaper;
using CarbonWise.BuildingBlocks.Application.Features.Papers.Commands.UpdatePaper;
using CarbonWise.BuildingBlocks.Application.Features.Papers.Commands.AddPaperMultiple;
using CarbonWise.BuildingBlocks.Application.Features.Papers.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml;
using CarbonWise.BuildingBlocks.Application.Features.Papers.Queries.FilterPapers;
using CarbonWise.BuildingBlocks.Application.Features.Papers.Queries.GetAllPapers;
using CarbonWise.BuildingBlocks.Application.Features.Papers.Queries.GetPaperById;

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
                var workbookPart = document.AddWorkbookPart();
                workbookPart.Workbook = new Workbook();

                var sheets = document.WorkbookPart.Workbook.AppendChild(new Sheets());
                uint sheetId = 1;

                CreatePaperSheet(document, sheets, "KAĞIT KULLANIM BİNASI", sheetId++);
                CreatePaperSheet(document, sheets, "İDARİ BİNA", sheetId++);

                workbookPart.Workbook.Save();
            }

            memoryStream.Position = 0;
            return File(
                memoryStream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "Paper_Sample.xlsx"
            );
        }

        private void CreatePaperSheet(SpreadsheetDocument document, Sheets sheets, string sheetName, uint sheetId)
        {
            var worksheetPart = document.WorkbookPart.AddNewPart<WorksheetPart>();
            worksheetPart.Worksheet = new Worksheet(new SheetData());

            var sheet = new Sheet()
            {
                Id = document.WorkbookPart.GetIdOfPart(worksheetPart),
                SheetId = sheetId,
                Name = sheetName
            };
            sheets.Append(sheet);

            var sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>();

            var headerRow = new Row() { RowIndex = 1 };
            headerRow.Append(
                CreateCell("Date", CellValues.String, 1),
                CreateCell("Usage", CellValues.String, 2)
            );
            sheetData.Append(headerRow);

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
        }

        private Cell CreateCell(string value, CellValues dataType, uint columnIndex)
        {
            var cell = new Cell()
            {
                DataType = dataType,
                CellReference = GetColumnName(columnIndex) + "1"
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
            var command = new AddPaperMultipleCommand { File = file };
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpGet("monthly-totals")]
        public async Task<IActionResult> GetMonthlyTotals([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var query = new GetPaperMonthlyTotalsQuery
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

        [HttpGet("building/{buildingId}")]
        public async Task<IActionResult> GetByBuilding(Guid buildingId)
        {
            try
            {
                var query = new GetPapersByBuildingQuery { BuildingId = buildingId };
                var result = await _mediator.Send(query);
                return Ok(result);
            }
            catch (ApplicationException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet("filter")]
        public async Task<IActionResult> Filter([FromQuery] PaperFilterRequest filter)
        {
            try
            {
                var query = new FilterPapersQuery
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
        public async Task<IActionResult> Create([FromBody] CreatePaperRequest request)
        {
            try
            {
                var command = new CreatePaperCommand
                {
                    Date = request.Date,
                    Usage = request.Usage,
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
        public Guid? BuildingId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class CreatePaperRequest
    {
        [Required]
        public DateTime Date { get; set; }

        [Required]
        public decimal Usage { get; set; }

        [Required]
        public Guid BuildingId { get; set; }
    }

    public class UpdatePaperRequest
    {
        [Required]
        public DateTime Date { get; set; }

        [Required]
        public decimal Usage { get; set; }
    }
}
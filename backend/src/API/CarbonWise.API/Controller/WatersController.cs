using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using CarbonWise.BuildingBlocks.Application.Features.Waters;
using CarbonWise.BuildingBlocks.Application.Features.Waters.Commands.CreateWater;
using CarbonWise.BuildingBlocks.Application.Features.Waters.Commands.DeleteWater;
using CarbonWise.BuildingBlocks.Application.Features.Waters.Commands.UpdateWater;
using CarbonWise.BuildingBlocks.Application.Features.Waters.Commands;
using CarbonWise.BuildingBlocks.Application.Features.Waters.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml;
using CarbonWise.BuildingBlocks.Application.Features.Waters.Commands;
using CarbonWise.BuildingBlocks.Application.Features.Waters.Queries.FilterWaters;
using CarbonWise.BuildingBlocks.Application.Features.Waters.Queries.GetAllWaters;
using CarbonWise.BuildingBlocks.Application.Features.Waters.Queries.GetWaterById;

namespace CarbonWise.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WatersController : ControllerBase
    {
        private readonly IMediator _mediator;

        public WatersController(IMediator mediator)
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

                CreateWaterSheet(document, sheets, "SU TESİSATI MALZEME BİNASI", sheetId++);
                CreateWaterSheet(document, sheets, "BİLGİSAYAR MÜHENDİSLİĞİ", sheetId++);

                workbookPart.Workbook.Save();
            }

            memoryStream.Position = 0;
            return File(
                memoryStream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "Water_Sample.xlsx"
            );
        }

        private void CreateWaterSheet(SpreadsheetDocument document, Sheets sheets, string sheetName, uint sheetId)
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
                CreateCell("InitialMeterValue", CellValues.String, 2),
                CreateCell("FinalMeterValue", CellValues.String, 3)
            );
            sheetData.Append(headerRow);

            var row2 = new Row() { RowIndex = 2 };
            row2.Append(
                CreateCell("01/01/2025", CellValues.String, 1),
                CreateCell("1000", CellValues.Number, 2),
                CreateCell("1100", CellValues.Number, 3)
            );
            sheetData.Append(row2);

            var row3 = new Row() { RowIndex = 3 };
            row3.Append(
                CreateCell("02/01/2025", CellValues.String, 1),
                CreateCell("1100", CellValues.Number, 2),
                CreateCell("1250", CellValues.Number, 3)
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
            var command = new AddWaterMultipleCommand { File = file };
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpGet("monthly-totals")]
        public async Task<IActionResult> GetMonthlyTotals([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var query = new GetWaterMonthlyTotalsQuery
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
            var query = new GetAllWatersQuery();
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var query = new GetWaterByIdQuery { Id = id };
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
                var query = new GetWatersByBuildingQuery { BuildingId = buildingId };
                var result = await _mediator.Send(query);
                return Ok(result);
            }
            catch (ApplicationException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet("filter")]
        public async Task<IActionResult> Filter([FromQuery] WaterFilterRequest filter)
        {
            try
            {
                var query = new FilterWatersQuery
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
        public async Task<IActionResult> Create([FromBody] CreateWaterRequest request)
        {
            try
            {
                var command = new CreateWaterCommand
                {
                    Date = request.Date,
                    InitialMeterValue = request.InitialMeterValue,
                    FinalMeterValue = request.FinalMeterValue,
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
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateWaterRequest request)
        {
            try
            {
                var command = new UpdateWaterCommand
                {
                    Id = id,
                    Date = request.Date,
                    InitialMeterValue = request.InitialMeterValue,
                    FinalMeterValue = request.FinalMeterValue
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
            var command = new DeleteWaterCommand { Id = id };
            var result = await _mediator.Send(command);

            if (!result)
            {
                return NotFound();
            }

            return NoContent();
        }
    }

    public class WaterFilterRequest
    {
        public Guid? BuildingId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class CreateWaterRequest
    {
        [Required]
        public DateTime Date { get; set; }

        [Required]
        public decimal InitialMeterValue { get; set; }

        [Required]
        public decimal FinalMeterValue { get; set; }

        [Required]
        public Guid BuildingId { get; set; }
    }

    public class UpdateWaterRequest
    {
        [Required]
        public DateTime Date { get; set; }

        [Required]
        public decimal InitialMeterValue { get; set; }

        [Required]
        public decimal FinalMeterValue { get; set; }
    }
}
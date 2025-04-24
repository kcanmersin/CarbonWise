using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using CarbonWise.BuildingBlocks.Application.Features.Electrics;
using CarbonWise.BuildingBlocks.Application.Features.Electrics.Commands.CreateElectric;
using CarbonWise.BuildingBlocks.Application.Features.Electrics.Commands.DeleteElectric;
using CarbonWise.BuildingBlocks.Application.Features.Electrics.Commands.UpdateElectric;
using CarbonWise.BuildingBlocks.Application.Features.Electrics.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CarbonWise.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ElectricsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ElectricsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("monthly-aggregate")]
        public async Task<IActionResult> GetMonthlyAggregate([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var query = new GetElectricMonthlyAggregateQuery
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


        [HttpGet("monthly-totals")]
        public async Task<IActionResult> GetMonthlyTotals([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var query = new GetElectricMonthlyTotalsQuery
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
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var query = new GetElectricByIdQuery { Id = id };
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
                var query = new GetElectricsByBuildingQuery { BuildingId = buildingId };
                var result = await _mediator.Send(query);
                return Ok(result);
            }
            catch (ApplicationException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet("filter")]
        public async Task<IActionResult> Filter([FromQuery] ElectricFilterRequest filter)
        {
            try
            {
                var query = new FilterElectricsQuery
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
        public async Task<IActionResult> Create([FromBody] CreateElectricRequest request)
        {
            try
            {
                var command = new CreateElectricCommand
                {
                    Date = request.Date,
                    InitialMeterValue = request.InitialMeterValue,
                    FinalMeterValue = request.FinalMeterValue,
                    KWHValue = request.KWHValue,
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
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateElectricRequest request)
        {
            try
            {
                var command = new UpdateElectricCommand
                {
                    Id = id,
                    Date = request.Date,
                    InitialMeterValue = request.InitialMeterValue,
                    FinalMeterValue = request.FinalMeterValue,
                    KWHValue = request.KWHValue
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
            var command = new DeleteElectricCommand { Id = id };
            var result = await _mediator.Send(command);

            if (!result)
            {
                return NotFound();
            }

            return NoContent();
        }
    }

    public class ElectricFilterRequest
    {
        public Guid? BuildingId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class CreateElectricRequest
    {
        [Required]
        public DateTime Date { get; set; }

        [Required]
        public decimal InitialMeterValue { get; set; }

        [Required]
        public decimal FinalMeterValue { get; set; }

        [Required]
        public decimal KWHValue { get; set; }

        [Required]
        public Guid BuildingId { get; set; }
    }

    public class UpdateElectricRequest
    {
        [Required]
        public DateTime Date { get; set; }

        [Required]
        public decimal InitialMeterValue { get; set; }

        [Required]
        public decimal FinalMeterValue { get; set; }

        [Required]
        public decimal KWHValue { get; set; }
    }
}
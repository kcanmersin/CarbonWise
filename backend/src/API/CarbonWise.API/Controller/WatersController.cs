// src/API/CarbonWise.API/Controllers/WatersController.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using CarbonWise.BuildingBlocks.Application.Features.Waters;
using CarbonWise.BuildingBlocks.Application.Features.Waters.Commands.CreateWater;
using CarbonWise.BuildingBlocks.Application.Features.Waters.Commands.DeleteWater;
using CarbonWise.BuildingBlocks.Application.Features.Waters.Commands.UpdateWater;
using CarbonWise.BuildingBlocks.Application.Features.Waters.Queries.FilterWaters;
using CarbonWise.BuildingBlocks.Application.Features.Waters.Queries.GetAllWaters;
using CarbonWise.BuildingBlocks.Application.Features.Waters.Queries.GetWaterById;
using MediatR;
using Microsoft.AspNetCore.Mvc;

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

        [HttpGet("filter")]
        public async Task<IActionResult> Filter([FromQuery] WaterFilterRequest filter)
        {
            try
            {
                var query = new FilterWatersQuery
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
        public async Task<IActionResult> Create([FromBody] CreateWaterRequest request)
        {
            try
            {
                var command = new CreateWaterCommand
                {
                    Date = request.Date,
                    InitialMeterValue = request.InitialMeterValue,
                    FinalMeterValue = request.FinalMeterValue
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
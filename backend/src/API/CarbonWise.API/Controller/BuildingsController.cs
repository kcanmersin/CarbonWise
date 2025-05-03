using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using CarbonWise.BuildingBlocks.Application.Features.Buildings;
using CarbonWise.BuildingBlocks.Application.Features.Buildings.Commands;
using CarbonWise.BuildingBlocks.Application.Features.Buildings.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CarbonWise.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BuildingsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public BuildingsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _mediator.Send(new GetAllBuildingsQuery());
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var result = await _mediator.Send(new GetBuildingByIdQuery { Id = id });

            if (result == null)
            {
                return NotFound();
            }

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateBuildingRequest request)
        {
            try
            {
                var command = new CreateBuildingCommand
                {
                    Name = request.Name,
                    E_MeterCode = request.E_MeterCode,
                    G_MeterCode = request.G_MeterCode
                };

                var result = await _mediator.Send(command);
                return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
            }
            catch (ApplicationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateBuildingRequest request)
        {
            try
            {
                var command = new UpdateBuildingCommand
                {
                    Id = id,
                    Name = request.Name,
                    E_MeterCode = request.E_MeterCode,
                    G_MeterCode = request.G_MeterCode
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
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _mediator.Send(new DeleteBuildingCommand { Id = id });

            if (!result)
            {
                return NotFound();
            }

            return NoContent();
        }
    }

    public class CreateBuildingRequest
    {
        [Required]
        public string Name { get; set; }

        public string E_MeterCode { get; set; }

        public string G_MeterCode { get; set; }
    }

    public class UpdateBuildingRequest
    {
        [Required]
        public string Name { get; set; }

        public string E_MeterCode { get; set; }

        public string G_MeterCode { get; set; }
    }
}
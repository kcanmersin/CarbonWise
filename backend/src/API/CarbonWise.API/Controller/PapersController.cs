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
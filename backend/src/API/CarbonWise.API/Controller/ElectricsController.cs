using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using CarbonWise.BuildingBlocks.Domain.Buildings;
using CarbonWise.BuildingBlocks.Domain.Electrics;
using CarbonWise.BuildingBlocks.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarbonWise.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ElectricsController : ControllerBase
    {
        private readonly IElectricRepository _electricRepository;
        private readonly IBuildingRepository _buildingRepository;
        private readonly IUnitOfWork _unitOfWork;

        public ElectricsController(
            IElectricRepository electricRepository,
            IBuildingRepository buildingRepository,
            IUnitOfWork unitOfWork)
        {
            _electricRepository = electricRepository;
            _buildingRepository = buildingRepository;
            _unitOfWork = unitOfWork;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var electric = await _electricRepository.GetByIdAsync(new ElectricId(id));

            if (electric == null)
            {
                return NotFound();
            }

            return Ok(MapToDto(electric));
        }

        [HttpGet("building/{buildingId}")]
        public async Task<IActionResult> GetByBuilding(Guid buildingId)
        {
            var building = await _buildingRepository.GetByIdAsync(new BuildingId(buildingId));
            if (building == null)
            {
                return NotFound("Building not found");
            }

            var electrics = await _electricRepository.GetByBuildingIdAsync(new BuildingId(buildingId));
            return Ok(electrics.Select(MapToDto));
        }

        [HttpGet("filter")]
        public async Task<IActionResult> Filter([FromQuery] ElectricFilterRequest filter)
        {
            if (filter.BuildingId.HasValue)
            {
                var building = await _buildingRepository.GetByIdAsync(new BuildingId(filter.BuildingId.Value));
                if (building == null)
                {
                    return NotFound("Building not found");
                }
            }

            List<Electric> electrics;

            if (filter.BuildingId.HasValue && filter.StartDate.HasValue && filter.EndDate.HasValue)
            {
                electrics = await _electricRepository.GetByBuildingIdAndDateRangeAsync(
                    new BuildingId(filter.BuildingId.Value),
                    filter.StartDate.Value,
                    filter.EndDate.Value);
            }
            else if (filter.BuildingId.HasValue)
            {
                electrics = await _electricRepository.GetByBuildingIdAsync(
                    new BuildingId(filter.BuildingId.Value));
            }
            else if (filter.StartDate.HasValue && filter.EndDate.HasValue)
            {
                electrics = await _electricRepository.GetByDateRangeAsync(
                    filter.StartDate.Value,
                    filter.EndDate.Value);
            }
            else
            {
                return BadRequest("At least one filter parameter is required");
            }

            return Ok(electrics.Select(MapToDto));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateElectricRequest request)
        {
            var building = await _buildingRepository.GetByIdAsync(new BuildingId(request.BuildingId));
            if (building == null)
            {
                return NotFound("Building not found");
            }

            try
            {
                var electric = Electric.Create(
                    request.Date,
                    request.InitialMeterValue,
                    request.FinalMeterValue,
                    request.KWHValue,
                    new BuildingId(request.BuildingId));

                await _electricRepository.AddAsync(electric);
                await _unitOfWork.CommitAsync();

                return CreatedAtAction(nameof(Get), new { id = electric.Id.Value }, MapToDto(electric));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateElectricRequest request)
        {
            var electric = await _electricRepository.GetByIdAsync(new ElectricId(id));

            if (electric == null)
            {
                return NotFound();
            }

            try
            {
                electric.Update(
                    request.Date,
                    request.InitialMeterValue,
                    request.FinalMeterValue,
                    request.KWHValue);

                await _electricRepository.UpdateAsync(electric);
                await _unitOfWork.CommitAsync();

                return Ok(MapToDto(electric));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var electric = await _electricRepository.GetByIdAsync(new ElectricId(id));

            if (electric == null)
            {
                return NotFound();
            }

            await _electricRepository.DeleteAsync(new ElectricId(id));
            await _unitOfWork.CommitAsync();

            return NoContent();
        }

        private static ElectricDto MapToDto(Electric electric)
        {
            return new ElectricDto
            {
                Id = electric.Id.Value,
                Date = electric.Date,
                InitialMeterValue = electric.InitialMeterValue,
                FinalMeterValue = electric.FinalMeterValue,
                Usage = electric.Usage,
                KWHValue = electric.KWHValue,
                BuildingId = electric.BuildingId.Value,
                BuildingName = electric.Building?.Name
            };
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

    public class ElectricDto
    {
        public Guid Id { get; set; }
        public DateTime Date { get; set; }
        public decimal InitialMeterValue { get; set; }
        public decimal FinalMeterValue { get; set; }
        public decimal Usage { get; set; }
        public decimal KWHValue { get; set; }
        public Guid BuildingId { get; set; }
        public string BuildingName { get; set; }
    }
}
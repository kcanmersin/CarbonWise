using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CarbonWise.BuildingBlocks.Domain.Buildings;
using CarbonWise.BuildingBlocks.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarbonWise.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BuildingsController : ControllerBase
    {
        private readonly IBuildingRepository _buildingRepository;
        private readonly IUnitOfWork _unitOfWork;

        public BuildingsController(
            IBuildingRepository buildingRepository,
            IUnitOfWork unitOfWork)
        {
            _buildingRepository = buildingRepository;
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var buildings = await _buildingRepository.GetAllAsync();
            return Ok(buildings.Select(MapToDto));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var building = await _buildingRepository.GetByIdAsync(new BuildingId(id));

            if (building == null)
            {
                return NotFound();
            }

            return Ok(MapToDto(building));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateBuildingRequest request)
        {
            try
            {
                var building = Building.Create(
                    request.Name,
                    request.E_MeterCode,
                    request.G_MeterCode);

                await _buildingRepository.AddAsync(building);
                await _unitOfWork.CommitAsync();

                return CreatedAtAction(nameof(Get), new { id = building.Id.Value }, MapToDto(building));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateBuildingRequest request)
        {
            var building = await _buildingRepository.GetByIdAsync(new BuildingId(id));

            if (building == null)
            {
                return NotFound();
            }

            try
            {
                building.Update(
                    request.Name,
                    request.E_MeterCode,
                    request.G_MeterCode);

                await _buildingRepository.UpdateAsync(building);
                await _unitOfWork.CommitAsync();

                return Ok(MapToDto(building));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var building = await _buildingRepository.GetByIdAsync(new BuildingId(id));

            if (building == null)
            {
                return NotFound();
            }

            await _buildingRepository.DeleteAsync(new BuildingId(id));
            await _unitOfWork.CommitAsync();

            return NoContent();
        }

        private static BuildingDto MapToDto(Building building)
        {
            return new BuildingDto
            {
                Id = building.Id.Value,
                Name = building.Name,
                E_MeterCode = building.E_MeterCode,
                G_MeterCode = building.G_MeterCode
            };
        }
    }

    public class CreateBuildingRequest
    {
        public string Name { get; set; }
        public string E_MeterCode { get; set; }
        public string G_MeterCode { get; set; }
    }

    public class UpdateBuildingRequest
    {
        public string Name { get; set; }
        public string E_MeterCode { get; set; }
        public string G_MeterCode { get; set; }
    }

    public class BuildingDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string E_MeterCode { get; set; }
        public string G_MeterCode { get; set; }
    }
}
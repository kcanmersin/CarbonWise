using System;
using System.Threading.Tasks;
using CarbonWise.BuildingBlocks.Domain.SchoolInfos;
using CarbonWise.BuildingBlocks.Domain.SchoolInfos;
using CarbonWise.BuildingBlocks.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarbonWise.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SchoolInfoController : ControllerBase
    {
        private readonly ISchoolInfoRepository _schoolInfoRepository;
        private readonly IUnitOfWork _unitOfWork;

        public SchoolInfoController(
            ISchoolInfoRepository schoolInfoRepository,
            IUnitOfWork unitOfWork)
        {
            _schoolInfoRepository = schoolInfoRepository;
            _unitOfWork = unitOfWork;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var schoolInfo = await _schoolInfoRepository.GetByIdAsync(new SchoolInfoId(id));

            if (schoolInfo == null)
            {
                return NotFound();
            }

            return Ok(MapToDto(schoolInfo));
        }

        [HttpGet("year/{year}")]
        public async Task<IActionResult> GetByYear(int year)
        {
            var schoolInfo = await _schoolInfoRepository.GetByYearAsync(year);

            if (schoolInfo == null)
            {
                return NotFound();
            }

            return Ok(MapToDto(schoolInfo));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateSchoolInfoRequest request)
        {
            try
            {
                var schoolInfo = SchoolInfo.Create(request.NumberOfPeople, request.Year);

                if (request.CampusVehicleEntry != null)
                {
                    var vehicleEntry = CampusVehicleEntry.Create(
                        request.CampusVehicleEntry.CarsManagedByUniversity,
                        request.CampusVehicleEntry.CarsEnteringUniversity,
                        request.CampusVehicleEntry.MotorcyclesEnteringUniversity);

                    schoolInfo.AssignVehicleEntry(vehicleEntry);
                }

                await _schoolInfoRepository.AddAsync(schoolInfo);
                await _unitOfWork.CommitAsync();

                return CreatedAtAction(nameof(Get), new { id = schoolInfo.Id.Value }, MapToDto(schoolInfo));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSchoolInfoRequest request)
        {
            var schoolInfo = await _schoolInfoRepository.GetByIdAsync(new SchoolInfoId(id));

            if (schoolInfo == null)
            {
                return NotFound();
            }

            try
            {
                schoolInfo.UpdateNumberOfPeople(request.NumberOfPeople);

                if (request.CampusVehicleEntry != null)
                {
                    if (schoolInfo.Vehicles == null)
                    {
                        var vehicleEntry = CampusVehicleEntry.Create(
                            request.CampusVehicleEntry.CarsManagedByUniversity,
                            request.CampusVehicleEntry.CarsEnteringUniversity,
                            request.CampusVehicleEntry.MotorcyclesEnteringUniversity);

                        schoolInfo.AssignVehicleEntry(vehicleEntry);
                    }
                    else
                    {
                        schoolInfo.Vehicles.UpdateCarsManagedByUniversity(request.CampusVehicleEntry.CarsManagedByUniversity);
                        schoolInfo.Vehicles.UpdateCarsEnteringUniversity(request.CampusVehicleEntry.CarsEnteringUniversity);
                        schoolInfo.Vehicles.UpdateMotorcyclesEnteringUniversity(request.CampusVehicleEntry.MotorcyclesEnteringUniversity);
                    }
                }

                await _schoolInfoRepository.UpdateAsync(schoolInfo);
                await _unitOfWork.CommitAsync();

                return Ok(MapToDto(schoolInfo));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        private static SchoolInfoDto MapToDto(SchoolInfo schoolInfo)
        {
            return new SchoolInfoDto
            {
                Id = schoolInfo.Id.Value,
                NumberOfPeople = schoolInfo.NumberOfPeople,
                Year = schoolInfo.Year,
                CampusVehicleEntry = schoolInfo.Vehicles != null
                    ? new CampusVehicleEntryDto
                    {
                        Id = schoolInfo.Vehicles.Id.Value,
                        CarsManagedByUniversity = schoolInfo.Vehicles.CarsManagedByUniversity,
                        CarsEnteringUniversity = schoolInfo.Vehicles.CarsEnteringUniversity,
                        MotorcyclesEnteringUniversity = schoolInfo.Vehicles.MotorcyclesEnteringUniversity
                    }
                    : null
            };
        }
    }

    public class CreateSchoolInfoRequest
    {
        public int NumberOfPeople { get; set; }
        public int Year { get; set; }
        public CreateCampusVehicleEntryRequest CampusVehicleEntry { get; set; }
    }

    public class CreateCampusVehicleEntryRequest
    {
        public int CarsManagedByUniversity { get; set; }
        public int CarsEnteringUniversity { get; set; }
        public int MotorcyclesEnteringUniversity { get; set; }
    }

    public class UpdateSchoolInfoRequest
    {
        public int NumberOfPeople { get; set; }
        public UpdateCampusVehicleEntryRequest CampusVehicleEntry { get; set; }
    }

    public class UpdateCampusVehicleEntryRequest
    {
        public int CarsManagedByUniversity { get; set; }
        public int CarsEnteringUniversity { get; set; }
        public int MotorcyclesEnteringUniversity { get; set; }
    }

    public class SchoolInfoDto
    {
        public Guid Id { get; set; }
        public int NumberOfPeople { get; set; }
        public int Year { get; set; }
        public CampusVehicleEntryDto CampusVehicleEntry { get; set; }
    }

    public class CampusVehicleEntryDto
    {
        public Guid Id { get; set; }
        public int CarsManagedByUniversity { get; set; }
        public int CarsEnteringUniversity { get; set; }
        public int MotorcyclesEnteringUniversity { get; set; }
    }
}
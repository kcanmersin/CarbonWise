using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CarbonWise.BuildingBlocks.Domain.Buildings;
using CarbonWise.BuildingBlocks.Domain.Waters;
using MediatR;

namespace CarbonWise.BuildingBlocks.Application.Features.Waters.Queries.FilterWaters
{
    public class FilterWatersQuery : IRequest<List<WaterDto>>
    {
        public Guid? BuildingId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class FilterWatersQueryHandler : IRequestHandler<FilterWatersQuery, List<WaterDto>>
    {
        private readonly IWaterRepository _waterRepository;
        private readonly IBuildingRepository _buildingRepository;

        public FilterWatersQueryHandler(
            IWaterRepository waterRepository,
            IBuildingRepository buildingRepository)
        {
            _waterRepository = waterRepository;
            _buildingRepository = buildingRepository;
        }

        public async Task<List<WaterDto>> Handle(FilterWatersQuery request, CancellationToken cancellationToken)
        {
            if (request.BuildingId.HasValue)
            {
                var building = await _buildingRepository.GetByIdAsync(new BuildingId(request.BuildingId.Value));
                if (building == null)
                {
                    throw new ApplicationException($"Building with id {request.BuildingId} not found");
                }
            }

            List<Water> waters;

            if (request.BuildingId.HasValue && request.StartDate.HasValue && request.EndDate.HasValue)
            {
                waters = await _waterRepository.GetByBuildingIdAndDateRangeAsync(
                    new BuildingId(request.BuildingId.Value),
                    request.StartDate.Value,
                    request.EndDate.Value);
            }
            else if (request.BuildingId.HasValue)
            {
                waters = await _waterRepository.GetByBuildingIdAsync(
                    new BuildingId(request.BuildingId.Value));
            }
            else if (request.StartDate.HasValue && request.EndDate.HasValue)
            {
                waters = await _waterRepository.GetByDateRangeAsync(
                    request.StartDate.Value,
                    request.EndDate.Value);
            }
            else
            {
                waters = await _waterRepository.GetAllAsync();
            }

            return waters.Select(w => new WaterDto
            {
                Id = w.Id.Value,
                Date = w.Date,
                InitialMeterValue = w.InitialMeterValue,
                FinalMeterValue = w.FinalMeterValue,
                Usage = w.Usage,
                BuildingId = w.BuildingId.Value,
                BuildingName = w.Building?.Name
            }).ToList();
        }
    }
}
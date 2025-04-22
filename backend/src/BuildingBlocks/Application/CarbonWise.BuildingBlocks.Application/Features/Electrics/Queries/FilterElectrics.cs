using CarbonWise.BuildingBlocks.Domain.Buildings;
using CarbonWise.BuildingBlocks.Domain.Electrics;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace CarbonWise.BuildingBlocks.Application.Features.Electrics.Queries
{

        public class FilterElectricsQuery : IRequest<List<ElectricDto>>
        {
            public Guid? BuildingId { get; set; }
            public DateTime? StartDate { get; set; }
            public DateTime? EndDate { get; set; }
        }

        public class FilterElectricsQueryHandler : IRequestHandler<FilterElectricsQuery, List<ElectricDto>>
        {
            private readonly IElectricRepository _electricRepository;
            private readonly IBuildingRepository _buildingRepository;

            public FilterElectricsQueryHandler(
                IElectricRepository electricRepository,
                IBuildingRepository buildingRepository)
            {
                _electricRepository = electricRepository;
                _buildingRepository = buildingRepository;
            }

            public async Task<List<ElectricDto>> Handle(FilterElectricsQuery request, CancellationToken cancellationToken)
            {
                if (request.BuildingId.HasValue)
                {
                    var building = await _buildingRepository.GetByIdAsync(new BuildingId(request.BuildingId.Value));
                    if (building == null)
                    {
                        throw new ApplicationException($"Building with id {request.BuildingId} not found");
                    }
                }

                List<Electric> electrics;

                if (request.BuildingId.HasValue && request.StartDate.HasValue && request.EndDate.HasValue)
                {
                    electrics = await _electricRepository.GetByBuildingIdAndDateRangeAsync(
                        new BuildingId(request.BuildingId.Value),
                        request.StartDate.Value,
                        request.EndDate.Value);
                }
                else if (request.BuildingId.HasValue)
                {
                    electrics = await _electricRepository.GetByBuildingIdAsync(
                        new BuildingId(request.BuildingId.Value));
                }
                else if (request.StartDate.HasValue && request.EndDate.HasValue)
                {
                    electrics = await _electricRepository.GetByDateRangeAsync(
                        request.StartDate.Value,
                        request.EndDate.Value);
                }
                else
                {
                    throw new ApplicationException("At least one filter parameter is required");
                }

                return electrics.Select(e => new ElectricDto
                {
                    Id = e.Id.Value,
                    Date = e.Date,
                    InitialMeterValue = e.InitialMeterValue,
                    FinalMeterValue = e.FinalMeterValue,
                    Usage = e.Usage,
                    KWHValue = e.KWHValue,
                    BuildingId = e.BuildingId.Value,
                    BuildingName = e.Building?.Name
                }).ToList();
            }
        }
}
